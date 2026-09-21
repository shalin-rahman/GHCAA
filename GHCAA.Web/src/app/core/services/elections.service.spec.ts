import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { API_ENDPOINTS } from '../constants/app.constants';
import { Election, ElectionBallot, ElectionSummary } from '../models/election.models';
import { ElectionsService } from './elections.service';

const ELECTION: Election = {
    id: 12,
    title: 'Executive Committee 2026',
    phase: 'Polling',
    positions: [],
    candidates: [],
    hasVoted: false,
    isActive: true
};

describe('ElectionsService', () => {
    let service: ElectionsService;
    let http: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [ElectionsService]
        });
        service = TestBed.inject(ElectionsService);
        http = TestBed.inject(HttpTestingController);
    });

    afterEach(() => http.verify());

    it('gets the current election from the planned public route', () => {
        service.getCurrent().subscribe(value => expect(value).toEqual(ELECTION));
        const request = http.expectOne(API_ENDPOINTS.ELECTIONS.CURRENT);
        expect(request.request.method).toBe('GET');
        request.flush(ELECTION);
    });

    it('posts a member ballot to the election route', () => {
        const ballot: ElectionBallot = { electionId: ELECTION.id, selections: [] };
        service.submitBallot(ELECTION.id, ballot).subscribe();
        const request = http.expectOne(API_ENDPOINTS.ELECTIONS.BALLOT(ELECTION.id));
        expect(request.request.method).toBe('POST');
        expect(request.request.body).toEqual(ballot);
        request.flush({});
    });

    it('loads certified results through the centralized route', () => {
        const summary = { election: ELECTION, results: [], totalVotes: 0, turnoutPercentage: 0 } satisfies ElectionSummary;
        service.getResults(ELECTION.id).subscribe(value => expect(value).toEqual(summary));
        const request = http.expectOne(API_ENDPOINTS.ELECTIONS.RESULTS(ELECTION.id));
        expect(request.request.method).toBe('GET');
        request.flush(summary);
    });
});
