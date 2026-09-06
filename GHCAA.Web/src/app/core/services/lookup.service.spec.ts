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

    it('getOptions maps the API rows to {value,label} when the group has rows', () => {
        service.getOptions('Gender').subscribe(res => {
            expect(res).toEqual([{ value: 'Male', label: 'Male' }]);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/Gender`);
        req.flush([{ value: 'Male', label: 'Male', displayOrder: 1 }]);
    });

    it('getOptions falls back to the built-in wording when the group has no rows', () => {
        service.getOptions('MembershipStatus').subscribe(res => {
            expect(res.find(o => o.value === 'Applied')?.label).toBe('Pending Approval');
            expect(res.find(o => o.value === 'InactivePayment')?.label).toBe('Inactive (Unpaid)');
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/MembershipStatus`);
        req.flush([]);
    });

    it('getOptions returns an empty list for an unknown group with no rows', () => {
        service.getOptions('NotARealGroup').subscribe(res => {
            expect(res).toEqual([]);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/NotARealGroup`);
        req.flush([]);
    });

    it('getAcademicYears maps API rows to numbers when the group has rows', () => {
        service.getAcademicYears().subscribe(res => {
            expect(res).toEqual([2020, 2021]);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/PassingYear`);
        req.flush([{ value: '2020', label: '2020' }, { value: '2021', label: '2021' }]);
    });

    it('getAcademicYears falls back to a computed 1950..current range when the group is empty', () => {
        service.getAcademicYears().subscribe(res => {
            expect(res[0]).toBe(new Date().getFullYear());
            expect(res[res.length - 1]).toBe(1950);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/PassingYear`);
        req.flush([]);
    });
});
