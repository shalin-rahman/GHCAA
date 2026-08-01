import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Title } from '@angular/platform-browser';
import { AuthService } from '../../core/services/auth.service';
import { NavService } from '../../core/services/nav.service';
import { filter } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Icon } from '../../common/icon/icon';
import { UserMenu } from '../../common/user-menu/user-menu';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';

@Component({
  selector: 'app-portal-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule, Icon, UserMenu, ImgFallbackDirective],
  templateUrl: './portal-layout.html',
  styleUrl: './portal-layout.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PortalLayout {
  auth = inject(AuthService);
  nav = inject(NavService);
  private router = inject(Router);
  private titleService = inject(Title);

  isSidebarCollapsed = signal(false);
  currentPageTitle = signal('Dashboard');

  constructor() {
    // Collapse sidebar by default on mobile
    if (typeof window !== 'undefined' && window.innerWidth <= 768) {
      this.isSidebarCollapsed.set(true);
    }

    // 29D.7: tie the router subscription to component lifecycle to avoid a leak.
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      takeUntilDestroyed()
    ).subscribe(() => {
      const url = this.router.url;
      const title = this.nav.labelFor(url, 'portal');
      this.currentPageTitle.set(title);
      this.titleService.setTitle(`${title} | Member Portal`);

      // Auto-collapse on mobile after navigation
      if (typeof window !== 'undefined' && window.innerWidth <= 768) {
        this.isSidebarCollapsed.set(true);
      }
    });
  }

  toggleSidebar() {
    this.isSidebarCollapsed.update(v => !v);
  }
}
