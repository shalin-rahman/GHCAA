import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { PollDto } from './poll.service';

export interface CreatePollDto {
    title: string;
    description?: string;
    allowMultipleChoice: boolean;
    expiryDate?: string;
    options: string[];
}

@Injectable({
    providedIn: 'root'
})
export class AdminPollService {
    private http = inject(HttpClient);

    getAllPolls(): Observable<PollDto[]> {
        return this.http.get<PollDto[]>(API_ENDPOINTS.ADMIN.POLLS);
    }

    createPoll(poll: CreatePollDto): Observable<any> {
        return this.http.post(API_ENDPOINTS.ADMIN.POLLS, poll);
    }

    toggleStatus(id: number, isActive: boolean): Observable<any> {
        return this.http.put(`${API_ENDPOINTS.ADMIN.POLLS}/${id}/toggle`, isActive);
    }

    deletePoll(id: number): Observable<any> {
        return this.http.delete(`${API_ENDPOINTS.ADMIN.POLLS}/${id}`);
    }
}
