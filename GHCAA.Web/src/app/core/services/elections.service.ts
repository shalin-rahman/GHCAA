import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import {
    CastVoteDto, CreateElectionRequest, Election, ElectionBallot, ElectionPhase,
    ElectionResult, ElectionResultDto, ElectionSummary, ElectionSummaryDto,
    NominationDto, NominationViewDto, SaveCandidateRequest, ScrutinyDto
} from '../models/election.models';

/** Fetches the static markdown election-form assets synced by `npm run sync:docs`. */
@Injectable({ providedIn: 'root' })
export class ElectionsService {
    private http = inject(HttpClient);

    getDocumentText(url: string): Observable<string> {
        return this.http.get(url, { responseType: 'text' });
    }

    getCurrent(): Observable<Election | null> {
        return this.http.get<Election | null>(API_ENDPOINTS.ELECTIONS.CURRENT);
    }

    getById(id: number): Observable<Election> {
        return this.http.get<Election>(`${API_ENDPOINTS.ELECTIONS.BASE}/${id}`);
    }

    getResults(id: number): Observable<ElectionSummary> {
        return this.http.get<ElectionSummary>(API_ENDPOINTS.ELECTIONS.RESULTS(id));
    }

    submitBallot(id: number, ballot: ElectionBallot): Observable<void> {
        return this.http.post<void>(API_ENDPOINTS.ELECTIONS.BALLOT(id), ballot);
    }

    getAdminElections(): Observable<Election[]> {
        return this.http.get<Election[]>(API_ENDPOINTS.ADMIN_ELECTIONS.BASE);
    }

    create(request: CreateElectionRequest): Observable<Election> {
        return this.http.post<Election>(API_ENDPOINTS.ADMIN_ELECTIONS.BASE, request);
    }

    publish(id: number): Observable<Election> {
        return this.http.post<Election>(API_ENDPOINTS.ADMIN_ELECTIONS.PUBLISH(id), {});
    }

    close(id: number): Observable<Election> {
        return this.http.post<Election>(API_ENDPOINTS.ADMIN_ELECTIONS.CLOSE(id), {});
    }

    addCandidate(id: number, request: SaveCandidateRequest): Observable<Election> {
        return this.http.post<Election>(API_ENDPOINTS.ADMIN_ELECTIONS.CANDIDATES(id), request);
    }

    removeCandidate(electionId: number, candidateId: number): Observable<void> {
        return this.http.delete<void>(`${API_ENDPOINTS.ADMIN_ELECTIONS.CANDIDATES(electionId)}/${candidateId}`);
    }

    createElection(request: CreateElectionRequest): Observable<ElectionSummaryDto> {
        return this.http.post<ElectionSummaryDto>(API_ENDPOINTS.ELECTIONS.BASE, request);
    }

    setPhase(id: number, phase: ElectionPhase): Observable<void> {
        return this.http.post<void>(API_ENDPOINTS.ELECTIONS.PHASE(id), phase);
    }

    freezeVoterRoll(id: number): Observable<{ count: number }> {
        return this.http.post<{ count: number }>(API_ENDPOINTS.ELECTIONS.FREEZE_VOTER_ROLL(id), {});
    }

    getNominations(id: number): Observable<NominationViewDto[]> {
        return this.http.get<NominationViewDto[]>(API_ENDPOINTS.ELECTIONS.NOMINATIONS(id));
    }

    nominate(id: number, request: NominationDto): Observable<NominationViewDto> {
        return this.http.post<NominationViewDto>(API_ENDPOINTS.ELECTIONS.NOMINATIONS(id), request);
    }

    scrutinise(nominationId: number, request: ScrutinyDto): Observable<void> {
        return this.http.post<void>(API_ENDPOINTS.ELECTIONS.SCRUTINY(nominationId), request);
    }

    withdrawNomination(nominationId: number): Observable<void> {
        return this.http.post<void>(API_ENDPOINTS.ELECTIONS.WITHDRAW(nominationId), {});
    }

    castVote(id: number, request: CastVoteDto): Observable<void> {
        return this.http.post<void>(API_ENDPOINTS.ELECTIONS.VOTE(id), request);
    }

    count(id: number): Observable<ElectionResultDto[]> {
        return this.http.post<ElectionResultDto[]>(API_ENDPOINTS.ELECTIONS.COUNT(id), {});
    }

    declare(id: number): Observable<void> {
        return this.http.post<void>(API_ENDPOINTS.ELECTIONS.DECLARE(id), {});
    }

    getOfficialDocument(id: number, formCode: string): Observable<Blob> {
        return this.http.get(API_ENDPOINTS.ELECTIONS.DOCUMENT(id, formCode), { responseType: 'blob' });
    }
}
