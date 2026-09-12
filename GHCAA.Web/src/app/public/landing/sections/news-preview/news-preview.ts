import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppDatePipe } from '../../../../core/pipes/app-date.pipe';
import { RouterLink } from '@angular/router';
import { NewsService } from '../../../../core/services/news.service';
import { NewsPost } from '../../../../core/models/business.models';

@Component({
    selector: 'landing-news',
    standalone: true,
    imports: [CommonModule, AppDatePipe, RouterLink],
    templateUrl: './news-preview.html',
    styleUrl: './news-preview.scss',
})
export class LandingNewsPreview implements OnInit {
    private newsService = inject(NewsService);
    news = signal<NewsPost[]>([]);
    isVisible = signal(true);

    ngOnInit() {
        this.newsService.getNews(undefined, true).subscribe({
            // 58.7: hide the whole section when there's nothing to show, not just on error —
            // an empty landing section previously still rendered its "no announcements" placeholder.
            next: (data) => {
                const items = data.slice(0, 3);
                this.news.set(items);
                this.isVisible.set(items.length > 0);
            },
            error: () => {
                this.news.set([]);
                this.isVisible.set(false);
            }
        });
    }
}

