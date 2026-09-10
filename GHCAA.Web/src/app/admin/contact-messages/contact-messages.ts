import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ContactService } from '../../core/services/contact.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { ModalHeaderComponent } from '../../common/modal-header/modal-header.component';

@Component({
  selector: 'app-contact-messages',
  standalone: true,
  imports: [CommonModule, FormsModule, LoadingPanelComponent, PageHeaderComponent, SearchBarComponent, ModalHeaderComponent],
  templateUrl: './contact-messages.html',
  styleUrl: './contact-messages.scss'
})
export class ContactMessages implements OnInit {
  private contactService = inject(ContactService);
  private notify = inject(NotificationService);
  private confirmDialog = inject(ConfirmDialogService);

  messages = signal<any[]>([]);
  loading = signal(true);
  selectedMessage = signal<any | null>(null);
  searchQuery = signal('');
  deletingId = signal<number | null>(null);

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

  async deleteMessage(msg: any) {
    if (this.deletingId() !== null) return;
    const ok = await firstValueFrom(this.confirmDialog.confirm({
      title: 'Delete enquiry',
      message: 'Delete this enquiry permanently?',
      confirmLabel: 'Delete',
      danger: true
    }));
    if (!ok) return;
    this.deletingId.set(msg.id);
    this.contactService.deleteMessage(msg.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.notify.success('Enquiry deleted.');
        if (this.selectedMessage()?.id === msg.id) {
          this.selectedMessage.set(null);
        }
        this.loadMessages();
      },
      error: () => {
        this.deletingId.set(null);
        this.notify.error('Failed to delete enquiry.');
      }
    });
  }
}
