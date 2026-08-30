import { Component, inject, signal, computed, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { GalleryService } from '../../../../core/services/gallery.service';
import { EventGallery, EventPhoto } from '../../../../core/models/business.models';
import { ImgFallbackDirective } from '../../../../common/directives/img-fallback.directive';

const SLIDE_INTERVAL_MS = 3000;
const MAX_ALBUMS = 8;

@Component({
    selector: 'landing-gallery-preview',
    standalone: true,
    imports: [CommonModule, RouterLink, ImgFallbackDirective],
    templateUrl: './gallery-preview.html',
    styleUrl: './gallery-preview.scss'
})
export class LandingGalleryPreview implements OnInit, OnDestroy {
    private galleryService = inject(GalleryService);
    galleries = signal<EventGallery[]>([]);
    isVisible = signal(true);

    // Every active album with at least one photo gets its own box — not just featured ones.
    albums = computed(() => this.galleries().filter(g => g.photos?.length).slice(0, MAX_ALBUMS));

    // Per-album slide position, advanced on a single shared timer so each album box cycles
    // through its own photos one after another instead of showing a single static cover.
    private slideIndex = signal<Record<number, number>>({});
    private timer?: ReturnType<typeof setInterval>;

    ngOnInit() {
        this.galleryService.getGalleries(true).subscribe({
            next: (data) => {
                this.galleries.set(data);
                // Only start cycling once there's at least one multi-photo album to cycle —
                // avoids a timer ticking forever (and scheduling change detection every tick)
                // on an empty/error/single-photo landing page.
                if (this.albums().some(g => g.photos.length > 1)) {
                    this.startCycling();
                }
            },
            error: () => {
                this.galleries.set([]);
                this.isVisible.set(false);
            }
        });
    }

    ngOnDestroy() {
        if (this.timer) clearInterval(this.timer);
    }

    private startCycling() {
        this.timer = setInterval(() => {
            const prev = this.slideIndex();
            const next: Record<number, number> = { ...prev };
            let changed = false;
            for (const g of this.albums()) {
                const count = g.photos.length;
                if (count > 1) {
                    next[g.id] = ((prev[g.id] ?? 0) + 1) % count;
                    changed = true;
                }
            }
            if (changed) this.slideIndex.set(next);
        }, SLIDE_INTERVAL_MS);
    }

    currentPhoto(g: EventGallery): EventPhoto | undefined {
        if (!g.photos.length) return undefined;
        const index = this.slideIndex()[g.id] ?? 0;
        return g.photos[index % g.photos.length];
    }

    viewFull(path: string) {
        window.open(path, '_blank', 'noopener');
    }
}
