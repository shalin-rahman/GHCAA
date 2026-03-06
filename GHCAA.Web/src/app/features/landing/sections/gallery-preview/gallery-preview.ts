import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GalleryService, EventGallery } from '../../../../core/services/gallery.service';

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

    ngOnInit() {
        this.galleryService.getGalleries().subscribe({
            next: (data) => this.galleries.set(data),
            error: () => this.galleries.set([])
        });
    }

    getFeaturedPhotos() {
        const allPhotos: any[] = [];
        // Only show photos from featured albums in the portal preview
        const featuredGalleries = this.galleries().filter(g => g.isFeatured);

        featuredGalleries.forEach(g => {
            g.photos.slice(0, 2).forEach(p => {
                allPhotos.push({
                    ...p,
                    galleryTitle: g.title
                });
            });
        });
        return allPhotos.slice(0, 8);
    }
}
