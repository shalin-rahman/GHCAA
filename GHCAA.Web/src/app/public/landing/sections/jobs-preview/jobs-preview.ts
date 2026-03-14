import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { JobService } from '../../../../core/services/job.service';
import { Job } from '../../../../core/models/business.models';

@Component({
    selector: 'landing-jobs',
    standalone: true,
    imports: [CommonModule, RouterLink],
    templateUrl: './jobs-preview.html',
    styleUrl: './jobs-preview.scss',
})
export class LandingJobsPreview implements OnInit {
    private jobService = inject(JobService);
    jobs = signal<Job[]>([]);

    ngOnInit() {
        this.jobService.getJobs().subscribe({
            next: (data) => this.jobs.set(data.slice(0, 2)),
            error: () => this.jobs.set([])
        });
    }
}


