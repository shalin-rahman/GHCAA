import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { JobService } from './job.service';
import { API_ENDPOINTS } from '../constants/api.endpoints';

describe('JobService', () => {
    let service: JobService;
    let httpMock: HttpTestingController;

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

    it('should fetch jobs', () => {
        const dummyJobs = [{ id: 1, title: 'Job 1' }];
        service.getJobs().subscribe(jobs => {
            expect(jobs.length).toBe(1);
            expect(jobs).toEqual(dummyJobs as any);
        });

        const req = httpMock.expectOne(API_ENDPOINTS.JOBS);
        expect(req.request.method).toBe('GET');
        req.flush(dummyJobs);
    });
});
