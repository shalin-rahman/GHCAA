import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GalleryService } from '../../core/services/gallery.service';
import { EventGallery } from '../../core/models/business.models';
import { NotificationService } from '../../core/services/notification.service';
import { AuthService } from '../../core/services/auth.service';
import { FormsModule } from '@angular/forms';
import { validateUploadFile } from '../../core/utils/file-validation.util';

@Component({
  selector: 'app-gallery',
  standalone: true,
  imports: [CommonModule, FormsModule],
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

  ngOnInit() {
    this.refresh();
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
            this.refresh();
        },
        error: () => {
            this.submitting.set(false);
            this.notify.error('Failed to upload memory.');
        }
    });
  }

  viewFull(path: string) {
    window.open(path, '_blank', 'noopener,noreferrer');
  }
}


