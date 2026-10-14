import { ChangeDetectionStrategy, Component, ElementRef, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges, inject } from '@angular/core';
import { LoadingPanelComponent } from '../loading-panel/loading-panel';

// 98.17: the footer of a paged list. It asks for the next page when it scrolls near the viewport,
// and keeps a button for browsers without IntersectionObserver or for keyboard users.
@Component({
  selector: 'app-load-more',
  standalone: true,
  imports: [LoadingPanelComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (loading) {
      <app-loading-panel [compact]="true" label="Loading more" />
    } @else if (hasMore) {
      <button type="button" class="btn btn-outline btn-sm" (click)="more.emit()">Load more</button>
    }
  `,
  styles: [':host { display: flex; justify-content: center; padding: 1.5rem 0 1rem; }'],
})
export class LoadMoreComponent implements OnInit, OnChanges, OnDestroy {
  @Input() hasMore = false;
  @Input() loading = false;
  @Output() more = new EventEmitter<void>();

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private observer?: IntersectionObserver;

  ngOnInit() {
    if (typeof IntersectionObserver === 'undefined') return;
    this.observer = new IntersectionObserver(
      entries => {
        if (entries[0]?.isIntersecting && this.hasMore && !this.loading) this.more.emit();
      },
      { rootMargin: '600px' }
    );
    this.observer.observe(this.host.nativeElement);
  }

  // The observer only reports a change in visibility, so a footer still in view after a short page
  // would never fire again. Observing afresh reports the current state once.
  ngOnChanges(changes: SimpleChanges) {
    const loading = changes['loading'];
    if (!this.observer || !loading || loading.firstChange || this.loading) return;
    this.observer.unobserve(this.host.nativeElement);
    this.observer.observe(this.host.nativeElement);
  }

  ngOnDestroy() {
    this.observer?.disconnect();
  }
}
