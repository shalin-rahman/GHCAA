import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JobService } from '../../core/services/job.service';
import { Job } from '../../core/models/business.models';
import { NotificationService } from '../../core/services/notification.service';
import { getJobCategoryLabel } from '../../core/constants/app.constants';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { NotifyToggleComponent } from '../../common/notify-toggle/notify-toggle.component';
import { ModalHeaderComponent } from '../../common/modal-header/modal-header.component';

@Component({
  selector: 'app-job-approval',
  standalone: true,
  imports: [CommonModule, FormsModule, LoadingPanelComponent, PageHeaderComponent, SearchBarComponent, NotifyToggleComponent, ModalHeaderComponent],
  templateUrl: './job-approval.html',
  styleUrl: './job-approval.scss'
})
export class JobApproval implements OnInit {
  private jobService = inject(JobService);
  private notify = inject(NotificationService);

  pendingJobs = signal<Job[]>([]);
  loading = signal(true);
  selectedJob = signal<Job | null>(null);
  rejectReason = signal('');
  notifyMember = signal(true);
  isProcessing = signal(false);
  searchQuery = signal('');

  filteredJobs = computed(() => {
    const q = this.searchQuery().toLowerCase().trim();
    if (!q) return this.pendingJobs();
    return this.pendingJobs().filter(j =>
      (j.title || '').toLowerCase().includes(q) ||
      (j.companyName || '').toLowerCase().includes(q) ||
      (j.postedByMemberName || '').toLowerCase().includes(q)
    );
  });

  ngOnInit() {
    this.loadPending();
  }

  loadPending() {
    this.loading.set(true);
    this.jobService.getPendingJobs().subscribe({
      next: (data) => {
        this.pendingJobs.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.pendingJobs.set([]);
        this.loading.set(false);
      }
    });
  }

  getCategoryName(categoryStr: string): string {
    return getJobCategoryLabel(categoryStr);
  }

  viewJob(job: Job) {
    this.selectedJob.set(job);
    this.rejectReason.set('');
    this.notifyMember.set(true);
  }

  approve() {
    const job = this.selectedJob();
    if (!job) return;

    this.isProcessing.set(true);
    this.jobService.approveJob(job.id, this.notifyMember()).subscribe({
      next: () => {
        this.notify.success('Job approved and published');
        this.selectedJob.set(null);
        this.loadPending();
        this.isProcessing.set(false);
      },
      error: () => {
        this.notify.error('Approval failed');
        this.isProcessing.set(false);
      }
    });
  }

  reject() {
    const job = this.selectedJob();
    if (!job || !this.rejectReason()) {
      this.notify.warning('Please provide a reason for rejection');
      return;
    }

    this.isProcessing.set(true);
    this.jobService.rejectJob(job.id, this.rejectReason(), this.notifyMember()).subscribe({
      next: () => {
        this.notify.success('Job rejected');
        this.selectedJob.set(null);
        this.loadPending();
        this.isProcessing.set(false);
      },
      error: () => {
        this.notify.error('Rejection failed');
        this.isProcessing.set(false);
      }
    });
  }
}
