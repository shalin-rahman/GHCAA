import { createAuthServiceMock, createNotificationServiceMock } from '../../core/testing/testing-utils';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Jobs } from './jobs';
import { JobService } from '../../core/services/job.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { LookupService } from '../../core/services/lookup.service';
import { of } from 'rxjs';
import { signal } from '@angular/core';

describe('Jobs Component', () => {
    let component: Jobs;
    let fixture: ComponentFixture<Jobs>;
    let jobServiceMock: any;
    let authServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        jobServiceMock = {
            getJobs: vi.fn().mockReturnValue(of([])),
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

    it('should load jobs on init', () => {
        expect(jobServiceMock.getJobs).toHaveBeenCalled();
    });

    it('should format category names correctly', () => {
        expect(component.getCategoryName('IT')).toBe('IT & Software Development');
        expect(component.getCategoryName('Mentorship')).toBe('Mentorship & Career Guidance');
        expect(component.getCategoryName('PublicSector')).toBe('Govt. & Public Sector');
    });
});
