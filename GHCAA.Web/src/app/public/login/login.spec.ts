import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Login } from './login';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { Router } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
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

    beforeEach(() => {
        vi.useFakeTimers();
        authServiceMock = {
            login: vi.fn(),
            getSocialProviders: vi.fn().mockReturnValue(of([]))
        };

        notificationServiceMock = {
            info: vi.fn(),
            error: vi.fn()
        };
    });

    beforeEach(async () => {
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

    afterEach(() => {
        vi.runOnlyPendingTimers();
        vi.useRealTimers();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should start a sequential login status and hide it on success', () => {
        authServiceMock.login.mockReturnValue(new Observable(subscriber => {
            setTimeout(() => subscriber.next({ role: 'User' }), 1500);
            return () => undefined;
        }));
        component.credentials = { username: 'user', password: 'password' };

        component.onLogin(mockForm);

        expect(component.loading()).toBe(true);
        expect(component.loginStatus()).toBeTruthy();
        expect(component.loginStatus()).toBe('Connecting');

        vi.advanceTimersByTime(1200);
        expect(component.loginStatus()).not.toBe('Connecting');

        vi.advanceTimersByTime(500);
        expect(component.loading()).toBe(false);
        expect(component.loginStatus()).toBeNull();
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

    it('should stop safely when the server does not respond within 8 seconds', () => {
        authServiceMock.login.mockReturnValue(new Observable(() => undefined));
        component.credentials = { username: 'slowuser', password: 'password' };

        component.onLogin(mockForm);
        vi.advanceTimersByTime(7999);

        expect(component.loading()).toBe(true);
        expect(component.errorMessage()).toBe('');

        vi.advanceTimersByTime(1);

        expect(component.loading()).toBe(false);
        expect(component.loginStatus()).toBeNull();
        expect(component.errorMessage()).toBe('Login timed out. Please try again.');
    });
});
