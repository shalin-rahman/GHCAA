import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GalleryService, EventGallery } from '../../core/services/gallery.service';

@Component({
  selector: 'app-gallery',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './gallery.html',
  styleUrl: './gallery.scss'
})
export class Gallery implements OnInit {
  private galleryService = inject(GalleryService);

  galleries = signal<EventGallery[]>([]);
  loading = signal(true);
  selectedGallery = signal<EventGallery | null>(null);

  ngOnInit() {
    this.galleryService.getGalleries().subscribe({
      next: (data) => {
        this.galleries.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  viewFull(path: string) {
    window.open(path, '_blank');
  }
}


