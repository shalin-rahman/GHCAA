import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GalleryService } from '../../core/services/gallery.service';
import { EventGallery } from '../../core/models/business.models';
import { NotificationService } from '../../core/services/notification.service';
import { AuthService } from '../../core/services/auth.service';
import { FormsModule } from '@angular/forms';
import { validateUploadFile } from '../../core/utils/file-validation.util';
import { LogoSpinnerComponent } from '../logo-spinner/logo-spinner';
import { ImgFallbackDirective } from '../directives/img-fallback.directive';
import { safeImageUrl } from '../../core/utils/image.util';
import { SUBMISSION_STATUS_MAP } from '../../core/constants/app.constants';

@Component({
  selector: 'app-gallery',
  standalone: true,
  imports: [CommonModule, FormsModule, LogoSpinnerComponent, ImgFallbackDirective],
  templateUrl: './gallery.html',
  styleUrl: './gallery.scss'
})
export class Gallery implements OnInit {
  private galleryService = inject(GalleryService);
  private notify = inject(NotificationService);
  auth = inject(AuthService);

  galleries = signal<EventGallery[]>([]);
  loading = signal(true);
  selectedGallery = signal<EventGallery | null>(null);
  
  showUpload = signal(false);
  submitting = signal(false);
  newMemory = {
    title: '',
    description: '',
    photo: null as File | null
  };
  // 29D.6: object-URL preview of the chosen file (was an empty <img src=""> firing a
  // spurious request to the current page URL).
  photoPreview = signal<string | null>(null);

  // My Albums (member-owned)
  myAlbums = signal<EventGallery[]>([]);
  loadingAlbums = signal(false);
  showAlbums = signal(false);
  showNewAlbum = signal(false);
  creatingAlbum = signal(false);
  newAlbum = { title: '', description: '' };
  addingPhotoToAlbumId = signal<number | null>(null);
  albumPhotoCaption = '';
  albumPhotoFile: File | null = null;

  constructor() {
    // Gated on authChecked() rather than a one-shot isAuthenticated() read in ngOnInit: the
    // /auth/me session restore is deferred (see AuthService), so on a fresh page load a member's
    // "My Albums" would otherwise never fetch — ngOnInit runs before the restore resolves and
    // nothing re-checks afterward. authChecked() is already true on construction whenever a
    // cached session was found, so the common case still loads immediately.
    this.auth.whenAuthenticated(() => this.loadMyAlbums());
  }

  ngOnInit() {
    this.refresh();
  }

  loadMyAlbums() {
    this.loadingAlbums.set(true);
    this.galleryService.getMyAlbums().subscribe({
      next: (data) => {
        this.myAlbums.set(data);
        this.loadingAlbums.set(false);
      },
      error: () => this.loadingAlbums.set(false)
    });
  }

  getStatusInfo(status: any) {
    return SUBMISSION_STATUS_MAP[status] || { label: 'Unknown', class: 'pending' };
  }

  createAlbum() {
    if (!this.newAlbum.title) {
      this.notify.error('Album title is required.');
      return;
    }
    this.creatingAlbum.set(true);
    this.galleryService.createAlbum(this.newAlbum.title, this.newAlbum.description).subscribe({
      next: () => {
        this.notify.success('Album created! Awaiting moderation.');
        this.creatingAlbum.set(false);
        this.showNewAlbum.set(false);
        this.newAlbum = { title: '', description: '' };
        this.loadMyAlbums();
      },
      error: () => {
        this.creatingAlbum.set(false);
        this.notify.error('Failed to create album.');
      }
    });
  }

  openAddPhoto(albumId: number) {
    this.addingPhotoToAlbumId.set(albumId);
    this.albumPhotoCaption = '';
    this.albumPhotoFile = null;
  }

  cancelAddPhoto() {
    this.addingPhotoToAlbumId.set(null);
    this.albumPhotoCaption = '';
    this.albumPhotoFile = null;
  }

  onAlbumFileSelected(e: any) {
    const file: File = e.target.files[0];
    if (!file) return;
    const err = validateUploadFile(file, 'image');
    if (err) { this.notify.error(err); e.target.value = ''; return; }
    this.albumPhotoFile = file;
  }

  addPhotoToAlbum(albumId: number) {
    if (!this.albumPhotoFile) {
      this.notify.error('Select a photo to add.');
      return;
    }
    this.galleryService.addPhotoToAlbum(albumId, this.albumPhotoFile, this.albumPhotoCaption).subscribe({
      next: () => {
        this.notify.success('Photo added! Awaiting moderation.');
        this.cancelAddPhoto();
        this.loadMyAlbums();
      },
      error: () => this.notify.error('Failed to add photo.')
    });
  }

  refresh() {
    this.loading.set(true);
    this.galleryService.getGalleries().subscribe({
      next: (data) => {
        this.galleries.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onFileSelected(e: any) {
    const file: File = e.target.files[0];
    if (!file) return;
    const err = validateUploadFile(file, 'image');
    if (err) { this.notify.error(err); e.target.value = ''; return; }
    this.newMemory.photo = file;
    this.setPreview(file);
  }

  private setPreview(file: File | null) {
    const prev = this.photoPreview();
    if (prev) URL.revokeObjectURL(prev);
    this.photoPreview.set(file ? URL.createObjectURL(file) : null);
  }

  clearPhoto() {
    this.newMemory.photo = null;
    this.setPreview(null);
  }

  submitMemory() {
    if (!this.newMemory.title || !this.newMemory.photo) {
        this.notify.error('Title and Photo are mandatory for sharing memories.');
        return;
    }
    this.submitting.set(true);
    this.galleryService.submitMemory(this.newMemory).subscribe({
        next: () => {
            this.notify.success('Legacy moment shared! Awaiting historical moderation.');
            this.submitting.set(false);
            this.showUpload.set(false);
            this.newMemory = { title: '', description: '', photo: null };
            this.setPreview(null);
            this.refresh();
        },
        error: () => {
            this.submitting.set(false);
            this.notify.error('Failed to upload memory.');
        }
    });
  }

  viewFull(path: string) {
    window.open(this.validImg(path), '_blank', 'noopener,noreferrer');
  }

  // Runtime 404s (file missing on server / ephemeral disk) are handled centrally by the
  // `appImgFallback` directive (30.9) on the template's <img>; this guards the other case —
  // a truthy but non-URL value (bad seed/import data) that would otherwise pass straight
  // through and fire a request the server can never satisfy.
  validImg(path?: string | null): string {
    return safeImageUrl(path);
  }
}


