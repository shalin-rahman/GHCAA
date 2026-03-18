import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GalleryService } from '../../../../core/services/gallery.service';
import { EventGallery, EventPhoto } from '../../../../core/models/business.models';

@Component({
    selector: 'landing-gallery-preview',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './gallery-preview.html',
    styleUrl: './gallery-preview.scss'
})
export class LandingGalleryPreview implements OnInit {
    private galleryService = inject(GalleryService);
    galleries = signal<EventGallery[]>([]);
    isVisible = signal(true);

    ngOnInit() {
        this.galleryService.getGalleries(true).subscribe({
            next: (data) => this.galleries.set(data),
            error: () => {
                this.galleries.set([]);
                this.isVisible.set(false);
            }
        });
    }

    getFeaturedPhotos() {
        const allPhotos: any[] = [];
        // Only show photos from featured albums in the portal preview
        const featuredGalleries = this.galleries().filter(g => g.isFeatured);

        featuredGalleries.forEach(g => {
            g.photos.slice(0, 2).forEach((p: EventPhoto) => {
                allPhotos.push({
                    ...p,
                    galleryTitle: g.title
                });
            });
        });
        return allPhotos.slice(0, 8);
    }


    viewFull(path: string) {
        window.open(path, '_blank');
    }
}


