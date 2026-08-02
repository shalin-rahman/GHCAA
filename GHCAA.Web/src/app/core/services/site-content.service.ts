import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { SiteContent, UpsertSiteContentDto } from '../models/business.models';

@Injectable({ providedIn: 'root' })
export class SiteContentService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.SITE_CONTENT;

    getByGroup(group: string, silent: boolean = true): Observable<SiteContent[]> {
        const headers = silent ? new HttpHeaders().set('X-Skip-Error-Notify', 'true') : undefined;
        return this.http.get<SiteContent[]>(`${this.apiUrl}?group=${encodeURIComponent(group)}`, { headers });
    }

    getAll(): Observable<SiteContent[]> {
        return this.http.get<SiteContent[]>(`${this.apiUrl}/admin`);
    }

    create(dto: UpsertSiteContentDto): Observable<SiteContent> {
        return this.http.post<SiteContent>(this.apiUrl, dto);
    }

    update(id: number, dto: UpsertSiteContentDto): Observable<SiteContent> {
        return this.http.put<SiteContent>(`${this.apiUrl}/${id}`, dto);
    }

    delete(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
}
