import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { AdminRoles } from './admin-roles';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';

describe('AdminRoles Component', () => {
    let component: AdminRoles;
    let fixture: ComponentFixture<AdminRoles>;
    let adminServiceMock: any;
    let notificationServiceMock: any;

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
            deleteSystemUser: vi.fn().mockReturnValue(of({}))
        };

        notificationServiceMock = createNotificationServiceMock();

        await TestBed.configureTestingModule({
            imports: [AdminRoles, ReactiveFormsModule],
            providers: [
                { provide: AdminService, useValue: adminServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock }
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
});
