import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="admin-wrapper">
      <nav class="admin-sidebar glass-effect">
        <div class="admin-brand">
          <img src="/assets/logo.jpg" alt="Logo" class="logo">
          <span>Admin Portal</span>
        </div>
        <div class="admin-nav nav-list">
          <a routerLink="/admin/approvals" routerLinkActive="active" class="admin-nav-item nav-item">
            <span class="icon">📝</span> Approvals
          </a>
          <a routerLink="/admin/ledger" routerLinkActive="active" class="admin-nav-item nav-item">
            <span class="icon">📖</span> Ledger
          </a>
          <div class="nav-spacer"></div>
          <a routerLink="/portal/dashboard" class="admin-nav-item nav-item exit-link">
            <span class="icon">🔙</span> Exit Admin
          </a>
        </div>
      </nav>
      
      <main class="admin-main">
        <header class="admin-header glass-card">
          <h1>Control Panel</h1>
          <div class="admin-user">
             Admin: <span>{{ auth.currentUser()?.username }}</span>
          </div>
        </header>
        <div class="admin-content">
          <router-outlet></router-outlet>
        </div>
      </main>
    </div>
  `,
  styles: [`
    .admin-wrapper {
      display: flex;
      min-height: 100vh;
      background: var(--bg-color);
    }
    .admin-sidebar {
      width: 240px;
      background: var(--surface-color);
      color: var(--text-main);
      display: flex;
      flex-direction: column;
      border-right: 1px solid var(--glass-border);
    }
    .admin-brand {
      height: 64px;
      display: flex;
      align-items: center;
      padding: 0 1.5rem;
      gap: 0.75rem;
      font-weight: 800;
      color: var(--primary-color);
      border-bottom: 1px solid var(--glass-border);
    }
    .logo { height: 32px; border-radius: 6px; }
    
    .admin-main {
      flex: 1;
      overflow-y: auto;
      background: var(--bg-color);
    }
    .admin-header {
      height: 64px;
      padding: 0 1.5rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
      background: var(--glass-bg);
      backdrop-filter: blur(10px);
      border-bottom: 1px solid var(--glass-border);
      position: sticky;
      top: 0;
      z-index: 90;
    }
    .admin-header h1 { font-size: 1.1rem; margin: 0; }
    .admin-user { font-size: 0.85rem; font-weight: 500; }
    .admin-content { padding: 1.5rem; }
  `]
})
export class AdminLayout {
  auth = inject(AuthService);
}
