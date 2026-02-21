import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../core/services/notification.service';

@Component({
    selector: 'app-toast',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="toast-container">
      @for (toast of notify.toasts(); track toast.id) {
        <div class="toast glass-effect" [ngClass]="toast.type" (click)="notify.remove(toast.id)">
          <span class="icon">{{ getIcon(toast.type) }}</span>
          <span class="message">{{ toast.message }}</span>
        </div>
      }
    </div>
  `,
    styles: [`
    .toast-container {
      position: fixed;
      top: 24px;
      right: 24px;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 12px;
      pointer-events: none;
    }
    .toast {
      pointer-events: auto;
      min-width: 300px;
      padding: 1rem 1.5rem;
      border-radius: 12px;
      display: flex;
      align-items: center;
      gap: 12px;
      cursor: pointer;
      animation: slideIn 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      
      &.success { border-left: 4px solid #4CAF50; }
      &.error { border-left: 4px solid #F44336; }
      &.info { border-left: 4px solid #2196F3; }
      &.warning { border-left: 4px solid #FF9800; }
    }
    .icon { font-size: 1.25rem; }
    .message { font-weight: 600; font-size: 0.9rem; }

    @keyframes slideIn {
      from { transform: translateX(100%); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }
  `]
})
export class ToastComponent {
    notify = inject(NotificationService);

    getIcon(type: string): string {
        switch (type) {
            case 'success': return '✅';
            case 'error': return '❌';
            case 'warning': return '⚠️';
            default: return 'ℹ️';
        }
    }
}
