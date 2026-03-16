import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, Router, NavigationEnd } from '@angular/router';
import { AppFooter } from '../../common/footer/footer';
import { ThemeService } from '../../core/services/theme.service';
import { CommonModule } from '@angular/common';
import { Title } from '@angular/platform-browser';
import { filter } from 'rxjs';

@Component({
  selector: 'app-public-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, AppFooter, CommonModule],
  templateUrl: './public-layout.html',
  styleUrl: './public-layout.scss'
})
export class PublicLayout {
  themeService = inject(ThemeService);
  private titleService = inject(Title);
  private router = inject(Router);
  
  isMobileMenuOpen = false;

  toggleMobileMenu() {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu() {
    this.isMobileMenuOpen = false;
  }

  constructor() {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.closeMobileMenu();
      const url = this.router.url;
      let title = 'Home';
      if (url.includes('login')) title = 'Members Login';
      else if (url.includes('register')) title = 'Join GHCAA';
      else if (url.includes('about')) title = 'About Us';
      else if (url.includes('contact')) title = 'Contact Us';
      else if (url.includes('gallery')) title = 'Event Gallery';
      else if (url.includes('events')) title = 'Association Events';

      this.titleService.setTitle(`${title} | Govt. Haraganga College Alumni Association`);
    });
  }
}

