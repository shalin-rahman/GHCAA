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
}
