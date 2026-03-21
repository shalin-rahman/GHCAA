import { Component, inject, OnDestroy, ChangeDetectorRef, HostBinding } from '@angular/core';
import { RouterOutlet, RouterLink, Router, NavigationEnd } from '@angular/router';
import { AppFooter } from '../../common/footer/footer';
import { ThemeService } from '../../core/services/theme.service';
import { CommonModule } from '@angular/common';
import { Title } from '@angular/platform-browser';
import { filter } from 'rxjs';
import { effect } from '@angular/core';

@Component({
  selector: 'app-public-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, AppFooter, CommonModule],
  templateUrl: './public-layout.html',
  styleUrl: './public-layout.scss'
})
export class PublicLayout implements OnDestroy {
  themeService = inject(ThemeService);
  private titleService = inject(Title);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  @HostBinding('style.background')
  get hostBackground(): string {
    const theme = this.themeService.activeSpecialTheme();
    // Only override with sidebarColor if it is explicitly configured;
    // otherwise return '' so the SCSS var(--page-bg, #000) / theme default applies.
    if (!theme || !theme.sidebarColor) return '';
    return theme.sidebarColor;
  }

  @HostBinding('style.transition')
  get hostTransition(): string { return 'background 0.6s ease'; }

  isMobileMenuOpen = false;

  // Typewriter display state
  displayedText = '';
  private _typewriterInterval: ReturnType<typeof setInterval> | null = null;
  private _typewriterTimeout: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    // React to active theme changes
    effect(() => {
      const theme = this.themeService.activeSpecialTheme();
      this._clearTimers();

      if (!theme || !theme.announcementText || theme.animationStyle === 'None') {
        this.displayedText = theme?.announcementText ?? '';
        return;
      }

      const fullText = theme.announcementText;

      if (theme.animationStyle === 'Typewriter') {
        // Typewriter: type it out char-by-char, then restart
        this._runTypewriter(fullText);
      } else {
        // Fade / 3D / Scroll — just show the full text and let CSS do the work
        this.displayedText = fullText;
      }
    });

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
      else if (url.includes('news')) title = 'Latest News';

      this.titleService.setTitle(`${title} | Govt. Haraganga College Alumni Association`);
    });
  }

  private _runTypewriter(fullText: string) {
    let i = 0;
    this.displayedText = '';

    const type = () => {
      if (i <= fullText.length) {
        this.displayedText = fullText.substring(0, i);
        this.cdr.markForCheck();
        i++;
        this._typewriterTimeout = setTimeout(type, 60); // 60ms per character
      } else {
        // Pause then restart
        this._typewriterTimeout = setTimeout(() => {
          i = 0;
          this._runTypewriter(fullText);
        }, 3500);
      }
    };
    type();
  }

  private _clearTimers() {
    if (this._typewriterInterval !== null) {
      clearInterval(this._typewriterInterval);
      this._typewriterInterval = null;
    }
    if (this._typewriterTimeout !== null) {
      clearTimeout(this._typewriterTimeout);
      this._typewriterTimeout = null;
    }
  }

  toggleMobileMenu() {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu() {
    this.isMobileMenuOpen = false;
  }

  ngOnDestroy() {
    this._clearTimers();
  }
}
