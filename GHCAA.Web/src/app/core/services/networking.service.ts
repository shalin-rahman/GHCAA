import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class NetworkingService {
    private http = inject(HttpClient);

    getCommittee(year?: number): Observable<any[]> {
        let params: Record<string, string> = {};
        if (year) params['year'] = year.toString();
        return this.http.get<any[]>('/api/networking/committee', { params });
    }

    searchMembers(filter: any): Observable<any[]> {
        return this.http.get<any[]>('/api/networking/search', { params: filter });
    }

    getLatestUpdates(count: number = 10): Observable<any[]> {
        let params: Record<string, string> = { count: count.toString() };
        return this.http.get<any[]>('/api/networking/updates', { params });
    }
}
