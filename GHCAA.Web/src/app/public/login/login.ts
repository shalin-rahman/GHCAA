import { Component, inject, ChangeDetectorRef, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { LoginDto, User } from '../../core/models/auth.models';
import { Icon } from '../../common/icon/icon';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';

declare var google: any;
declare var FB: any;

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, Icon, ImgFallbackDirective],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login implements OnInit {
  private auth = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private notify = inject(NotificationService);
  private cdr = inject(ChangeDetectorRef);

  credentials: LoginDto = { username: '', password: '' };
  loading = signal(false);
  errorMessage = signal('');
  showPassword = signal(false);
  socialProviders = signal<any[]>([]);
  // 29D.8: where to send the user after a successful login (set by authGuard).
  private returnUrl: string | null = null;

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

  loadSocialProviders() {
    this.auth.getSocialProviders().subscribe({
      next: (providers) => {
        this.socialProviders.set(providers);
        const googleConfig = providers.find(p => p.provider === 'Google' && p.isEnabled);
        if (googleConfig) {
          this.initGoogleAuth(googleConfig.clientId);
        }
        const fbConfig = providers.find(p => p.provider === 'Facebook' && p.isEnabled);
        if (fbConfig) {
          this.initFacebookAuth(fbConfig.clientId);
        }
      },
      // 29F.2: social login is optional — degrade quietly (hide the buttons) rather than
      // showing a scary error on the login page, but don't swallow the failure silently.
      error: (err) => console.error('Failed to load social login providers', err)
    });
  }

  isProviderEnabled(provider: string): boolean {
    return this.socialProviders().some(p => p.provider === provider && p.isEnabled);
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
    this.loading.set(true);
    this.auth.googleLogin(idToken).subscribe({
      next: (user) => this.handleAuthSuccess(user),
      error: (err) => this.handleAuthError(err)
    });
  }

  loginWithFacebook() {
    FB.login((response: any) => {
      if (response.authResponse) {
        this.loading.set(true);
        this.auth.facebookLogin(response.authResponse.accessToken).subscribe({
          next: (user) => this.handleAuthSuccess(user),
          error: (err) => this.handleAuthError(err)
        });
      }
    }, { scope: 'public_profile,email' });
  }

  private handleAuthSuccess(user: User) {
    this.loading.set(false);
    this.navigateAfterLogin(user);
  }

  private navigateAfterLogin(user: User) {
    // 29D.8: prefer the guard-supplied returnUrl (only for in-app paths, never an external
    // or protocol-relative URL) before falling back to the role default.
    if (this.returnUrl && this.returnUrl.startsWith('/') && !this.returnUrl.startsWith('//')) {
      this.router.navigateByUrl(this.returnUrl);
      return;
    }
    if (user.role === 'Admin' || user.role === 'SuperAdmin') {
      this.router.navigate(['/admin/approvals']);
    } else {
      this.router.navigate(['/portal/dashboard']);
    }
  }

  private handleAuthError(err: any) {
    this.loading.set(false);
    this.errorMessage.set(err.error?.message || 'Authentication failed. Please try again.');
  }

  togglePassword() {
    this.showPassword.set(!this.showPassword());
  }

  onLogin(form: any) {
    if (form.invalid) {
      form.control.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    this.auth.login(this.credentials).subscribe({
      next: (user) => {
        this.loading.set(false);
        this.navigateAfterLogin(user);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message || 'Invalid username or password.');
      }
    });
  }

  forgotPassword() {
    this.notify.info('Password reset is currently handled by the Admin Desk. Please contact your batch representative or email help@ghcaa.com.');
  }
}


