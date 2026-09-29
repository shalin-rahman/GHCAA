import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { ElectionPersonaDto, SaveElectionPersonaDto } from '../models/election.models';

// Spec 023 (37.12b). Admin CRUD for election personas.
@Injectable({ providedIn: 'root' })
export class ElectionPersonasService {
    private http = inject(HttpClient);

    list(): Observable<ElectionPersonaDto[]> {
        return this.http.get<ElectionPersonaDto[]>(API_ENDPOINTS.ADMIN_ELECTION_PERSONAS.BASE);
    }

    create(request: SaveElectionPersonaDto): Observable<ElectionPersonaDto> {
        return this.http.post<ElectionPersonaDto>(API_ENDPOINTS.ADMIN_ELECTION_PERSONAS.BASE, request);
    }

    update(id: number, request: SaveElectionPersonaDto): Observable<ElectionPersonaDto> {
        return this.http.put<ElectionPersonaDto>(API_ENDPOINTS.ADMIN_ELECTION_PERSONAS.BY_ID(id), request);
    }

    setActive(id: number, isActive: boolean): Observable<void> {
        return this.http.post<void>(API_ENDPOINTS.ADMIN_ELECTION_PERSONAS.ACTIVE(id), isActive);
    }

    delete(id: number): Observable<void> {
        return this.http.delete<void>(API_ENDPOINTS.ADMIN_ELECTION_PERSONAS.BY_ID(id));
    }
}
