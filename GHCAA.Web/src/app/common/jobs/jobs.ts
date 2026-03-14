import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JobService, Job } from '../../core/services/job.service';
import { NotificationService } from '../../core/services/notification.service';
import { AuthService } from '../../core/services/auth.service';
import { JOB_CATEGORIES } from '../../core/constants/app.constants';

@Component({
  selector: 'app-jobs',
  standalone: true,
  imports: [CommonModule, FormsModule],
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
    category: 'IT',
    description: '',
    requirements: '',
    applicationDeadline: '',
    applicationEmail: ''
  };

  ngOnInit() {
    this.loadJobs();
  }

  loadJobs(category?: number) {
    this.loading.set(true);
    const params: any = {};
    if (category !== undefined) params.category = category;
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
      category: job.category,
      description: job.description,
      requirements: job.requirements,
      deadline: job.applicationDeadline, // Backend usually expects YYYY-MM-DD
      applicationEmail: job.applicationEmail
    };
    this.showForm.set(true);
    this.selectedJob.set(null);
  }

  closeForm() {
    this.showForm.set(false);
    this.isEditing.set(false);
    this.editingId.set(null);
    this.newJob = { title: '', companyName: '', location: '', category: 'IT', description: '', requirements: '', applicationDeadline: '', applicationEmail: '' };
  }

  onFilterChange(e: any) {
    const val = e.target.value;
    this.loadJobs(val === '' ? undefined : Number(val));
  }

  getCategoryName(categoryStr: string): string {
    // Backend returns string enum names like 'IT', 'Finance', etc.
    const cat = JOB_CATEGORIES.find(c => c.name.toLowerCase().includes(categoryStr?.toLowerCase()) || c.id.toString() === categoryStr);
    return cat?.name || categoryStr || 'General';
  }

  viewJob(job: Job) {
    this.selectedJob.set(job);
  }

  canEdit(job: Job): boolean {
    const user = this.auth.currentUser();
    if (!user) return false;
    return user.role === 'Admin' || user.role === 'SuperAdmin' || job.postedByMemberId === user.memberId;
  }
}


