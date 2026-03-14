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
        category: 'IT',
        description: 'D',
        requirements: 'R',
        applicationDeadline: '2026-12-31',
        postedDate: '2026-03-08',
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
        req.flush([mockJob]);
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
});
