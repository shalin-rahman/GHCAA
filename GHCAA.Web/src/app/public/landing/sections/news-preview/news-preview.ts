import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { NewsService } from '../../../../core/services/news.service';
import { NewsPost } from '../../../../core/models/business.models';

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
    isVisible = signal(true);

    ngOnInit() {
        this.newsService.getNews(undefined, true).subscribe({
            next: (data) => this.news.set(data.slice(0, 3)),
            error: () => {
                this.news.set([]);
                this.isVisible.set(false);
            }
        });
    }
}


