import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppDatePipe } from '../../core/pipes/app-date.pipe';
import { FormsModule } from '@angular/forms';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';
import { NewsService } from '../../core/services/news.service';
import { NewsPost, PostType } from '../../core/models/business.models';
import { validateUploadFile } from '../../core/utils/file-validation.util';
import { firstValueFrom } from 'rxjs';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { ARTICLE_CATEGORIES, getArticleCategoryLabel, POST_TYPE_TABS, matchesPostType, SUBMISSION_STATUS_MAP } from '../../core/constants/app.constants';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';

@Component({
    selector: 'app-admin-news',
    standalone: true,
    imports: [CommonModule, AppDatePipe, FormsModule, LoadingPanelComponent, LogoSpinnerComponent, PageHeaderComponent, SearchBarComponent, ImgFallbackDirective],
    templateUrl: './admin-news.html',
    styleUrl: './admin-news.scss'
})
export class AdminNews implements OnInit {
    private newsService = inject(NewsService);
    private notify = inject(NotificationService);
    private confirmDialog = inject(ConfirmDialogService);

    categories = ARTICLE_CATEGORIES;
    newsList = signal<NewsPost[]>([]);
    searchQuery = signal('');
    postTypeFilter = signal<'' | PostType>('');

    readonly postTypeTabs = POST_TYPE_TABS;

    filteredNews = computed(() => {
        const query = this.searchQuery().toLowerCase();
        const type = this.postTypeFilter();
        return this.newsList().filter(post => {
            if (!matchesPostType(post.postType, type)) return false;
            if (!query) return true;
            return post.title.toLowerCase().includes(query) ||
                getArticleCategoryLabel(post.articleCategory).toLowerCase().includes(query);
        });
    });

    loading = signal(true);
    showForm = signal(false);
    selectedPost = signal<NewsPost | null>(null);
    saving = signal(false);
    editingId = signal<number | null>(null);
    uploadingImage = signal(false);
    uploadingDocument = signal(false);
    stagedImageFile: File | null = null;
    stagedImageName = signal<string | null>(null);
    stagedDocumentFile: File | null = null;
    stagedDocumentName = signal<string | null>(null);

    form: any = { title: '', content: '', articleCategory: 'Regular', postType: 'News', imageUrl: '', attachmentUrl: '', attachmentFileName: '', isActive: true, status: 2, publishDate: this.toDateInputValue(new Date()), collaborators: [] };

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
        this.form = { title: '', content: '', articleCategory: 'Regular', postType: 'News', imageUrl: '', attachmentUrl: '', attachmentFileName: '', isActive: true, status: 2, publishDate: this.toDateInputValue(new Date()), collaborators: [] };
        this.stagedImageFile = null;
        this.stagedImageName.set(null);
        this.stagedDocumentFile = null;
        this.stagedDocumentName.set(null);
        this.showForm.set(true);
    }

    editPost(post: NewsPost) {
        this.editingId.set(post.id);
        this.stagedImageFile = null;
        this.stagedImageName.set(null);
        this.stagedDocumentFile = null;
        this.stagedDocumentName.set(null);
        this.form = {
            title: post.title,
            content: post.content,
            articleCategory: post.articleCategory,
            postType: post.postType || 'News',
            imageUrl: post.imageUrl || '',
            attachmentUrl: post.attachmentUrl || '',
            attachmentFileName: post.attachmentFileName || '',
            isActive: post.isActive,
            status: post.status ?? 2,
            publishDate: this.toDateInputValue(post.createdAt),
            collaborators: post.collaborators || []
        };
        this.showForm.set(true);
        window.scrollTo(0, 0);
    }

    onImageSelect(event: Event) {
        // Stage the file locally only; the actual upload happens on Save/Update
        // (saveNews) so an abandoned/cancelled form never leaves an orphaned
        // image on the server.
        const input = event.target as HTMLInputElement;
        const file = input.files?.[0];
        if (!file) return;

        const error = validateUploadFile(file, 'image');
        if (error) {
            this.notify.error(error);
            input.value = '';
            return;
        }

        this.stagedImageFile = file;
        this.stagedImageName.set(file.name);
    }

    onDocumentSelect(event: Event) {
        // Stage only, same convention as the image field — uploading immediately (the previous
        // behavior) orphaned the file on the server if the form was then cancelled instead of saved.
        const input = event.target as HTMLInputElement;
        const file = input.files?.[0];
        if (!file) return;

        const error = validateUploadFile(file, 'pdf');
        if (error) {
            this.notify.error(error);
            input.value = '';
            return;
        }

        this.stagedDocumentFile = file;
        this.stagedDocumentName.set(file.name);
    }

    removeDocument() {
        this.form.attachmentUrl = '';
        this.form.attachmentFileName = '';
        this.stagedDocumentFile = null;
        this.stagedDocumentName.set(null);
    }

    cancelForm() {
        this.showForm.set(false);
        this.editingId.set(null);
        this.stagedImageFile = null;
        this.stagedImageName.set(null);
        this.stagedDocumentFile = null;
        this.stagedDocumentName.set(null);
    }

    saveNews(form: any) {
        if (form.invalid) {
            form.control.markAllAsTouched();
            this.notify.error('Please complete all mandatory fields correctly.');
            return;
        }

        if (this.saving()) return;

        this.saving.set(true);
        this.uploadStagedImageThenDocument();
    }

    private uploadStagedImageThenDocument() {
        const stagedImage = this.stagedImageFile;
        if (stagedImage) {
            this.uploadingImage.set(true);
            this.newsService.uploadImage(stagedImage).subscribe({
                next: (res) => {
                    this.form.imageUrl = res.url;
                    this.uploadingImage.set(false);
                    this.stagedImageFile = null;
                    this.stagedImageName.set(null);
                    this.uploadStagedDocumentThenSubmit();
                },
                error: () => {
                    this.uploadingImage.set(false);
                    this.saving.set(false);
                    this.notify.error('Image upload failed.');
                }
            });
            return;
        }

        this.uploadStagedDocumentThenSubmit();
    }

    private uploadStagedDocumentThenSubmit() {
        const stagedDocument = this.stagedDocumentFile;
        if (stagedDocument) {
            this.uploadingDocument.set(true);
            this.newsService.uploadDocument(stagedDocument).subscribe({
                next: (res) => {
                    this.form.attachmentUrl = res.url;
                    this.form.attachmentFileName = res.fileName;
                    this.uploadingDocument.set(false);
                    this.stagedDocumentFile = null;
                    this.stagedDocumentName.set(null);
                    this.submitNews();
                },
                error: () => {
                    this.uploadingDocument.set(false);
                    this.saving.set(false);
                    this.notify.error('Document upload failed.');
                }
            });
            return;
        }

        this.submitNews();
    }

    private submitNews() {
        const id = this.editingId();
        // The date input only carries a bare "yyyy-MM-dd" string with no timezone/time component;
        // PublishDate is a timestamptz column, so it must go over the wire as a real UTC ISO string
        // (same conversion admin-events.ts's toSafeISO does) or Npgsql rejects the save.
        const payload = { ...this.form, publishDate: this.toSafeISO(this.form.publishDate) };
        const obs = id ? this.newsService.updateNews(id, payload) : this.newsService.createNews(payload);

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

    async deletePost(id: number) {
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Delete post',
            message: 'Delete this post permanently?',
            confirmLabel: 'Delete',
            danger: true
        }));
        if (!ok) return;

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

    getStatusMeta(status: any): { label: string, class: string } {
        return SUBMISSION_STATUS_MAP[status] || { label: 'Unknown', class: 'pending' };
    }

    private toSafeISO(val: any): string | undefined {
        if (!val) return undefined;
        const d = new Date(val);
        return isNaN(d.getTime()) ? undefined : d.toISOString();
    }

    private toDateInputValue(date: string | Date): string {
        const d = new Date(date);
        if (isNaN(d.getTime())) return '';
        const pad = (n: number) => String(n).padStart(2, '0');
        return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
    }

    updateCollaborators(event: string) {
        this.form.collaborators = event.split(',')
            .map(s => s.trim())
            .filter(s => s.length > 0);
    }
}
