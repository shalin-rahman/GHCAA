import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { NewsService, NewsPost } from '../../../../core/services/news.service';

@Component({
    selector: 'landing-news',
    standalone: true,
    imports: [CommonModule, RouterLink],
    templateUrl: './news-preview.html',
    styleUrl: './news-preview.scss',
})
export class LandingNewsPreview implements OnInit {
    private newsService = inject(NewsService);
    news = signal<NewsPost[]>([]);

    ngOnInit() {
        this.newsService.getNews().subscribe({
            next: (data) => this.news.set(data.slice(0, 3)),
            error: () => this.news.set([])
        });
    }
}


