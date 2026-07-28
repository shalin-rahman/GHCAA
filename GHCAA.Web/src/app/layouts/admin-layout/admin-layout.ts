import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive, Router, NavigationEnd } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { AuthService } from '../../core/services/auth.service';
import { NavService } from '../../core/services/nav.service';
import { OrgConfigService } from '../../core/services/org-config.service';
import { filter } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BreadcrumbComponent } from '../../common/breadcrumb/breadcrumb.component';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, BreadcrumbComponent],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.scss'
})
export class AdminLayout {
  auth = inject(AuthService);
  nav = inject(NavService);
  orgConfig = inject(OrgConfigService);
  private titleService = inject(Title);
  private router = inject(Router);

  currentPageTitle = signal('Control Panel');
  // 29F.4: mobile sidebar toggle (admin layout previously had no way to open/close the
  // sidebar on small screens, leaving the nav either permanently covering content or
  // inaccessible).
  isSidebarOpen = signal(false);

  constructor() {
    // 29D.7: tie the router subscription to component lifecycle to avoid a leak.
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      takeUntilDestroyed()
    ).subscribe(() => {
      const url = this.router.url;
      const allAdminItems = this.nav.adminNavItems();
      const match = allAdminItems.find(x => url.includes(x.path));
      const title = match?.label ?? 'Control Panel';
      this.currentPageTitle.set(title);
      this.titleService.setTitle(`${title} | ${this.orgConfig.config()?.branding?.shortName ?? 'Admin'} Admin`);
      // Auto-close the mobile drawer after navigating.
      this.isSidebarOpen.set(false);
    });
  }

  toggleSidebar() {
    this.isSidebarOpen.update(v => !v);
  }

  // 29F.4: real logout was missing — the header only had an "Exit Admin" link that hopped
  // to the member portal without ending the session.
  logout() {
    this.auth.logout();
  }
}
