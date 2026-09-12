import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { CommonModule } from '@angular/common';
import { AppDatePipe } from '../../core/pipes/app-date.pipe';
import { FormsModule } from '@angular/forms';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { firstValueFrom } from 'rxjs';
import { GalleryService } from '../../core/services/gallery.service';
import { EventGallery, EventPhoto } from '../../core/models/business.models';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { toWireDate, toDisplayDate } from '../../core/utils/date.util';

@Component({
    selector: 'app-admin-gallery',
    standalone: true,
    imports: [CommonModule, AppDatePipe, FormsModule, PageHeaderComponent, SearchBarComponent, ImgFallbackDirective, LoadingPanelComponent],
    templateUrl: './admin-gallery.html',
    styleUrl: './admin-gallery.scss'
})
export class AdminGallery implements OnInit {
    private galleryService = inject(GalleryService);
    private notify = inject(NotificationService);
    private confirmDialog = inject(ConfirmDialogService);

    galleries = signal<EventGallery[]>([]);
    selectedGallery = signal<EventGallery | null>(null);
    loading = signal(true);
    isSubmitting = signal(false);
    isUploading = signal(false);
    uploadedFiles = signal<File[]>([]);
    searchQuery = signal('');
    // 54.1: table is the default view; grid remains available for cover-photo browsing.
    viewMode = signal<'table' | 'grid'>('table');

    getImageUrl(path: string | null | undefined): string {
        if (!path) return '';
        // If it already has a leading slash, don't add another one
        // If it starts with http, return as is
        if (path.startsWith('http')) return path;
        const cleanPath = path.startsWith('/') ? path : '/' + path;
        // Ensure we don't have double slashes at the start which browser treats as protocol-relative
        return cleanPath.replace(/^\/\//, '/');
    }

    filteredGalleries = computed(() => {
        const q = this.searchQuery().toLowerCase().trim();
        if (!q) return this.galleries();
        return this.galleries().filter(g =>
            (g.title || '').toLowerCase().includes(q) ||
            (g.location || '').toLowerCase().includes(q) ||
            (g.description || '').toLowerCase().includes(q)
        );
    });

    // Form State
    showForm = signal(false);
    editingId = signal<number | null>(null);
    formatDateToDMY(d: any) {
        return toDisplayDate(d);
    }

    newGallery = {
        title: '',
        description: '',
        eventDate: this.formatDateToDMY(new Date()),
        location: ''
    };

    ngOnInit() {
        this.loadGalleries();
    }

    loadGalleries() {
        this.loading.set(true);
        this.galleryService.getAllGalleries().subscribe({
            next: (data: any[]) => {
                // Robust mapping for case-insensitive property access
                const mapped = (data || []).map((g: any) => {
                    const result: any = { ...g };
                    // Handle photos casing
                    const photos = g.photos || g.Photos || [];
                    result.photos = photos.map((p: any) => ({
                        id: p.id || p.Id,
                        photoPath: p.photoPath || p.PhotoPath,
                        uploadedAt: p.uploadedAt || p.UploadedAt
                    }));
                    return result;
                });
                this.galleries.set(mapped);
                this.loading.set(false);
            },
            error: () => {
                this.notify.error('Failed to load galleries');
                this.loading.set(false);
            }
        });
    }

    toggleForm() {
        this.showForm.set(!this.showForm());
        if (!this.showForm()) {
            this.resetForm();
        }
    }

    openEditForm(gallery: EventGallery) {
        this.editingId.set(gallery.id);
        this.newGallery = {
            title: gallery.title,
            description: gallery.description || '',
            eventDate: this.formatDateToDMY(gallery.eventDate),
            location: gallery.location || ''
        };
        this.showForm.set(true);
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    resetForm() {
        this.editingId.set(null);
        this.newGallery = {
            title: '',
            description: '',
            eventDate: this.formatDateToDMY(new Date()),
            location: ''
        };
    }

    onSubmit(form: any) {
        if (form.invalid) {
            Object.values(form.controls).forEach((control: any) => control.markAsTouched());
            this.notify.error('Title and Date are required assets.');
            return;
        }

        const editId = this.editingId();
        this.isSubmitting.set(true);

        const payload = { ...this.newGallery, eventDate: toWireDate(this.newGallery.eventDate) };
        const request = editId
            ? this.galleryService.updateGallery(editId, payload)
            : this.galleryService.createGallery(payload);

        request.subscribe({
            next: () => {
                this.notify.success(editId ? 'Gallery updated successfully' : 'Gallery created successfully');
                this.showForm.set(false);
                this.isSubmitting.set(false);
                this.loadGalleries();
                this.resetForm();
            },
            error: () => {
                this.notify.error(editId ? 'Failed to update gallery' : 'Failed to create gallery');
                this.isSubmitting.set(false);
            }
        });
    }

    toggleActive(id: number) {
        this.galleryService.toggleActive(id).subscribe({
            next: (res) => {
                this.notify.success(`Gallery is now ${res.isActive ? 'Active' : 'Hidden'}`);
                this.loadGalleries();
            },
            error: () => this.notify.error('Failed to toggle status')
        });
    }

    toggleFeatured(id: number) {
        this.galleryService.toggleFeatured(id).subscribe({
            next: (res) => {
                this.notify.success(`Gallery is now ${res.isFeatured ? 'Featured' : 'Regular'}`);
                this.loadGalleries();
            },
            error: () => this.notify.error('Failed to toggle featured status')
        });
    }

    async deleteGallery(id: number) {
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Delete gallery',
            message: 'Are you sure you want to delete this gallery? All photos will be unlinked.',
            confirmLabel: 'Delete',
            danger: true
        }));
        if (!ok) return;

        this.galleryService.deleteGallery(id).subscribe({
            next: () => {
                this.notify.success('Gallery deleted');
                this.loadGalleries();
            },
            error: () => this.notify.error('Failed to delete gallery')
        });
    }

    selectGallery(gallery: EventGallery) {
        this.selectedGallery.set(gallery);
        this.uploadedFiles.set([]);
    }

    onPhotosSelected(event: any) {
        const files: FileList = event.target.files;
        if (files) {
            this.uploadedFiles.set(Array.from(files));
        }
    }

    async uploadSelectedPhotos() {
        const gallery = this.selectedGallery();
        const files = this.uploadedFiles();
        if (!gallery || files.length === 0) return;

        this.isUploading.set(true);
        const paths: string[] = [];

        try {
            for (const file of files) {
                const res = await firstValueFrom(this.galleryService.uploadPhoto(file));
                if (res?.path) paths.push(res.path);
            }

            if (paths.length > 0) {
                await firstValueFrom(this.galleryService.addPhotos(gallery.id, paths));
                this.notify.success(`🚀 ${paths.length} Photo(s) uploaded successfully!`);
                
                // Refresh data from server
                this.galleryService.getAllGalleries().subscribe({
                    // 29F.2: surface HTTP failures instead of failing silently
                    next: all => {
                        this.galleries.set(all);
                        const fresh = all.find(g => g.id === gallery.id);
                        if (fresh) this.selectedGallery.set(fresh);
                    },
                    error: () => this.notify.error('Failed to refresh gallery.')
                });
            }
        } catch (error) {
            this.notify.error('Error uploading photos');
        } finally {
            this.isUploading.set(false);
            this.uploadedFiles.set([]);
        }
    }

    async removePhoto(photoId: number) {
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Remove photo',
            message: 'Remove this photo from the library?',
            confirmLabel: 'Remove',
            danger: true
        }));
        if (!ok) return;

        this.galleryService.removePhoto(photoId).subscribe({
            next: () => {
                // Instantly update the UI by filtering out the removed photo
                const currentGallery = this.selectedGallery();
                if (currentGallery) {
                    const updatedPhotos = (currentGallery.photos || []).filter((p: EventPhoto) => p.id !== photoId);
                    this.selectedGallery.set({ ...currentGallery, photos: updatedPhotos });
                }

                // Also refresh the background galleries list
                this.loadGalleries();
            },
            error: () => this.notify.error('Failed to remove photo')
        });
    }

    closeDetail() {
        this.selectedGallery.set(null);
    }
}
