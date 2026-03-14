import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="pagination-bar">
      <p class="pagination-info">
        Showing <strong>{{ (currentPage - 1) * pageSize + 1 }}</strong>
        – <strong>{{ Math.min(currentPage * pageSize, totalItems) }}</strong>
        of <strong>{{ totalItems }}</strong> results
      </p>

      <nav class="pagination-nav">
        <button (click)="onPrev()" [disabled]="currentPage === 1" class="page-btn nav-btn">←</button>

        <ng-container *ngFor="let page of visiblePages">
          <button *ngIf="page !== -1" (click)="onPage(page)"
            class="page-btn" [class.active]="page === currentPage">
            {{ page }}
          </button>
          <span *ngIf="page === -1" class="page-ellipsis">…</span>
        </ng-container>

        <button (click)="onNext()" [disabled]="currentPage === totalPages" class="page-btn nav-btn">→</button>
      </nav>
    </div>
  `,
  styles: [`
    .pagination-bar {
      display: flex;
      align-items: center;
      justify-content: space-between;
      flex-wrap: wrap;
      gap: 1rem;
      margin-top: 1.5rem;
      padding: 0.75rem 1.25rem;
      background: var(--glass-bg);
      border: 1px solid var(--glass-border);
      border-radius: 1rem;
    }

    .pagination-info {
      font-size: 0.8rem;
      color: var(--text-muted);
      margin: 0;
    }

    .pagination-info strong {
      color: var(--text-main);
      font-weight: 700;
    }

    .pagination-nav {
      display: flex;
      align-items: center;
      gap: 0.3rem;
    }

    .page-btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-width: 2.25rem;
      height: 2.25rem;
      padding: 0 0.5rem;
      border: 1px solid var(--border-color);
      background: var(--bg-color);
      color: var(--text-main);
      font-size: 0.85rem;
      font-weight: 600;
      border-radius: 0.5rem;
      cursor: pointer;
      transition: all 0.15s ease;
    }

    .page-btn:hover:not(:disabled) {
      background: var(--accent-color);
      color: #000;
      border-color: var(--accent-color);
    }

    .page-btn:disabled {
      opacity: 0.35;
      cursor: not-allowed;
    }

    .page-btn.active {
      background: linear-gradient(135deg, var(--accent-color), var(--accent-gold-dark));
      color: #000;
      border-color: var(--accent-color);
      font-weight: 800;
      box-shadow: var(--shadow-gold);
      transform: scale(1.08);
    }

    .nav-btn {
      font-size: 1.1rem;
      font-weight: 700;
    }

    .page-ellipsis {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-width: 2rem;
      color: var(--text-muted);
      font-size: 0.85rem;
    }
  `]
})
export class PaginationComponent {
  @Input() currentPage: number = 1;
  @Input() totalPages: number = 1;
  @Input() totalItems: number = 0;
  @Input() pageSize: number = 10;
  @Output() pageChange = new EventEmitter<number>();

  Math = Math;

  get visiblePages(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;

    if (this.totalPages <= maxVisible) {
      for (let i = 1; i <= this.totalPages; i++) pages.push(i);
    } else {
      pages.push(1);
      if (this.currentPage > 3) pages.push(-1);
      const start = Math.max(2, this.currentPage - 1);
      const end = Math.min(this.totalPages - 1, this.currentPage + 1);
      for (let i = start; i <= end; i++) {
        if (!pages.includes(i)) pages.push(i);
      }
      if (this.currentPage < this.totalPages - 2) pages.push(-1);
      if (!pages.includes(this.totalPages)) pages.push(this.totalPages);
    }
    return pages;
  }

  onPrev() { if (this.currentPage > 1) this.pageChange.emit(this.currentPage - 1); }
  onNext() { if (this.currentPage < this.totalPages) this.pageChange.emit(this.currentPage + 1); }
  onPage(page: number) { this.pageChange.emit(page); }
}
