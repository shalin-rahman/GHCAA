import { createAuthServiceMock, createNotificationServiceMock } from '../../core/testing/testing-utils';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Jobs } from './jobs';
import { JobService } from '../../core/services/job.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { LookupService } from '../../core/services/lookup.service';
import { of, throwError } from 'rxjs';
import { signal } from '@angular/core';

describe('Jobs Component', () => {
    let component: Jobs;
    let fixture: ComponentFixture<Jobs>;
    let jobServiceMock: any;
    let authServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        jobServiceMock = {
            getJobsPage: vi.fn().mockReturnValue(of({ items: [], totalItems: 0, totalPages: 0, page: 1, pageSize: 20 })),
            createJob: vi.fn().mockReturnValue(of({ success: true })),
            updateJob: vi.fn().mockReturnValue(of({ success: true }))
        };

        authServiceMock = {
            currentUser: signal(null)
        };

        notificationServiceMock = createNotificationServiceMock();

        await TestBed.configureTestingModule({
            imports: [Jobs],
            providers: [
                { provide: JobService, useValue: jobServiceMock },
                { provide: AuthService, useValue: authServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock },
                { provide: LookupService, useValue: { getOptions: vi.fn().mockReturnValue(of([])), getAcademicYears: vi.fn().mockReturnValue(of([])) } }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Jobs);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load the first page of jobs on init', () => {
        expect(jobServiceMock.getJobsPage).toHaveBeenCalledWith(1, 20, {});
    });

    // NFR-P1: the list grows a page at a time and a filter change starts again from page 1.
    it('appends the next page and resets on a filter change', () => {
        const job = (id: number) => ({ id, title: `Job ${id}`, description: '' });
        jobServiceMock.getJobsPage
            .mockReturnValueOnce(of({ items: [job(1)], totalItems: 2, totalPages: 2, page: 1, pageSize: 20 }))
            .mockReturnValueOnce(of({ items: [job(2)], totalItems: 2, totalPages: 2, page: 2, pageSize: 20 }))
            .mockReturnValueOnce(of({ items: [job(3)], totalItems: 1, totalPages: 1, page: 1, pageSize: 20 }));

        component.loadJobs();
        expect(component.hasMore()).toBe(true);
        component.loadNextPage();
        expect(component.jobs().map(j => j.id)).toEqual([1, 2]);
        expect(component.hasMore()).toBe(false);

        component.onFilterChange({ target: { value: 'IT' } });
        expect(jobServiceMock.getJobsPage).toHaveBeenLastCalledWith(1, 20, { jobCategory: 'IT' });
        expect(component.jobs().map(j => j.id)).toEqual([3]);
    });

    // NFR-P1: a failed page is asked for again rather than skipped.
    it('retries the same page after a failure', () => {
        const job = (id: number) => ({ id, title: `Job ${id}`, description: '' });
        jobServiceMock.getJobsPage
            .mockReturnValueOnce(of({ items: [job(1)], totalItems: 2, totalPages: 2, page: 1, pageSize: 20 }))
            .mockReturnValueOnce(throwError(() => new Error('offline')))
            .mockReturnValueOnce(of({ items: [job(2)], totalItems: 2, totalPages: 2, page: 2, pageSize: 20 }));

        component.loadJobs();
        component.loadNextPage();
        component.loadNextPage();

        expect(jobServiceMock.getJobsPage.mock.calls.slice(-2).map((c: unknown[]) => c[0])).toEqual([2, 2]);
        expect(component.jobs().map(j => j.id)).toEqual([1, 2]);
    });

    // NFR-P1: reloading after a search, post or delete keeps the chosen category.
    it('keeps the category filter when the list reloads', () => {
        component.onFilterChange({ target: { value: 'IT' } });
        component.loadJobs();
        expect(jobServiceMock.getJobsPage).toHaveBeenLastCalledWith(1, 20, { jobCategory: 'IT' });
    });

    it('does not ask for another page when none is left', () => {
        jobServiceMock.getJobsPage.mockClear();
        component.loadNextPage();
        expect(jobServiceMock.getJobsPage).not.toHaveBeenCalled();
    });

    it('should format category names correctly', () => {
        expect(component.getCategoryName('IT')).toBe('IT & Software Development');
        expect(component.getCategoryName('Mentorship')).toBe('Mentorship & Career Guidance');
        expect(component.getCategoryName('PublicSector')).toBe('Govt. & Public Sector');
    });
});
