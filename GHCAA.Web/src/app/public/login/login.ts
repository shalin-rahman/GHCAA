import { Component, inject, ChangeDetectorRef, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { LoginDto } from '../../core/models/auth.models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  private auth = inject(AuthService);
  private router = inject(Router);
  private notify = inject(NotificationService);
  private cdr = inject(ChangeDetectorRef);

  credentials: LoginDto = { username: '', password: '' };
  loading = signal(false);
  errorMessage = signal('');

  onLogin() {
    this.loading.set(true);
    this.errorMessage.set('');

    this.auth.login(this.credentials).subscribe({
      next: (user) => {
        this.loading.set(false);
        if (user.role === 'Admin' || user.role === 'SuperAdmin') {
          this.router.navigate(['/admin/approvals']);
        } else {
          this.router.navigate(['/portal/dashboard']);
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set('Invalid username or password.');
      }
    });
  }

  forgotPassword() {
    this.notify.info('Password reset is currently handled by the Admin Desk. Please contact your batch representative or email help@ghcaa.com.');
  }
}


