import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NewsService } from '../../core/services/news.service';
import { NewsPost } from '../../core/models/business.models';
import { NotificationService } from '../../core/services/notification.service';

import { ARTICLE_CATEGORIES } from '../../core/constants/app.constants';


@Component({
    selector: 'app-admin-news',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './admin-news.html',
    styleUrl: './admin-news.scss'
})
export class AdminNews implements OnInit {
    private newsService = inject(NewsService);
    private notify = inject(NotificationService);

    categories = ARTICLE_CATEGORIES;

    newsList = signal<NewsPost[]>([]);
    loading = signal(true);
    showForm = signal(false);
    selectedPost = signal<NewsPost | null>(null);
    saving = signal(false);
    editingId = signal<number | null>(null);

    form: any = { title: '', content: '', category: 'Regular', imageUrl: '', isActive: true };


    ngOnInit() { this.loadNews(); }

    loadNews() {
        this.loading.set(true);
        this.newsService.getNewsAdmin().subscribe({
            next: (data: NewsPost[]) => { this.newsList.set(data); this.loading.set(false); },
            error: () => this.loading.set(false)
        });
    }

    openForm() {
        this.editingId.set(null);
        this.form = { title: '', content: '', category: 'Regular', imageUrl: '', isActive: true };

        this.showForm.set(true);
    }

    uploadingImage = signal(false);

    editPost(post: NewsPost) {
        this.editingId.set(post.id);
        this.form = {
            title: post.title,
            content: post.content,
            category: post.category,
            imageUrl: post.imageUrl || '',
            isActive: post.isActive
        };
        this.showForm.set(true);
        window.scrollTo(0, 0);
    }

    onImageSelect(event: Event) {
        const file = (event.target as HTMLInputElement).files?.[0];
        if (!file) return;

        this.uploadingImage.set(true);
        this.newsService.uploadImage(file).subscribe({
            next: (res) => {
                this.form.imageUrl = res.url;
                this.notify.success('Image uploaded successfully');
                this.uploadingImage.set(false);
            },
            error: () => {
                this.notify.error('Image upload failed');
                this.uploadingImage.set(false);
            }
        });
    }

    cancelForm() {
        this.showForm.set(false);
        this.editingId.set(null);
    }

    saveNews() {
        if (!this.form.title || !this.form.content) {
            this.notify.error('Please complete all mandatory fields.');
            return;
        }
        if (this.saving()) return;

        this.saving.set(true);
        const id = this.editingId();
        const obs = id ? this.newsService.updateNews(id, this.form) : this.newsService.createNews(this.form);
        obs.subscribe({
            next: () => {
                this.notify.success(id ? 'Post updated successfully.' : 'News post published!');
                this.saving.set(false);
                this.cancelForm();
                this.loadNews();
            },
            error: () => { this.saving.set(false); this.notify.error('Failed to save post.'); }
        });
    }

    deletePost(id: number) {
        if (!confirm('Delete this post permanently?')) return;
        this.newsService.deleteNews(id).subscribe({
            next: () => { this.notify.success('Post deleted.'); this.loadNews(); },
            error: () => this.notify.error('Failed to delete post.')
        });
    }

    getCategoryLabel(cat: any): string {
        const c = this.categories.find(x => x.value === cat);
        return c ? c.label : (cat || 'Article');
    }

}


