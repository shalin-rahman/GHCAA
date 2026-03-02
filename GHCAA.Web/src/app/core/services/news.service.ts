import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api.endpoints';

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

// Assuming these DTOs are defined elsewhere or will be added
interface CreateNewsDto {
    title: string;
    content: string;
    category: string;
    imageUrl?: string;
    isActive: boolean;
    authorName: string;
}

interface UpdateNewsDto {
    title?: string;
    content?: string;
    category?: string;
    imageUrl?: string;
    isActive?: boolean;
    authorName?: string;
}

@Injectable({
    providedIn: 'root'
})
export class NewsService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.NEWS;

    getNews(): Observable<NewsPost[]> {
        return this.http.get<NewsPost[]>(this.apiUrl);
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
}
