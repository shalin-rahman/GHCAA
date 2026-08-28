import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AlertService } from '../../core/services/alert.service';
import { ProfileService } from '../../core/services/profile.service';
import { EventsService } from '../../core/services/events.service';
import { NetworkingService } from '../../core/services/networking.service';
import { NewsService } from '../../core/services/news.service';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';
import { Icon } from '../../common/icon/icon';
import { getMembershipTypeLabel } from '../../core/constants/app.constants';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe, Icon, ImgFallbackDirective],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {
  alertService = inject(AlertService);
  private profileService = inject(ProfileService);
  private eventsService = inject(EventsService);
  private networkingService = inject(NetworkingService);
  private newsService = inject(NewsService);

  profile = signal<any>(null);
  eventCount = signal<number>(0);
  recentMembers = signal<any[]>([]);
  recentNews = signal<any[]>([]);
  upcomingEvents = signal<any[]>([]);
  loading = signal(true);

  get profileCompletion(): number {
    return this.profile()?.profileCompletionPercentage ?? 0;
  }
  get contributionPoints(): number {
    return this.profile()?.contributionPoints ?? 0;
  }
  get memberRank(): string {
    return this.profile()?.rank ?? '—';
  }

  ngOnInit() {
    this.alertService.loadNotifications();

    forkJoin({
      profile: this.profileService.getProfile().pipe(catchError(() => of(null))),
      events: this.eventsService.getEvents().pipe(catchError(() => of([]))),
      members: this.networkingService.getRecentlyJoined(6).pipe(catchError(() => of({items:[]}))),
      news: this.newsService.getNews(undefined, true).pipe(catchError(() => of([])))
    }).subscribe({
      next: ({ profile, events, members, news }) => {
        this.profile.set(profile);

        const upcoming = (events as any[]).filter(e => new Date(e.startDate) >= new Date());
        this.eventCount.set(upcoming.length);
        this.upcomingEvents.set(
          upcoming.sort((a,b) => new Date(a.startDate).getTime() - new Date(b.startDate).getTime()).slice(0, 3)
        );

        const membersData = members as any;
        this.recentMembers.set(membersData?.items || membersData || []);

        const sortedNews = [...(news as any[])]
          .sort((a, b) => new Date(b.publishedAt || b.createdAt).getTime() - new Date(a.publishedAt || a.createdAt).getTime())
          .slice(0, 3);
        this.recentNews.set(sortedNews);

        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  // 35.2: same defect as 35.1 — the local map labelled index 6 'Life' when the domain enum's
  // member 6 is Guest. Delegates to the shared helper; the label already includes " Member".
  getMembershipType = getMembershipTypeLabel;

  getStatusColor(status: any): string {
    const colors: Record<number, string> = {
      0: '#f59e0b', 1: '#10b981', 2: '#6b7280', 3: '#ef4444', 4: '#ef4444'
    };
    return colors[status as number] || 'var(--accent-color)';
  }

  getStatusLabel(status: any): string {
    const labels: Record<number, string> = {
      0: 'Pending', 1: 'Active', 2: 'Inactive', 3: 'Resigned', 4: 'Terminated'
    };
    return labels[status as number] || 'Unknown';
  }
}
