import { createAuthServiceMock } from '../../core/testing/testing-utils';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Login } from './login';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { provideRouter } from '@angular/router';

describe('Login Component', () => {
    let component: Login;
    let fixture: ComponentFixture<Login>;
    let authServiceMock: any;
    let notificationServiceMock: any;
    let router: Router;
    const mockForm = { 
        invalid: false, 
        control: { markAllAsTouched: vi.fn() } 
    };

    beforeEach(async () => {
        authServiceMock = {
            login: vi.fn()
        };

        notificationServiceMock = {
            info: vi.fn(),
            error: vi.fn()
        };

        await TestBed.configureTestingModule({
            imports: [Login],
            providers: [
                { provide: AuthService, useValue: authServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock },
                provideRouter([])
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Login);
        component = fixture.componentInstance;
        router = TestBed.inject(Router);
        vi.spyOn(router, 'navigate');
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should navigate to admin portal for admin role', () => {
        authServiceMock.login.mockReturnValue(of({ role: 'Admin' }));
        component.credentials = { username: 'admin', password: 'password' };
        component.onLogin(mockForm);
        expect(router.navigate).toHaveBeenCalledWith(['/admin/approvals']);
    });

    it('should navigate to portal dashboard for user role', () => {
        authServiceMock.login.mockReturnValue(of({ role: 'User' }));
        component.credentials = { username: 'user', password: 'password' };
        component.onLogin(mockForm);
        expect(router.navigate).toHaveBeenCalledWith(['/portal/dashboard']);
    });

    it('should show error on login failure', () => {
        authServiceMock.login.mockReturnValue(throwError(() => ({ error: { message: 'Invalid username or password.' } })));
        component.onLogin(mockForm);
        expect(component.errorMessage()).toBe('Invalid username or password.');
    });
});
