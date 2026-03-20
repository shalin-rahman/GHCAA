import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';

@Injectable({
    providedIn: 'root'
})
export class LookupService {
    private http = inject(HttpClient);

    getLookups(lookupGroup?: string, silent: boolean = false): Observable<any[]> {
        const url = lookupGroup ? `${API_ENDPOINTS.LOOKUPS}/${lookupGroup}` : API_ENDPOINTS.LOOKUPS;
        const headers = silent ? new HttpHeaders().set('X-Skip-Error-Notify', 'true') : undefined;
        return this.http.get<any[]>(url, { headers });
    }

    getStats(silent: boolean = false): Observable<any> {
        const headers = silent ? new HttpHeaders().set('X-Skip-Error-Notify', 'true') : undefined;
        return this.http.get<any>(`${API_ENDPOINTS.LOOKUPS}/stats`, { headers });
    }
}
