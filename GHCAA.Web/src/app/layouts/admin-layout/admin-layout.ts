import { Component, inject, signal } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router, NavigationEnd } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { AuthService } from '../../core/services/auth.service';
import { NavService } from '../../core/services/nav.service';
import { filter } from 'rxjs';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.scss'
})
export class AdminLayout {
  auth = inject(AuthService);
  nav = inject(NavService);
  private titleService = inject(Title);
  private router = inject(Router);

  currentPageTitle = signal('Control Panel');
  
  constructor() {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      const url = this.router.url;
      const allAdminItems = this.nav.adminNavItems();
      const match = allAdminItems.find(x => url.includes(x.path));
      const title = match?.label ?? 'Control Panel';
      this.currentPageTitle.set(title);
      this.titleService.setTitle(`${title} | Admin Portal`);
    });
  }
}
