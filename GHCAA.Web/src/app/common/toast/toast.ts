import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../core/services/notification.service';
import { Icon } from '../icon/icon';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule, Icon],
  templateUrl: './toast.html'
})
export class ToastComponent {
  notify = inject(NotificationService);

  getIcon(type: string): string {
    switch (type) {
      case 'success': return 'toast-success';
      case 'error': return 'toast-error';
      case 'warning': return 'toast-warning';
      default: return 'toast-info';
    }
  }
}


