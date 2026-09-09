import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { EventGallery, EventPhoto } from '../models/business.models';
import { buildHttpParams, getSilentHeaders } from '../utils/http.util';


@Injectable({
    providedIn: 'root'
})
export class GalleryService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.GALLERY;

    getGalleries(silent: boolean = false): Observable<EventGallery[]> {
        const headers = getSilentHeaders(silent);
        return this.http.get<EventGallery[]>(this.apiUrl, { headers });
    }

    getAllGalleries(): Observable<EventGallery[]> {
        return this.http.get<EventGallery[]>(`${this.apiUrl}/all`);
    }

    getGallery(id: number): Observable<EventGallery> {
        return this.http.get<EventGallery>(`${this.apiUrl}/${id}`);
    }

    // Admin Methods
    toggleActive(id: number): Observable<{ isActive: boolean }> {
        return this.http.patch<{ isActive: boolean }>(`${this.apiUrl}/admin/${id}/toggle-active`, {});
    }

    toggleFeatured(id: number): Observable<{ isFeatured: boolean }> {
        return this.http.patch<{ isFeatured: boolean }>(`${this.apiUrl}/admin/${id}/toggle-featured`, {});
    }

    uploadPhoto(file: File): Observable<{ path: string }> {
        const formData = new FormData();
        formData.append('file', file);
        return this.http.post<{ path: string }>(`${this.apiUrl}/upload-photo`, formData);
    }

    createGallery(gallery: any): Observable<EventGallery> {
        return this.http.post<EventGallery>(`${this.apiUrl}/admin`, gallery);
    }

    updateGallery(id: number, gallery: any): Observable<EventGallery> {
        return this.http.put<EventGallery>(`${this.apiUrl}/admin/${id}`, gallery);
    }

    addPhotos(galleryId: number, photoPaths: string[]): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/admin/${galleryId}/photos`, photoPaths);
    }

    deleteGallery(id: number): Observable<boolean> {
        return this.http.delete<boolean>(`${this.apiUrl}/admin/${id}`);
    }

    removePhoto(photoId: number): Observable<boolean> {
        return this.http.delete<boolean>(`${this.apiUrl}/admin/photos/${photoId}`);
    }

    submitMemory(memory: any): Observable<any> {
        const formData = new FormData();
        formData.append('title', memory.title);
        formData.append('description', memory.description);
        if (memory.photo) {
            formData.append('photo', memory.photo);
        }
        return this.http.post(`${this.apiUrl}`, formData);
    }

    // Member album management
    createAlbum(title: string, description?: string): Observable<EventGallery> {
        return this.http.post<EventGallery>(`${this.apiUrl}/albums`, { title, description });
    }

    getMyAlbums(): Observable<EventGallery[]> {
        return this.http.get<EventGallery[]>(`${this.apiUrl}/albums/mine`);
    }

    addPhotoToAlbum(albumId: number, file: File, caption?: string): Observable<EventPhoto> {
        const formData = new FormData();
        formData.append('file', file);
        if (caption) {
            formData.append('caption', caption);
        }
        return this.http.post<EventPhoto>(`${this.apiUrl}/albums/${albumId}/photos`, formData);
    }

    // Admin approval workflow
    getPendingApprovals(): Observable<{ galleries: EventGallery[], photos: EventPhoto[] }> {
        return this.http.get<{ galleries: EventGallery[], photos: EventPhoto[] }>(`${this.apiUrl}/admin/pending`);
    }

    approveGallery(id: number, notifyMember = true): Observable<void> {
        const params = buildHttpParams({ notifyMember });
        return this.http.post<void>(`${this.apiUrl}/admin/${id}/approve`, {}, { params });
    }

    rejectGallery(id: number, reason: string, notifyMember = true): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/admin/${id}/reject`, { reason, notifyMember });
    }

    approvePhoto(photoId: number, notifyMember = true): Observable<void> {
        const params = buildHttpParams({ notifyMember });
        return this.http.post<void>(`${this.apiUrl}/photos/${photoId}/approve`, {}, { params });
    }

    rejectPhoto(photoId: number, reason: string, notifyMember = true): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/photos/${photoId}/reject`, { reason, notifyMember });
    }
}
