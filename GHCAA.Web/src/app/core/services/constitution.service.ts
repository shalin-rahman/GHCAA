import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';

/** Mirrors `GHCAA.Domain.Models.Constitution`. */
export interface Constitution {
    id: number;
    version: string;
    /** Full text of the governing document. */
    content: string;
    pdfUrl?: string | null;
    effectiveDate: string;
    supersededDate?: string | null;
    isActive: boolean;
    changeSummary: string;
}

/**
 * Public reader for the versioned constitution repository.
 *
 * `GET constitution` and `GET constitution/history` are `[AllowAnonymous]`, so this works
 * on the public site with no session. `vote` is member-authenticated (the endpoint reads the
 * `MemberId` claim) and is only reachable from the portal.
 */
@Injectable({ providedIn: 'root' })
export class ConstitutionService {
    private http = inject(HttpClient);

    /**
     * The active version. The API answers 404 when no active row exists, which is a normal
     * state rather than an error — callers fall back to the static PDF — so error toasts
     * are suppressed here.
     */
    getCurrent(): Observable<Constitution> {
        const headers = new HttpHeaders().set('X-Skip-Error-Notify', 'true');
        return this.http.get<Constitution>(API_ENDPOINTS.GOVERNANCE.CONSTITUTION, { headers });
    }

    getHistory(): Observable<Constitution[]> {
        const headers = new HttpHeaders().set('X-Skip-Error-Notify', 'true');
        return this.http.get<Constitution[]>(API_ENDPOINTS.GOVERNANCE.CONSTITUTION_HISTORY, { headers });
    }

    /** One vote per member is enforced server-side by a unique (ConstitutionId, MemberId) index. */
    vote(id: number, isFor: boolean, comments?: string): Observable<{ message: string }> {
        const query = comments ? `?comments=${encodeURIComponent(comments)}` : '';
        return this.http.post<{ message: string }>(
            `${API_ENDPOINTS.GOVERNANCE.CONSTITUTION_VOTE(id)}${query}`,
            isFor
        );
    }
}
