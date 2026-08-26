import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JobService } from '../../core/services/job.service';
import { Job } from '../../core/models/business.models';
import { NotificationService } from '../../core/services/notification.service';
import { getJobCategoryLabel } from '../../core/constants/app.constants';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';

@Component({
  selector: 'app-job-approval',
  standalone: true,
  imports: [CommonModule, FormsModule, LogoSpinnerComponent, PageHeaderComponent, SearchBarComponent],
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
  }

  approve() {
    const job = this.selectedJob();
    if (!job) return;

    this.isProcessing.set(true);
    this.jobService.approveJob(job.id).subscribe({
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
    this.jobService.rejectJob(job.id, this.rejectReason()).subscribe({
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
