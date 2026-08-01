import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NewsService } from '../../core/services/news.service';
import { NewsPost } from '../../core/models/business.models';
import { getArticleCategoryLabel } from '../../core/constants/app.constants';
import { ImgFallbackDirective } from '../directives/img-fallback.directive';
import { LogoSpinnerComponent } from '../logo-spinner/logo-spinner';

@Component({
  selector: 'app-news',
  standalone: true,
  imports: [CommonModule, ImgFallbackDirective, LogoSpinnerComponent],
  templateUrl: './news.html',
  styleUrl: './news.scss'
})
export class News implements OnInit {
  private newsService = inject(NewsService);
  getArticleCategoryLabel = getArticleCategoryLabel;

  news = signal<NewsPost[]>([]);
  loading = signal(true);
  selectedPost = signal<NewsPost | null>(null);

  ngOnInit() {
    this.newsService.getNews().subscribe({
      next: (data) => {
        this.news.set(data);
        this.loading.set(false);
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


