import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ProfileService } from '../../core/services/profile.service';
import { NotificationService } from '../../core/services/notification.service';
import { ROUTES } from '../../core/constants/app.constants';
import { Icon } from '../../common/icon/icon';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, FormsModule, Icon],
  templateUrl: './change-password.html',
  styleUrl: './change-password.scss'
})
export class ChangePassword {
  private profileService = inject(ProfileService);
  private auth = inject(AuthService);
  private router = inject(Router);
  private notify = inject(NotificationService);

  model = { oldPassword: '', newPassword: '', confirmPassword: '' };
  loading = signal(false);
  errorMessage = signal('');
  showOld = signal(false);
  showNew = signal(false);

  // True when the portal is forcing this change (default password after admin approval).
  forced = computed(() => this.auth.currentUser()?.mustChangePassword ?? false);

  // Mirrors the API's ChangePasswordDto policy: >=8 chars, upper, lower, digit.
  private readonly policy = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/;

  passwordsMatch = computed(() => this.model.newPassword === this.model.confirmPassword);

  onSubmit(form: any) {
    if (form.invalid || !this.passwordsMatch()) {
      form.control.markAllAsTouched();
      return;
    }

    if (!this.policy.test(this.model.newPassword)) {
      this.errorMessage.set('New password must be at least 8 characters and include an uppercase letter, a lowercase letter, and a digit.');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    this.profileService.changePassword(this.model.oldPassword, this.model.newPassword).subscribe({
      next: () => {
        this.loading.set(false);
        // 29A.1: drop the forced flag so authGuard lets the user into the portal.
        this.auth.clearMustChangePassword();
        this.notify.success('Password changed successfully.');
        this.router.navigate([ROUTES.PORTAL_DASHBOARD]);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message || err.error?.Message || 'Password change failed. Verify your current password.');
      }
    });
  }

  toggleOld() { this.showOld.set(!this.showOld()); }
  toggleNew() { this.showNew.set(!this.showNew()); }
}
