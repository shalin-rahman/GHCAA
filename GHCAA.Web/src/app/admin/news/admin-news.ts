import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';
import { NewsService } from '../../core/services/news.service';
import { NewsPost } from '../../core/models/business.models';
import { NotificationService } from '../../core/services/notification.service';
import { ARTICLE_CATEGORIES, getArticleCategoryLabel } from '../../core/constants/app.constants';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';

@Component({
    selector: 'app-admin-news',
    standalone: true,
    imports: [CommonModule, FormsModule, LogoSpinnerComponent, PageHeaderComponent, SearchBarComponent, ImgFallbackDirective],
    templateUrl: './admin-news.html',
    styleUrl: './admin-news.scss'
})
export class AdminNews implements OnInit {
    private newsService = inject(NewsService);
    private notify = inject(NotificationService);

    categories = ARTICLE_CATEGORIES;
    newsList = signal<NewsPost[]>([]);
    searchQuery = signal('');
    
    filteredNews = computed(() => {
        const query = this.searchQuery().toLowerCase();
        if (!query) return this.newsList();
        
        return this.newsList().filter(post => 
            post.title.toLowerCase().includes(query) || 
            getArticleCategoryLabel(post.articleCategory).toLowerCase().includes(query)
        );
    });

    loading = signal(true);
    showForm = signal(false);
    selectedPost = signal<NewsPost | null>(null);
    saving = signal(false);
    editingId = signal<number | null>(null);
    uploadingImage = signal(false);

    form: any = { title: '', content: '', articleCategory: 'Regular', imageUrl: '', isActive: true, status: 2, collaborators: [] };

    ngOnInit() { 
        this.loadNews(); 
    }

    loadNews() {
        this.loading.set(true);
        this.newsService.getNewsAdmin().subscribe({
            next: (data: NewsPost[]) => { 
                this.newsList.set(data); 
                this.loading.set(false); 
            },
            error: () => this.loading.set(false)
        });
    }

    openForm() {
        this.editingId.set(null);
        this.form = { title: '', content: '', articleCategory: 'Regular', imageUrl: '', isActive: true, status: 2, collaborators: [] };
        this.showForm.set(true);
    }

    editPost(post: NewsPost) {
        this.editingId.set(post.id);
        this.form = {
            title: post.title,
            content: post.content,
            articleCategory: post.articleCategory,
            imageUrl: post.imageUrl || '',
            isActive: post.isActive,
            status: post.status ?? 2,
            collaborators: post.collaborators || []
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

    saveNews(form: any) {
        if (form.invalid) {
            form.control.markAllAsTouched();
            this.notify.error('Please complete all mandatory fields correctly.');
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
            error: () => { 
                this.saving.set(false); 
                this.notify.error('Failed to save post.'); 
            }
        });
    }

    deletePost(id: number) {
        if (!confirm('Delete this post permanently?')) return;
        this.newsService.deleteNews(id).subscribe({
            next: () => { 
                this.notify.success('Post deleted.'); 
                this.loadNews(); 
            },
            error: () => this.notify.error('Failed to delete post.')
        });
    }

    getCategoryLabel(cat: any): string {
        return getArticleCategoryLabel(cat);
    }

    updateCollaborators(event: string) {
        this.form.collaborators = event.split(',')
            .map(s => s.trim())
            .filter(s => s.length > 0);
    }
}
