import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NewsService } from '../../core/services/news.service';
import { NewsPost, SubmissionStatus } from '../../core/models/business.models';
import { NotificationService } from '../../core/services/notification.service';
import { ARTICLE_CATEGORIES, SUBMISSION_STATUS_MAP, SUBMISSION_STATUS } from '../../core/constants/app.constants';

@Component({
  selector: 'app-member-articles',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './articles.html',
  styleUrl: './articles.scss'
})
export class MemberArticles implements OnInit {
  private newsService = inject(NewsService);
  private notify = inject(NotificationService);

  mySubmissions = signal<NewsPost[]>([]);
  loading = signal(true);
  isSubmitting = signal(false);
  showForm = signal(false);

  // Form State
  articleForm: {
    id: number;
    title: string;
    content: string;
    articleCategory: any;
    imageUrl: string;
    status: SubmissionStatus;
  } = {
    id: 0,
    title: '',
    content: '',
    articleCategory: 'Regular',
    imageUrl: '',
    status: SUBMISSION_STATUS.DRAFT
  };

  categories = ARTICLE_CATEGORIES;
  photoPreview = signal<string | null>(null);
  private selectedFile: File | null = null;

  ngOnInit() {
    this.loadMySubmissions();
  }

  loadMySubmissions() {
    this.loading.set(true);
    this.newsService.getMySubmissions().subscribe({
      next: (data) => {
        this.mySubmissions.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.mySubmissions.set([]);
        this.loading.set(false);
      }
    });
  }

  resetForm() {
    this.articleForm = {
      id: 0,
      title: '',
      content: '',
      articleCategory: 'Regular',
      imageUrl: '',
      status: SUBMISSION_STATUS.DRAFT
    };
    this.photoPreview.set(null);
    this.selectedFile = null;
  }

  openCreate() {
    this.resetForm();
    this.showForm.set(true);
  }

  editArticle(article: NewsPost) {
    this.articleForm = {
      id: article.id,
      title: article.title,
      content: article.content,
      articleCategory: article.articleCategory,
      imageUrl: article.imageUrl || '',
      status: article.status
    };
    this.photoPreview.set(article.imageUrl || null);
    this.showForm.set(true);
  }


  onPhotoSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.selectedFile = file;
    const reader = new FileReader();
    reader.onload = (e) => this.photoPreview.set(e.target?.result as string);
    reader.readAsDataURL(file);
  }

  async save(status: SubmissionStatus) {
    if (!this.articleForm.title || !this.articleForm.content) {
      this.notify.warning('Title and content are required');
      return;
    }

    this.isSubmitting.set(true);
    this.articleForm.status = status;

    try {
      if (this.selectedFile) {
        const uploadRes = await this.newsService.uploadImage(this.selectedFile).toPromise();
        this.articleForm.imageUrl = uploadRes?.url || '';
      }

      const saveObs = status === SUBMISSION_STATUS.DRAFT 
        ? this.newsService.saveDraft(this.articleForm)
        : this.newsService.submitArticle(this.articleForm);


      saveObs.subscribe({
        next: () => {
          this.notify.success(status === SUBMISSION_STATUS.DRAFT ? 'Draft saved' : 'Article submitted for approval');
          this.showForm.set(false);
          this.loadMySubmissions();
          this.isSubmitting.set(false);
        },
        error: (err) => {
          this.notify.error(err?.error?.message || 'Failed to save article');
          this.isSubmitting.set(false);
        }
      });
    } catch (e) {
      this.notify.error('File upload failed');
      this.isSubmitting.set(false);
    }
  }

  getStatusInfo(status: any) {
    return SUBMISSION_STATUS_MAP[status] || { label: 'Unknown', class: 'pending' };
  }


  getCategoryLabel(val: string) {
    return this.categories.find(c => c.value === val)?.label || val;
  }
}
