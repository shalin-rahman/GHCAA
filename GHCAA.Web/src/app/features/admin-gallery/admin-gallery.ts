import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GalleryService, EventGallery } from '../../core/services/gallery.service';

@Component({
    selector: 'app-admin-gallery',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './admin-gallery.html',
    styleUrl: './admin-gallery.scss'
})
export class AdminGallery implements OnInit {
    private galleryService = inject(GalleryService);

    galleries = signal<EventGallery[]>([]);
    loading = signal(true);
    isSubmitting = signal(false);

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
        if (!this.newGallery.title) return;

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
}
