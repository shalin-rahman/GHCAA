import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppDatePipe } from '../../../../core/pipes/app-date.pipe';
import { RouterLink } from '@angular/router';
import { JobService } from '../../../../core/services/job.service';
import { Job } from '../../../../core/models/business.models';

@Component({
    selector: 'landing-jobs',
    standalone: true,
    imports: [CommonModule, AppDatePipe, RouterLink],
    templateUrl: './jobs-preview.html',
    styleUrl: './jobs-preview.scss',
})
export class LandingJobsPreview implements OnInit {
    private jobService = inject(JobService);
    jobs = signal<Job[]>([]);
    isVisible = signal(true);

    ngOnInit() {
        this.jobService.getJobs(undefined, true).subscribe({
            // 58.7: hide the whole section when there's nothing to show, not just on error.
            next: (data) => {
                const items = data.slice(0, 2);
                this.jobs.set(items);
                this.isVisible.set(items.length > 0);
            },
            error: () => {
                this.jobs.set([]);
                this.isVisible.set(false);
            }
        });
    }
}

