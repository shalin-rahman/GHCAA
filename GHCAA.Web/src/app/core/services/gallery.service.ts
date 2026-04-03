import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { EventGallery } from '../models/business.models';


@Injectable({
    providedIn: 'root'
})
export class GalleryService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.GALLERY;

    getGalleries(silent: boolean = false): Observable<EventGallery[]> {
        const headers = silent ? new HttpHeaders().set('X-Skip-Error-Notify', 'true') : undefined;
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
}
