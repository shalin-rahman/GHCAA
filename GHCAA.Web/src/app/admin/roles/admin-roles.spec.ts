import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { AdminRoles } from './admin-roles';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { of, throwError } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';

describe('AdminRoles Component', () => {
    let component: AdminRoles;
    let fixture: ComponentFixture<AdminRoles>;
    let adminServiceMock: any;
    let notificationServiceMock: any;
    let confirmMock: any;

    beforeEach(async () => {
        adminServiceMock = {
            getSystemUsers: vi.fn().mockReturnValue(of([])),
            getRoles: vi.fn().mockReturnValue(of([])),
            createSystemUser: vi.fn().mockReturnValue(of({ success: true })),
            createCustomRole: vi.fn().mockReturnValue(of({ success: true })),
            assignUserRole: vi.fn().mockReturnValue(of({ success: true })),
            removeUserRole: vi.fn().mockReturnValue(of({ success: true })),
            resetSystemUserPassword: vi.fn().mockReturnValue(of({})),
            setSystemUserActive: vi.fn().mockReturnValue(of({})),
            deleteSystemUser: vi.fn().mockReturnValue(of({})),
            resetSystemUserAuthenticator: vi.fn().mockReturnValue(of(undefined)),
            unlockSystemUser: vi.fn().mockReturnValue(of(undefined))
        };
        confirmMock = { confirm: vi.fn().mockReturnValue(of(true)) };

        notificationServiceMock = createNotificationServiceMock();

        await TestBed.configureTestingModule({
            imports: [AdminRoles, ReactiveFormsModule],
            providers: [
                { provide: AdminService, useValue: adminServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock },
                { provide: ConfirmDialogService, useValue: confirmMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminRoles);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load users and roles on init', () => {
        expect(adminServiceMock.getSystemUsers).toHaveBeenCalled();
        expect(adminServiceMock.getRoles).toHaveBeenCalled();
    });

    it('should call api to create new admin user', () => {
        component.createForm.patchValue({
            username: 'admin2',
            password: 'password123',
            role: 'Admin'
        });

        component.createAdmin();

        expect(adminServiceMock.createSystemUser).toHaveBeenCalledWith(expect.anything());
        expect(notificationServiceMock.success).toHaveBeenCalledWith('System user created successfully');
    });

    it('should show error if createAdmin form is invalid', () => {
        component.createForm.patchValue({ username: '' });
        component.createAdmin();
        expect(notificationServiceMock.error).toHaveBeenCalledWith('Please provide a valid username and password (min 6 chars).');
    });

    it('should create custom role', () => {
        component.customRoleName.set('SuperAdmin');
        component.createCustomRole();

        expect(adminServiceMock.createCustomRole).toHaveBeenCalledWith('SuperAdmin');
        expect(notificationServiceMock.success).toHaveBeenCalledWith("Custom role 'SuperAdmin' created successfully");
    });

    it('should assign role to user', () => {
        component.assignRole(1, 'Editor');
        expect(adminServiceMock.assignUserRole).toHaveBeenCalledWith(1, 'Editor');
        expect(notificationServiceMock.success).toHaveBeenCalledWith('Role Editor assigned');
    });

    it('should remove role from user', () => {
        component.removeRole(1, 'Editor');
        expect(adminServiceMock.removeUserRole).toHaveBeenCalledWith(1, 'Editor');
        expect(notificationServiceMock.success).toHaveBeenCalledWith('Role Editor removed');
    });

    it('FR-39: removes a user\'s authenticator app after a danger confirm', async () => {
        await component.resetAuthenticator({ id: 7, username: 'ops' });
        expect(confirmMock.confirm).toHaveBeenCalledWith(expect.objectContaining({ danger: true }));
        expect(adminServiceMock.resetSystemUserAuthenticator).toHaveBeenCalledWith(7);
        expect(notificationServiceMock.success).toHaveBeenCalledWith('Authenticator app removed.');
    });

    it('FR-39: says so when the user has no authenticator app', async () => {
        adminServiceMock.resetSystemUserAuthenticator.mockReturnValue(throwError(() => ({ status: 404 })));
        await component.resetAuthenticator({ id: 7, username: 'ops' });
        expect(notificationServiceMock.error).toHaveBeenCalledWith('No authenticator app set up for this user.');
    });

    it('FR-39: does nothing when the reset is cancelled', async () => {
        confirmMock.confirm.mockReturnValue(of(false));
        await component.resetAuthenticator({ id: 7, username: 'ops' });
        expect(adminServiceMock.resetSystemUserAuthenticator).not.toHaveBeenCalled();
    });

    it('FR-39: unlocks an account after a confirm and reloads the list', async () => {
        adminServiceMock.getSystemUsers.mockClear();
        await component.unlockUser({ id: 7, username: 'ops' });
        expect(adminServiceMock.unlockSystemUser).toHaveBeenCalledWith(7);
        expect(notificationServiceMock.success).toHaveBeenCalledWith('Account unlocked.');
        expect(adminServiceMock.getSystemUsers).toHaveBeenCalled();
    });

    it('FR-39: does nothing when the unlock is cancelled', async () => {
        confirmMock.confirm.mockReturnValue(of(false));
        await component.unlockUser({ id: 7, username: 'ops' });
        expect(adminServiceMock.unlockSystemUser).not.toHaveBeenCalled();
    });

    it('FR-39: shows the server reason when the unlock fails', async () => {
        adminServiceMock.unlockSystemUser.mockReturnValue(throwError(() => ({ status: 403, error: { detail: 'Step-up required.' } })));
        await component.unlockUser({ id: 7, username: 'ops' });
        expect(notificationServiceMock.error).toHaveBeenCalledWith('Step-up required.');
    });

    it('FR-39: shows Unlock only for a user whose lock is still running', () => {
        const future = new Date(Date.now() + 10 * 60_000).toISOString();
        const past = new Date(Date.now() - 60_000).toISOString();
        component.users.set([
            { id: 1, username: 'locked', isActive: true, roles: [], lockedUntil: future },
            { id: 2, username: 'expired', isActive: true, roles: [], lockedUntil: past },
            { id: 3, username: 'never', isActive: true, roles: [] }
        ]);
        component.loading.set(false);
        fixture.detectChanges();

        const buttons = fixture.nativeElement.querySelectorAll('[data-testid="unlock-user"]');
        expect(buttons.length).toBe(1);
    });
});
