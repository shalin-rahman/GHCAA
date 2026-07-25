import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ContactService } from '../../core/services/contact.service';
import { NotificationService } from '../../core/services/notification.service';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';

@Component({
  selector: 'app-contact-messages',
  standalone: true,
  imports: [CommonModule, FormsModule, LogoSpinnerComponent],
  templateUrl: './contact-messages.html',
  styleUrl: './contact-messages.scss'
})
export class ContactMessages implements OnInit {
  private contactService = inject(ContactService);
  private notify = inject(NotificationService);

  messages = signal<any[]>([]);
  loading = signal(true);
  selectedMessage = signal<any | null>(null);
  searchQuery = signal('');

  filteredMessages = computed(() => {
    const q = this.searchQuery().toLowerCase().trim();
    if (!q) return this.messages();
    return this.messages().filter(m =>
      (m.fullName || '').toLowerCase().includes(q) ||
      (m.email || '').toLowerCase().includes(q) ||
      (m.subject || '').toLowerCase().includes(q)
    );
  });

  ngOnInit() {
    this.loadMessages();
  }

  loadMessages() {
    this.loading.set(true);
    this.contactService.getMessages().subscribe({
      next: (data) => {
        this.messages.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.messages.set([]);
        this.loading.set(false);
        this.notify.error('Failed to load messages');
      }
    });
  }

  viewMessage(msg: any) {
    this.selectedMessage.set(msg);
    if (!msg.isRead) {
      this.markAsRead(msg);
    }
  }

  markAsRead(msg: any) {
    this.contactService.markAsRead(msg.id).subscribe({
      // 29F.2: surface HTTP failures instead of failing silently
      next: () => {
        msg.isRead = true;
      },
      error: () => this.notify.error('Failed to mark message as read.')
    });
  }
}
