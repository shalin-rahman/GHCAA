import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';

@Injectable({ providedIn: 'root' })
export class MentorshipService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.MENTORSHIP;

    getSent(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/sent`);
    }

    getReceived(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/received`);
    }

    send(mentorId: number, message?: string, domain?: string): Observable<any> {
        return this.http.post(this.apiUrl, { mentorId, message, domain });
    }

    respond(id: number, accept: boolean, note?: string): Observable<any> {
        return this.http.post(`${this.apiUrl}/${id}/respond`, { accept, note });
    }

    markComplete(id: number): Observable<any> {
        return this.http.post(`${this.apiUrl}/${id}/complete`, {});
    }
}
