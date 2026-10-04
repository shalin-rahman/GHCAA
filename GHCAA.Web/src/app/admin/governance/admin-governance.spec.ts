import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { AdminGovernance } from './admin-governance';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { LookupService } from '../../core/services/lookup.service';
import { of } from 'rxjs';

describe('AdminGovernance Component', () => {
    let component: AdminGovernance;
    let fixture: ComponentFixture<AdminGovernance>;
    let adminServiceMock: any;
    let notificationServiceMock: any;
    let lookupServiceMock: any;

    beforeEach(async () => {
        adminServiceMock = {
            getGovernancePeriods: vi.fn().mockReturnValue(of([])),
            getCommitteeMembers: vi.fn().mockReturnValue(of([])),
            createGovernancePeriod: vi.fn().mockReturnValue(of({ success: true })),
            updateGovernancePeriod: vi.fn().mockReturnValue(of({ success: true })),
            activateGovernancePeriod: vi.fn().mockReturnValue(of({ success: true })),
            assignCommitteeRole: vi.fn().mockReturnValue(of({ success: true })),
            endCommitteeTerm: vi.fn().mockReturnValue(of({ success: true })),
            getCommitteeSeats: vi.fn().mockReturnValue(of([])),
            getMembers: vi.fn().mockReturnValue(of([]))
        };
        notificationServiceMock = createNotificationServiceMock();
        lookupServiceMock = {
            getOptions: vi.fn().mockReturnValue(of([{ value: '1', label: 'President' }]))
        };

        await TestBed.configureTestingModule({
            imports: [AdminGovernance],
            providers: [
                { provide: AdminService, useValue: adminServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock },
                { provide: LookupService, useValue: lookupServiceMock }
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
        component.assignData.set({ memberId: 5, position: 'President', reason: '', notifyMember: false });

        component.assignRole();

        expect(adminServiceMock.assignCommitteeRole).toHaveBeenCalledWith(
            1,
            expect.objectContaining({ notifyMember: false })
        );
    });

    it('assignRole posts notifyMember:true when the admin opts in', () => {
        component.selectedPeriod.set({ id: 1 });
        component.assignData.set({ memberId: 5, position: 'President', reason: '', notifyMember: true });

        component.assignRole();

        expect(adminServiceMock.assignCommitteeRole).toHaveBeenCalledWith(
            1,
            expect.objectContaining({ notifyMember: true })
        );
    });

    it('FR-34: confirmRemoveMember sends the reason with notifyMember=false by default', () => {
        component.selectedPeriod.set({ id: 1 });
        component.removeMember(9);
        component.removeReason.set('Resigned');

        component.confirmRemoveMember();

        expect(adminServiceMock.endCommitteeTerm).toHaveBeenCalledWith(9, { reason: 'Resigned', note: null, notifyMember: false });
    });

    it('FR-34: confirmRemoveMember sends notifyMember=true when the admin opts in', () => {
        component.selectedPeriod.set({ id: 1 });
        component.removeMember(9);
        component.removeReason.set('TermEnded');
        component.removeNotifyMember.set(true);

        component.confirmRemoveMember();

        expect(adminServiceMock.endCommitteeTerm).toHaveBeenCalledWith(9, expect.objectContaining({ notifyMember: true }));
    });

    it('FR-34: confirmRemoveMember does nothing until a reason is picked', () => {
        component.selectedPeriod.set({ id: 1 });
        component.removeMember(9);

        component.confirmRemoveMember();

        expect(component.canEndTerm()).toBe(false);
        expect(adminServiceMock.endCommitteeTerm).not.toHaveBeenCalled();
    });

    it('FR-34: reason Other needs a note, and the note is trimmed', () => {
        component.selectedPeriod.set({ id: 1 });
        component.removeMember(9);
        component.removeReason.set('Other');
        component.removeNote.set('   ');
        expect(component.canEndTerm()).toBe(false);

        component.removeNote.set('  Moved abroad  ');
        component.confirmRemoveMember();

        expect(adminServiceMock.endCommitteeTerm).toHaveBeenCalledWith(9, expect.objectContaining({ reason: 'Other', note: 'Moved abroad' }));
    });

    it('FR-34: assignRole to a held seat needs the outgoing holder reason', () => {
        component.selectedPeriod.set({ id: 1 });
        component.committeeMembers.set([{ id: 3, position: 'President', member: { fullName: 'Old Holder' } }]);
        component.assignData.set({ memberId: 5, position: 'President', reason: '', notifyMember: false, endCurrentHolderReason: '', endCurrentHolderNote: '' });

        component.assignRole();
        expect(adminServiceMock.assignCommitteeRole).not.toHaveBeenCalled();
        expect(notificationServiceMock.error).toHaveBeenCalled();

        component.assignData.update(d => ({ ...d, endCurrentHolderReason: 'Died' }));
        component.assignRole();
        expect(adminServiceMock.assignCommitteeRole).toHaveBeenCalledWith(1, expect.objectContaining({ endCurrentHolderReason: 'Died', endCurrentHolderNote: null }));
    });

    it('FR-34: assignRole to an empty seat sends no outgoing reason', () => {
        component.selectedPeriod.set({ id: 1 });
        component.assignData.set({ memberId: 5, position: 'President', reason: '', notifyMember: false, endCurrentHolderReason: 'Died', endCurrentHolderNote: 'x' });

        component.assignRole();

        expect(adminServiceMock.assignCommitteeRole).toHaveBeenCalledWith(1, expect.objectContaining({ endCurrentHolderReason: null, endCurrentHolderNote: null }));
    });

    it('FR-34: vacantSeats lists only seats without a holder', () => {
        component.seats.set([
            { position: 'President', holder: { id: 1 } },
            { position: 'Treasurer', holder: null, vacancyReason: 'Resigned' }
        ]);

        expect(component.vacantSeats().map(s => s.position)).toEqual(['Treasurer']);
    });
});
