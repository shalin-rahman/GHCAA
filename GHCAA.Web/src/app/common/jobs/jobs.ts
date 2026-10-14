import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppDatePipe } from '../../core/pipes/app-date.pipe';
import { FormsModule } from '@angular/forms';
import { JobService } from '../../core/services/job.service';
import { Job } from '../../core/models/business.models';
import { firstValueFrom, Subscription } from 'rxjs';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { AuthService } from '../../core/services/auth.service';
import { getJobCategoryLabel, SUBMISSION_STATUS_MAP, LOOKUP_GROUPS, SEARCH_DEBOUNCE_MS, LIST_PAGE_SIZE } from '../../core/constants/app.constants';
import { appendById } from '../../core/utils/paging.util';
import { LookupService } from '../../core/services/lookup.service';
import { LoadingPanelComponent } from '../loading-panel/loading-panel';
import { PageHeaderComponent } from '../page-header/page-header.component';
import { SearchBarComponent } from '../search-bar/search-bar.component';
import { LoadMoreComponent } from '../load-more/load-more';
import { toWireDate, toDisplayDate } from '../../core/utils/date.util';
import { debounce } from '../../core/utils/debounce.util';

@Component({
  selector: 'app-jobs',
  standalone: true,
  imports: [CommonModule, AppDatePipe, FormsModule, LoadingPanelComponent, PageHeaderComponent, SearchBarComponent, LoadMoreComponent],
  templateUrl: './jobs.html',
  styleUrl: './jobs.scss'
})
export class Jobs implements OnInit {
  private jobService = inject(JobService);
  private notify = inject(NotificationService);
  private confirmDialog = inject(ConfirmDialogService);
  private lookupService = inject(LookupService);

  jobs = signal<Job[]>([]);
  loading = signal(true);
  loadingMore = signal(false);
  hasMore = signal(false);
  private page = 1;
  private jobCategory?: any;
  private pageSub?: Subscription;
  showForm = signal(false);
  // 58.2: table is the default view; card view stays available via a toggle.
  viewMode = signal<'table' | 'card'>('table');
  submitting = signal(false);
  selectedJob = signal<Job | null>(null);
  searchQuery = '';
  isEditing = signal(false);
  editingId = signal<number | null>(null);
  private auth = inject(AuthService);

  // 82.42: sourced from /lookups/JobCategory via LookupService, filled in ngOnInit. Kept as
  // {id,name} here (not {value,label}) so jobs.html's existing cat.id/cat.name bindings didn't
  // need touching.
  categories: { id: string; name: string }[] = [];

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
    this.lookupService.getOptions(LOOKUP_GROUPS.JobCategory).subscribe(
      opts => this.categories = opts.map(o => ({ id: o.value, name: o.label }))
    );
  }

  // Reloads with the current category and search, so a post, delete or search keeps the filter.
  loadJobs() {
    this.loading.set(true);
    this.fetchPage(1);
  }

  loadNextPage() {
    if (!this.hasMore() || this.loading() || this.loadingMore()) return;
    this.loadingMore.set(true);
    this.fetchPage(this.page + 1);
  }

  // A filter or search change drops any page still in flight, so old results never land in the new list.
  // The page number is kept only on success, so a failed page is asked for again.
  private fetchPage(page: number) {
    const filter: { jobCategory?: string | number; query?: string } = {};
    if (this.jobCategory !== undefined) filter.jobCategory = this.jobCategory;
    if (this.searchQuery) filter.query = this.searchQuery;

    this.pageSub?.unsubscribe();
    this.pageSub = this.jobService.getJobsPage(page, LIST_PAGE_SIZE, filter).subscribe({
      next: (result) => {
        this.page = result.page;
        this.jobs.set(result.page > 1 ? appendById(this.jobs(), result.items) : result.items);
        this.hasMore.set(result.page < result.totalPages);
        this.loading.set(false);
        this.loadingMore.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.loadingMore.set(false);
      }
    });
  }

  private debouncedLoadJobs = debounce(() => this.loadJobs(), SEARCH_DEBOUNCE_MS);

  onSearch() {
    this.debouncedLoadJobs();
  }

  postJob() {
    if (!this.newJob.title || !this.newJob.companyName) {
      this.notify.error('Please fill in the required fields marked with *');
      return;
    }

    this.submitting.set(true);
    const payload = { ...this.newJob, applicationDeadline: toWireDate(this.newJob.applicationDeadline) };
    const obs = this.isEditing() && this.editingId()
      ? this.jobService.updateJob(this.editingId()!, payload)
      : this.jobService.createJob(payload);

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
    return toDisplayDate(d);
  }

  closeForm() {
    this.showForm.set(false);
    this.isEditing.set(false);
    this.editingId.set(null);
    this.newJob = { title: '', companyName: '', location: '', jobCategory: 'IT', description: '', requirements: '', applicationDeadline: '', applicationEmail: '' };
  }

  onFilterChange(e: any) {
    const val = e.target.value;
    this.jobCategory = val === '' ? undefined : val;
    this.loadJobs();
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

  // Shows a Pending badge only to the poster on their own not-yet-approved jobs.
  isOwnPending(job: Job): boolean {
    const user = this.auth.currentUser();
    if (!user) return false;
    return job.postedByMemberId === user.memberId && job.status === 'Pending';
  }

  getStatusInfo(status: any) {
    return SUBMISSION_STATUS_MAP[status] || { label: 'Unknown', class: 'pending' };
  }

  async deleteJob(id: number) {
    const ok = await firstValueFrom(this.confirmDialog.confirm({
      title: 'Remove opportunity',
      message: 'Are you sure you want to remove this opportunity permanently?',
      confirmLabel: 'Remove',
      danger: true
    }));
    if (!ok) return;

    this.jobService.deleteJob(id).subscribe({
        next: () => {
            this.notify.success('Post removed from community hub.');
            this.loadJobs();
        },
        error: () => this.notify.error('Failed to remove post.')
    });
  }
}
