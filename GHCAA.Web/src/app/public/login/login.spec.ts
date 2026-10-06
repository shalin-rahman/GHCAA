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

    // 7.17, spec 012 FR-034. This spec does not render the template, so it checks the state
    // the template reads rather than the DOM.
    it('loads no provider script when the server lists none', () => {
        const initGoogle = vi.spyOn(component, 'initGoogleAuth').mockImplementation(() => undefined);
        const initFacebook = vi.spyOn(component, 'initFacebookAuth').mockImplementation(() => undefined);

        component.loadSocialProviders();

        expect(component.socialProviders()).toEqual([]);
        expect(initGoogle).not.toHaveBeenCalled();
        expect(initFacebook).not.toHaveBeenCalled();
    });

    it('enables only the providers the server lists', () => {
        const initGoogle = vi.spyOn(component, 'initGoogleAuth').mockImplementation(() => undefined);
        const initFacebook = vi.spyOn(component, 'initFacebookAuth').mockImplementation(() => undefined);
        authServiceMock.getSocialProviders.mockReturnValue(of([{ provider: 'Google', clientId: 'g-id' }]));

        component.loadSocialProviders();

        expect(component.isProviderEnabled('Google')).toBe(true);
        expect(component.isProviderEnabled('Facebook')).toBe(false);
        expect(initGoogle).toHaveBeenCalledWith('g-id');
        expect(initFacebook).not.toHaveBeenCalled();
    });

    it('should name only the real steps, in order: the check, then opening the portal', async () => {
        let finishNavigation!: (ok: boolean) => void;
        vi.mocked(router.navigate).mockReturnValue(new Promise<boolean>(resolve => finishNavigation = resolve));
        authServiceMock.login.mockReturnValue(new Observable(subscriber => {
            setTimeout(() => subscriber.next({ role: 'User' }), 2500);
            return () => undefined;
        }));
        component.credentials = { username: 'user', password: 'password' };

        component.onLogin(mockForm);

        expect(component.loading()).toBe(true);
        expect(component.loginStatus()).toBe('Checking your username and password');

        // The password check is still running, so the text must not move on by itself.
        vi.advanceTimersByTime(2000);
        expect(component.loginStatus()).toBe('Checking your username and password');

        vi.advanceTimersByTime(500);
        expect(component.loading()).toBe(true);
        expect(component.loginStatus()).toBe('Opening your portal');

        finishNavigation(true);
        await Promise.resolve();
        expect(component.loading()).toBe(false);
        expect(component.loginStatus()).toBeNull();
    });

    it('should give the button back when the navigation after login is refused', async () => {
        vi.mocked(router.navigate).mockResolvedValue(false);
        authServiceMock.login.mockReturnValue(of({ role: 'User' }));
        component.credentials = { username: 'user', password: 'password' };

        component.onLogin(mockForm);
        await Promise.resolve();

        expect(component.loading()).toBe(false);
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

    it('should show the reason the API gives in a problem response', () => {
        authServiceMock.login.mockReturnValue(throwError(() => ({ status: 401, error: { title: 'Unauthorized', detail: 'Invalid username or password' } })));
        component.onLogin(mockForm);
        expect(component.errorMessage()).toBe('Invalid username or password');
    });

    it('should still sign in when the server answers after the old 8 second limit', async () => {
        vi.mocked(router.navigate).mockResolvedValue(true);
        authServiceMock.login.mockReturnValue(new Observable(subscriber => {
            setTimeout(() => subscriber.next({ role: 'User' }), 20000);
            return () => undefined;
        }));
        component.credentials = { username: 'slowuser', password: 'password' };

        component.onLogin(mockForm);
        vi.advanceTimersByTime(20000);
        await Promise.resolve();

        expect(component.errorMessage()).toBe('');
        expect(component.loading()).toBe(false);
        expect(router.navigate).toHaveBeenCalledWith(['/portal/dashboard']);
    });

    it('should say once that the server is starting up when the reply is slow', () => {
        authServiceMock.login.mockReturnValue(new Observable(() => undefined));
        component.credentials = { username: 'slowuser', password: 'password' };

        component.onLogin(mockForm);
        vi.advanceTimersByTime(4999);
        expect(component.loginStatus()).toBe('Checking your username and password');

        vi.advanceTimersByTime(1);
        const waking = component.loginStatus();
        expect(waking).toBe('The server is starting up. This can take up to a minute.');

        // It stays put rather than cycling through more lines.
        vi.advanceTimersByTime(40000);
        expect(component.loading()).toBe(true);
        expect(component.loginStatus()).toBe(waking);
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
