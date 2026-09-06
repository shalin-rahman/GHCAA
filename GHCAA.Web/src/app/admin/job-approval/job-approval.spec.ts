import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { JobApproval } from './job-approval';
import { JobService } from '../../core/services/job.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';

describe('JobApproval Component', () => {
    let component: JobApproval;
    let fixture: ComponentFixture<JobApproval>;
    let jobServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        jobServiceMock = {
            getPendingJobs: vi.fn().mockReturnValue(of([])),
            approveJob: vi.fn().mockReturnValue(of({ success: true })),
            rejectJob: vi.fn().mockReturnValue(of({ success: true }))
        };

        notificationServiceMock = createNotificationServiceMock();

        await TestBed.configureTestingModule({
            imports: [JobApproval],
            providers: [
                { provide: JobService, useValue: jobServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(JobApproval);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load pending jobs on init', () => {
        expect(jobServiceMock.getPendingJobs).toHaveBeenCalled();
    });

    it('should approve the selected job', () => {
        const job: any = { id: 1, title: 'Test Job', companyName: 'Acme' };
        component.viewJob(job);
        component.approve();
        expect(jobServiceMock.approveJob).toHaveBeenCalledWith(1, true);
    });

    it('should reject the selected job with a reason', () => {
        const job: any = { id: 5, title: 'Test Job', companyName: 'Acme' };
        component.viewJob(job);
        component.rejectReason.set('Not appropriate');
        component.reject();
        expect(jobServiceMock.rejectJob).toHaveBeenCalledWith(5, 'Not appropriate', true);
    });

    // 82.52 batch 3: notifyMember defaults true (matches the unconditional notify these
    // already did) and the admin can opt out per job.
    it('viewJob resets notifyMember to true and honors opt-out on approve', () => {
        const job: any = { id: 2, title: 'Another Job', companyName: 'Acme' };
        component.viewJob(job);
        expect(component.notifyMember()).toBe(true);
        component.notifyMember.set(false);
        component.approve();
        expect(jobServiceMock.approveJob).toHaveBeenCalledWith(2, false);
    });

    it('rejectJob carries notifyMember:false when the admin opts out', () => {
        const job: any = { id: 3, title: 'Third Job', companyName: 'Acme' };
        component.viewJob(job);
        component.rejectReason.set('Not appropriate');
        component.notifyMember.set(false);
        component.reject();
        expect(jobServiceMock.rejectJob).toHaveBeenCalledWith(3, 'Not appropriate', false);
    });
});
