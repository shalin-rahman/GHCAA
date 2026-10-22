import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { ElectionContentDto, ElectionContentKind, UpsertElectionContentDto } from '../models/election.models';

// 37.14s. Election pages and form layouts. Reads are public; edits are admin only.
@Injectable({ providedIn: 'root' })
export class ElectionContentService {
    private http = inject(HttpClient);

    // Published rows without bodies, in display order.
    list(kind?: ElectionContentKind): Observable<ElectionContentDto[]> {
        const params = kind ? new HttpParams().set('kind', kind) : undefined;
        return this.http.get<ElectionContentDto[]>(API_ENDPOINTS.ELECTION_CONTENT.BASE, { params });
    }

    // One published row with its tokens filled from org config and the active positions.
    get(kind: ElectionContentKind, key: string): Observable<ElectionContentDto> {
        return this.http.get<ElectionContentDto>(API_ENDPOINTS.ELECTION_CONTENT.BY_KEY(kind, key));
    }

    // Every row, unpublished ones too, without bodies.
    adminList(): Observable<ElectionContentDto[]> {
        return this.http.get<ElectionContentDto[]>(API_ENDPOINTS.ELECTION_CONTENT.ADMIN);
    }

    // The stored body with its {{tokens}} left in, for editing.
    adminGet(id: number): Observable<ElectionContentDto> {
        return this.http.get<ElectionContentDto>(API_ENDPOINTS.ELECTION_CONTENT.ADMIN_BY_ID(id));
    }

    create(request: UpsertElectionContentDto): Observable<ElectionContentDto> {
        return this.http.post<ElectionContentDto>(API_ENDPOINTS.ELECTION_CONTENT.BASE, request);
    }

    update(id: number, request: UpsertElectionContentDto): Observable<ElectionContentDto> {
        return this.http.put<ElectionContentDto>(API_ENDPOINTS.ELECTION_CONTENT.BY_ID(id), request);
    }
}
