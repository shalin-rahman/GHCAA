import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { PostType } from '../models/business.models';
import { PollDto } from './poll.service';

export interface PollAnnouncementDto {
    postType: PostType;
    title: string;
    content: string;
    publishDate?: string | null;
    membersOnly: boolean;
}

export interface CreatePollDto {
    title: string;
    description?: string;
    allowMultipleChoice: boolean;
    expiryDate?: string;
    options: string[];
    allowComments: boolean;
    category?: string;
    // Optional news or notice posted with the poll.
    announcement?: PollAnnouncementDto | null;
}

@Injectable({
    providedIn: 'root'
})
export class AdminPollService {
    private http = inject(HttpClient);

    getAllPolls(category?: string, includeArchived = false): Observable<PollDto[]> {
        let params = new HttpParams().set('includeArchived', includeArchived);
        if (category) params = params.set('category', category);
        return this.http.get<PollDto[]>(API_ENDPOINTS.ADMIN.POLLS, { params });
    }

    createPoll(poll: CreatePollDto): Observable<any> {
        return this.http.post(API_ENDPOINTS.ADMIN.POLLS, poll);
    }

    toggleStatus(id: number, isActive: boolean): Observable<any> {
        return this.http.put(`${API_ENDPOINTS.ADMIN.POLLS}/${id}/toggle`, isActive);
    }

    setCommentsLocked(id: number, locked: boolean): Observable<unknown> {
        return this.http.put(`${API_ENDPOINTS.ADMIN.POLLS}/${id}/comments/lock`, { locked });
    }

    setAllowComments(id: number, allowComments: boolean): Observable<unknown> {
        return this.http.put(`${API_ENDPOINTS.ADMIN.POLLS}/${id}/comments`, { allowComments });
    }

    // hideAnnouncement also hides the news or notice that was posted with the poll.
    deletePoll(id: number, hideAnnouncement = false): Observable<any> {
        const params = new HttpParams().set('hideAnnouncement', hideAnnouncement);
        return this.http.delete(`${API_ENDPOINTS.ADMIN.POLLS}/${id}`, { params });
    }
}
