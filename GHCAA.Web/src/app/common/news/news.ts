import { Component, inject, signal, computed, OnInit, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { NewsService } from '../../core/services/news.service';
import { NewsPost, PostType } from '../../core/models/business.models';
import { getArticleCategoryLabel, POST_TYPE_TABS, matchesPostType } from '../../core/constants/app.constants';
import { ImgFallbackDirective } from '../directives/img-fallback.directive';
import { LogoSpinnerComponent } from '../logo-spinner/logo-spinner';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-news',
  standalone: true,
  imports: [CommonModule, ImgFallbackDirective, LogoSpinnerComponent],
  templateUrl: './news.html',
  styleUrl: './news.scss'
})
export class News implements OnInit {
  private newsService = inject(NewsService);
  private route = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);
  private router = inject(Router);
  getArticleCategoryLabel = getArticleCategoryLabel;

  news = signal<NewsPost[]>([]);
  loading = signal(true);
  selectedPost = signal<NewsPost | null>(null);
  // 58.2: table is the default view; card view stays available via a toggle.
  viewMode = signal<'table' | 'card'>('table');

  /** '' = everything; the feed is one list filtered client-side, so switching costs no request. */
  postTypeFilter = signal<'' | PostType>('');
  readonly postTypeTabs = POST_TYPE_TABS;

  filteredNews = computed(() =>
    this.news().filter(post => matchesPostType(post.postType, this.postTypeFilter()))
  );

  setFilter(type: '' | PostType) {
    this.applyFilter(type);
    // Keep the tab in the URL so /news?type=Notice is shareable and the nav link stays honest.
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { type: type || null },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
  }

  private applyFilter(type: '' | PostType) {
    this.postTypeFilter.set(type);
    const current = this.selectedPost();
    // Don't leave a detail panel open for a post the active tab no longer lists.
    if (current && !matchesPostType(current.postType, type)) {
      this.selectedPost.set(null);
    }
  }

  ngOnInit() {
    this.route.queryParamMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      const type = params.get('type');
      this.applyFilter(type === 'Notice' || type === 'News' ? type : '');
    });

    this.newsService.getNews(undefined, true).subscribe({
      next: (data) => {
        this.news.set(data);
        this.loading.set(false);

        // 82.32: /portal/news/:id and /news/:id had no route (NG04002), reached from the landing
        // page and member dashboard news feeds. There is no detail route by design — a post opens
        // inline via selectPost() — so those links pass the id as a query param instead, and this
        // opens the matching post the same way a card click does.
        const id = this.route.snapshot.queryParamMap.get('id');
        if (id) {
          const post = data.find(p => p.id === Number(id));
          if (post) this.selectPost(post);
        }
      },
      error: () => this.loading.set(false)
    });
  }

  selectPost(post: NewsPost) {
    this.selectedPost.set(post);
    setTimeout(() => {
      document.getElementById('news-detail')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }, 100);
  }
}


