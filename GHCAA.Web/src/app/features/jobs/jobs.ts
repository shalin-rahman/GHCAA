import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JobService, Job } from '../../core/services/job.service';
import { NotificationService } from '../../core/services/notification.service';

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

  categories = [
    { id: 0, name: 'IT & Technology' },
    { id: 1, name: 'Finance & Accounts' },
    { id: 2, name: 'Engineering' },
    { id: 3, name: 'Marketing' },
    { id: 4, name: 'Education' },
    { id: 5, name: 'Healthcare' },
    { id: 6, name: 'Other' }
  ];

  newJob: any = {
    title: '',
    companyName: '',
    location: '',
    category: 0,
    description: '',
    requirements: '',
    deadline: '',
    applicationEmail: ''
  };

  ngOnInit() {
    this.loadJobs();
  }

  loadJobs(category?: number) {
    this.loading.set(true);
    this.jobService.getJobs(category).subscribe({
      next: (data) => {
        this.jobs.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  postJob() {
    this.submitting.set(true);
    this.jobService.postJob(this.newJob).subscribe({
      next: () => {
        this.notify.success('Job opportunity posted successfully! It will be visible to all alumni.');
        this.submitting.set(false);
        this.showForm.set(false);
        this.loadJobs();
      },
      error: () => {
        this.submitting.set(false);
        this.notify.error('Failed to post job. Please ensure all mandatory fields are filled.');
      }
    });
  }

  onCategoryChange(e: any) {
    const cat = e.target.value;
    this.loadJobs(cat ? Number(cat) : undefined);
  }

  getCategoryName(catId: any): string {
    const c = this.categories.find(x => x.id == catId);
    return c ? c.name : 'General';
  }
}
