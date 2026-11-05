import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive, Router, NavigationEnd } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { AuthService } from '../../core/services/auth.service';
import { NavService } from '../../core/services/nav.service';
import { OrgConfigService } from '../../core/services/org-config.service';
import { filter } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BreadcrumbComponent } from '../../common/breadcrumb/breadcrumb.component';
import { Icon } from '../../common/icon/icon';
import { UserMenu } from '../../common/user-menu/user-menu';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, BreadcrumbComponent, Icon, UserMenu, ImgFallbackDirective, SearchBarComponent],
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
  menuSearch = signal('');

  filteredNavSections = computed(() => {
    const query = this.menuSearch().trim().toLowerCase();
    const sections = this.nav.adminNavSections();
    if (!query) return sections;

    return sections
      .map(section => {
        const matchesSection = section.name.toLowerCase().includes(query);
        // Labels only. Every path starts with /admin/, so matching paths made
        // short queries like "a" or "ad" match the whole menu.
        const items = matchesSection
          ? section.items
          : section.items.filter(item => item.label.toLowerCase().includes(query));
        return { name: section.name, items };
      })
      .filter(section => section.items.length > 0);
  });

  constructor() {
    // 29D.7: tie the router subscription to component lifecycle to avoid a leak.
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      takeUntilDestroyed()
    ).subscribe(() => {
      const url = this.router.url;
      const title = this.nav.labelFor(url, 'admin');
      this.currentPageTitle.set(title);
      this.titleService.setTitle(`${title} | ${this.orgConfig.config()?.branding?.shortName ?? 'Admin'} Admin`);
      // Auto-close the mobile drawer after navigating.
      this.isSidebarOpen.set(false);
    });
  }

  clearMenuSearch() {
    this.menuSearch.set('');
  }

  toggleSidebar() {
    this.isSidebarOpen.update(v => !v);
  }
}
