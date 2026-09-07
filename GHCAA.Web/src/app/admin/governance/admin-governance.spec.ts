import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { AdminGovernance } from './admin-governance';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';

describe('AdminGovernance Component', () => {
    let component: AdminGovernance;
    let fixture: ComponentFixture<AdminGovernance>;
    let adminServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        adminServiceMock = {
            getGovernancePeriods: vi.fn().mockReturnValue(of([])),
            getCommitteeMembers: vi.fn().mockReturnValue(of([])),
            createGovernancePeriod: vi.fn().mockReturnValue(of({ success: true })),
            updateGovernancePeriod: vi.fn().mockReturnValue(of({ success: true })),
            activateGovernancePeriod: vi.fn().mockReturnValue(of({ success: true })),
            assignCommitteeRole: vi.fn().mockReturnValue(of({ success: true })),
            removeCommitteeMember: vi.fn().mockReturnValue(of({ success: true })),
            getMembers: vi.fn().mockReturnValue(of([]))
        };
        notificationServiceMock = createNotificationServiceMock();

        await TestBed.configureTestingModule({
            imports: [AdminGovernance],
            providers: [
                { provide: AdminService, useValue: adminServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminGovernance);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load periods and committee on init', () => {
        expect(adminServiceMock.getGovernancePeriods).toHaveBeenCalled();
    });

    // 82.52: notifyMember defaults to off and is threaded through to the API on both
    // assignment and removal, so an admin's choice per action actually reaches the backend.
    it('assignRole posts notifyMember:false by default', () => {
        component.selectedPeriod.set({ id: 1 });
        component.assignData.set({ memberId: 5, position: 8, reason: '', notifyMember: false });

        component.assignRole();

        expect(adminServiceMock.assignCommitteeRole).toHaveBeenCalledWith(
            1,
            expect.objectContaining({ notifyMember: false })
        );
    });

    it('assignRole posts notifyMember:true when the admin opts in', () => {
        component.selectedPeriod.set({ id: 1 });
        component.assignData.set({ memberId: 5, position: 8, reason: '', notifyMember: true });

        component.assignRole();

        expect(adminServiceMock.assignCommitteeRole).toHaveBeenCalledWith(
            1,
            expect.objectContaining({ notifyMember: true })
        );
    });

    it('confirmRemoveMember deletes with notifyMember=false by default', () => {
        component.selectedPeriod.set({ id: 1 });
        component.removeMember(9);

        component.confirmRemoveMember();

        expect(adminServiceMock.removeCommitteeMember).toHaveBeenCalledWith(9, false);
    });

    it('confirmRemoveMember deletes with notifyMember=true when the admin opts in', () => {
        component.selectedPeriod.set({ id: 1 });
        component.removeMember(9);
        component.removeNotifyMember.set(true);

        component.confirmRemoveMember();

        expect(adminServiceMock.removeCommitteeMember).toHaveBeenCalledWith(9, true);
    });
});
