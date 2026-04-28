import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';

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
}

@Injectable({
    providedIn: 'root'
})
export class PollService {
    private http = inject(HttpClient);

    getActivePolls(): Observable<PollDto[]> {
        return this.http.get<PollDto[]>(API_ENDPOINTS.POLLS.ACTIVE);
    }

    getPollById(id: number): Observable<PollDto> {
        return this.http.get<PollDto>(`${API_ENDPOINTS.POLLS.BASE}/${id}`);
    }

    vote(pollId: number, optionIds: number[]): Observable<any> {
        return this.http.post(`${API_ENDPOINTS.POLLS.BASE}/${pollId}/vote`, { optionIds });
    }
}
