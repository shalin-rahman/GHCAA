import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class LookupService {
    private http = inject(HttpClient);

    getLookups(category?: string): Observable<any[]> {
        const url = category ? `/api/lookups/${category}` : '/api/lookups';
        return this.http.get<any[]>(url);
    }
}
