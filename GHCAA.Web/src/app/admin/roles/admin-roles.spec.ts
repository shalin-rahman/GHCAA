import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminRoles } from './admin-roles';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { NotificationService } from '../../core/services/notification.service';
import { of, throwError } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

describe('AdminRoles Component', () => {
    let component: AdminRoles;
    let fixture: ComponentFixture<AdminRoles>;
    let httpMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        httpMock = {
            get: vi.fn().mockReturnValue(of([])),
            post: vi.fn().mockReturnValue(of({ success: true }))
        };

        notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn(),
            warning: vi.fn()
        };

        await TestBed.configureTestingModule({
            imports: [AdminRoles, ReactiveFormsModule],
            providers: [
                { provide: HttpClient, useValue: httpMock },
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
        expect(httpMock.get).toHaveBeenCalledWith('/api/roles/users');
        expect(httpMock.get).toHaveBeenCalledWith('/api/roles');
    });

    it('should call api to create new admin user', () => {
        component.createForm.patchValue({
            username: 'admin2',
            password: 'password123',
            role: 'Admin'
        });
        
        component.createAdmin();
        
        expect(httpMock.post).toHaveBeenCalledWith('/api/roles/users', expect.anything());
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
        
        expect(httpMock.post).toHaveBeenCalledWith('/api/roles', '"SuperAdmin"', expect.anything());
        expect(notificationServiceMock.success).toHaveBeenCalledWith("Custom role 'SuperAdmin' created successfully");
    });

    it('should assign role to user', () => {
        component.assignRole(1, 'Editor');
        expect(httpMock.post).toHaveBeenCalledWith('/api/roles/assign', null, expect.anything());
        expect(notificationServiceMock.success).toHaveBeenCalledWith('Role Editor assigned');
    });

    it('should remove role from user', () => {
        component.removeRole(1, 'Editor');
        expect(httpMock.post).toHaveBeenCalledWith('/api/roles/remove', null, expect.anything());
        expect(notificationServiceMock.success).toHaveBeenCalledWith('Role Editor removed');
    });
});


