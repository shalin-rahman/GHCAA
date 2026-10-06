import { TestBed } from '@angular/core/testing';
import { of, Subject } from 'rxjs';
import { createAuthServiceMock, createNotificationServiceMock } from '../../../core/testing/testing-utils';
import { ElectionOfficials } from './election-officials';
import { ElectionsService } from '../../../core/services/elections.service';
import { ElectionPersonasService } from '../../../core/services/election-personas.service';
import { AdminService } from '../../../core/services/admin.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';
import { AdminElectionDto, ElectionApprovalDto, ElectionAppointmentDto } from '../../../core/models/election.models';

// TODO 37.13y.
describe('ElectionOfficials', () => {
    let electionsMock: any;
    let notifyMock: any;
    let confirmMock: any;
    const election = { id: 7, title: 'EC 2026', phase: 'Campaign', positions: [], candidates: [] } as unknown as AdminElectionDto;
    const appointment = (over: Partial<ElectionAppointmentDto> = {}): ElectionAppointmentDto => ({
        id: 3, electionId: 7, electionTitle: 'EC 2026', personaId: 2, personaName: 'Polling Officer', declarationText: '',
        userId: 9, memberId: null, displayName: 'Rina', email: 'rina@example.org', phone: null,
        appointedAt: '2026-10-01T10:00:00Z', acceptedAt: '2026-10-01T11:00:00Z', declarationSignedAt: null,
        revokedAt: null, revokedReason: null, expiresAt: null, isLive: true, isReturningOfficer: false, ...over
    });
    const approval: ElectionApprovalDto = {
        id: 40, electionId: 7, action: 'EmergencyRevoke', requestedByUserId: 1, requestedBy: 'shalin',
        requestedAt: '2026-10-01T10:00:00Z', expiresAt: '2026-10-03T10:00:00Z'
    };

    const create = async (superAdmin = false) => {
        electionsMock = {
            getAppointments: vi.fn().mockReturnValue(of([appointment(), appointment({ id: 4, revokedAt: '2026-10-02T10:00:00Z', isLive: false })])),
            appoint: vi.fn().mockReturnValue(of(appointment({ id: 5, displayName: 'Tanvir', isLive: false }))),
            revokeAppointment: vi.fn().mockReturnValue(of(undefined)),
            emergencyRevoke: vi.fn().mockReturnValue(of(approval))
        };
        notifyMock = createNotificationServiceMock();
        confirmMock = { confirm: vi.fn().mockReturnValue(of(true)) };
        await TestBed.configureTestingModule({
            imports: [ElectionOfficials],
            providers: [
                { provide: ElectionsService, useValue: electionsMock },
                { provide: ElectionPersonasService, useValue: { listActive: vi.fn().mockReturnValue(of([])) } },
                { provide: AdminService, useValue: { getMembers: vi.fn().mockReturnValue(of({ items: [] })) } },
                { provide: AuthService, useValue: createAuthServiceMock({ hasRole: superAdmin }) },
                { provide: NotificationService, useValue: notifyMock },
                { provide: ConfirmDialogService, useValue: confirmMock }
            ]
        }).compileComponents();
        const fixture = TestBed.createComponent(ElectionOfficials);
        fixture.componentRef.setInput('election', election);
        fixture.detectChanges();
        return fixture.componentInstance;
    };

    it('splits live and revoked appointments', async () => {
        const component = await create();
        expect(electionsMock.getAppointments).toHaveBeenCalledWith(7);
        expect(component.open().map(a => a.id)).toEqual([3]);
        expect(component.closed().map(a => a.id)).toEqual([4]);
    });

    it('drops a member search reply when the query has moved on', async () => {
        const component = await create();
        const reply = new Subject<{ items: { id: number; fullName: string; membershipNumber: string }[] }>();
        (TestBed.inject(AdminService).getMembers as any).mockReturnValue(reply);
        component.memberQuery.set('kar');
        (component as any).runSearch('kar');
        component.memberQuery.set('karim');
        reply.next({ items: [{ id: 11, fullName: 'Karim', membershipNumber: 'M-1' }] });
        expect(component.memberHits()).toEqual([]);
    });

    it('needs a role, then a member or a name and email', async () => {
        const component = await create();
        expect(component.buildRequest()).toBe('Choose a role.');
        component.form.personaId = 2;
        component.form.displayName = 'Tanvir';
        expect(component.buildRequest()).toBe('Choose a member, or give a name and email.');
    });

    it('sends a picked member without the name fields', async () => {
        const component = await create();
        component.form.personaId = 2;
        component.form.displayName = 'left over';
        component.pickMember({ id: 11, fullName: 'Karim', membershipNumber: 'M-1' });
        expect(component.buildRequest()).toEqual({ personaId: 2, memberId: 11, displayName: null, email: null, phone: null, isReturningOfficer: false });
    });

    it('appoints someone outside the membership and clears the form', async () => {
        const component = await create();
        component.form = { personaId: 2, displayName: ' Tanvir ', email: 'tanvir@example.org', phone: '', isReturningOfficer: true };
        component.appoint();
        expect(electionsMock.appoint).toHaveBeenCalledWith(7, { personaId: 2, memberId: null, displayName: 'Tanvir', email: 'tanvir@example.org', phone: null, isReturningOfficer: true });
        expect(component.appointments().some(a => a.id === 5)).toBe(true);
        expect(component.form.personaId).toBe(0);
    });

    it('does not call the API when the form is incomplete', async () => {
        const component = await create();
        component.appoint();
        expect(notifyMock.warning).toHaveBeenCalledWith('Choose a role.');
        expect(electionsMock.appoint).not.toHaveBeenCalled();
    });

    it('revokes only after the confirm dialog', async () => {
        const component = await create();
        confirmMock.confirm.mockReturnValue(of(false));
        await component.revoke(appointment());
        expect(electionsMock.revokeAppointment).not.toHaveBeenCalled();

        confirmMock.confirm.mockReturnValue(of(true));
        await component.revoke(appointment());
        expect(electionsMock.revokeAppointment).toHaveBeenCalledWith(3, null);
    });

    it('flags SuperAdmin from the auth service', async () => {
        expect((await create(true)).isSuperAdmin()).toBe(true);
    });

    it('needs a reason for an emergency revoke and passes the approval up', async () => {
        const component = await create(true);
        const emitted: ElectionApprovalDto[] = [];
        component.pending.subscribe(value => emitted.push(value));
        component.startEmergency(appointment());

        component.requestEmergency(appointment());
        expect(electionsMock.emergencyRevoke).not.toHaveBeenCalled();

        component.emergencyReason = ' Conflict of interest found ';
        component.requestEmergency(appointment());
        expect(electionsMock.emergencyRevoke).toHaveBeenCalledWith(3, 'Conflict of interest found');
        expect(emitted).toEqual([approval]);
        expect(component.emergencyId()).toBeNull();
    });
});
