import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { API_ENDPOINTS } from '../constants/app.constants';
import { AdminElectionDto, CastBallotDto, ElectionApprovalDto, ElectionResultDto, ElectionSummaryDto } from '../models/election.models';
import { ElectionsService } from './elections.service';

const ELECTION: ElectionSummaryDto = {
    id: 12, title: 'Executive Committee 2026', phase: 'Polling',
    ecPeriodId: 3, voterCount: 40, eligibleVoterCount: 120
};

const ADMIN_ELECTION: AdminElectionDto = {
    id: 12, title: 'Executive Committee 2026', phase: 'Announced',
    description: '', isActive: true, hasVoted: false, positions: [], candidates: []
};

describe('ElectionsService', () => {
    let service: ElectionsService;
    let http: HttpTestingController;
    beforeEach(() => {
        TestBed.configureTestingModule({ imports: [HttpClientTestingModule], providers: [ElectionsService] });
        service = TestBed.inject(ElectionsService);
        http = TestBed.inject(HttpTestingController);
    });
    afterEach(() => http.verify());

    it('gets the current election from the public route', () => {
        let value: any;
        service.getCurrent().subscribe(v => value = v);
        const request = http.expectOne(API_ENDPOINTS.ELECTIONS.CURRENT);
        expect(request.request.method).toBe('GET');
        request.flush(ELECTION);
        expect(value).toEqual(ELECTION);
    });

    it('gets the latest declared election and skips the error toast', () => {
        let value: any;
        service.getLatestDeclared().subscribe(v => value = v);
        const request = http.expectOne(API_ENDPOINTS.ELECTIONS.LATEST_DECLARED);
        expect(request.request.method).toBe('GET');
        expect(request.request.headers.get('X-Skip-Error-Notify')).toBe('true');
        request.flush(ELECTION);
        expect(value).toEqual(ELECTION);
    });

    it('FR-051: gets the finalized public candidate list without an error toast on 404', () => {
        const candidates = [{ seatId: 1, positionTitle: 'President', places: 1, candidates: [{ nominationId: 4, name: 'Candidate', photoUrl: null, statement: 'Statement' }] }];
        let value: unknown;
        service.getPublicCandidateList(ELECTION.id).subscribe(v => value = v);
        const request = http.expectOne(API_ENDPOINTS.ELECTIONS.CANDIDATE_LIST(ELECTION.id));
        expect(request.request.method).toBe('GET');
        expect(request.request.headers.get('X-Skip-Error-Notify')).toBe('true');
        request.flush(candidates);
        expect(value).toEqual(candidates);
    });

    it('casts the whole ballot in one request and returns the tracking code', () => {
        const ballot: CastBallotDto = { seats: [{ electionSeatId: 1, nominationIds: [5] }, { electionSeatId: 2, nominationIds: [] }] };
        let code: string | undefined;
        service.castBallot(ELECTION.id, ballot).subscribe(r => code = r.trackingCode);
        const request = http.expectOne(API_ENDPOINTS.ELECTIONS.VOTE(ELECTION.id));
        expect(request.request.method).toBe('POST');
        expect(request.request.body).toEqual(ballot);
        request.flush({ trackingCode: 'ABCD-EFGH-JKMN' });
        expect(code).toBe('ABCD-EFGH-JKMN');
    });

    it('runs the official count through the admin-only count route', () => {
        const results: ElectionResultDto[] = [{ electionSeatId: 1, nominationId: 5, voteCount: 10, isElected: true, isTie: false }];
        let value: any;
        service.count(ELECTION.id).subscribe(v => value = v);
        const request = http.expectOne(API_ENDPOINTS.ELECTIONS.COUNT(ELECTION.id));
        expect(request.request.method).toBe('POST');
        request.flush(results);
        expect(value).toEqual(results);
    });

    it('FR-017: requests declaration and returns the pending approval', () => {
        const approval: ElectionApprovalDto = {
            id: 31, electionId: 12, action: 'Declare', requestedByUserId: 1, requestedBy: 'alice',
            requestedAt: '2026-10-01T10:00:00Z', expiresAt: '2026-10-03T10:00:00Z'
        };
        let value: ElectionApprovalDto | null | undefined;
        service.declare(ELECTION.id).subscribe(v => value = v);
        const request = http.expectOne(API_ENDPOINTS.ELECTIONS.DECLARE(ELECTION.id));
        expect(request.request.method).toBe('POST');
        expect(request.request.body).toEqual({});
        request.flush(approval, { status: 202, statusText: 'Accepted' });
        expect(value).toEqual(approval);
    });

    it('returns null when declaration runs immediately', () => {
        let value: ElectionApprovalDto | null | undefined;
        service.declare(ELECTION.id).subscribe(v => value = v);
        const request = http.expectOne(API_ENDPOINTS.ELECTIONS.DECLARE(ELECTION.id));
        request.flush(null, { status: 200, statusText: 'OK' });
        expect(value).toBeNull();
    });

    // FR-37.1a. The API reads a JSON string, so a text/plain body would get a 415.
    it('sends the phase as a JSON string and reads a 202 as a stored request', () => {
        let pending: unknown;
        service.setPhase(ELECTION.id, 'Scrutiny').subscribe(value => pending = value);
        const request = http.expectOne(API_ENDPOINTS.ELECTIONS.PHASE(ELECTION.id));
        expect(request.request.body).toBe('"Scrutiny"');
        expect(request.request.headers.get('Content-Type')).toBe('application/json');
        request.flush({ id: 30 }, { status: 202, statusText: 'Accepted' });
        expect(pending).toEqual({ id: 30 });
    });

    it('reads a 200 phase change as done at once', () => {
        let pending: unknown = 'unset';
        service.setPhase(ELECTION.id, 'Scrutiny').subscribe(value => pending = value);
        http.expectOne(API_ENDPOINTS.ELECTIONS.PHASE(ELECTION.id)).flush(null);
        expect(pending).toBeNull();
    });

    it('sends the reason when the voter roll is unfrozen', () => {
        let value: any;
        service.unfreezeVoterRoll(ELECTION.id, 'Wrong roll').subscribe(v => value = v);
        const request = http.expectOne(API_ENDPOINTS.ELECTIONS.UNFREEZE_VOTER_ROLL(ELECTION.id));
        expect(request.request.method).toBe('POST');
        expect(request.request.body).toEqual({ reason: 'Wrong roll' });
        request.flush({ removed: 3 });
        expect(value.removed).toBe(3);
    });

    it('sends the returning officer key with the first count', () => {
        let run: unknown;
        service.runCount(ELECTION.id, 'PKCS8').subscribe(value => run = value);
        const request = http.expectOne(API_ENDPOINTS.ELECTIONS.COUNT(ELECTION.id));
        expect(request.request.body).toEqual({ privateKey: 'PKCS8' });
        request.flush([]);
        expect(run).toEqual({ results: [], pending: null });
    });

    // FR-39 (37.13h)
    it('reports a count stored for a second person', () => {
        const pending = { id: 3, electionId: ELECTION.id, action: 'Count', requestedByUserId: 1, requestedBy: 'alice', requestedAt: '', expiresAt: '' };
        let run: unknown;
        service.runCount(ELECTION.id, 'PKCS8').subscribe(value => run = value);
        http.expectOne(API_ENDPOINTS.ELECTIONS.COUNT(ELECTION.id)).flush(pending, { status: 202, statusText: 'Accepted' });
        expect(run).toEqual({ results: null, pending });
    });

    it('stores only the public key for an election', () => {
        let value: any;
        service.setBallotKey(ADMIN_ELECTION.id, 'SPKI').subscribe(v => value = v);
        const request = http.expectOne(API_ENDPOINTS.ADMIN_ELECTIONS.BALLOT_KEY(ADMIN_ELECTION.id));
        expect(request.request.method).toBe('POST');
        expect(request.request.body).toEqual({ publicKey: 'SPKI' });
        request.flush(ADMIN_ELECTION);
        expect(value).toEqual({ election: ADMIN_ELECTION, pending: null });
    });

    // FR-39 (spec 023, 37.12f): 202 means the step waits for a second person.
    it('reports a 202 reply as a waiting request', () => {
        const pending: ElectionApprovalDto = {
            id: 30, electionId: 12, action: 'ReplaceBallotKey', requestedByUserId: 1, requestedBy: 'alice',
            requestedAt: '2026-10-01T10:00:00Z', expiresAt: '2026-10-03T10:00:00Z', keyFingerprint: 'cd34'
        };
        let result: unknown;
        service.setBallotKey(ADMIN_ELECTION.id, 'SPKI').subscribe(value => result = value);
        http.expectOne(API_ENDPOINTS.ADMIN_ELECTIONS.BALLOT_KEY(ADMIN_ELECTION.id)).flush(pending, { status: 202, statusText: 'Accepted' });
        expect(result).toEqual({ election: null, pending });
    });

    it('sends approve and reject to the approval routes', () => {
        service.approve(30).subscribe();
        expect(http.expectOne(API_ENDPOINTS.ELECTIONS.APPROVE(30)).request.method).toBe('POST');
        service.reject(30, 'wrong key').subscribe();
        const request = http.expectOne(API_ENDPOINTS.ELECTIONS.REJECT(30));
        expect(request.request.body).toEqual({ reason: 'wrong key' });
    });

    it('loads the admin election list', () => {
        let value: any;
        service.getAdminElections().subscribe(v => value = v);
        const request = http.expectOne(API_ENDPOINTS.ADMIN_ELECTIONS.BASE);
        expect(request.request.method).toBe('GET');
        request.flush([ADMIN_ELECTION]);
        expect(value).toEqual([ADMIN_ELECTION]);
    });

    it('reads the ballot, the declared results and the tracking codes', () => {
        service.getBallot(12).subscribe();
        const ballot = http.expectOne(API_ENDPOINTS.ELECTIONS.BALLOT(12));
        expect(ballot.request.method).toBe('GET');
        ballot.flush({});

        service.getPublicResults(12).subscribe();
        const results = http.expectOne(API_ENDPOINTS.ELECTIONS.RESULTS(12));
        expect(results.request.headers.has('X-Skip-Error-Notify')).toBe(true);
        results.flush({});

        service.getOpenedBallots(12).subscribe();
        const opened = http.expectOne(API_ENDPOINTS.ELECTIONS.OPENED_BALLOTS(12));
        expect(opened.request.method).toBe('GET');
        expect(opened.request.headers.has('X-Skip-Error-Notify')).toBe(true);
        opened.flush({ electionId: 12, seats: [] });

        service.verifyAudit(12).subscribe();
        const verify = http.expectOne(API_ENDPOINTS.ELECTIONS.AUDIT_VERIFY(12));
        expect(verify.request.headers.has('X-Skip-Error-Notify')).toBe(true);
        verify.flush({ ok: true, entries: 0, firstBrokenSeq: null });

        service.getAudit(12).subscribe();
        http.expectOne(API_ENDPOINTS.ELECTIONS.AUDIT(12)).flush([]);

        service.getTrackingCodes(12).subscribe();
        const codes = http.expectOne(API_ENDPOINTS.ELECTIONS.TRACKING_CODES(12));
        expect(codes.request.headers.has('X-Skip-Error-Notify')).toBe(true);
        codes.flush([]);
    });

    // FR-37.1b. The ballot has no seats before the candidate list, so nominations read this route.
    it('reads the seats a member can be nominated for', () => {
        const seats = [{ seatId: 10, positionTitle: 'President', places: 1 }];
        let value: any;
        service.getSeats(12).subscribe(v => value = v);
        const request = http.expectOne(API_ENDPOINTS.ELECTIONS.SEATS(12));
        expect(request.request.method).toBe('GET');
        request.flush(seats);
        expect(value).toEqual(seats);
    });

    it('FR-25: verifies a printed form on the anonymous route and skips the error toast', () => {
        service.verifyDocument(12, 'ER-27C').subscribe();
        const req = http.expectOne('/api/elections/12/documents/ER-27C/verification');
        expect(req.request.method).toBe('GET');
        expect(req.request.headers.has('X-Skip-Error-Notify')).toBe(true);
        req.flush({});
    });

    it('creates an election through the admin route', () => {
        let value: any;
        service.create({
            title: ADMIN_ELECTION.title, ecPeriodId: 1, nominationOpensOn: '', nominationClosesOn: '',
            pollingOpensOn: '', pollingClosesOn: '', description: '', positions: [{ title: 'President', seats: 1 }]
        }).subscribe(v => value = v);
        const request = http.expectOne(API_ENDPOINTS.ADMIN_ELECTIONS.BASE);
        expect(request.request.method).toBe('POST');
        request.flush(ADMIN_ELECTION);
        expect(value).toEqual(ADMIN_ELECTION);
    });

    // FR-25
    it('asks for a filled document when the mode is given', () => {
        service.getOfficialDocument(12, 'ER-05', 'Filled').subscribe();
        const request = http.expectOne(r => r.url === API_ENDPOINTS.ELECTIONS.DOCUMENT(12, 'ER-05'));
        expect(request.request.params.get('mode')).toBe('Filled');
        expect(request.request.responseType).toBe('blob');
        request.flush(new Blob());
    });

    // FR-25
    it('leaves the mode to the server when none is given', () => {
        service.getOfficialDocument(12, 'ER-05').subscribe();
        const request = http.expectOne(r => r.url === API_ENDPOINTS.ELECTIONS.DOCUMENT(12, 'ER-05'));
        expect(request.request.params.has('mode')).toBe(false);
        request.flush(new Blob());
    });

    // FR-25
    it('gets a blank form without an election id', () => {
        service.getBlankForm('ER-17A').subscribe();
        const request = http.expectOne('/api/elections/forms/ER-17A/blank');
        expect(request.request.method).toBe('GET');
        expect(request.request.responseType).toBe('blob');
        request.flush(new Blob());
    });
});
