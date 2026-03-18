import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { NewsPost, CreateNewsDto, UpdateNewsDto } from '../models/business.models';


@Injectable({
    providedIn: 'root'
})
export class NewsService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.NEWS;

    getNews(category?: string, silent: boolean = false): Observable<NewsPost[]> {
        let url = this.apiUrl;
        if (category) url += `?category=${category}`;
        const headers = silent ? new HttpHeaders().set('X-Skip-Error-Notify', 'true') : undefined;
        return this.http.get<NewsPost[]>(url, { headers });
    }

    getNewsById(id: number): Observable<NewsPost> {
        return this.http.get<NewsPost>(`${this.apiUrl}/${id}`);
    }

    // Admin methods
    getNewsAdmin(): Observable<NewsPost[]> {
        return this.http.get<NewsPost[]>(`${this.apiUrl}/admin`);
    }

    createNews(dto: CreateNewsDto): Observable<NewsPost> {
        return this.http.post<NewsPost>(this.apiUrl, dto);
    }

    updateNews(id: number, dto: UpdateNewsDto): Observable<NewsPost> {
        return this.http.put<NewsPost>(`${this.apiUrl}/${id}`, dto);
    }

    deleteNews(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/${id}`);
    }

    uploadImage(file: File): Observable<{ url: string, relativePath: string }> {
        const formData = new FormData();
        formData.append('file', file);
        return this.http.post<{ url: string, relativePath: string }>(`${this.apiUrl}/upload-image`, formData);
    }

    // Member Submission Methods
    getMySubmissions(): Observable<NewsPost[]> {
        return this.http.get<NewsPost[]>(`${this.apiUrl}/my-submissions`);
    }

    submitArticle(dto: any): Observable<NewsPost> {
        return this.http.post<NewsPost>(`${this.apiUrl}/submit`, dto);
    }

    saveDraft(dto: any): Observable<NewsPost> {
        return this.http.post<NewsPost>(`${this.apiUrl}/submit`, { ...dto, status: 'Draft' });
    }


    // Admin Approval
    getPendingSubmissions(): Observable<NewsPost[]> {
        return this.http.get<NewsPost[]>(`${this.apiUrl}/pending`);
    }

    approveSubmission(id: number): Observable<any> {
        return this.http.post(`${this.apiUrl}/${id}/approve`, {});
    }

    rejectSubmission(id: number, reason: string): Observable<any> {
        return this.http.post(`${this.apiUrl}/${id}/reject`, { reason });
    }
}
