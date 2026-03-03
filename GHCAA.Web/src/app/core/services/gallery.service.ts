import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api.endpoints';

export interface EventPhoto {
    id: number;
    photoPath: string;
    uploadedAt: string;
}

export interface EventGallery {
    id: number;
    title: string;
    description: string;
    eventDate: string;
    location: string;
    photos: EventPhoto[];
}

@Injectable({
    providedIn: 'root'
})
export class GalleryService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.GALLERY;

    getGalleries(): Observable<EventGallery[]> {
        return this.http.get<EventGallery[]>(this.apiUrl);
    }

    getGallery(id: number): Observable<EventGallery> {
        return this.http.get<EventGallery>(`${this.apiUrl}/${id}`);
    }

    // Admin Methods
    uploadPhoto(file: File): Observable<{ path: string }> {
        const formData = new FormData();
        formData.append('file', file);
        return this.http.post<{ path: string }>(`${this.apiUrl}/upload-photo`, formData);
    }

    createGallery(gallery: any): Observable<EventGallery> {
        return this.http.post<EventGallery>(`${this.apiUrl}/admin`, gallery);
    }

    addPhotos(galleryId: number, photoPaths: string[]): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/admin/${galleryId}/photos`, photoPaths);
    }

    deleteGallery(id: number): Observable<boolean> {
        return this.http.delete<boolean>(`${this.apiUrl}/admin/${id}`);
    }
}
