import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import {
    AdminElectionDto, AdminElectionStepResult, CastBallotDto, ElectionApprovalDto, CastBallotResultDto, CreateElectionRequest, ElectionAppointmentDto, ElectionPhase,
    ElectionResultDto, ElectionSummaryDto,
    NominationDto, NominationViewDto, SaveCandidateRequest, ScrutinyDto
} from '../models/election.models';

/** Fetches the static markdown election-form assets synced by `npm run sync:docs`. */
@Injectable({ providedIn: 'root' })
export class ElectionsService {
    private http = inject(HttpClient);

    getDocumentText(url: string): Observable<string> {
        return this.http.get(url, { responseType: 'text' });
    }

    getCurrent(): Observable<ElectionSummaryDto | null> {
        return this.http.get<ElectionSummaryDto | null>(API_ENDPOINTS.ELECTIONS.CURRENT);
    }

    getById(id: number): Observable<ElectionSummaryDto> {
        return this.http.get<ElectionSummaryDto>(`${API_ENDPOINTS.ELECTIONS.BASE}/${id}`);
    }

    getAdminElections(): Observable<AdminElectionDto[]> {
        return this.http.get<AdminElectionDto[]>(API_ENDPOINTS.ADMIN_ELECTIONS.BASE);
    }

    create(request: CreateElectionRequest): Observable<AdminElectionDto> {
        return this.http.post<AdminElectionDto>(API_ENDPOINTS.ADMIN_ELECTIONS.BASE, request);
    }

    publish(id: number): Observable<AdminElectionStepResult> {
        return this.runStep(API_ENDPOINTS.ADMIN_ELECTIONS.PUBLISH(id), {});
    }

    close(id: number): Observable<AdminElectionStepResult> {
        return this.runStep(API_ENDPOINTS.ADMIN_ELECTIONS.CLOSE(id), {});
    }

    // Spec 023 (37.12f). A 202 means the step was stored until a second person approves it.
    private runStep(url: string, body: unknown): Observable<AdminElectionStepResult> {
        return this.http.post<AdminElectionDto | ElectionApprovalDto>(url, body, { observe: 'response' }).pipe(
            map(response => response.status === 202
                ? { election: null, pending: response.body as ElectionApprovalDto }
                : { election: response.body as AdminElectionDto, pending: null }));
    }

    getApprovals(id: number): Observable<ElectionApprovalDto[]> {
        return this.http.get<ElectionApprovalDto[]>(API_ENDPOINTS.ELECTIONS.APPROVALS(id));
    }

    approve(approvalId: number): Observable<void> {
        return this.http.post<void>(API_ENDPOINTS.ELECTIONS.APPROVE(approvalId), {});
    }

    reject(approvalId: number, reason: string | null): Observable<void> {
        return this.http.post<void>(API_ENDPOINTS.ELECTIONS.REJECT(approvalId), { reason });
    }

    addCandidate(id: number, request: SaveCandidateRequest): Observable<AdminElectionDto> {
        return this.http.post<AdminElectionDto>(API_ENDPOINTS.ADMIN_ELECTIONS.CANDIDATES(id), request);
    }

    removeCandidate(electionId: number, candidateId: number): Observable<void> {
        return this.http.delete<void>(`${API_ENDPOINTS.ADMIN_ELECTIONS.CANDIDATES(electionId)}/${candidateId}`);
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

    castBallot(id: number, request: CastBallotDto): Observable<CastBallotResultDto> {
        return this.http.post<CastBallotResultDto>(API_ENDPOINTS.ELECTIONS.VOTE(id), request);
    }

    setBallotKey(id: number, publicKey: string): Observable<AdminElectionStepResult> {
        return this.runStep(API_ENDPOINTS.ADMIN_ELECTIONS.BALLOT_KEY(id), { publicKey });
    }

    // The first count needs the returning officer's private key. After that the stored results
    // come back without it.
    count(id: number, privateKey?: string): Observable<ElectionResultDto[]> {
        return this.http.post<ElectionResultDto[]>(API_ENDPOINTS.ELECTIONS.COUNT(id), privateKey ? { privateKey } : {});
    }

    declare(id: number): Observable<void> {
        return this.http.post<void>(API_ENDPOINTS.ELECTIONS.DECLARE(id), {});
    }

    getOfficialDocument(id: number, formCode: string): Observable<Blob> {
        return this.http.get(API_ENDPOINTS.ELECTIONS.DOCUMENT(id, formCode), { responseType: 'blob' });
    }

    getMyAppointments(): Observable<ElectionAppointmentDto[]> {
        return this.http.get<ElectionAppointmentDto[]>(API_ENDPOINTS.ELECTION_APPOINTMENTS.MINE);
    }

    // Accept needs step-up; the global interceptor raises the dialog and retries.
    acceptAppointment(id: number): Observable<void> {
        return this.http.post<void>(API_ENDPOINTS.ELECTION_APPOINTMENTS.ACCEPT(id), { agreeToDeclaration: true });
    }

    declineAppointment(id: number, reason: string | null): Observable<void> {
        return this.http.post<void>(API_ENDPOINTS.ELECTION_APPOINTMENTS.DECLINE(id), { reason });
    }
}
