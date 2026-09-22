import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ScholarshipService } from './scholarship.service';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('ScholarshipService', () => {
    let service: ScholarshipService;
    let http: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [ScholarshipService, provideHttpClient(), provideHttpClientTesting()]
        });
        service = TestBed.inject(ScholarshipService);
        http = TestBed.inject(HttpTestingController);
    });

    afterEach(() => http.verify());

    it('loads public calls', () => {
        service.getPublicCalls().subscribe();
        expect(http.expectOne(`${API_ENDPOINTS.SCHOLARSHIPS}/public/calls`).request.method).toBe('GET');
    });

    it('posts an application for a call', () => {
        const dto = { applicantName: 'A', applicantEmail: 'a@example.com', applicantPhone: '1', institutionName: 'I', class: '8', guardianName: 'G', householdIncome: 1, needStatement: 'N', meritStatement: 'M' };
        service.submitApplication(4, dto).subscribe();
        const req = http.expectOne(`${API_ENDPOINTS.SCHOLARSHIPS}/calls/4/applications`);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual(dto);
    });

    it('passes email when looking up status', () => {
        service.getStatus('REF/1', 'a@example.com').subscribe();
        const req = http.expectOne(r => r.url === `${API_ENDPOINTS.SCHOLARSHIPS}/status/REF%2F1`);
        expect(req.request.params.get('email')).toBe('a@example.com');
    });

    it('surfaces an unknown status reference as a not-found response', () => {
        let error: { status?: number } | undefined;
        service.getStatus('UNKNOWN', 'a@example.com').subscribe({
            error: response => error = response
        });

        const req = http.expectOne(request =>
            request.url === `${API_ENDPOINTS.SCHOLARSHIPS}/status/UNKNOWN`
            && request.params.get('email') === 'a@example.com');
        req.flush({}, { status: 404, statusText: 'Not Found' });
        expect(error?.status).toBe(404);
    });
});
