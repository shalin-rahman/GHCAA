import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';

@Injectable({ providedIn: 'root' })
export class FamilyLinkService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.FAMILY_LINKS;

    getSent(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/sent`);
    }

    getReceived(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/received`);
    }

    send(targetMembershipNumber: string, relationship: string, note?: string): Observable<any> {
        return this.http.post(`${this.apiUrl}/send`, { targetMembershipNumber, relationship, note });
    }

    respond(requestId: number, approve: boolean): Observable<any> {
        return this.http.post(`${this.apiUrl}/respond`, { requestId, approve });
    }

    cancel(requestId: number): Observable<any> {
        return this.http.post(`${this.apiUrl}/${requestId}/cancel`, {});
    }

    remove(requestId: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/remove/${requestId}`);
    }

    getFamily(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/my-family`);
    }

    search(name: string): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/search`, { params: { name } });
    }
}
