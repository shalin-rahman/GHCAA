import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { LoginDto } from '../../core/models/auth.models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="login-page">
      <div class="glass-card login-card">
        <div class="login-header">
          <img src="/assets/logo.jpg" alt="Logo" class="logo">
          <h2>Member Portal</h2>
          <p>Login to your account</p>
        </div>

        <form (ngSubmit)="onLogin()" #loginForm="ngForm">
          <div class="form-group">
            <label>Username</label>
            <input 
              type="text" 
              name="username" 
              [(ngModel)]="credentials.username" 
              required 
              placeholder="Enter your username"
              class="form-control"
            >
          </div>

          <div class="form-group">
            <label>Password</label>
            <input 
              type="password" 
              name="password" 
              [(ngModel)]="credentials.password" 
              required 
              placeholder="••••••••"
              class="form-control"
            >
          </div>

          <div class="error-msg" *ngIf="errorMessage">
            {{ errorMessage }}
          </div>

          <button 
            type="submit" 
            class="btn btn-primary w-full" 
            [disabled]="loginForm.invalid || loading"
          >
            {{ loading ? 'Authenticating...' : 'Login' }}
          </button>

          <div class="form-footer">
            <a href="#">Forgot Password?</a>
            <span>Don't have an account? <a routerLink="/register">Apply for Membership</a></span>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .login-page {
      height: 90vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--bg-color);
    }
    .login-card {
      width: 100%;
      max-width: 450px;
      padding: 3rem;
      border: 1px solid var(--glass-border);
    }
    .login-header {
      text-align: center;
      margin-bottom: 2.5rem;
    }
    .logo {
      height: 80px;
      border-radius: 12px;
      margin-bottom: 1rem;
    }
    .login-header p {
      color: var(--text-muted);
    }
    .form-group {
      margin-bottom: 1.5rem;
    }
    .form-group label {
      display: block;
      margin-bottom: 0.5rem;
      font-weight: 600;
      color: var(--text-muted);
    }
    .form-control {
      width: 100%;
      padding: 0.8rem 1rem;
      border: 1px solid var(--border-color);
      background: var(--bg-color);
      color: var(--text-main);
      border-radius: 8px;
      font-size: 1rem;
      transition: 0.3s;
    }
    .form-control:focus {
      outline: none;
      border-color: var(--primary-color);
    }
    .w-full { width: 100%; }
    .error-msg {
      color: var(--danger-color);
      font-size: 0.85rem;
      margin-bottom: 1rem;
      text-align: center;
    }
    .form-footer {
      margin-top: 2rem;
      text-align: center;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      font-size: 0.9rem;
    }
    .form-footer a {
      color: var(--primary-color);
      font-weight: 600;
      text-decoration: none;
    }
  `]
})
export class Login {
  private auth = inject(AuthService);
  private router = inject(Router);

  credentials: LoginDto = { username: '', password: '' };
  loading = false;
  errorMessage = '';

  onLogin() {
    this.loading = true;
    this.errorMessage = '';

    this.auth.login(this.credentials).subscribe({
      next: (user) => {
        this.loading = false;
        if (user.role === 'Admin') {
          this.router.navigate(['/admin/approvals']);
        } else {
          this.router.navigate(['/portal/dashboard']);
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = 'Invalid username or password.';
      }
    });
  }
}
