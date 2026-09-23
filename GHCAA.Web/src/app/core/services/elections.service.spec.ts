import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { API_ENDPOINTS } from '../constants/app.constants';
import { AdminElectionDto, CastVoteDto, ElectionResultDto, ElectionSummaryDto } from '../models/election.models';
import { ElectionsService } from './elections.service';

const ELECTION: ElectionSummaryDto = {
    id: 12, title: 'Executive Committee 2026', phase: 'Polling',
    nominationOpensOn: '2026-01-01', nominationClosesOn: '2026-01-15',
    pollingOpensOn: '2026-02-01', pollingClosesOn: '2026-02-07'
};

const ADMIN_ELECTION: AdminElectionDto = {
    id: 12, title: 'Executive Committee 2026', phase: 'Announced',
    description: '', positions: [], candidates: []
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
        service.getCurrent().subscribe(value => expect(value).toEqual(ELECTION));
        const request = http.expectOne(API_ENDPOINTS.ELECTIONS.CURRENT);
        expect(request.request.method).toBe('GET');
        request.flush(ELECTION);
    });

    it('casts a member vote through the vote route', () => {
        const vote: CastVoteDto = { electionSeatId: 1, nominationId: 5 };
        service.castVote(ELECTION.id, vote).subscribe();
        const request = http.expectOne(API_ENDPOINTS.ELECTIONS.VOTE(ELECTION.id));
        expect(request.request.method).toBe('POST');
        expect(request.request.body).toEqual(vote);
        request.flush({});
    });

    it('runs the official count through the admin-only count route', () => {
        const results: ElectionResultDto[] = [{ electionSeatId: 1, nominationId: 5, voteCount: 10, isElected: true, isTie: false }];
        service.count(ELECTION.id).subscribe(value => expect(value).toEqual(results));
        const request = http.expectOne(API_ENDPOINTS.ELECTIONS.COUNT(ELECTION.id));
        expect(request.request.method).toBe('POST');
        request.flush(results);
    });

    it('loads the admin election list', () => {
        service.getAdminElections().subscribe(value => expect(value).toEqual([ADMIN_ELECTION]));
        const request = http.expectOne(API_ENDPOINTS.ADMIN_ELECTIONS.BASE);
        expect(request.request.method).toBe('GET');
        request.flush([ADMIN_ELECTION]);
    });

    it('creates an election through the admin route', () => {
        service.create({
            title: ADMIN_ELECTION.title, ecPeriodId: 1, nominationOpensOn: '', nominationClosesOn: '',
            pollingOpensOn: '', pollingClosesOn: '', description: '', positions: [{ title: 'President', seats: 1 }]
        }).subscribe(value => expect(value).toEqual(ADMIN_ELECTION));
        const request = http.expectOne(API_ENDPOINTS.ADMIN_ELECTIONS.BASE);
        expect(request.request.method).toBe('POST');
        request.flush(ADMIN_ELECTION);
    });
});
