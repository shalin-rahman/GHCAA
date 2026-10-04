import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Title } from '@angular/platform-browser';
import { NavService } from '../../core/services/nav.service';
import { filter } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Icon } from '../../common/icon/icon';
import { UserMenu } from '../../common/user-menu/user-menu';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';
import { OrgConfigService } from '../../core/services/org-config.service';

@Component({
  selector: 'app-portal-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule, Icon, UserMenu, ImgFallbackDirective],
  templateUrl: './portal-layout.html',
  styleUrl: './portal-layout.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PortalLayout {
  nav = inject(NavService);
  orgConfig = inject(OrgConfigService);
  private router = inject(Router);
  private titleService = inject(Title);

  isSidebarCollapsed = signal(false);
  // Under 768px the sidebar is an off-canvas drawer, same as admin-layout. It has its own
  // flag so the desktop icon-rail state never hides labels inside the drawer.
  isMobileMenuOpen = signal(false);
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
      this.titleService.setTitle(`${title} | ${this.orgConfig.config()?.branding?.shortName ?? 'GHCAA'} Member Portal`);

      this.isMobileMenuOpen.set(false);
    });
  }

  toggleSidebar() {
    if (typeof window !== 'undefined' && window.innerWidth < 768) {
      this.isMobileMenuOpen.update(v => !v);
    } else {
      this.isSidebarCollapsed.update(v => !v);
    }
  }
}
