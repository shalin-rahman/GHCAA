import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminPollService, CreatePollDto } from '../../core/services/admin-poll.service';
import { PollDto } from '../../core/services/poll.service';
import { firstValueFrom } from 'rxjs';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { Icon } from '../../common/icon/icon';
import { ModalHeaderComponent } from '../../common/modal-header/modal-header.component';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';

@Component({
  selector: 'app-admin-polls',
  standalone: true,
  imports: [CommonModule, FormsModule, LogoSpinnerComponent, Icon, ModalHeaderComponent, PageHeaderComponent],
  templateUrl: './polls.html',
  styleUrl: './polls.scss'
})
export class AdminPolls implements OnInit {
  private pollService = inject(AdminPollService);
  private notify = inject(NotificationService);
  private confirmDialog = inject(ConfirmDialogService);

  polls = signal<PollDto[]>([]);
  loading = signal(false);
  showCreateModal = signal(false);
  // 58.2: table is the default view; card view (with the per-option progress bars) stays
  // available via a toggle.
  viewMode = signal<'table' | 'card'>('table');
  selectedPoll = signal<PollDto | null>(null);
  creating = signal(false);
  togglingId = signal<number | null>(null);
  deletingId = signal<number | null>(null);

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
    if (this.creating()) return;
    if (!this.newPoll.title || this.newPoll.options.some(o => !o)) {
      this.notify.warning('Please fill in all fields.');
      return;
    }

    this.creating.set(true);
    this.pollService.createPoll(this.newPoll).subscribe({
      next: () => {
        this.creating.set(false);
        this.notify.success('Poll created successfully.');
        this.showCreateModal.set(false);
        this.loadPolls();
        this.resetForm();
      },
      error: () => {
        this.creating.set(false);
        this.notify.error('Failed to create poll.');
      }
    });
  }

  toggleStatus(poll: PollDto) {
    if (this.togglingId() !== null) return;
    const newStatus = !poll.isActive;
    this.togglingId.set(poll.id);
    this.pollService.toggleStatus(poll.id, newStatus).subscribe({
      next: () => {
        this.togglingId.set(null);
        poll.isActive = newStatus;
        this.notify.success(`Poll ${newStatus ? 'activated' : 'deactivated'}.`);
      },
      error: () => {
        this.togglingId.set(null);
        this.notify.error('Failed to update status.');
      }
    });
  }

  async deletePoll(id: number) {
    if (this.deletingId() !== null) return;
    const ok = await firstValueFrom(this.confirmDialog.confirm({
      title: 'Delete poll',
      message: 'Are you sure you want to delete this poll?',
      confirmLabel: 'Delete',
      danger: true
    }));
    if (!ok) return;

    this.deletingId.set(id);
    this.pollService.deletePoll(id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.notify.success('Poll deleted.');
        this.loadPolls();
      },
      error: () => {
        this.deletingId.set(null);
        this.notify.error('Failed to delete poll.');
      }
    });
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
