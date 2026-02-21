import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JobService, Job } from '../../core/services/job.service';

@Component({
  selector: 'app-jobs',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="header">
      <h2>Job Hub</h2>
      <button class="btn btn-primary" (click)="showForm.set(true)">+ Post a Job</button>
    </div>

    <!-- Filters -->
    <div class="filters glass-card" *ngIf="!showForm()">
      <input type="text" placeholder="Search jobs..." class="search-input">
      <select class="filter-select" (change)="onCategoryChange($event)">
        <option value="">All Categories</option>
        @for (cat of categories; track cat.id) {
            <option [value]="cat.id">{{ cat.name }}</option>
        }
      </select>
    </div>

    <!-- Job Creation Form -->
    <div class="glass-card form-container" *ngIf="showForm()">
        <div class="form-header">
            <h3>Post a New Opportunity</h3>
            <button class="close-btn" (click)="showForm.set(false)">✕</button>
        </div>
        <form (submit)="postJob()" class="job-form">
            <div class="form-grid">
                <div class="form-group">
                    <label>Job Title *</label>
                    <input type="text" [(ngModel)]="newJob.title" name="title" required>
                </div>
                <div class="form-group">
                    <label>Company Name *</label>
                    <input type="text" [(ngModel)]="newJob.companyName" name="companyName" required>
                </div>
                <div class="form-group">
                    <label>Location *</label>
                    <input type="text" [(ngModel)]="newJob.location" name="location" required>
                </div>
                <div class="form-group">
                    <label>Category *</label>
                    <select [(ngModel)]="newJob.category" name="category" required>
                        @for (cat of categories; track cat.id) {
                            <option [value]="cat.id">{{ cat.name }}</option>
                        }
                    </select>
                </div>
                <div class="form-group full-width">
                    <label>Description *</label>
                    <textarea [(ngModel)]="newJob.description" name="description" required rows="3"></textarea>
                </div>
                <div class="form-group full-width">
                    <label>Requirements *</label>
                    <textarea [(ngModel)]="newJob.requirements" name="requirements" required rows="3"></textarea>
                </div>
                <div class="form-group">
                    <label>Deadline *</label>
                    <input type="date" [(ngModel)]="newJob.deadline" name="deadline" required>
                </div>
                <div class="form-group">
                    <label>Application Link/Email</label>
                    <input type="text" [(ngModel)]="newJob.applicationEmail" name="appEmail">
                </div>
            </div>
            <div class="form-actions">
                <button type="submit" class="btn btn-primary" [disabled]="submitting()">
                    {{ submitting() ? 'Posting...' : 'Public Opportunity' }}
                </button>
                <button type="button" class="btn btn-secondary" (click)="showForm.set(false)">Cancel</button>
            </div>
        </form>
    </div>

    <!-- Job List -->
    <div class="job-list" *ngIf="!showForm()">
      @if (loading()) {
        <div class="loading">Searching opportunities...</div>
      } @else {
        @for (job of jobs(); track job.id) {
          <div class="glass-card job-card">
            <div class="job-main">
              <div class="job-info">
                <h3>{{ job.title }}</h3>
                <p class="company">{{ job.companyName }} • {{ job.location }}</p>
                <div class="tags">
                  <span class="tag tag-type">{{ getCategoryName(job.category) }}</span>
                  <span class="tag tag-deadline">Deadline: {{ job.deadline | date }}</span>
                </div>
              </div>
              <button class="btn btn-accent">Apply Now</button>
            </div>
          </div>
        } @empty {
          <div class="empty-state glass-card">
            <p>No job opportunities found. Be the first to post!</p>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
    .filters { padding: 1.5rem; margin-bottom: 2.5rem; display: flex; gap: 1.5rem; }
    .search-input { flex: 1; padding: 0.8rem; border: 1px solid var(--glass-border); border-radius: 8px; background: var(--bg-color); color: var(--text-main); }
    .filter-select { width: 200px; padding: 0.8rem; border-radius: 8px; border: 1px solid var(--glass-border); background: var(--bg-color); color: var(--text-main); }
    
    .form-container { padding: 2.5rem; margin-bottom: 3rem; background: var(--surface-color); }
    .form-header { display: flex; justify-content: space-between; margin-bottom: 2rem; color: var(--primary-color); }
    .close-btn { background: none; border: none; font-size: 1.5rem; cursor: pointer; opacity: 0.5; }
    .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1.5rem; }
    .full-width { grid-column: span 2; }
    .form-group label { display: block; font-weight: 700; margin-bottom: 0.5rem; font-size: 0.8rem; color: var(--text-muted); }
    .form-group input, .form-group select, .form-group textarea { width: 100%; padding: 0.75rem; border: 1px solid var(--glass-border); border-radius: 8px; background: var(--bg-color); color: var(--text-main); }
    .form-actions { margin-top: 2rem; display: flex; gap: 1rem; }

    .job-card { padding: 2rem; transition: 0.3s; margin-bottom: 1.5rem; }
    .job-card:hover { transform: translateY(-3px); border-color: var(--primary-color); }
    .job-main { display: flex; justify-content: space-between; align-items: center; }
    .company { color: var(--text-muted); margin-bottom: 1rem; }
    .tags { display: flex; gap: 0.75rem; }
    .tag { padding: 0.2rem 0.75rem; border-radius: 15px; font-size: 0.7rem; font-weight: 800; border: 1px solid transparent; }
    .tag-type { background: rgba(0, 77, 64, 0.1); color: var(--primary-color); }
    .tag-deadline { background: rgba(178, 34, 34, 0.1); color: var(--danger-color); }
    .loading { text-align: center; padding: 3rem; }

    @media (max-width: 768px) {
      .job-main { flex-direction: column; align-items: flex-start; gap: 1.5rem; }
      .filters { flex-direction: column; }
      .form-grid { grid-template-columns: 1fr; }
      .full-width { grid-column: span 1; }
    }
  `]
})
export class Jobs implements OnInit {
  private jobService = inject(JobService);

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
        alert('Job posted successfully!');
        this.submitting.set(false);
        this.showForm.set(false);
        this.loadJobs();
      },
      error: () => {
        this.submitting.set(false);
        alert('Failed to post job. Please ensure all fields are valid.');
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
