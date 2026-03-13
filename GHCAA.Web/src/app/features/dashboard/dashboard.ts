import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AlertService } from '../../core/services/alert.service';
import { AuthService } from '../../core/services/auth.service';
import { ProfileService } from '../../core/services/profile.service';
import { JobService } from '../../core/services/job.service';
import { EventsService } from '../../core/services/events.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {
  alertService = inject(AlertService);
  private auth = inject(AuthService);
  private profileService = inject(ProfileService);
  private jobService = inject(JobService);
  private eventsService = inject(EventsService);

  profile = signal<any>(null);
  jobCount = signal<number>(0);
  eventCount = signal<number>(0);

  activities = signal([
    { id: 1, title: 'Annual Reunion', desc: 'Photos from the 2024 reunion uploaded.', time: '2h ago', color: 'var(--primary-color)' },
    { id: 2, title: 'Job Alert', desc: 'New Software Engineering role posted by Alumni.', time: '5h ago', color: 'var(--accent-color)' },
    { id: 3, title: 'Membership', desc: 'Your dues for 2025 are ready to pay.', time: '1d ago', color: 'var(--danger-color)' }
  ]);

  ngOnInit() {
    this.profileService.getProfile().subscribe({
      next: p => this.profile.set(p),
      error: () => {}
    });
    this.alertService.loadNotifications();
    this.loadStats();
  }

  loadStats() {
    this.jobService.getJobs().subscribe({
      next: jobs => this.jobCount.set(jobs.length),
      error: () => this.jobCount.set(0)
    });
    this.eventsService.getEvents().subscribe({
      next: evs => this.eventCount.set(evs.length),
      error: () => this.eventCount.set(0)
    });
  }

  getMembershipType(type: any): string {
    const types = ['Founding', 'Executive', 'General', 'Associate', 'Honorary', 'Advisory', 'Life'];
    return types[type] || 'General';
  }
}
