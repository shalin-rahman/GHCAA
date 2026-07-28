import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

/**
 * Shared search input for admin/portal filter bars.
 *
 * Renders ONLY the `.search-wrap` (icon + input + clear button) — NOT the
 * outer `.filter-bar`, so pages keep their own filter row and can place extra
 * `<select>` filters alongside it. Styling lives centrally in styles.scss
 * (`.filter-bar .search-wrap …`); fix the look in one place, every search
 * updates.
 *
 * Usage:
 *   <app-search-bar
 *       [value]="searchQuery()"
 *       (valueChange)="searchQuery.set($event)"
 *       placeholder="Search headlines…"></app-search-bar>
 */
@Component({
  selector: 'app-search-bar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="search-wrap">
      <span class="search-icon">🔍</span>
      <input
        type="text"
        class="search-input"
        [ngModel]="value"
        (ngModelChange)="onInput($event)"
        [placeholder]="placeholder" />
      @if (value) {
      <button class="clear-search" (click)="clear()" title="Clear search">×</button>
      }
    </div>
  `,
  styles: [
    `
      :host {
        display: block;
        flex: 1 1 auto;
        min-width: 0;
      }
    `,
  ],
})
export class SearchBarComponent {
  @Input() value = '';
  @Input() placeholder = 'Search…';
  @Output() valueChange = new EventEmitter<string>();

  onInput(v: string): void {
    this.value = v;
    this.valueChange.emit(v);
  }

  clear(): void {
    this.value = '';
    this.valueChange.emit('');
  }
}
