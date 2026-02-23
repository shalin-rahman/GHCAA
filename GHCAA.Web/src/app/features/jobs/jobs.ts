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
  selectedJob = signal<Job | null>(null);

  categories = [
    { id: 0, name: 'IT & Software Development' },
    { id: 1, name: 'Finance & Banking' },
    { id: 2, name: 'Engineering & Construction' },
    { id: 3, name: 'Marketing & Sales' },
    { id: 4, name: 'Education & Research' },
    { id: 5, name: 'Healthcare & Pharma' },
    { id: 6, name: 'Govt. & Public Sector' },
    { id: 7, name: 'Mentorship & Career Guidance' },
    { id: 8, name: 'Other Opportunities' }
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
    if (!this.newJob.title || !this.newJob.companyName) {
      this.notify.error('Please fill in the required fields marked with *');
      return;
    }

    this.submitting.set(true);
    this.jobService.postJob(this.newJob).subscribe({
      next: () => {
        this.notify.success('Opportunity shared with the alumni community!');
        this.submitting.set(false);
        this.showForm.set(false);
        this.loadJobs();
        // Reset form
        this.newJob = { title: '', companyName: '', location: '', category: 0, description: '', requirements: '', deadline: '', applicationEmail: '' };
      },
      error: () => {
        this.submitting.set(false);
        this.notify.error('Failed to post opportunity.');
      }
    });
  }

  onFilterChange(e: any) {
    const val = e.target.value;
    this.loadJobs(val === '' ? undefined : Number(val));
  }

  getCategoryName(id: number): string {
    return this.categories.find(c => c.id === id)?.name || 'General';
  }

  viewJob(job: Job) {
    this.selectedJob.set(job);
  }
}
