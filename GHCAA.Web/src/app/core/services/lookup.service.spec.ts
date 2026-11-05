import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { LookupService } from './lookup.service';
import { API_ENDPOINTS, LOOKUP_GROUPS, setOrgLabelMaps } from '../constants/app.constants';

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

    it('the MemberCategory fallback uses the labels loaded after startup', () => {
        setOrgLabelMaps({ memberCategoryLabels: { Active: 'Core Member' } });
        let labels: string[] = [];
        service.getOptionsOrFallback(LOOKUP_GROUPS.MemberCategory).subscribe(o => labels = o.map(x => x.label));
        httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/${LOOKUP_GROUPS.MemberCategory}`).flush([]);
        setOrgLabelMaps(null);
        expect(labels).toContain('Core Member');
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

    it('getOptionsOrFallback falls back when the request fails', () => {
        let res: { value: string; label: string }[] = [];
        service.getOptionsOrFallback('TShirtSize').subscribe(r => res = r);
        httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/TShirtSize`).flush('boom', { status: 500, statusText: 'Server Error' });
        expect(res.map(o => o.value)).toContain('XL');
    });

    it('getValues returns the plain values of a group', () => {
        let res: string[] = [];
        service.getValues('ProfessionalSector').subscribe(r => res = r);
        httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/ProfessionalSector`).flush([{ value: 'Banking', label: 'Banking' }]);
        expect(res).toEqual(['Banking']);
    });

    it('getAcademicData puts the HSC streams after None and drops served duplicates', () => {
        let res: any;
        service.getAcademicData().subscribe(r => res = r);
        httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/Degree`).flush([{ value: 'HSC', label: 'HSC' }]);
        httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/AcademicSubject`)
            .flush([{ value: 'None', label: 'None' }, { value: 'Physics', label: 'Physics' }, { value: 'Science', label: 'Science' }]);
        httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/ProfessionalSector`).flush([]);
        expect(res.certificates).toEqual(['HSC']);
        expect(res.subjects).toEqual(['None', 'Science', 'Arts & Humanities', 'Business Studies', 'Physics']);
        expect(res.sectors).toEqual([]);
    });

    it('getAcademicData keeps only None and the HSC streams when every group fails', () => {
        let res: any;
        service.getAcademicData().subscribe(r => res = r);
        const error = { status: 500, statusText: 'Server Error' };
        httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/Degree`).flush('x', error);
        httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/AcademicSubject`).flush('x', error);
        httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/ProfessionalSector`).flush('x', error);
        expect(res).toEqual({ certificates: [], subjects: ['None', 'Science', 'Arts & Humanities', 'Business Studies'], sectors: [] });
    });

    it('getAcademicData adds None first when the server omits it', () => {
        let res: any;
        service.getAcademicData().subscribe(r => res = r);
        httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/Degree`).flush([]);
        httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/AcademicSubject`).flush([{ value: 'Physics', label: 'Physics' }]);
        httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/ProfessionalSector`).flush([]);
        expect(res.subjects).toEqual(['None', 'Science', 'Arts & Humanities', 'Business Studies', 'Physics']);
    });

    it('fetches a group once and serves later callers from the cache', () => {
        let first: any[] = [];
        let second: any[] = [];
        service.getOptions('Gender').subscribe(r => first = r);
        httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/Gender`).flush([{ value: 'Male', label: 'Male' }]);
        service.getValues('Gender').subscribe(r => second = r);
        httpMock.expectNone(`${API_ENDPOINTS.LOOKUPS}/Gender`);
        expect(first).toEqual([{ value: 'Male', label: 'Male' }]);
        expect(second).toEqual(['Male']);
    });

    it('does not cache a failed request', () => {
        service.getOptionsOrFallback('Gender').subscribe();
        httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/Gender`).flush('x', { status: 500, statusText: 'Server Error' });
        let res: any[] = [];
        service.getOptions('Gender').subscribe(r => res = r);
        httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/Gender`).flush([{ value: 'Female', label: 'Female' }]);
        expect(res).toEqual([{ value: 'Female', label: 'Female' }]);
    });

    it('getAcademicYears maps API rows to numbers when the group has rows', () => {
        service.getAcademicYears().subscribe(res => {
            expect(res).toEqual([2020, 2021]);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/PassingYear`);
        req.flush([{ value: '2020', label: '2020' }, { value: '2021', label: '2021' }]);
    });

    it('getAcademicYears falls back through Dhaka year + 6 down to 1950 when the group is empty', () => {
        service.getAcademicYears().subscribe(res => {
            const dhakaYear = new Date(Date.now() + 6 * 60 * 60 * 1000).getUTCFullYear();
            expect(res[0]).toBe(dhakaYear + 6);
            expect(res[res.length - 1]).toBe(1950);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.LOOKUPS}/PassingYear`);
        req.flush([]);
    });
});
