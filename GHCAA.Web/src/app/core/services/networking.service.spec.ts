import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { NetworkingService } from './networking.service';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('NetworkingService', () => {
    let service: NetworkingService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [NetworkingService]
        });
        service = TestBed.inject(NetworkingService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should fetch committee members', () => {
        const dummyCommittee = [{ id: 1, fullName: 'Member' }];
        service.getCommittee().subscribe(members => {
            expect(members.length).toBe(1);
            expect(members).toEqual(dummyCommittee);
        });

        const req = httpMock.expectOne(API_ENDPOINTS.NETWORKING.COMMITTEE);
        expect(req.request.method).toBe('GET');
        req.flush(dummyCommittee);
    });

    it('should fetch committee periods', () => {
        const dummyPeriods = [{ id: 1, title: '2023-2025' }];
        service.getPeriods().subscribe(periods => {
            expect(periods.length).toBe(1);
            expect(periods).toEqual(dummyPeriods);
        });

        const req = httpMock.expectOne(API_ENDPOINTS.NETWORKING.COMMITTEE_PERIODS);
        expect(req.request.method).toBe('GET');
        req.flush(dummyPeriods);
    });

    it('should search members with filters', () => {
        const filters = { query: 'test', year: 2020 };
        const dummyResults = [{ id: 1, fullName: 'Test User' }];

        service.searchMembers(filters).subscribe(results => {
            expect(results.length).toBe(1);
            expect(results).toEqual(dummyResults);
        });

        const req = httpMock.expectOne(r => r.url === API_ENDPOINTS.NETWORKING.SEARCH && r.params.get('query') === 'test');
        expect(req.request.method).toBe('GET');
        req.flush(dummyResults);
    });
});
