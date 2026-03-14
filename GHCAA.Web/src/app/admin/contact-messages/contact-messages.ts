import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ContactService } from '../../core/services/contact.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-contact-messages',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './contact-messages.html',
  styleUrl: './contact-messages.scss'
})
export class ContactMessages implements OnInit {
  private contactService = inject(ContactService);
  private notify = inject(NotificationService);

  messages = signal<any[]>([]);
  loading = signal(true);
  selectedMessage = signal<any | null>(null);

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
      next: () => {
        msg.isRead = true;
        // Optionally update the signal if needed, but since we modified the object in the array it should reflect
      }
    });
  }
}
