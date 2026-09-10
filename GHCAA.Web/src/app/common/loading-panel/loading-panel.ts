import { Component, Input } from '@angular/core';
import { LogoSpinnerComponent } from '../logo-spinner/logo-spinner';

@Component({
  selector: 'app-loading-panel',
  standalone: true,
  imports: [LogoSpinnerComponent],
  template: `
    <div
      class="loading-panel"
      [class.loading-panel--compact]="compact"
      [class.loading-panel--overlay]="overlay"
      role="status"
      aria-live="polite"
      aria-busy="true">
      <app-logo-spinner [size]="size" [ripple]="false" />
      @if (label) {
        <span class="loading-panel__label">{{ label }}</span>
      }
    </div>
  `,
  host: {
    '[class.loading-panel-host]': 'true',
  },
})
export class LoadingPanelComponent {
  @Input() compact = false;
  @Input() overlay = false;
  @Input() label = 'Loading';
  @Input() size = 64;
}
