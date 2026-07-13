import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminPollService, CreatePollDto } from '../../core/services/admin-poll.service';
import { PollDto } from '../../core/services/poll.service';
import { NotificationService } from '../../core/services/notification.service';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';

@Component({
  selector: 'app-admin-polls',
  standalone: true,
  imports: [CommonModule, FormsModule, LogoSpinnerComponent],
  templateUrl: './polls.html',
  styleUrl: './polls.scss'
})
export class AdminPolls implements OnInit {
  private pollService = inject(AdminPollService);
  private notify = inject(NotificationService);

  polls = signal<PollDto[]>([]);
  loading = signal(false);
  showCreateModal = signal(false);

  newPoll: CreatePollDto = {
    title: '',
    description: '',
    allowMultipleChoice: false,
    options: ['', '']
  };

  ngOnInit() {
    this.loadPolls();
  }

  loadPolls() {
    this.loading.set(true);
    this.pollService.getAllPolls().subscribe({
      next: (data) => {
        this.polls.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.notify.error('Failed to load polls.');
        this.loading.set(false);
      }
    });
  }

  addOption() {
    this.newPoll.options.push('');
  }

  removeOption(index: number) {
    if (this.newPoll.options.length > 2) {
      this.newPoll.options.splice(index, 1);
    }
  }

  onCreatePoll() {
    if (!this.newPoll.title || this.newPoll.options.some(o => !o)) {
      this.notify.warning('Please fill in all fields.');
      return;
    }

    this.pollService.createPoll(this.newPoll).subscribe({
      next: () => {
        this.notify.success('Poll created successfully.');
        this.showCreateModal.set(false);
        this.loadPolls();
        this.resetForm();
      },
      error: () => this.notify.error('Failed to create poll.')
    });
  }

  toggleStatus(poll: PollDto) {
    const newStatus = !poll.isActive;
    this.pollService.toggleStatus(poll.id, newStatus).subscribe({
      next: () => {
        poll.isActive = newStatus;
        this.notify.success(`Poll ${newStatus ? 'activated' : 'deactivated'}.`);
      },
      error: () => this.notify.error('Failed to update status.')
    });
  }

  deletePoll(id: number) {
    if (confirm('Are you sure you want to delete this poll?')) {
      this.pollService.deletePoll(id).subscribe({
        next: () => {
          this.notify.success('Poll deleted.');
          this.loadPolls();
        },
        error: () => this.notify.error('Failed to delete poll.')
      });
    }
  }

  private resetForm() {
    this.newPoll = {
      title: '',
      description: '',
      allowMultipleChoice: false,
      options: ['', '']
    };
  }

  trackByIndex(index: number, obj: any): any {
    return index;
  }
}
