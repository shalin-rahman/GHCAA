import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { ArchiveCollection, ArchiveItem } from '../models/archive.models';

@Injectable({ providedIn: 'root' })
export class ArchiveService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.ARCHIVE;

    getPublicCollections(search?: string): Observable<ArchiveCollection[]> {
        let params = new HttpParams();
        if (search?.trim()) params = params.set('search', search.trim());
        return this.http.get<ArchiveCollection[]>(`${this.apiUrl}/public`, { params });
    }

    getPublicItem(id: number): Observable<ArchiveItem> {
        return this.http.get<ArchiveItem>(`${this.apiUrl}/items/${id}`);
    }
}
