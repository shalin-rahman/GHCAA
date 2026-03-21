import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AdminService, DashboardStats } from '../../core/services/admin.service';
import { NewsService } from '../../core/services/news.service';
import { EventsService } from '../../core/services/events.service';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

interface StatCard {
  icon: string;
  value: string | number;
  label: string;
  sub?: string;
  variant?: 'accent' | 'dark' | 'success' | 'danger';
  link?: string;
  linkLabel?: string;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss'
})
export class AdminDashboard implements OnInit {
  private adminService = inject(AdminService);
  private newsService = inject(NewsService);
  private eventsService = inject(EventsService);

  stats = signal<DashboardStats | null>(null);
  recentNews = signal<any[]>([]);
  upcomingEvents = signal<any[]>([]);
  loading = signal(true);
  now = new Date();

  ngOnInit() {
    forkJoin({
      stats: this.adminService.getStats().pipe(catchError(() => of(null))),
      news: this.newsService.getNewsAdmin().pipe(catchError(() => of([]))),
      events: this.eventsService.getEvents().pipe(catchError(() => of([])))
    }).subscribe({
      next: ({ stats, news, events }) => {
        this.stats.set(stats ?? {
          totalMembers: 0, applied: 0, active: 0, inactive: 0, balance: 0,
          lastUpdated: new Date().toISOString()
        });
        // Take last 4 news items sorted by date
        const sorted = [...(news as any[])].sort(
          (a, b) => new Date(b.publishedAt || b.createdAt).getTime() - new Date(a.publishedAt || a.createdAt).getTime()
        );
        this.recentNews.set(sorted.slice(0, 4));

        // Take up to 4 upcoming events
        const upcoming = [...(events as any[])]
          .filter(e => new Date(e.startDate) >= new Date())
          .sort((a, b) => new Date(a.startDate).getTime() - new Date(b.startDate).getTime())
          .slice(0, 4);
        this.upcomingEvents.set(upcoming);

        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  get membershipRate(): number {
    const s = this.stats();
    if (!s || !s.totalMembers) return 0;
    return Math.round((s.active / s.totalMembers) * 100);
  }

  get inactiveCount(): number {
    return this.stats()?.inactive ?? 0;
  }

  formatBDT(value: number): string {
    return `৳${value.toLocaleString('en-BD')}`;
  }
}
