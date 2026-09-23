import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { MentorshipRequestDto, MentorshipAdminRow } from '../models/business.models';

@Injectable({ providedIn: 'root' })
export class MentorshipService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.MENTORSHIP;

    getSent(): Observable<MentorshipRequestDto[]> {
        return this.http.get<MentorshipRequestDto[]>(`${this.apiUrl}/sent`);
    }

    getReceived(): Observable<MentorshipRequestDto[]> {
        return this.http.get<MentorshipRequestDto[]>(`${this.apiUrl}/received`);
    }

    send(mentorId: number, message?: string, domain?: string): Observable<MentorshipRequestDto> {
        return this.http.post<MentorshipRequestDto>(this.apiUrl, { mentorId, message, domain });
    }

    respond(id: number, accept: boolean, note?: string): Observable<MentorshipRequestDto> {
        return this.http.post<MentorshipRequestDto>(`${this.apiUrl}/${id}/respond`, { accept, note });
    }

    markComplete(id: number): Observable<MentorshipRequestDto> {
        return this.http.post<MentorshipRequestDto>(`${this.apiUrl}/${id}/complete`, {});
    }

    getAllForAdmin(): Observable<MentorshipAdminRow[]> {
        return this.http.get<MentorshipAdminRow[]>(`${this.apiUrl}/admin/all`);
    }
}
