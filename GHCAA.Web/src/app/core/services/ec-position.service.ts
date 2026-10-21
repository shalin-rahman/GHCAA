import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, shareReplay, tap } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';

/** Mirrors `GHCAA.Application.DTOs.EcPositionDto`. */
export interface EcPosition {
    id: number;
    key: string;
    name: string;
    nameBn?: string | null;
    displayOrder: number;
    seatCount: number;
    isActive: boolean;
    showOnBoard: boolean;
    signsAsHead: boolean;
    signsAsSecretary: boolean;
}

export interface SaveEcPositionRequest {
    name: string;
    nameBn?: string | null;
    displayOrder: number;
    seatCount: number;
    showOnBoard: boolean;
    signsAsHead: boolean;
    signsAsSecretary: boolean;
}

/** Mirrors `InvalidPositionRowDto`: a term or seat whose position has no EcPositions row. */
export interface InvalidPositionRow {
    kind: string;
    id: number;
    position: number;
    description: string;
}

// 95.2: committee positions come from the EcPositions table, so pickers and labels follow
// whatever the admin has set up instead of a fixed list.
@Injectable({ providedIn: 'root' })
export class EcPositionService {
    private http = inject(HttpClient);
    private active$?: Observable<EcPosition[]>;

    /** Active positions in display order. Cached until an admin change clears it. */
    getActive(): Observable<EcPosition[]> {
        this.active$ ??= this.http.get<EcPosition[]>(API_ENDPOINTS.GOVERNANCE.POSITIONS).pipe(shareReplay(1));
        return this.active$;
    }

    /** Admin list, retired positions included. */
    getAll(): Observable<EcPosition[]> {
        return this.http.get<EcPosition[]>(this.adminUrl());
    }

    create(body: SaveEcPositionRequest): Observable<EcPosition> {
        return this.http.post<EcPosition>(this.adminUrl(), body).pipe(tap(() => this.clearCache()));
    }

    update(id: number, body: SaveEcPositionRequest): Observable<EcPosition> {
        return this.http.put<EcPosition>(this.adminUrl(id), body).pipe(tap(() => this.clearCache()));
    }

    retire(id: number): Observable<unknown> {
        return this.http.post(`${this.adminUrl(id)}/retire`, {}).pipe(tap(() => this.clearCache()));
    }

    restore(id: number): Observable<unknown> {
        return this.http.post(`${this.adminUrl(id)}/restore`, {}).pipe(tap(() => this.clearCache()));
    }

    remove(id: number): Observable<unknown> {
        return this.http.delete(this.adminUrl(id)).pipe(tap(() => this.clearCache()));
    }

    /** Terms and seats the upgrade left with a position that has no row. */
    getInvalidRows(): Observable<InvalidPositionRow[]> {
        return this.http.get<InvalidPositionRow[]>(`${this.adminUrl()}/invalid-rows`);
    }

    repair(row: InvalidPositionRow, positionId: number): Observable<unknown> {
        return this.http.post(`${this.adminUrl()}/repair`, { kind: row.kind, id: row.id, positionId });
    }

    clearCache(): void {
        this.active$ = undefined;
    }

    private adminUrl(id?: number): string {
        const base = `${API_ENDPOINTS.ADMIN.GOVERNANCE}/positions`;
        return id === undefined ? base : `${base}/${id}`;
    }
}
