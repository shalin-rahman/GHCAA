import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { AdminGovernance } from './admin-governance';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { EcPositionService } from '../../core/services/ec-position.service';
import { of, throwError } from 'rxjs';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';

describe('AdminGovernance Component', () => {
    let component: AdminGovernance;
    let fixture: ComponentFixture<AdminGovernance>;
    let adminServiceMock: any;
    let notificationServiceMock: any;
    let positionServiceMock: any;

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
        positionServiceMock = {
            getActive: vi.fn().mockReturnValue(of([
                { id: 1, key: 'President', name: 'President', seatCount: 1 },
                { id: 16, key: 'ExecutiveMember', name: 'Executive Member', seatCount: 3 }
            ]))
        };

        await TestBed.configureTestingModule({
            imports: [AdminGovernance],
            providers: [
                { provide: AdminService, useValue: adminServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock },
                { provide: EcPositionService, useValue: positionServiceMock }
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
        component.assignData.set({ memberId: 5, position: 1, reason: '', notifyMember: false });

        component.assignRole();

        expect(adminServiceMock.assignCommitteeRole).toHaveBeenCalledWith(
            1,
            expect.objectContaining({ notifyMember: false })
        );
    });

    it('assignRole posts notifyMember:true when the admin opts in', () => {
        component.selectedPeriod.set({ id: 1 });
        component.assignData.set({ memberId: 5, position: 1, reason: '', notifyMember: true });

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
        component.committeeMembers.set([{ id: 3, position: 'President', positionId: 1, member: { fullName: 'Old Holder' } }]);
        component.assignData.set({ memberId: 5, position: 1, reason: '', notifyMember: false, endCurrentHolderReason: '', endCurrentHolderNote: '' });

        component.assignRole();
        expect(adminServiceMock.assignCommitteeRole).not.toHaveBeenCalled();
        expect(notificationServiceMock.error).toHaveBeenCalled();

        component.assignData.update(d => ({ ...d, endCurrentHolderReason: 'Died' }));
        component.assignRole();
        expect(adminServiceMock.assignCommitteeRole).toHaveBeenCalledWith(1, expect.objectContaining({ endCurrentHolderReason: 'Died', endCurrentHolderNote: null }));
    });

    it('FR-34: assignRole to an empty seat sends no outgoing reason', () => {
        component.selectedPeriod.set({ id: 1 });
        component.assignData.set({ memberId: 5, position: 1, reason: '', notifyMember: false, endCurrentHolderReason: 'Died', endCurrentHolderNote: 'x' });

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

    it('FR-34: the position picker lists positions from the positions service', () => {
        expect(positionServiceMock.getActive).toHaveBeenCalled();
        expect(component.ecPositions().map(p => p.id)).toEqual([1, 16]);
    });

    it('FR-34: a multi-seat position never asks to replace a holder', () => {
        component.committeeMembers.set([{ id: 3, position: 'ExecutiveMember', positionId: 16, member: { fullName: 'One Holder' } }]);

        expect(component.holderOf(16)).toBeUndefined();
        expect(component.holderOf(1)).toBeUndefined();
    });

    it('FR-34: role labels come only from the name the API sends', () => {
        expect(component.getRoleName({ position: 'ExecutiveMember', positionName: 'Executive Member' })).toBe('Executive Member');
        expect(component.getRoleName({ position: 'GeneralSecretary' })).toBe('');
    });

    it('FR-34: activating a period shows the reason the API gives', async () => {
        vi.spyOn(TestBed.inject(ConfirmDialogService), 'confirm').mockReturnValue(of(true));
        adminServiceMock.activateGovernancePeriod.mockReturnValue(throwError(() => ({ error: { detail: 'Period not found.' } })));

        await component.activatePeriod(4);

        expect(notificationServiceMock.error).toHaveBeenCalledWith('Period not found.');
    });

    it('FR-34: a failed positions load is reported', () => {
        positionServiceMock.getActive.mockReturnValue(throwError(() => new Error('down')));

        component.loadPositions();

        expect(notificationServiceMock.error).toHaveBeenCalledWith('Could not load committee positions.');
    });
});
