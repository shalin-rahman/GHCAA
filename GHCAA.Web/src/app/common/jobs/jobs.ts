import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JobService } from '../../core/services/job.service';
import { Job } from '../../core/models/business.models';
import { NotificationService } from '../../core/services/notification.service';
import { AuthService } from '../../core/services/auth.service';
import { JOB_CATEGORIES, getJobCategoryLabel } from '../../core/constants/app.constants';
import { LogoSpinnerComponent } from '../logo-spinner/logo-spinner';

@Component({
  selector: 'app-jobs',
  standalone: true,
  imports: [CommonModule, FormsModule, LogoSpinnerComponent],
  templateUrl: './jobs.html',
  styleUrl: './jobs.scss'
})
export class Jobs implements OnInit {
  private jobService = inject(JobService);
  private notify = inject(NotificationService);

  jobs = signal<Job[]>([]);
  loading = signal(true);
  showForm = signal(false);
  submitting = signal(false);
  selectedJob = signal<Job | null>(null);
  searchQuery = '';
  isEditing = signal(false);
  editingId = signal<number | null>(null);
  private auth = inject(AuthService);

  categories = JOB_CATEGORIES;

  newJob: any = {
    title: '',
    companyName: '',
    location: '',
    jobCategory: 'IT',
    description: '',
    requirements: '',
    applicationDeadline: '',
    applicationEmail: ''
  };

  ngOnInit() {
    this.loadJobs();
  }

  loadJobs(jobCategory?: any) {
    this.loading.set(true);
    const params: any = {};
    if (jobCategory !== undefined) params.jobCategory = jobCategory;
    if (this.searchQuery) params.query = this.searchQuery;

    this.jobService.getJobs(params).subscribe({
      next: (data) => {
        this.jobs.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onSearch() {
    this.loadJobs();
  }

  postJob() {
    if (!this.newJob.title || !this.newJob.companyName) {
      this.notify.error('Please fill in the required fields marked with *');
      return;
    }

    this.submitting.set(true);
    const obs = this.isEditing() && this.editingId()
      ? this.jobService.updateJob(this.editingId()!, this.newJob)
      : this.jobService.createJob(this.newJob);

    obs.subscribe({
      next: () => {
        this.notify.success(this.isEditing() ? 'Opportunity updated!' : 'Opportunity shared with the alumni community!');
        this.submitting.set(false);
        this.closeForm();
        this.loadJobs();
      },
      error: () => {
        this.submitting.set(false);
        this.notify.error('Failed to post opportunity.');
      }
    });
  }

  editJob(job: Job) {
    this.isEditing.set(true);
    this.editingId.set(job.id);
    this.newJob = {
      title: job.title,
      companyName: job.companyName,
      location: job.location,
      jobCategory: job.jobCategory,
      description: job.description,
      requirements: job.requirements,
      applicationDeadline: this.formatDateToDMY(job.applicationDeadline),
      applicationEmail: job.applicationEmail
    };
    this.showForm.set(true);
    this.selectedJob.set(null);
  }

  formatDateToDMY(d: any) {
    if (!d) return '';
    const date = new Date(d);
    if (isNaN(date.getTime())) return d;
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    return `${day}-${month}-${year}`;
  }

  closeForm() {
    this.showForm.set(false);
    this.isEditing.set(false);
    this.editingId.set(null);
    this.newJob = { title: '', companyName: '', location: '', jobCategory: 'IT', description: '', requirements: '', applicationDeadline: '', applicationEmail: '' };
  }

  onFilterChange(e: any) {
    const val = e.target.value;
    this.loadJobs(val === '' ? undefined : val);
  }

  getCategoryName(categoryStr: string): string {
    return getJobCategoryLabel(categoryStr);
  }


  viewJob(job: Job) {
    this.selectedJob.set(job);
  }

  canEdit(job: Job): boolean {
    const user = this.auth.currentUser();
    if (!user) return false;
    return user.role === 'Admin' || user.role === 'SuperAdmin' || job.postedByMemberId === user.memberId;
  }

  deleteJob(id: number) {
    if (!confirm('Are you sure you want to remove this opportunity permanently?')) return;
    this.jobService.deleteJob(id).subscribe({
        next: () => {
            this.notify.success('Post removed from community hub.');
            this.loadJobs();
        },
        error: () => this.notify.error('Failed to remove post.')
    });
  }
}
