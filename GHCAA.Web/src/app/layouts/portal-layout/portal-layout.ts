import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';
import { ThemeService } from '../../core/services/theme.service';
import { filter, map } from 'rxjs';

@Component({
  selector: 'app-portal-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  template: `
    <div class="app-shell" [class.collapsed]="isSidebarCollapsed()">
      <!-- Unified Desktop Sidebar -->
      <aside class="sidebar glass-effect hide-mobile" [class.collapsed]="isSidebarCollapsed()">
        <div class="sidebar-brand">
          <img src="/assets/logo.jpg" alt="Logo" class="brand-logo">
          <span class="brand-name" *ngIf="!isSidebarCollapsed()">HARAGANGIAN</span>
        </div>
        
        <nav class="nav-list">
          @for (item of getNavItems(); track item.path) {
            <a [routerLink]="item.path" routerLinkActive="active" class="nav-item">
              <span class="icon">{{ item.icon }}</span>
              <span class="label" *ngIf="!isSidebarCollapsed()">{{ item.label }}</span>
            </a>
          }
          
          <div class="nav-spacer"></div>
          
          @if (isAdmin()) {
            <a routerLink="/admin/approvals" routerLinkActive="active" class="nav-item admin-link">
              <span class="icon">🛠️</span>
              <span class="label" *ngIf="!isSidebarCollapsed()">Admin Panel</span>
            </a>
          }
        </nav>

        <div class="sidebar-footer">
          <button (click)="auth.logout()" class="btn btn-secondary" style="width: 100%;">
            <span class="icon">🚪</span>
            <span *ngIf="!isSidebarCollapsed()">Logout</span>
          </button>
        </div>
      </aside>

      <!-- Main Application Shell -->
      <main class="main-content">
        <header class="top-bar">
          <div class="left-section">
            <button class="touch-target btn-icon hide-mobile toggle-btn" (click)="toggleSidebar()">
                <img src="/assets/logo.jpg" alt="Logo" class="mini-logo">
            </button>
            <div class="breadcrumbs">
                <span class="text-muted">Portal</span>
                <span class="separator">/</span>
                <span class="current-page">{{ currentPageTitle() }}</span>
            </div>
          </div>

          <div class="right-section">
            <button class="touch-target btn-icon theme-toggle" (click)="theme.toggleTheme()" [title]="'Toggle Dark/Light Mode'">
                {{ theme.theme() === 'light' ? '🌙' : '☀️' }}
            </button>
            <div class="user-profile">
                <div class="avatar">{{ auth.currentUser()?.username?.[0] }}</div>
                <div class="user-details hide-mobile">
                    <span class="username">{{ auth.currentUser()?.username }}</span>
                    <small class="role">{{ isAdmin() ? 'Administrator' : 'Alumni Member' }}</small>
                </div>
            </div>
          </div>
        </header>

        <section class="page-content container">
          <router-outlet></router-outlet>
        </section>
      </main>

      <!-- Mobile Bottom Navigation Shell -->
      <nav class="mobile-nav show-mobile">
        @for (item of getMobileItems(); track item.path) {
          <a [routerLink]="item.path" routerLinkActive="active" class="touch-target b-nav-item">
            <span class="icon">{{ item.icon }}</span>
          </a>
        }
        <button (click)="auth.logout()" class="touch-target b-nav-item">
            <span>🚪</span>
        </button>
      </nav>
    </div>
  `,
  styles: [`
    .sidebar-brand {
      height: 64px;
      display: flex;
      align-items: center;
      padding: 0 1.5rem;
      gap: 0.75rem;
      border-bottom: 1px solid var(--glass-border);
    }
    .brand-logo { width: 32px; height: 32px; border-radius: 6px; }
    .brand-name { font-weight: 800; font-size: 1.1rem; color: var(--primary-color); white-space: nowrap; }
    
    .nav-list { display: flex; flex-direction: column; gap: 0.25rem; padding: 0.75rem 0.5rem; }
    .nav-item {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.6rem 1rem;
      text-decoration: none;
      color: var(--text-muted);
      border-radius: 8px;
      font-weight: 500;
      font-size: 0.85rem;
      transition: 0.2s;
    }
    .nav-item:hover, .nav-item.active {
      background: rgba(212, 175, 55, 0.08);
      color: var(--primary-color);
    }
    .nav-item .icon { font-size: 1.1rem; width: 20px; text-align: center; }
    
    .sidebar-footer { margin-top: auto; padding: 1rem; border-top: 1px solid var(--glass-border); }
    .user-pill { display: flex; align-items: center; gap: 0.75rem; padding: 0.5rem; border-radius: 8px; background: var(--bg-color); }

    .nav-spacer { flex: 1; }
    

    .top-bar {
      justify-content: space-between;
    }
    
    .left-section, .right-section { display: flex; align-items: center; gap: 1.5rem; }
    
    .mini-logo {
        height: 28px;
        width: 28px;
        border-radius: 50%;
        transition: 0.3s;
    }
    .toggle-btn:hover .mini-logo {
        transform: rotate(15deg) scale(1.1);
    }

    .breadcrumbs {
        display: flex;
        gap: 0.5rem;
        align-items: center;
        font-weight: 700;
        font-size: 0.9rem;
        .separator { opacity: 0.3; }
        .current-page { color: var(--primary-color); }
    }

    .btn-icon {
        background: none;
        border: none;
        font-size: 1.25rem;
        transition: 0.2s;
        &:hover { transform: scale(1.1); }
    }

    .user-profile {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        padding-left: 1rem;
        border-left: 1px solid var(--glass-border);
    }
    
    .avatar {
        width: 36px;
        height: 36px;
        background: var(--primary-color);
        color: white;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        font-weight: 800;
    }
    
    .user-details {
        display: flex;
        flex-direction: column;
        line-height: 1.2;
        .username { font-weight: 700; font-size: 0.85rem; }
        .role { font-size: 0.7rem; opacity: 0.7; }
    }

    .b-nav-item.active {
        color: var(--primary-color);
        border-top: 3px solid var(--primary-color);
    }
    
    .page-content {
        padding-top: 2rem;
        padding-bottom: 5rem;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PortalLayout {
  auth = inject(AuthService);
  theme = inject(ThemeService);
  private router = inject(Router);

  isSidebarCollapsed = signal(false);
  currentPageTitle = signal('Dashboard');

  constructor() {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      map(() => this.router.url)
    ).subscribe(url => {
      const title = this.getNavItems().find(x => url.includes(x.path))?.label || 'Dashboard';
      this.currentPageTitle.set(title);
    });
  }

  toggleSidebar() {
    this.isSidebarCollapsed.update(v => !v);
  }

  isAdmin() {
    return this.auth.currentUser()?.role === 'Admin';
  }

  getNavItems() {
    return [
      { path: 'dashboard', label: 'Dashboard', icon: '📊' },
      { path: 'messages', label: 'Messaging', icon: '💬' },
      { path: 'jobs', label: 'Job Hub', icon: '💼' },
      { path: 'directory', label: 'Alumni Directory', icon: '🔍' },
      { path: 'governance', label: 'Governance', icon: '⚖️' },
      { path: 'id-card', label: 'Digital ID', icon: '🆔' },
      { path: 'profile', label: 'My Profile', icon: '👤' }
    ];
  }

  getMobileItems() {
    return [
      { path: 'dashboard', icon: '📊' },
      { path: 'jobs', icon: '💼' },
      { path: 'messages', icon: '💬' },
      { path: 'profile', icon: '👤' }
    ];
  }
}
