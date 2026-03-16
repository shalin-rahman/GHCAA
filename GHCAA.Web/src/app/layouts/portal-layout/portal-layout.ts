import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Title } from '@angular/platform-browser';
import { AuthService } from '../../core/services/auth.service';
import { ThemeService } from '../../core/services/theme.service';
import { NavService } from '../../core/services/nav.service';
import { filter } from 'rxjs';

@Component({
  selector: 'app-portal-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './portal-layout.html',
  styleUrl: './portal-layout.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PortalLayout {
  auth = inject(AuthService);
  theme = inject(ThemeService);
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

    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      const url = this.router.url;
      const match = this.nav.portalNavItems().find(x => url.includes(x.path.replace('/portal/', '')));
      const title = match?.label ?? 'Dashboard';
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
