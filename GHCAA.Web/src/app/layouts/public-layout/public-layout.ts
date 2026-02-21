import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { AppFooter } from '../../shared/footer/footer';

@Component({
  selector: 'app-public-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, AppFooter],
  template: `
    <nav class="glass-nav">
      <div class="container nav-content">
        <div class="logo">
          <img src="/assets/logo.jpg" alt="GHCAA Logo" class="logo-img">
          <div class="logo-text">
            <span class="abbr">GHCAA</span>
            <span class="full hide-mobile">Govt. Haraganga College Alumni Association</span>
          </div>
        </div>
        <div class="nav-links hide-mobile">
          <a routerLink="/" class="nav-link">Home</a>
          <a routerLink="/about" class="nav-link">About Us</a>
          <a routerLink="/register" class="nav-link">Join</a>
          <a routerLink="/contact" class="nav-link">Contact</a>
          <a routerLink="/login" class="btn btn-primary">Member Login</a>
        </div>
      </div>
    </nav>
    
    <main>
      <router-outlet></router-outlet>
    </main>
    
    <app-footer></app-footer>
  `,
  styles: [`
    .glass-nav {
      position: sticky;
      top: 0;
      z-index: 1000;
      background: var(--primary-color);
      backdrop-filter: blur(10px);
      -webkit-backdrop-filter: blur(10px);
      border-bottom: 1px solid var(--glass-border);
      color: white;
      padding: 0.5rem 0;
    }
    .nav-content { display: flex; justify-content: space-between; align-items: center; }
    .logo { display: flex; align-items: center; gap: 0.75rem; }
    .logo-img { height: 44px; border-radius: 50%; border: 1px solid rgba(255,255,255,0.2); }
    .logo-text { display: flex; flex-direction: column; line-height: 1.1; }
    .abbr { font-weight: 800; font-size: 1.1rem; letter-spacing: 1px; }
    .full { font-size: 0.65rem; opacity: 0.8; text-transform: uppercase; font-weight: 600; }
    
    .nav-links { display: flex; gap: 2rem; align-items: center; }
    .nav-link { color: white; text-decoration: none; font-weight: 700; font-size: 0.9rem; border-bottom: 2px solid transparent; transition: 0.3s; }
    .nav-link:hover { border-bottom-color: var(--accent-color); }

    main { min-height: calc(100vh - 400px); }

    @media (max-width: 600px) {
        .logo-text .full { display: none; }
    }
  `]
})
export class PublicLayout { }
