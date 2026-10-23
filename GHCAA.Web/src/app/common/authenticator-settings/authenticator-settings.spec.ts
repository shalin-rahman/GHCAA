import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { AuthenticatorSettings } from './authenticator-settings';
import { StepUpService } from '../../core/services/step-up.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { NotificationService } from '../../core/services/notification.service';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';

describe('AuthenticatorSettings', () => {
    let fixture: ComponentFixture<AuthenticatorSettings>;
    let component: AuthenticatorSettings;
    let stepUp: any;
    let confirm: any;
    let notify: any;

    const notEnrolled = { enrolled: false, pending: false, recoveryCodesLeft: 0 };
    const enrolled = { enrolled: true, pending: false, recoveryCodesLeft: 10 };

    async function create(status: any = notEnrolled) {
        stepUp = {
            authenticatorStatus: vi.fn().mockReturnValue(of(status)),
            setupAuthenticator: vi.fn().mockReturnValue(of({
                secret: 'ABCDEFGH', otpAuthUri: 'otpauth://totp/x', qrCodeDataUri: 'data:image/png;base64,AA=='
            })),
            setupAuthenticatorWithPassword: vi.fn().mockReturnValue(of({
                secret: 'PWSECRET', otpAuthUri: 'otpauth://totp/x', qrCodeDataUri: 'data:image/png;base64,AA=='
            })),
            confirmAuthenticator: vi.fn().mockReturnValue(of({ codes: ['AAAAA-11111', 'BBBBB-22222'] })),
            regenerateRecoveryCodes: vi.fn().mockReturnValue(of({ codes: ['CCCCC-33333'] })),
            removeAuthenticator: vi.fn().mockReturnValue(of(undefined))
        };
        confirm = { confirm: vi.fn().mockReturnValue(of(true)) };
        notify = createNotificationServiceMock();

        await TestBed.configureTestingModule({
            imports: [AuthenticatorSettings],
            providers: [
                { provide: StepUpService, useValue: stepUp },
                { provide: ConfirmDialogService, useValue: confirm },
                { provide: NotificationService, useValue: notify }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AuthenticatorSettings);
        component = fixture.componentInstance;
        fixture.detectChanges();
    }

    const text = () => (fixture.nativeElement as HTMLElement).textContent ?? '';

    it('FR-39: offers setup when no app is enrolled', async () => {
        await create();
        expect(stepUp.authenticatorStatus).toHaveBeenCalled();
        expect(text()).toContain('Set up an app');
    });

    it('FR-39: shows the QR code and key, then the recovery codes once the code is confirmed', async () => {
        await create();
        component.startSetup();
        fixture.detectChanges();
        const img = fixture.nativeElement.querySelector('img.authenticator-qr') as HTMLImageElement;
        expect(img.getAttribute('src')).toBe('data:image/png;base64,AA==');
        expect(text()).toContain('ABCDEFGH');

        component.code = ' 123456 ';
        component.confirmSetup();
        fixture.detectChanges();

        expect(stepUp.confirmAuthenticator).toHaveBeenCalledWith('123456');
        expect(component.setup()).toBeNull();
        expect(text()).toContain('AAAAA-11111');
        expect(text()).toContain('BBBBB-22222');

        component.doneWithCodes();
        fixture.detectChanges();
        expect(text()).not.toContain('AAAAA-11111');
    });

    // 95.6: no email means no email step-up, so setup asks for the password.
    it('FR-39: asks for the password when there is no email, and sets up with it', async () => {
        await create({ ...notEnrolled, setupNeedsPassword: true });
        expect(fixture.nativeElement.querySelector('#authenticatorPassword')).not.toBeNull();

        component.startSetup();
        expect(stepUp.setupAuthenticatorWithPassword).not.toHaveBeenCalled();

        component.password = 'secret-pw';
        component.startSetup();
        fixture.detectChanges();
        expect(stepUp.setupAuthenticatorWithPassword).toHaveBeenCalledWith('secret-pw');
        expect(stepUp.setupAuthenticator).not.toHaveBeenCalled();
        expect(component.password).toBe('');
        expect(text()).toContain('PWSECRET');
    });

    it('FR-39: shows the server error when the password is wrong', async () => {
        await create({ ...notEnrolled, setupNeedsPassword: true });
        stepUp.setupAuthenticatorWithPassword.mockReturnValue(throwError(() => ({ error: { detail: 'That password is wrong.' } })));
        component.password = 'bad';
        component.startSetup();
        fixture.detectChanges();
        expect(text()).toContain('That password is wrong.');
        expect(component.setup()).toBeNull();
    });

    it('FR-39: uses the step-up setup and shows no password box when there is an email', async () => {
        await create();
        expect(fixture.nativeElement.querySelector('#authenticatorPassword')).toBeNull();
        component.startSetup();
        expect(stepUp.setupAuthenticator).toHaveBeenCalled();
        expect(stepUp.setupAuthenticatorWithPassword).not.toHaveBeenCalled();
    });

    it('FR-39: does not send a code that is not 6 digits', async () => {
        await create();
        component.startSetup();
        component.code = '12345';
        component.confirmSetup();
        expect(stepUp.confirmAuthenticator).not.toHaveBeenCalled();
    });

    it('FR-39: keeps the setup open and shows the error when the code is wrong', async () => {
        await create();
        stepUp.confirmAuthenticator.mockReturnValue(throwError(() => ({ error: { detail: 'Wrong code.' } })));
        component.startSetup();
        component.code = '000000';
        component.confirmSetup();
        expect(component.setup()).not.toBeNull();
        expect(component.errorMessage()).toBe('Wrong code.');
        expect(component.busy()).toBe(false);
    });

    it('FR-39: shows codes left and gets new codes after the user confirms', async () => {
        await create(enrolled);
        expect(text()).toContain('Recovery codes left: 10');

        component.regenerateCodes();
        expect(stepUp.regenerateRecoveryCodes).toHaveBeenCalled();
        expect(component.recoveryCodes()).toEqual(['CCCCC-33333']);
    });

    it('FR-39: leaves the codes alone when the user cancels', async () => {
        await create(enrolled);
        confirm.confirm.mockReturnValue(of(false));
        component.regenerateCodes();
        component.remove();
        expect(stepUp.regenerateRecoveryCodes).not.toHaveBeenCalled();
        expect(stepUp.removeAuthenticator).not.toHaveBeenCalled();
    });

    it('FR-39: removes the app after a danger confirm', async () => {
        await create(enrolled);
        component.remove();
        expect(confirm.confirm).toHaveBeenCalledWith(expect.objectContaining({ danger: true }));
        expect(stepUp.removeAuthenticator).toHaveBeenCalled();
        expect(notify.success).toHaveBeenCalledWith('Authenticator app removed.');
    });

    it('shows an error when the status cannot be loaded', async () => {
        await create();
        stepUp.authenticatorStatus.mockReturnValue(throwError(() => new Error('down')));
        component.loadStatus();
        expect(component.errorMessage()).toBe('Could not load the authenticator app settings.');
    });
});
