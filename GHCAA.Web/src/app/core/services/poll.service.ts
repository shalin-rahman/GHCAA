import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS, DISCUSSION } from '../constants/app.constants';
import { CommentDto, CommentSource, CommentThreadDto } from '../models/discussion.models';

export interface PollOptionDto {
    id: number;
    text: string;
    voteCount: number;
    percentage: number;
}

export interface PollDto {
    id: number;
    title: string;
    description?: string;
    allowMultipleChoice: boolean;
    isActive: boolean;
    createdAt: string;
    expiryDate?: string;
    options: PollOptionDto[];
    totalVotes: number;
    hasVoted: boolean;
    selectedOptionIds: number[];
    isArchived: boolean;
    allowComments: boolean;
    commentsLocked: boolean;
    commentCount: number;
    category: string;
    announcementId?: number | null;
}

@Injectable({
    providedIn: 'root'
})
export class PollService {
    private http = inject(HttpClient);

    getActivePolls(category?: string): Observable<PollDto[]> {
        const params = category ? new HttpParams().set('category', category) : undefined;
        return this.http.get<PollDto[]>(API_ENDPOINTS.POLLS.ACTIVE, { params });
    }

    getPollById(id: number): Observable<PollDto> {
        return this.http.get<PollDto>(`${API_ENDPOINTS.POLLS.BASE}/${id}`);
    }

    vote(pollId: number, optionIds: number[]): Observable<any> {
        return this.http.post(`${API_ENDPOINTS.POLLS.BASE}/${pollId}/vote`, { optionIds });
    }

    getComments(pollId: number, page = 1, pageSize: number = DISCUSSION.PAGE_SIZE): Observable<CommentThreadDto> {
        const params = new HttpParams().set('page', page).set('pageSize', pageSize);
        return this.http.get<CommentThreadDto>(`${API_ENDPOINTS.POLLS.BASE}/${pollId}/comments`, { params });
    }

    postComment(pollId: number, content: string, parentId?: number | null): Observable<CommentDto> {
        return this.http.post<CommentDto>(`${API_ENDPOINTS.POLLS.BASE}/${pollId}/comments`, { content, parentId: parentId ?? null });
    }

    removeComment(pollId: number, commentId: number, reason?: string): Observable<unknown> {
        return this.http.delete(`${API_ENDPOINTS.POLLS.BASE}/${pollId}/comments/${commentId}`, { body: { reason: reason || null } });
    }

    // Binds the comment routes of one poll for <app-comment-thread>.
    commentSource(pollId: number): CommentSource {
        return {
            load: (page, pageSize) => this.getComments(pollId, page, pageSize),
            post: (content, parentId) => this.postComment(pollId, content, parentId),
            remove: (commentId, reason) => this.removeComment(pollId, commentId, reason)
        };
    }
}
