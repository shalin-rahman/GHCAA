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
            forgotPassword: vi.fn().mockReturnValue(of({})),
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
            setTimeout(() => subscriber.next({ role: 'User' }), 2500);
            return () => undefined;
        }));
        component.credentials = { username: 'user', password: 'password' };

        component.onLogin(mockForm);

        expect(component.loading()).toBe(true);
        expect(component.loginStatus()).toBe('Checking your details');

        vi.advanceTimersByTime(2000);
        expect(component.loginStatus()).toBe('Verifying your password');

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

    it('should still sign in when the server answers after the old 8 second limit', () => {
        authServiceMock.login.mockReturnValue(new Observable(subscriber => {
            setTimeout(() => subscriber.next({ role: 'User' }), 20000);
            return () => undefined;
        }));
        component.credentials = { username: 'slowuser', password: 'password' };

        component.onLogin(mockForm);
        vi.advanceTimersByTime(20000);

        expect(component.errorMessage()).toBe('');
        expect(component.loading()).toBe(false);
        expect(router.navigate).toHaveBeenCalledWith(['/portal/dashboard']);
    });

    it('should tell the user the server is waking up and keep cycling instead of freezing', () => {
        authServiceMock.login.mockReturnValue(new Observable(() => undefined));
        component.credentials = { username: 'slowuser', password: 'password' };

        component.onLogin(mockForm);
        // Past the four normal steps the slow-server messages take over.
        vi.advanceTimersByTime(4 * 2000);
        expect(component.loginStatus()).toBe('Waking up the server');

        const seen = new Set<string>();
        for (let i = 0; i < 20; i++) {
            vi.advanceTimersByTime(2000);
            seen.add(component.loginStatus()!);
        }

        expect(component.loading()).toBe(true);
        expect(seen).toEqual(new Set(['Waking up the server', 'This can take up to a minute', 'Still signing you in']));
    });

    it('should stop and cancel the request when the server does not respond within 60 seconds', () => {
        const teardown = vi.fn();
        authServiceMock.login.mockReturnValue(new Observable(() => teardown));
        component.credentials = { username: 'slowuser', password: 'password' };

        component.onLogin(mockForm);
        vi.advanceTimersByTime(59999);

        expect(component.loading()).toBe(true);
        expect(component.errorMessage()).toBe('');

        vi.advanceTimersByTime(1);

        expect(component.loading()).toBe(false);
        expect(component.loginStatus()).toBeNull();
        expect(component.errorMessage()).toBe('Login timed out. Please try again.');
        expect(teardown).toHaveBeenCalled();
    });

    it('should ask for an email before sending a reset link', () => {
        component.credentials = { username: 'shalin', password: '' };

        component.forgotPassword();

        expect(authServiceMock.forgotPassword).not.toHaveBeenCalled();
        expect(notificationServiceMock.info).toHaveBeenCalledWith(expect.stringContaining('Type your email'));
    });

    it('should request a reset link for an email and show the generic message', () => {
        component.credentials = { username: ' someone@example.com ', password: '' };

        component.forgotPassword();

        expect(authServiceMock.forgotPassword).toHaveBeenCalledWith('someone@example.com');
        expect(notificationServiceMock.info).toHaveBeenCalledWith(expect.stringContaining('If that email has an account'));
    });

    it('should show the rate-limit message when the API returns 429', () => {
        authServiceMock.forgotPassword.mockReturnValue(throwError(() => ({ status: 429 })));
        component.credentials = { username: 'someone@example.com', password: '' };

        component.forgotPassword();

        expect(notificationServiceMock.error).toHaveBeenCalledWith(expect.stringContaining('Too many'));
    });
});
