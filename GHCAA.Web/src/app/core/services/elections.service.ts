import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import {
    AdminElectionDto, AdminElectionStepResult, ElectionBoardDto, ElectionAuditEntryDto, ElectionAuditVerifyDto, OpenedBallotsDto, BallotViewDto, PublicElectionResultsDto, AppointRequest, CastBallotDto, ElectionApprovalDto, ElectionReadinessDto, CastBallotResultDto, CreateElectionRequest, ElectionAppointmentDto, ElectionPhase,
    ElectionCountStepResult, ElectionResultDto, ElectionRulesUnlockDto, ElectionSummaryDto,
    ElectionSeatViewDto, NominationDto, NominationViewDto, SaveCandidateRequest, ScrutinyDto,
    DecideRecountRequest, ElectionRecountDto, RecordRecountRequest, RecountFigureDto, ScrutinyCheckDto, ScrutinyCheckItem, UpdateElectionFormDetailsRequest,
    ElectionDocumentVerifyDto, ElectionPrintMode
} from '../models/election.models';

@Injectable({ providedIn: 'root' })
export class ElectionsService {
    private http = inject(HttpClient);
    // A 404 on the public result calls means "not yet", so the page explains it instead of a toast.
    private readonly quietHeaders = new HttpHeaders().set('X-Skip-Error-Notify', 'true');

    getCurrent(): Observable<ElectionSummaryDto | null> {
        return this.http.get<ElectionSummaryDto | null>(API_ENDPOINTS.ELECTIONS.CURRENT);
    }

    // Anonymous. The newest declared or archived election. A 404 means none has been declared yet.
    getLatestDeclared(): Observable<ElectionSummaryDto> {
        return this.http.get<ElectionSummaryDto>(API_ENDPOINTS.ELECTIONS.LATEST_DECLARED, { headers: this.quietHeaders });
    }

    getById(id: number): Observable<ElectionSummaryDto> {
        return this.http.get<ElectionSummaryDto>(`${API_ENDPOINTS.ELECTIONS.BASE}/${id}`);
    }

    // Anonymous. A 404 means the election does not exist, so the page just hides the board.
    getBoard(id: number): Observable<ElectionBoardDto> {
        return this.http.get<ElectionBoardDto>(API_ENDPOINTS.ELECTIONS.BOARD(id), { headers: this.quietHeaders });
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

    // 37.14m. The records the printed election forms read.
    updateFormDetails(id: number, request: UpdateElectionFormDetailsRequest): Observable<AdminElectionDto> {
        return this.http.put<AdminElectionDto>(API_ENDPOINTS.ADMIN_ELECTIONS.FORM_DETAILS(id), request);
    }

    // The admin list carries consent times. The public one does not.
    getAdminNominations(id: number): Observable<NominationViewDto[]> {
        return this.http.get<NominationViewDto[]>(API_ENDPOINTS.ADMIN_ELECTIONS.NOMINATIONS(id));
    }

    recordConsent(id: number, nominationId: number, consentAt: string): Observable<void> {
        return this.http.put<void>(API_ENDPOINTS.ADMIN_ELECTIONS.CONSENT(id, nominationId), { consentAt });
    }

    getScrutinyChecks(id: number, nominationId: number): Observable<ScrutinyCheckDto[]> {
        return this.http.get<ScrutinyCheckDto[]>(API_ENDPOINTS.ADMIN_ELECTIONS.SCRUTINY_CHECKS(id, nominationId));
    }

    setScrutinyCheck(id: number, nominationId: number, item: ScrutinyCheckItem, isChecked: boolean): Observable<ScrutinyCheckDto[]> {
        return this.http.put<ScrutinyCheckDto[]>(API_ENDPOINTS.ADMIN_ELECTIONS.SCRUTINY_CHECKS(id, nominationId), { item, isChecked });
    }

    getRecounts(id: number): Observable<ElectionRecountDto[]> {
        return this.http.get<ElectionRecountDto[]>(API_ENDPOINTS.ADMIN_ELECTIONS.RECOUNTS(id));
    }

    recordRecount(id: number, request: RecordRecountRequest): Observable<ElectionRecountDto> {
        return this.http.post<ElectionRecountDto>(API_ENDPOINTS.ADMIN_ELECTIONS.RECOUNTS(id), request);
    }

    decideRecount(id: number, recountId: number, request: DecideRecountRequest): Observable<void> {
        return this.http.put<void>(API_ENDPOINTS.ADMIN_ELECTIONS.RECOUNT_DECISION(id, recountId), request);
    }

    recordRecountFigures(id: number, recountId: number, figures: RecountFigureDto[]): Observable<void> {
        return this.http.put<void>(API_ENDPOINTS.ADMIN_ELECTIONS.RECOUNT_FIGURES(id, recountId), { figures });
    }

    // The API reads the body as a JSON string. A plain string body would go as text/plain and get a 415.
    // Returns the stored request on a 202, or null when the phase changed at once.
    setPhase(id: number, phase: ElectionPhase): Observable<ElectionApprovalDto | null> {
        return this.http.post<ElectionApprovalDto | null>(API_ENDPOINTS.ELECTIONS.PHASE(id), JSON.stringify(phase), {
            headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
            observe: 'response'
        }).pipe(map(response => response.status === 202 ? response.body : null));
    }

    freezeVoterRoll(id: number): Observable<{ count: number }> {
        return this.http.post<{ count: number }>(API_ENDPOINTS.ELECTIONS.FREEZE_VOTER_ROLL(id), {});
    }

    unfreezeVoterRoll(id: number, reason: string): Observable<{ removed: number }> {
        return this.http.post<{ removed: number }>(API_ENDPOINTS.ELECTIONS.UNFREEZE_VOTER_ROLL(id), { reason });
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

    getSeats(id: number): Observable<ElectionSeatViewDto[]> {
        return this.http.get<ElectionSeatViewDto[]>(API_ENDPOINTS.ELECTIONS.SEATS(id));
    }

    getBallot(id: number): Observable<BallotViewDto> {
        return this.http.get<BallotViewDto>(API_ENDPOINTS.ELECTIONS.BALLOT(id));
    }

    // Anonymous. 404 until Declared, or when the organisation does not publish per-seat ballots.
    getOpenedBallots(id: number): Observable<OpenedBallotsDto> {
        return this.http.get<OpenedBallotsDto>(API_ENDPOINTS.ELECTIONS.OPENED_BALLOTS(id), { headers: this.quietHeaders });
    }

    // Anonymous. Names the first broken audit entry when the chain fails.
    verifyAudit(id: number): Observable<ElectionAuditVerifyDto> {
        return this.http.get<ElectionAuditVerifyDto>(API_ENDPOINTS.ELECTIONS.AUDIT_VERIFY(id), { headers: this.quietHeaders });
    }

    // Officials with the ViewAudit permission.
    getAudit(id: number): Observable<ElectionAuditEntryDto[]> {
        return this.http.get<ElectionAuditEntryDto[]>(API_ENDPOINTS.ELECTIONS.AUDIT(id));
    }

    // Anonymous. The server answers 404 until the result is declared.
    getPublicResults(id: number): Observable<PublicElectionResultsDto> {
        return this.http.get<PublicElectionResultsDto>(API_ENDPOINTS.ELECTIONS.RESULTS(id), { headers: this.quietHeaders });
    }

    // Anonymous. The server answers 404 until the count has run.
    getTrackingCodes(id: number): Observable<string[]> {
        return this.http.get<string[]>(API_ENDPOINTS.ELECTIONS.TRACKING_CODES(id), { headers: this.quietHeaders });
    }

    setBallotKey(id: number, publicKey: string): Observable<AdminElectionStepResult> {
        return this.runStep(API_ENDPOINTS.ADMIN_ELECTIONS.BALLOT_KEY(id), { publicKey });
    }

    // Reads the stored results. Without a key the server never counts or stores a count request.
    count(id: number): Observable<ElectionResultDto[]> {
        return this.http.post<ElectionResultDto[]>(API_ENDPOINTS.ELECTIONS.COUNT(id), {});
    }

    // Counts with the returning officer's private key. 37.13h: a 202 means the count waits for a
    // second person, and the same call runs it once they approve.
    runCount(id: number, privateKey: string): Observable<ElectionCountStepResult> {
        return this.http.post<ElectionResultDto[] | ElectionApprovalDto>(API_ENDPOINTS.ELECTIONS.COUNT(id), { privateKey }, { observe: 'response' }).pipe(
            map(response => response.status === 202
                ? { results: null, pending: response.body as ElectionApprovalDto }
                : { results: response.body as ElectionResultDto[], pending: null }));
    }

    declare(id: number): Observable<void> {
        return this.http.post<void>(API_ENDPOINTS.ELECTIONS.DECLARE(id), {});
    }

    // FR-050. Needs a step-up. 409 when it was already recorded, 400 before the declare.
    keyDestroyed(id: number): Observable<void> {
        return this.http.post<void>(API_ENDPOINTS.ELECTIONS.KEY_DESTROYED(id), {});
    }

    // 37.12w. What still stands between this election and polling.
    getReadiness(id: number): Observable<ElectionReadinessDto> {
        return this.http.get<ElectionReadinessDto>(API_ENDPOINTS.ELECTIONS.READINESS(id));
    }

    // Needs a step-up. 409 when it was already recorded, 400 once polling is open.
    attestReadiness(id: number, condition: string): Observable<void> {
        return this.http.post<void>(API_ENDPOINTS.ELECTIONS.READINESS_ATTEST(id), { condition });
    }

    // 37.14s. With no mode the server uses the org's DefaultPrintMode.
    getOfficialDocument(id: number, formCode: string, mode?: ElectionPrintMode): Observable<Blob> {
        const params = mode ? new HttpParams().set('mode', mode) : undefined;
        return this.http.get(API_ENDPOINTS.ELECTIONS.DOCUMENT(id, formCode), { responseType: 'blob', params });
    }

    // 37.14s. Any signed-in user. Ruled lines, no election data and no QR.
    getBlankForm(formCode: string): Observable<Blob> {
        return this.http.get(API_ENDPOINTS.ELECTIONS.BLANK_FORM(formCode), { responseType: 'blob' });
    }

    // 37.14n. Anonymous. A 404 is the answer the page shows, so the global toast stays quiet.
    verifyDocument(id: number, formCode: string): Observable<ElectionDocumentVerifyDto> {
        return this.http.get<ElectionDocumentVerifyDto>(API_ENDPOINTS.ELECTIONS.VERIFY_DOCUMENT(id, formCode), { headers: this.quietHeaders });
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

    // 37.13y. Officials of one election, for the admin panel.
    getAppointments(electionId: number): Observable<ElectionAppointmentDto[]> {
        return this.http.get<ElectionAppointmentDto[]>(API_ENDPOINTS.ELECTION_APPOINTMENTS.FOR_ELECTION(electionId));
    }

    appoint(electionId: number, request: AppointRequest): Observable<ElectionAppointmentDto> {
        return this.http.post<ElectionAppointmentDto>(API_ENDPOINTS.ELECTION_APPOINTMENTS.FOR_ELECTION(electionId), request);
    }

    revokeAppointment(id: number, reason: string | null): Observable<void> {
        return this.http.post<void>(API_ENDPOINTS.ELECTION_APPOINTMENTS.REVOKE(id), { reason });
    }

    // Always waits for a second person, so the server answers 202 with the stored request.
    emergencyRevoke(id: number, reason: string): Observable<ElectionApprovalDto> {
        return this.http.post<ElectionApprovalDto>(API_ENDPOINTS.ELECTION_APPOINTMENTS.EMERGENCY_REVOKE(id), { reason });
    }

    // 37.13z. The server answers 204 when no unlock is open, which HttpClient hands back as null.
    getRulesUnlock(): Observable<ElectionRulesUnlockDto | null> {
        return this.http.get<ElectionRulesUnlockDto | null>(API_ENDPOINTS.ELECTION_RULES_UNLOCK.BASE);
    }

    openRulesUnlock(reason: string, minutes: number | null): Observable<ElectionRulesUnlockDto> {
        return this.http.post<ElectionRulesUnlockDto>(API_ENDPOINTS.ELECTION_RULES_UNLOCK.BASE, { reason, minutes });
    }

    closeRulesUnlock(): Observable<void> {
        return this.http.post<void>(API_ENDPOINTS.ELECTION_RULES_UNLOCK.CLOSE, {});
    }
}
