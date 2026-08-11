import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import {
    ForumCategory,
    ForumTopic,
    ForumPost,
    CreateForumTopicDto,
    CreateForumPostDto
} from '../models/business.models';

@Injectable({ providedIn: 'root' })
export class ForumService {
    private http = inject(HttpClient);
    private base = API_ENDPOINTS.FORUM;

    // ── Categories ────────────────────────────────────────────────────────────
    getCategories(): Observable<ForumCategory[]> {
        return this.http.get<ForumCategory[]>(`${this.base}/categories`);
    }

    // ── Topics ────────────────────────────────────────────────────────────────
    getTopics(categoryId: number, page = 1, pageSize = 20): Observable<ForumTopic[]> {
        return this.http.get<ForumTopic[]>(
            `${this.base}/categories/${categoryId}/topics?page=${page}&pageSize=${pageSize}`
        );
    }

    getTopic(topicId: number): Observable<ForumTopic> {
        return this.http.get<ForumTopic>(`${this.base}/topics/${topicId}`);
    }

    createTopic(dto: CreateForumTopicDto): Observable<ForumTopic> {
        return this.http.post<ForumTopic>(`${this.base}/topics`, dto);
    }

    deleteTopic(topicId: number): Observable<void> {
        return this.http.delete<void>(`${this.base}/topics/${topicId}`);
    }

    // ── Posts ─────────────────────────────────────────────────────────────────
    getPosts(topicId: number, page = 1, pageSize = 20): Observable<ForumPost[]> {
        return this.http.get<ForumPost[]>(
            `${this.base}/topics/${topicId}/posts?page=${page}&pageSize=${pageSize}`
        );
    }

    createPost(dto: CreateForumPostDto): Observable<ForumPost> {
        return this.http.post<ForumPost>(`${this.base}/topics/${dto.topicId}/posts`, dto);
    }

    deletePost(postId: number): Observable<void> {
        return this.http.delete<void>(`${this.base}/posts/${postId}`);
    }
}
