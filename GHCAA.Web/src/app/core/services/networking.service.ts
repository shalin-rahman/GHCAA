import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';

@Injectable({
    providedIn: 'root'
})
export class NetworkingService {
    private http = inject(HttpClient);

    getCommittee(params: any = {}): Observable<any[]> {
        return this.http.get<any[]>(API_ENDPOINTS.NETWORKING.COMMITTEE, { params });
    }

    getPeriods(): Observable<any[]> {
        return this.http.get<any[]>(API_ENDPOINTS.NETWORKING.COMMITTEE_PERIODS);
    }

    searchMembers(filter: any): Observable<any[]> {
        return this.http.get<any[]>(API_ENDPOINTS.NETWORKING.SEARCH, { params: filter });
    }

    getUpdates(params: any): Observable<any[]> {
        return this.http.get<any[]>(API_ENDPOINTS.NETWORKING.UPDATES, { params });
    }
}
