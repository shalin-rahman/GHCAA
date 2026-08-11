import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Shared admin/portal page header.
 *
 * Centralises the "redundant-zero" gating rule that was previously duplicated
 * across ~15 pages: the count subtitle and any count-dependent stat badge are
 * shown ONLY when there are records; when empty, a neutral subtitle shows
 * instead and the empty-state row becomes the sole "no records" signal.
 *
 * Content projection keeps the varied subtitle text per page while the gating
 * logic lives in exactly one place.
 *
 * Slots:
 *   [subtitle] — count line (e.g. "Showing X of Y …"); shown when total > 0
 *   [stat]     — count-dependent badge (e.g. "Pending: N"); shown when total > 0
 *   [actions]  — always-visible controls (e.g. "Add New" button)
 *
 * Usage:
 *   <app-page-header title="News Posts"
 *                    [total]="newsList().length"
 *                    emptySubtitle="Publish and manage news announcements">
 *     <small subtitle>Showing {{ filteredNews().length }} of {{ newsList().length }} news entries</small>
 *     <button actions class="btn btn-accent" (click)="openForm()">Draft New Story</button>
 *   </app-page-header>
 */
@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page-header">
      <div class="header-text">
        <h2>{{ title }}</h2>
        @if (total > 0) {
        <ng-content select="[subtitle]"></ng-content>
        } @else {
        <small>{{ emptySubtitle }}</small>
        }
      </div>
      <div class="header-actions">
        @if (total > 0) {
        <ng-content select="[stat]"></ng-content>
        }
        <ng-content select="[actions]"></ng-content>
      </div>
    </div>
  `,
})
export class PageHeaderComponent {
  @Input() title = '';
  @Input() total = 0;
  @Input() emptySubtitle = '';
}
