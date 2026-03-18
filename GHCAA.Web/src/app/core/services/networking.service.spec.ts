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

    it('should get committee', () => {
        service.getCommittee({ periodId: 1 }).subscribe(res => {
            expect(res.length).toBe(0);
        });
        const req = httpMock.expectOne(req => req.url === API_ENDPOINTS.NETWORKING.COMMITTEE);
        expect(req.request.method).toBe('GET');
        req.flush([]);
    });

    it('should get committee silently with special header', () => {
        service.getCommittee({}, true).subscribe();
        const req = httpMock.expectOne(req => req.url === API_ENDPOINTS.NETWORKING.COMMITTEE);
        expect(req.request.headers.get('X-Skip-Error-Notify')).toBe('true');
        req.flush([]);
    });

    it('should get periods', () => {
        service.getPeriods().subscribe(res => {
            expect(res.length).toBe(0);
        });
        const req = httpMock.expectOne(API_ENDPOINTS.NETWORKING.COMMITTEE_PERIODS);
        expect(req.request.method).toBe('GET');
        req.flush([]);
    });

    it('should search members', () => {
        service.searchMembers({ query: 'shalin' }).subscribe(res => {
            expect(res.items.length).toBe(0);
        });
        const req = httpMock.expectOne(req => req.url === API_ENDPOINTS.NETWORKING.SEARCH);
        expect(req.request.method).toBe('GET');
        req.flush({ items: [], totalItems: 0, totalPages: 0, currentPage: 1, pageSize: 12 });
    });

    it('should get updates', () => {
        service.getUpdates({ count: 5 }).subscribe(res => {
            expect(res.length).toBe(0);
        });
        const req = httpMock.expectOne(req => req.url === API_ENDPOINTS.NETWORKING.UPDATES);
        expect(req.request.method).toBe('GET');
        req.flush([]);
    });
});
