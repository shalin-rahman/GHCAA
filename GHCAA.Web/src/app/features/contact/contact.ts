import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ContactService } from '../../core/services/contact.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './contact.html',
  styleUrl: './contact.scss'
})
export class Contact {
  private contactService = inject(ContactService);
  private notify = inject(NotificationService);
  loading = signal(false);

  model = {
    fullName: '',
    email: '',
    subject: 'Membership Enquiry',
    message: ''
  };

  onSubmit(form: any) {
    if (form.invalid) return;
    this.loading.set(true);

    this.contactService.sendMessage(this.model).subscribe({
      next: (res: any) => {
        this.loading.set(false);
        this.notify.success(res.message || 'Your enquiry has been sent. We will respond within 24-48 hours.');
        form.reset();
        this.model.subject = 'Membership Enquiry';
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to send enquiry. Please check your connection and try again.');
      }
    });
  }
}
