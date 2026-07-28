import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Shared breadcrumb trail (e.g. "GHCAA ADMIN / DASHBOARD").
 *
 * Styles are kept INSIDE this component on purpose: because the markup lives in
 * the component's own template, its component-scoped CSS always matches — so
 * unlike page-header/search-bar (where markup moved out of the host page and
 * left the host's scoped rules orphaned), this control can never suffer the
 * "styles stop applying" encapsulation leak. Fix it here, fixed everywhere.
 *
 * Usage:
 *   <app-breadcrumb [root]="'GHCAA Admin'" [current]="currentPageTitle()"></app-breadcrumb>
 */
@Component({
  selector: 'app-breadcrumb',
  standalone: true,
  imports: [CommonModule],
  template: `
    <nav class="breadcrumb" aria-label="Breadcrumb">
      @if (root) {
      <span class="crumb root">{{ root }}</span>
      <span class="sep" aria-hidden="true">/</span>
      }
      <span class="crumb current" aria-current="page">{{ current }}</span>
    </nav>
  `,
  styles: [`
    .breadcrumb {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.72rem;
      font-weight: 800;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      min-width: 0;
    }
    .crumb {
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
    .root { color: var(--text-muted); }
    .sep {
      color: var(--glass-border);
      font-weight: 400;
    }
    .current { color: var(--text-main); }
  `],
})
export class BreadcrumbComponent {
  @Input() root = '';
  @Input() current = '';
}
