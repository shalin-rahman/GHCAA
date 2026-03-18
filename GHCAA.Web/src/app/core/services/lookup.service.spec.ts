import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { LookupService } from './lookup.service';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('LookupService', () => {
    let service: LookupService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [LookupService]
        });
        service = TestBed.inject(LookupService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should get lookups', () => {
        service.getLookups('test').subscribe(res => {
            expect(res).toEqual([]);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/test`);
        expect(req.request.method).toBe('GET');
        req.flush([]);
    });

    it('should get stats', () => {
        service.getStats().subscribe(res => {
            expect(res).toEqual({});
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/stats`);
        expect(req.request.method).toBe('GET');
        req.flush({});
    });

    it('should get stats silently with special header', () => {
        service.getStats(true).subscribe();
        const req = httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/stats`);
        expect(req.request.headers.get('X-Skip-Error-Notify')).toBe('true');
        req.flush({});
    });
});
