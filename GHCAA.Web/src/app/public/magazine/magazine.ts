import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NewsService } from '../../core/services/news.service';
import { NewsPost } from '../../core/models/business.models';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';

@Component({
  selector: 'app-magazine',
  standalone: true,
  imports: [CommonModule, LogoSpinnerComponent, ImgFallbackDirective],
  templateUrl: './magazine.html',
  styleUrl: './magazine.scss'
})
export class Magazine implements OnInit {
  private newsService = inject(NewsService);

  articles = signal<NewsPost[]>([]);
  loading = signal(true);
  selectedArticle = signal<NewsPost | null>(null);

  ngOnInit() {
    this.newsService.getNews('Magazine').subscribe({
      next: (data) => {
        this.articles.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  selectArticle(article: NewsPost) {
    this.selectedArticle.set(article);
    setTimeout(() => {
      document.getElementById('article-detail')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }, 100);
  }
}
