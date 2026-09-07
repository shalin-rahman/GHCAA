import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';
import { safeImageUrl } from '../../core/utils/image.util';
import { AdminService, DashboardStats } from '../../core/services/admin.service';
import { NavService } from '../../core/services/nav.service';
import { NewsService } from '../../core/services/news.service';
import { EventsService } from '../../core/services/events.service';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { SUBMISSION_STATUS_MAP } from '../../core/constants/app.constants';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';

interface StatCard {
  icon: string;
  value: string | number;
  label: string;
  sub?: string;
  variant?: 'accent' | 'dark' | 'success' | 'danger';
  link?: string;
  linkLabel?: string;
}

interface PendingRow {
  label: string;
  count: number;
  link: string;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe, ImgFallbackDirective, PageHeaderComponent],
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
  publishedArticleCount = signal(0);
  upcomingEvents = signal<any[]>([]);
  pendingRows = signal<PendingRow[]>([]);
  loading = signal(true);
  now = new Date();

  ngOnInit() {
    forkJoin({
      stats: this.adminService.getStats().pipe(catchError(() => of(null))),
      news: this.newsService.getNewsAdmin().pipe(catchError(() => of([]))),
      events: this.eventsService.getEvents().pipe(catchError(() => of([]))),
      pending: this.adminService.getPendingSummary().pipe(catchError(() => of(null)))
    }).subscribe({
      next: ({ stats, news, events, pending }) => {
        this.pendingRows.set(this.mapPendingRows(pending));
        // Handle case-insensitive stats mapping (PascalCase from C# vs camelCase in JS)
        const s: any = stats || {};
        const mappedStats: DashboardStats = {
          totalMembers: s.totalMembers ?? s.TotalMembers ?? 0,
          applied: s.applied ?? s.Applied ?? 0,
          active: s.active ?? s.Active ?? 0,
          inactive: s.inactive ?? s.Inactive ?? 0,
          // Keep null distinct from 0: the API returns null for non-SuperAdmin (balance is
          // restricted), and coercing that to 0 showed every plain Admin a confident "৳0".
          balance: s.balance ?? s.Balance ?? null,
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
        // Count the whole set, not the 4 shown below it — the tile is a total, and
        // getNewsAdmin returns drafts too, so only published items count toward it.
        this.publishedArticleCount.set(
          newsItems.filter((n: any) => (n.isPublished ?? n.IsPublished) !== false).length
        );

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

  get inactiveCount(): number {
    return this.stats()?.inactive ?? 0;
  }

  // Each queue's own endpoint owns the real list and its paging; this only reads back
  // how many items came back, for the "awaiting your action" summary on this dashboard.
  private mapPendingRows(pending: any): PendingRow[] {
    if (!pending) return [];
    const countOf = (list: any): number => Array.isArray(list) ? list.length : (list?.totalItems ?? list?.TotalItems ?? 0);

    return [
      { label: 'Article submissions', count: countOf(pending.news ?? pending.News), link: '/admin/article-approvals' },
      { label: 'Gallery albums', count: countOf(pending.galleries ?? pending.Galleries), link: '/admin/gallery-approvals' },
      { label: 'Gallery photos', count: countOf(pending.photos ?? pending.Photos), link: '/admin/gallery-approvals' },
      { label: 'Job postings', count: countOf(pending.jobs ?? pending.Jobs), link: '/admin/job-approvals' },
      { label: 'Member applications', count: countOf(pending.members ?? pending.Members), link: '/admin/approvals' },
      { label: 'Event registrations', count: countOf(pending.eventRegistrations ?? pending.EventRegistrations), link: '/admin/events' },
    ].filter(row => row.count > 0);
  }

  formatBDT(value: number): string {
    return `৳${value.toLocaleString('en-BD')}`;
  }

  // '' sentinel fallback (rather than the shared util's default logo) so the template's
  // existing @if/@else (thumb vs. 📄 placeholder icon) keeps working unchanged.
  safeImg(item: { coverImageUrl?: string | null; imageUrl?: string | null }): string {
    return safeImageUrl(item.coverImageUrl || item.imageUrl, '');
  }

  newsStatus(item: { status?: string | number }): { label: string; class: string } {
    return SUBMISSION_STATUS_MAP[item.status ?? 'Approved'] ?? SUBMISSION_STATUS_MAP['Approved'];
  }
}
