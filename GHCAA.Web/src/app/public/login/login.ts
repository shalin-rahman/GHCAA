import { Component, inject, ChangeDetectorRef, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { LoginDto, SocialProviderConfig, User } from '../../core/models/auth.models';
import { Icon } from '../../common/icon/icon';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';
import { ROUTES } from '../../core/constants/app.constants';
import { OrgConfigService } from '../../core/services/org-config.service';
import { Observable, Subscription } from 'rxjs';

// Signing in is one request and then a page change, so the text below the button follows those
// in order. Each line shows once and the list never loops. The button already says "Signing in",
// so none of these repeat it.
const LOGIN_STATUS_CHECKING = 'Checking your username and password';
const LOGIN_STATUS_OPENING = 'Signed in. Opening your portal';
// A normal login answers well inside 5 seconds. Past that the server was most likely asleep, so
// the lines after it explain the wait as it gets longer. The last one stays until the reply or
// the timeout.
const SLOW_LOGIN_STEPS: ReadonlyArray<{ afterMs: number; text: string }> = [
  { afterMs: 5000, text: 'The server is starting up. This can take up to a minute.' },
  { afterMs: 20000, text: 'Still starting up. Your sign-in has been sent.' },
  { afterMs: 40000, text: 'Nearly there. Please keep this page open.' }
];
// A Render free-tier wake plus a cold database can take most of a minute. The old 8s limit gave
// up while the request was still running, so a login that then succeeded was thrown away and the
// user was left on this page.
const AUTH_TIMEOUT_MS = 60000;

declare var google: any;
declare var FB: any;

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, Icon, ImgFallbackDirective],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login implements OnInit, OnDestroy {
  private auth = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private notify = inject(NotificationService);
  private cdr = inject(ChangeDetectorRef);
  orgConfig = inject(OrgConfigService);

  credentials: LoginDto = { username: '', password: '' };
  loading = signal(false);
  errorMessage = signal('');
  loginStatus = signal<string | null>(null);
  showPassword = signal(false);
  socialProviders = signal<SocialProviderConfig[]>([]);
  // 29D.8: where to send the user after a successful login (set by authGuard).
  private returnUrl: string | null = null;
  private slowLoginTimers: number[] = [];
  private authTimeoutTimer: number | null = null;
  private loginRequest: Subscription | null = null;
  private loginAttemptId = 0;

  ngOnInit() {
    this.loadSocialProviders();
    // 29D.8: honor the guard's returnUrl and surface the session-expired reason instead
    // of silently discarding both (previously every login went to a fixed default page).
    const params = this.route.snapshot.queryParamMap;
    this.returnUrl = params.get('returnUrl');
    if (params.get('expired')) {
      this.errorMessage.set('Your session expired. Please sign in again to continue.');
    }
  }

  ngOnDestroy() {
    this.cancelLoginRequest();
    this.clearLoginProgress();
    this.loginAttemptId += 1;
  }

  private clearLoginTimers(): void {
    this.slowLoginTimers.forEach(timer => window.clearTimeout(timer));
    this.slowLoginTimers = [];

    if (this.authTimeoutTimer !== null) {
      window.clearTimeout(this.authTimeoutTimer);
      this.authTimeoutTimer = null;
    }
  }

  private clearLoginProgress(): void {
    this.clearLoginTimers();
    this.loginStatus.set(null);
  }

  private startLoginProgress(attemptId: number): void {
    this.clearLoginProgress();
    this.loginAttemptId = attemptId;
    this.loginStatus.set(LOGIN_STATUS_CHECKING);
    this.loading.set(true);
    this.errorMessage.set('');

    this.slowLoginTimers = SLOW_LOGIN_STEPS.map(step => window.setTimeout(() => {
      if (this.loginAttemptId === attemptId) {
        this.loginStatus.set(step.text);
      }
    }, step.afterMs));

    this.authTimeoutTimer = window.setTimeout(() => {
      if (this.loginAttemptId !== attemptId) {
        return;
      }

      // Cancel the request too, so a late reply can't sign the user in behind this message.
      this.cancelLoginRequest();
      this.finishLoginProgress();
      this.errorMessage.set('Login timed out. Please try again.');
    }, AUTH_TIMEOUT_MS);
  }

  private cancelLoginRequest(): void {
    this.loginRequest?.unsubscribe();
    this.loginRequest = null;
  }

  private runLogin(request: Observable<User>, fallbackError: string): void {
    const attemptId = this.loginAttemptId + 1;
    this.startLoginProgress(attemptId);
    this.cancelLoginRequest();
    this.loginRequest = request.subscribe({
      next: (user) => this.handleAuthSuccess(user, attemptId),
      error: (err) => this.handleAuthError(err, attemptId, fallbackError)
    });
  }

  private finishLoginProgress(): void {
    this.loginAttemptId += 1;
    this.clearLoginProgress();
    this.loading.set(false);
  }

  loadSocialProviders() {
    this.auth.getSocialProviders().subscribe({
      next: (providers) => {
        this.socialProviders.set(providers);
        // The server already dropped anything that cannot sign in, so no script loads on an empty list.
        const googleConfig = providers.find(p => p.provider === 'Google');
        if (googleConfig) {
          this.initGoogleAuth(googleConfig.clientId);
        }
        const fbConfig = providers.find(p => p.provider === 'Facebook');
        if (fbConfig) {
          this.initFacebookAuth(fbConfig.clientId);
        }
      },
      // 29F.2: social login is optional — degrade quietly (hide the buttons) rather than
      // showing a scary error on the login page, but don't swallow the failure silently.
      error: (err) => console.error('Failed to load social login providers', err)
    });
  }

  isProviderEnabled(provider: SocialProviderConfig['provider']): boolean {
    return this.socialProviders().some(p => p.provider === provider);
  }

  initGoogleAuth(clientId: string) {
    if (typeof google === 'undefined') {
      const script = document.createElement('script');
      script.src = 'https://accounts.google.com/gsi/client';
      script.async = true;
      script.defer = true;
      script.onload = () => {
        google.accounts.id.initialize({
          client_id: clientId,
          callback: (response: any) => this.handleGoogleLogin(response.credential)
        });
      };
      document.head.appendChild(script);
    } else {
      google.accounts.id.initialize({
        client_id: clientId,
        callback: (response: any) => this.handleGoogleLogin(response.credential)
      });
    }
  }

  initFacebookAuth(appId: string) {
    if (typeof FB === 'undefined') {
      (window as any).fbAsyncInit = function() {
        FB.init({
          appId      : appId,
          cookie     : true,
          xfbml      : true,
          version    : 'v18.0'
        });
      };

      const script = document.createElement('script');
      script.src = 'https://connect.facebook.net/en_US/sdk.js';
      script.async = true;
      script.defer = true;
      document.head.appendChild(script);
    }
  }

  loginWithGoogle() {
    google.accounts.id.prompt(); // Show one tap or login prompt
  }

  handleGoogleLogin(idToken: string) {
    this.runLogin(this.auth.googleLogin(idToken), 'Authentication failed. Please try again.');
  }

  loginWithFacebook() {
    if (this.loading()) {
      return;
    }

    FB.login((response: any) => {
      if (response.authResponse) {
        this.runLogin(this.auth.facebookLogin(response.authResponse.accessToken), 'Authentication failed. Please try again.');
      }
    }, { scope: 'public_profile,email' });
  }

  private handleAuthSuccess(user: User, attemptId: number) {
    if (this.loginAttemptId !== attemptId) {
      return;
    }

    // Stay busy until the page change ends. The portal's code still has to load, and if a guard
    // turns the navigation away the button has to come back.
    this.clearLoginTimers();
    this.loginStatus.set(LOGIN_STATUS_OPENING);
    const done = () => this.finishLoginProgress();
    this.navigateAfterLogin(user).then(done, done);
  }

  private navigateAfterLogin(user: User): Promise<boolean> {
    // 29D.8: prefer the guard-supplied returnUrl (only for in-app paths, never an external
    // or protocol-relative URL) before falling back to the role default.
    if (this.returnUrl && this.returnUrl.startsWith('/') && !this.returnUrl.startsWith('//')) {
      return this.router.navigateByUrl(this.returnUrl);
    }
    if (user.role === 'Admin' || user.role === 'SuperAdmin') {
      return this.router.navigate(['/admin/approvals']);
    }
    return this.router.navigate([ROUTES.PORTAL_DASHBOARD]);
  }

  private handleAuthError(err: any, attemptId: number, fallbackError: string) {
    if (this.loginAttemptId !== attemptId) {
      return;
    }

    this.finishLoginProgress();
    this.errorMessage.set(err.error?.detail || err.error?.message || fallbackError);
  }

  togglePassword() {
    this.showPassword.set(!this.showPassword());
  }

  onLogin(form: any) {
    if (form.invalid || this.loading()) {
      if (form.invalid) {
        form.control.markAllAsTouched();
      }
      return;
    }

    this.runLogin(this.auth.login(this.credentials), 'Invalid username or password.');
  }

  forgotPassword() {
    const email = this.credentials.username.trim();
    if (!email.includes('@')) {
      this.notify.info('Type your email address in the Username box, then click Forgot password again.');
      return;
    }

    // The API answers the same way whether or not the email has an account, so the message does too.
    const sent = 'If that email has an account, a reset link is on its way. Check your inbox.';
    this.auth.forgotPassword(email).subscribe({
      next: () => this.notify.info(sent),
      error: (err) => {
        if (err?.status === 429) {
          this.notify.error('Too many reset requests. Please wait a few minutes and try again.');
        } else {
          this.notify.info(sent);
        }
      }
    });
  }
}
