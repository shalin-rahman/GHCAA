import { Component, Input, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ThemeService } from '../../core/services/theme.service';
import { ProfileService } from '../../core/services/profile.service';
import { Icon } from '../icon/icon';
import { ImgFallbackDirective } from '../directives/img-fallback.directive';

/**
 * Shared identity + quick-actions cluster for the top-right of both the portal and
 * admin headers (TODO 30.8 / 30.15). Previously the portal header owned the theme
 * toggle, avatar/photo, and sign-out button while the admin header had none of these
 * (only a plain username + role-badge text pair) — this component centralizes that
 * markup so both layouts render identically and stay in sync going forward.
 *
 * Reuses the exact `.theme-toggle` / `.user-profile` / `.avatar` / `.avatar-img` /
 * `.user-details` / `.username` / `.role` / `.logout-toggle` classes that are already
 * styled globally in styles.scss (via portal-layout's `.right-section` scope, which
 * applies wherever this component is placed) — no new CSS required.
 */
@Component({
  selector: 'app-user-menu',
  standalone: true,
  imports: [Icon, ImgFallbackDirective, RouterLink],
  templateUrl: './user-menu.html',
  styleUrl: './user-menu.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserMenu {
  auth = inject(AuthService);
  theme = inject(ThemeService);
  private profileService = inject(ProfileService);

  /** Caller-computed role/label text shown under the username (e.g. "Administrator", "Alumni Member", or the raw role). */
  @Input({ required: true }) roleLabel!: string;

  /** 30.36: when true, shows an "Admin Panel" quick-link in this menu — set by the caller
   * from the same `nav.isAdmin()` check that already gates the portal sidebar's Admin
   * Panel link. Defaults to false so admin-layout's own usage (already inside the admin
   * panel) is unaffected. */
  @Input() isAdmin = false;

  profilePhotoUrl = signal<string | null>(null);

  constructor() {
    this.profileService.getProfile().subscribe({
      next: (p) => this.profilePhotoUrl.set(this.getImageUrl(p.photoPath)),
      error: () => {}
    });
  }

  private getImageUrl(path: string | null | undefined): string | null {
    if (!path) return null;
    if (path.startsWith('http')) return path;
    const cleanPath = path.startsWith('/') ? path : '/' + path;
    return cleanPath.replace(/^\/\//, '/');
  }
}
