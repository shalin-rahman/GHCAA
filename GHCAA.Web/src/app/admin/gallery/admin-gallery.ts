import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { GalleryService } from '../../core/services/gallery.service';
import { EventGallery, EventPhoto } from '../../core/models/business.models';
import { NotificationService } from '../../core/services/notification.service';

@Component({
    selector: 'app-admin-gallery',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './admin-gallery.html',
    styleUrl: './admin-gallery.scss'
})
export class AdminGallery implements OnInit {
    private galleryService = inject(GalleryService);
    private notify = inject(NotificationService);

    galleries = signal<EventGallery[]>([]);
    selectedGallery = signal<EventGallery | null>(null);
    loading = signal(true);
    isSubmitting = signal(false);
    isUploading = signal(false);
    uploadedFiles = signal<File[]>([]);

    // Form State
    showForm = signal(false);
    newGallery = {
        title: '',
        description: '',
        eventDate: new Date().toISOString().split('T')[0],
        location: ''
    };

    ngOnInit() {
        this.loadGalleries();
    }

    loadGalleries() {
        this.loading.set(true);
        this.galleryService.getAllGalleries().subscribe({
            next: (data) => {
                this.galleries.set(data);
                this.loading.set(false);
            },
            error: () => {
                alert('Failed to load galleries');
                this.loading.set(false);
            }
        });
    }

    toggleForm() {
        this.showForm.set(!this.showForm());
    }

    onSubmit() {
        if (!this.newGallery.title || !this.newGallery.eventDate) {
            this.notify.error('Title and Date are required assets.');
            return;
        }

        this.isSubmitting.set(true);
        this.galleryService.createGallery(this.newGallery).subscribe({
            next: () => {
                alert('Gallery created successfully');
                this.showForm.set(false);
                this.isSubmitting.set(false);
                this.loadGalleries();
                // Reset form
                this.newGallery = {
                    title: '',
                    description: '',
                    eventDate: new Date().toISOString().split('T')[0],
                    location: ''
                };
            },
            error: () => {
                alert('Failed to create gallery');
                this.isSubmitting.set(false);
            }
        });
    }

    toggleActive(id: number) {
        this.galleryService.toggleActive(id).subscribe({
            next: (res) => {
                alert(`Gallery is now ${res.isActive ? 'Active' : 'Hidden'}`);
                this.loadGalleries();
            },
            error: () => alert('Failed to toggle status')
        });
    }

    toggleFeatured(id: number) {
        this.galleryService.toggleFeatured(id).subscribe({
            next: (res) => {
                alert(`Gallery is now ${res.isFeatured ? 'Featured' : 'Regular'}`);
                this.loadGalleries();
            },
            error: () => alert('Failed to toggle featured status')
        });
    }

    deleteGallery(id: number) {
        if (!confirm('Are you sure you want to delete this gallery? All photos will be unlinked.')) return;

        this.galleryService.deleteGallery(id).subscribe({
            next: () => {
                alert('Gallery deleted');
                this.loadGalleries();
            },
            error: () => alert('Failed to delete gallery')
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
                alert(`🚀 ${paths.length} Photo(s) uploaded successfully!`);
                
                // Refresh data from server
                this.galleryService.getAllGalleries().subscribe(all => {
                    this.galleries.set(all);
                    const fresh = all.find(g => g.id === gallery.id);
                    if (fresh) this.selectedGallery.set(fresh);
                });
            }
        } catch (error) {
            alert('Error uploading photos');
        } finally {
            this.isUploading.set(false);
            this.uploadedFiles.set([]);
        }
    }

    removePhoto(photoId: number) {
        if (!confirm('Remove this photo from the library?')) return;

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
            error: () => alert('Failed to remove photo')
        });
    }

    closeDetail() {
        this.selectedGallery.set(null);
    }
}


