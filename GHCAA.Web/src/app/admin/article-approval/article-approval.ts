import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NewsService, NewsPost } from '../../core/services/news.service';
import { NotificationService } from '../../core/services/notification.service';
import { ARTICLE_CATEGORIES, SUBMISSION_STATUS_MAP } from '../../core/constants/app.constants';

@Component({
  selector: 'app-article-approval',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './article-approval.html',
  styleUrl: './article-approval.scss'
})
export class ArticleApproval implements OnInit {
  private newsService = inject(NewsService);
  private notify = inject(NotificationService);

  pendingArticles = signal<NewsPost[]>([]);
  loading = signal(true);
  selectedArticle = signal<NewsPost | null>(null);
  rejectReason = signal('');
  isProcessing = signal(false);

  ngOnInit() {
    this.loadPending();
  }

  loadPending() {
    this.loading.set(true);
    this.newsService.getPendingSubmissions().subscribe({
      next: (data) => {
        this.pendingArticles.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.pendingArticles.set([]);
        this.loading.set(false);
      }
    });
  }

  viewArticle(article: NewsPost) {
    this.selectedArticle.set(article);
    this.rejectReason.set('');
  }

  approve() {
    const article = this.selectedArticle();
    if (!article) return;

    this.isProcessing.set(true);
    this.newsService.approveSubmission(article.id).subscribe({
      next: () => {
        this.notify.success('Article approved and published');
        this.selectedArticle.set(null);
        this.loadPending();
        this.isProcessing.set(false);
      },
      error: () => {
        this.notify.error('Approval failed');
        this.isProcessing.set(false);
      }
    });
  }

  reject() {
    const article = this.selectedArticle();
    if (!article || !this.rejectReason()) {
      this.notify.warning('Please provide a reason for rejection');
      return;
    }

    this.isProcessing.set(true);
    this.newsService.rejectSubmission(article.id, this.rejectReason()).subscribe({
      next: () => {
        this.notify.success('Article rejected');
        this.selectedArticle.set(null);
        this.loadPending();
        this.isProcessing.set(false);
      },
      error: () => {
        this.notify.error('Rejection failed');
        this.isProcessing.set(false);
      }
    });
  }
}
