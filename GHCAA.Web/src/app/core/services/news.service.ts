import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface NewsPost {
    id: number;
    title: string;
    content: string;
    category: string;
    imageUrl?: string;
    isActive: boolean;
    authorName: string;
    createdAt: string;
}

@Injectable({
    providedIn: 'root'
})
export class NewsService {
    private http = inject(HttpClient);

    getNews(): Observable<NewsPost[]> {
        return this.http.get<NewsPost[]>('/api/news');
    }

    getPost(id: number): Observable<NewsPost> {
        return this.http.get<NewsPost>(`/api/news/${id}`);
    }

    // Admin methods
    getAdminNews(): Observable<NewsPost[]> {
        return this.http.get<NewsPost[]>('/api/news/admin');
    }

    createNews(dto: any): Observable<NewsPost> {
        return this.http.post<NewsPost>('/api/news', dto);
    }

    updateNews(id: number, dto: any): Observable<NewsPost> {
        return this.http.put<NewsPost>(`/api/news/${id}`, dto);
    }

    deleteNews(id: number): Observable<any> {
        return this.http.delete(`/api/news/${id}`);
    }
}
