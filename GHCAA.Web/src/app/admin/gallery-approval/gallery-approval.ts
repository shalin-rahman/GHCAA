import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';
import { GalleryService } from '../../core/services/gallery.service';
import { EventGallery, EventPhoto } from '../../core/models/business.models';
import { NotificationService } from '../../core/services/notification.service';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';

type PendingItem = { kind: 'album'; album: EventGallery } | { kind: 'photo'; photo: EventPhoto };

@Component({
  selector: 'app-gallery-approval',
  standalone: true,
  imports: [CommonModule, FormsModule, LogoSpinnerComponent, PageHeaderComponent, SearchBarComponent, ImgFallbackDirective],
  templateUrl: './gallery-approval.html',
  styleUrl: './gallery-approval.scss'
})
export class GalleryApproval implements OnInit {
  private galleryService = inject(GalleryService);
  private notify = inject(NotificationService);

  pendingAlbums = signal<EventGallery[]>([]);
  pendingPhotos = signal<EventPhoto[]>([]);
  loading = signal(true);
  selectedItem = signal<PendingItem | null>(null);
  rejectReason = signal('');
  isProcessing = signal(false);
  searchQuery = signal('');

  filteredAlbums = computed(() => {
    const q = this.searchQuery().toLowerCase().trim();
    if (!q) return this.pendingAlbums();
    return this.pendingAlbums().filter(a => (a.title || '').toLowerCase().includes(q));
  });

  filteredPhotos = computed(() => {
    const q = this.searchQuery().toLowerCase().trim();
    if (!q) return this.pendingPhotos();
    return this.pendingPhotos().filter(p => (p.caption || '').toLowerCase().includes(q));
  });

  totalPending = computed(() => this.pendingAlbums().length + this.pendingPhotos().length);

  ngOnInit() {
    this.loadPending();
  }

  loadPending() {
    this.loading.set(true);
    this.galleryService.getPendingApprovals().subscribe({
      next: (data) => {
        this.pendingAlbums.set(data.galleries || []);
        this.pendingPhotos.set(data.photos || []);
        this.loading.set(false);
      },
      error: () => {
        this.pendingAlbums.set([]);
        this.pendingPhotos.set([]);
        this.loading.set(false);
      }
    });
  }

  viewAlbum(album: EventGallery) {
    this.selectedItem.set({ kind: 'album', album });
    this.rejectReason.set('');
  }

  viewPhoto(photo: EventPhoto) {
    this.selectedItem.set({ kind: 'photo', photo });
    this.rejectReason.set('');
  }

  approve() {
    const item = this.selectedItem();
    if (!item) return;

    this.isProcessing.set(true);
    const obs = item.kind === 'album'
      ? this.galleryService.approveGallery(item.album.id)
      : this.galleryService.approvePhoto(item.photo.id);

    obs.subscribe({
      next: () => {
        this.notify.success(item.kind === 'album' ? 'Album approved' : 'Photo approved');
        this.selectedItem.set(null);
        this.loadPending();
        this.isProcessing.set(false);
      },
      error: () => {
        this.notify.error('Approval failed');
        this.isProcessing.set(false);
      }
    });
  }

  reject() {
    const item = this.selectedItem();
    if (!item || !this.rejectReason()) {
      this.notify.warning('Please provide a reason for rejection');
      return;
    }

    this.isProcessing.set(true);
    const obs = item.kind === 'album'
      ? this.galleryService.rejectGallery(item.album.id, this.rejectReason())
      : this.galleryService.rejectPhoto(item.photo.id, this.rejectReason());

    obs.subscribe({
      next: () => {
        this.notify.success(item.kind === 'album' ? 'Album rejected' : 'Photo rejected');
        this.selectedItem.set(null);
        this.loadPending();
        this.isProcessing.set(false);
      },
      error: () => {
        this.notify.error('Rejection failed');
        this.isProcessing.set(false);
      }
    });
  }

  validImg(path?: string | null): string {
    return path && (path.startsWith('/') || path.startsWith('http')) ? path : '/assets/logo.png';
  }
}
