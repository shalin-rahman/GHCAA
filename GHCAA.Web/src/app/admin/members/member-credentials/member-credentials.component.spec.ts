import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Subject, of, throwError } from 'rxjs';
import { createNotificationServiceMock } from '../../../core/testing/testing-utils';
import { NotificationService } from '../../../core/services/notification.service';
import { CredentialVerificationService, IssuedCredential } from '../../../core/services/credential-verification.service';
import { MemberCredentialsComponent } from './member-credentials.component';

describe('MemberCredentialsComponent', () => {
    let fixture: ComponentFixture<MemberCredentialsComponent>;
    let component: MemberCredentialsComponent;
    let service: any;
    let notify: any;

    const valid: IssuedCredential = {
        memberId: 42, shortCode: 'ABCD234567', credentialType: 'IdCard',
        issuedOn: '2026-01-01T00:00:00Z', expiresOn: '2099-01-01T00:00:00Z', isRevoked: false
    };
    const revoked: IssuedCredential = {
        memberId: 42, shortCode: 'EFGH234567', credentialType: 'MembershipCertificate',
        issuedOn: '2025-01-01T00:00:00Z', isRevoked: true, revokedReason: 'Lost', revokedOn: '2025-06-01T00:00:00Z'
    };

    beforeEach(async () => {
        service = {
            getMemberCredentials: vi.fn().mockReturnValue(of([valid, revoked])),
            revoke: vi.fn().mockReturnValue(of(undefined)),
            reissue: vi.fn().mockReturnValue(of({ ...valid, shortCode: 'JKLM234567' }))
        };
        notify = createNotificationServiceMock();

        await TestBed.configureTestingModule({
            imports: [MemberCredentialsComponent],
            providers: [
                { provide: CredentialVerificationService, useValue: service },
                { provide: NotificationService, useValue: notify }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(MemberCredentialsComponent);
        component = fixture.componentInstance;
        fixture.componentRef.setInput('memberId', 42);
        fixture.detectChanges();
    });

    function buttons(): HTMLButtonElement[] {
        return Array.from(fixture.nativeElement.querySelectorAll('button'));
    }

    it('loads and lists the member codes', () => {
        expect(service.getMemberCredentials).toHaveBeenCalledWith(42);
        const text = fixture.nativeElement.textContent;
        expect(text).toContain('ABCD234567');
        expect(text).toContain('Revoked');
        expect(text).toContain('Lost');
    });

    it('offers Revoke only for codes that are not revoked', () => {
        expect(buttons().filter(b => b.textContent?.trim() === 'Revoke')).toHaveLength(1);
        expect(buttons().filter(b => b.textContent?.trim() === 'Reissue')).toHaveLength(2);
    });

    it('needs a reason before the confirm button works', () => {
        component.start('ABCD234567', 'revoke');
        fixture.detectChanges();

        const confirm = buttons().find(b => b.textContent?.includes('Confirm revoke'))!;
        expect(confirm.disabled).toBe(true);

        component.submit();
        expect(service.revoke).not.toHaveBeenCalled();
    });

    it('revokes with the trimmed reason and reloads', () => {
        component.start('ABCD234567', 'revoke');
        component.reason.set('  Lost card ');
        component.submit();

        expect(service.revoke).toHaveBeenCalledWith('ABCD234567', 'Lost card');
        expect(notify.success).toHaveBeenCalled();
        expect(service.getMemberCredentials).toHaveBeenCalledTimes(2);
        expect(component.pending()).toBeNull();
    });

    it('reissues and names the new code', () => {
        component.start('ABCD234567', 'reissue');
        component.reason.set('Name changed');
        component.submit();

        expect(service.reissue).toHaveBeenCalledWith('ABCD234567', 'Name changed');
        expect(notify.success).toHaveBeenCalledWith(expect.stringContaining('JKLM234567'));
    });

    it('ignores a late reply for the member the admin already left', () => {
        const slow = new Subject<IssuedCredential[]>();
        service.getMemberCredentials.mockImplementation((id: number) => id === 42 ? slow : of([{ ...valid, memberId: 7, shortCode: 'NPQR234567' }]));
        fixture.componentRef.setInput('memberId', 42);
        component.ngOnChanges();
        fixture.componentRef.setInput('memberId', 7);
        component.ngOnChanges();

        slow.next([valid]);

        expect(component.items().map(i => i.shortCode)).toEqual(['NPQR234567']);
    });

    it('treats an expiry without a zone as UTC', () => {
        expect(component.isExpired({ ...valid, expiresOn: '2000-01-01T00:00:00' })).toBe(true);
        expect(component.isExpired({ ...valid, expiresOn: undefined })).toBe(false);
    });

    it('lists nothing when the load fails', () => {
        service.getMemberCredentials.mockReturnValue(throwError(() => new Error('down')));
        component.ngOnChanges();

        expect(component.items()).toEqual([]);
        expect(component.loading()).toBe(false);
    });

    it('keeps the reason box open on failure and leaves the toast to the interceptor', () => {
        service.revoke.mockReturnValue(throwError(() => ({ error: { detail: 'Step-up cancelled.' } })));
        component.start('ABCD234567', 'revoke');
        component.reason.set('Lost');
        component.submit();

        expect(notify.error).not.toHaveBeenCalled();
        expect(notify.success).not.toHaveBeenCalled();
        expect(component.pending()).not.toBeNull();
        expect(component.busy()).toBe(false);
    });
});
