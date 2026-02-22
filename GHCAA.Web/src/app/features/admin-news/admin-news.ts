import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NewsService, NewsPost } from '../../core/services/news.service';
import { NotificationService } from '../../core/services/notification.service';

export const NEWS_CATEGORIES = [
    { value: 0, label: 'General' },
    { value: 1, label: 'Events' },
    { value: 2, label: 'Achievements' },
    { value: 3, label: 'Announcements' },
    { value: 4, label: 'Obituaries' }
];

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

    categories = NEWS_CATEGORIES;
    newsList = signal<NewsPost[]>([]);
    loading = signal(true);
    showForm = signal(false);
    saving = signal(false);
    editingId = signal<number | null>(null);

    form: any = { title: '', content: '', category: 0, imageUrl: '', isActive: true };

    ngOnInit() { this.loadNews(); }

    loadNews() {
        this.loading.set(true);
        this.newsService.getAdminNews().subscribe({
            next: (data) => { this.newsList.set(data); this.loading.set(false); },
            error: () => this.loading.set(false)
        });
    }

    openForm() {
        this.editingId.set(null);
        this.form = { title: '', content: '', category: 0, imageUrl: '', isActive: true };
        this.showForm.set(true);
    }

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

    cancelForm() {
        this.showForm.set(false);
        this.editingId.set(null);
    }

    saveNews() {
        if (!this.form.title || !this.form.content) return;
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
        const c = this.categories.find(x => x.value === +cat);
        return c ? c.label : 'General';
    }
}
