import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api.endpoints';

@Injectable({
    providedIn: 'root'
})
export class LookupService {
    private http = inject(HttpClient);

    getLookups(category?: string): Observable<any[]> {
        const url = category ? `${API_ENDPOINTS.LOOKUPS}/${category}` : API_ENDPOINTS.LOOKUPS;
        return this.http.get<any[]>(url);
    }

    getStats(): Observable<any> {
        return this.http.get<any>(`${API_ENDPOINTS.LOOKUPS}/stats`);
    }
}
