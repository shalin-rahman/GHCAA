import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AdminService, DashboardStats } from '../../core/services/admin.service';
import { NavService } from '../../core/services/nav.service';
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
  public nav = inject(NavService);
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
        // Handle case-insensitive stats mapping (PascalCase from C# vs camelCase in JS)
        const s: any = stats || {};
        const mappedStats: DashboardStats = {
          totalMembers: s.totalMembers ?? s.TotalMembers ?? 0,
          applied: s.applied ?? s.Applied ?? 0,
          active: s.active ?? s.Active ?? 0,
          inactive: s.inactive ?? s.Inactive ?? 0,
          balance: s.balance ?? s.Balance ?? 0,
          lastUpdated: s.lastUpdated ?? s.LastUpdated ?? new Date().toISOString()
        };
        
        this.stats.set(mappedStats);
        
        // Take last 4 news items sorted by date
        const newsItems = Array.isArray(news) ? news : ((news as any)?.items || []);
        const sorted = [...newsItems].sort(
          (a, b) => new Date(b.publishedAt || b.PublishedAt || b.createdAt || b.CreatedAt).getTime() - 
                     new Date(a.publishedAt || a.PublishedAt || a.createdAt || a.CreatedAt).getTime()
        );
        this.recentNews.set(sorted.slice(0, 4));

        // Take up to 4 upcoming events
        const eventItems = Array.isArray(events) ? events : ((events as any)?.items || []);
        const upcoming = [...eventItems]
          .filter(e => new Date(e.startDate || e.StartDate) >= new Date())
          .sort((a, b) => new Date(a.startDate || a.StartDate).getTime() - new Date(b.startDate || b.StartDate).getTime())
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
