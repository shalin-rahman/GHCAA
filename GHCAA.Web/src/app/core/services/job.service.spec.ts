import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { JobService } from './job.service';
import { Job } from '../models/business.models';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('JobService', () => {
    let service: JobService;
    let httpMock: HttpTestingController;

    const mockJob: Job = {
        id: 1,
        title: 'Software Engineer',
        companyName: 'Tech Co',
        location: 'Remote',
        jobCategory: 'IT',
        description: 'D',
        requirements: 'R',
        applicationDeadline: '31-12-2026',
        postedDate: '08-03-2026',
        postedByMemberId: 10,
        postedByMemberName: 'Member',
        isActive: true,
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [JobService]
        });
        service = TestBed.inject(JobService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should get jobs', () => {
        service.getJobs({ query: 'tech' }).subscribe(res => {
            expect(res.length).toBe(1);
        });
        const req = httpMock.expectOne(req => req.url === API_ENDPOINTS.JOBS);
        expect(req.request.method).toBe('GET');
        expect(req.request.params.get('query')).toBe('tech');
        req.flush([mockJob]);
    });

    it('should pass backend job category filters through query params', () => {
        service.getJobs({ jobCategory: 'Mentorship', query: 'career' }).subscribe();
        const req = httpMock.expectOne(req => req.url === API_ENDPOINTS.JOBS);
        expect(req.request.method).toBe('GET');
        expect(req.request.params.get('jobCategory')).toBe('Mentorship');
        expect(req.request.params.get('query')).toBe('career');
        req.flush([]);
    });

    it('should get jobs silently with special header', () => {
        service.getJobs(undefined, true).subscribe();
        const req = httpMock.expectOne(req => req.url === API_ENDPOINTS.JOBS);
        expect(req.request.headers.get('X-Skip-Error-Notify')).toBe('true');
        req.flush([]);
    });

    it('should get job by id', () => {
        service.getJobById(1).subscribe(res => {
            expect(res.title).toBe('Software Engineer');
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.JOBS}/1`);
        expect(req.request.method).toBe('GET');
        req.flush(mockJob);
    });

    it('should create job', () => {
        service.createJob(mockJob).subscribe(res => {
            expect(res.id).toBe(1);
        });
        const req = httpMock.expectOne(API_ENDPOINTS.JOBS);
        expect(req.request.method).toBe('POST');
        req.flush(mockJob);
    });

    it('should update job', () => {
        service.updateJob(1, { id: 1, title: 'New Title' }).subscribe(res => {
            expect(res.success).toBe(true);
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.JOBS}/1`);
        expect(req.request.method).toBe('PUT');
        req.flush({ success: true });
    });

    // NFR-P1
    it('should send the filters with the page request', () => {
        service.getJobsPage(3, 20, { jobCategory: 'IT', query: 'dev' }).subscribe(page => expect(page.page).toBe(3));
        const req = httpMock.expectOne(r => r.url === API_ENDPOINTS.JOBS);
        expect(req.request.params.get('page')).toBe('3');
        expect(req.request.params.get('jobCategory')).toBe('IT');
        expect(req.request.params.get('query')).toBe('dev');
        req.flush({ items: [], totalItems: 0, totalPages: 0, page: 3, pageSize: 20 });
    });
});
