import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/** Fetches the static markdown election-form assets synced by `npm run sync:docs`. */
@Injectable({ providedIn: 'root' })
export class ElectionsService {
    private http = inject(HttpClient);

    getDocumentText(url: string): Observable<string> {
        return this.http.get(url, { responseType: 'text' });
    }
}
