import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NewsService, NewsPost } from '../../core/services/news.service';

@Component({
  selector: 'app-news',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './news.html',
  styleUrl: './news.scss'
})
export class News implements OnInit {
  private newsService = inject(NewsService);

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
