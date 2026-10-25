// @vitest-environment jsdom
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { OfficialsDashboard } from './officials-dashboard';
import { ElectionsService } from '../../core/services/elections.service';
import { ElectionPersonasService } from '../../core/services/election-personas.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { ELECTION_READINESS_LABELS, ELECTION_TOO_FEW_APPROVERS_WARNING } from '../../core/models/election.models';

describe('OfficialsDashboard', () => {
    let elections: any;
    let notify: any;
    let confirm: any;

    const readiness = {
        ready: false,
        items: [
            { key: 'ballot-key', met: true, attestation: false, advisory: false },
            { key: 'mock-election', met: false, attestation: true, advisory: false },
            { key: 'approvers', met: false, attestation: false, advisory: true }
        ]
    };

    function election(myPermissions: string[], phase = 'Polling') {
        return { id: 1, title: 'EC 2026', phase, myPermissions, eligibleVoterCount: 40, ballotKeyFingerprint: 'abcd' };
    }

    function create(myPermissions: string[], board: any = { turnout: null }, phase = 'Polling'): OfficialsDashboard {
        elections = {
            getAdminElections: vi.fn().mockReturnValue(of([election(myPermissions, phase)])),
            getBoard: vi.fn().mockReturnValue(of(board)),
            getNominations: vi.fn().mockReturnValue(of([{ id: 1, status: 'Submitted' }, { id: 2, status: 'Accepted' }])),
            getAudit: vi.fn().mockReturnValue(of([{ seq: 1 }, { seq: 2 }])),
            getAppointments: vi.fn().mockReturnValue(of([])),
            getApprovals: vi.fn().mockReturnValue(of([])),
            approve: vi.fn().mockReturnValue(of(undefined)),
            reject: vi.fn().mockReturnValue(of(undefined)),
            keyDestroyed: vi.fn().mockReturnValue(of(undefined)),
            getReadiness: vi.fn().mockReturnValue(of(readiness)),
            attestReadiness: vi.fn().mockReturnValue(of(undefined))
        };
        notify = { success: vi.fn(), error: vi.fn() };
        confirm = { confirm: vi.fn().mockReturnValue(of(true)) };
        TestBed.configureTestingModule({
            providers: [
                { provide: ElectionsService, useValue: elections },
                { provide: ElectionPersonasService, useValue: { listActive: vi.fn().mockReturnValue(of([{ id: 5, name: 'Polling Officer', minCount: 2, maxCount: null }])) } },
                { provide: NotificationService, useValue: notify },
                { provide: ConfirmDialogService, useValue: confirm }
            ]
        });
        return TestBed.runInInjectionContext(() => new OfficialsDashboard());
    }

    it('loads nothing the caller has no permission for', () => {
        const page = create(['ChangePhase']);

        expect(elections.getNominations).not.toHaveBeenCalled();
        expect(elections.getAudit).not.toHaveBeenCalled();
        expect(elections.getAppointments).not.toHaveBeenCalled();
        expect(page.can('ViewDashboard')).toBe(false);
        expect(page.showKeyCard()).toBe(false);
    });

    it('loads each card only for its own permission', () => {
        const page = create(['DecideNominations', 'ViewAudit']);

        expect(page.waitingNominations()).toBe(1);
        expect(page.audit().map(a => a.seq)).toEqual([2, 1]);
        expect(elections.getAppointments).not.toHaveBeenCalled();
    });

    it('warns when a role has fewer people than its minimum', () => {
        const page = create(['ViewDashboard']);

        expect(page.warnings()).toEqual([{ persona: 'Polling Officer', text: '0 of at least 2' }]);
    });

    it('hides turnout while polling because the board sends none', () => {
        const page = create(['ViewDashboard'], { turnout: null });

        expect(page.turnout()).toBeNull();
    });

    it('shows turnout once the board sends it', () => {
        const page = create(['ViewDashboard'], { turnout: { ballotsCast: 12, rollSize: 40 } }, 'Closed');

        expect(page.turnout()).toEqual({ ballotsCast: 12, rollSize: 40 });
    });

    it('hides approve on the caller\'s own request', () => {
        const page = create(['ViewDashboard', 'Approve']);

        expect(page.canApprove({ id: 9, requestedByMe: true } as any)).toBe(false);
        expect(page.canApprove({ id: 10, requestedByMe: false } as any)).toBe(true);
    });

    it('hides approve without the Approve permission', () => {
        const page = create(['ViewDashboard']);

        expect(page.canApprove({ id: 10, requestedByMe: false } as any)).toBe(false);
    });

    it('loads approvals only on request', () => {
        const page = create(['ViewDashboard', 'Approve']);
        expect(elections.getApprovals).not.toHaveBeenCalled();

        page.loadApprovals();

        expect(elections.getApprovals).toHaveBeenCalledWith(1);
        expect(page.approvals()).toEqual([]);
    });

    it('hides the destroy-key box before the declare', () => {
        expect(create(['SetBallotKey'], undefined, 'Polling').showDestroyKey()).toBe(false);
    });

    it('hides the destroy-key box from someone without the key permission', () => {
        expect(create(['ViewDashboard'], undefined, 'Declared').showDestroyKey()).toBe(false);
    });

    it('offers the destroy-key box after the declare to the key holder', () => {
        expect(create(['SetBallotKey'], undefined, 'Declared').showDestroyKey()).toBe(true);
    });

    it('records the key as destroyed once and hides the box', () => {
        const page = create(['SetBallotKey'], undefined, 'Declared');

        page.markKeyDestroyed({ checked: true } as HTMLInputElement);

        expect(elections.keyDestroyed).toHaveBeenCalledWith(1);
        expect(page.showDestroyKey()).toBe(false);
    });

    it('unticks the box when recording fails', () => {
        const page = create(['SetBallotKey'], undefined, 'Declared');
        elections.keyDestroyed.mockReturnValue(throwError(() => ({ status: 500 })));
        const box = { checked: true } as HTMLInputElement;

        page.markKeyDestroyed(box);

        expect(box.checked).toBe(false);
        expect(page.showDestroyKey()).toBe(true);
    });

    it('treats a 409 as already recorded and hides the box', () => {
        const page = create(['SetBallotKey'], undefined, 'Declared');
        elections.keyDestroyed.mockReturnValue(throwError(() => ({ status: 409 })));

        page.markKeyDestroyed({ checked: true } as HTMLInputElement);

        expect(page.showDestroyKey()).toBe(false);
    });

    it('rejects an approval after confirmation and drops the row', async () => {
        const page = create(['ViewDashboard', 'Approve']);
        page.approvals.set([{ id: 10, action: 'Declare', requestedBy: 'Asha' } as any, { id: 11, action: 'Count', requestedBy: 'Ben' } as any]);

        await page.reject(page.approvals()![0]);

        expect(elections.reject).toHaveBeenCalledWith(10, null);
        expect(page.approvals()!.map(a => a.id)).toEqual([11]);
    });

    it('does not reject when the confirmation is declined', async () => {
        const page = create(['ViewDashboard', 'Approve']);
        confirm.confirm.mockReturnValue(of(false));
        page.approvals.set([{ id: 10, action: 'Declare', requestedBy: 'Asha' } as any]);

        await page.reject(page.approvals()![0]);

        expect(elections.reject).not.toHaveBeenCalled();
        expect(page.approvals()!.length).toBe(1);
    });

    describe('readiness (37.12w)', () => {
        it('loads the list before polling for a caller who can view the dashboard', () => {
            const page = create(['ViewDashboard'], { turnout: null }, 'Campaign');

            expect(elections.getReadiness).toHaveBeenCalledWith(1);
            expect(page.showReadiness()).toBe(true);
            expect(page.readiness()!.items.length).toBe(3);
        });

        it('does not load it once polling is open or without the dashboard permission', () => {
            create(['ViewDashboard'], { turnout: null }, 'Polling');
            expect(elections.getReadiness).not.toHaveBeenCalled();

            TestBed.resetTestingModule();
            create(['ChangePhase'], { turnout: null }, 'Campaign');
            expect(elections.getReadiness).not.toHaveBeenCalled();
        });

        it('labels each item, folding in the approver warning', () => {
            const page = create(['ViewDashboard'], { turnout: null }, 'Campaign');
            const [key, mock, approvers] = page.readiness()!.items;

            expect(page.readinessLabel(key)).toBe(ELECTION_READINESS_LABELS['ballot-key']);
            expect(page.readinessLabel(approvers)).toBe(ELECTION_TOO_FEW_APPROVERS_WARNING);
            // FR-37.13t. Once enough officials can approve, the item reads as a plain tick, not the warning.
            expect(page.readinessLabel({ ...approvers, met: true })).toBe(ELECTION_READINESS_LABELS['approvers']);
            expect(page.readinessLabel({ ...mock, key: 'new-item' })).toBe('new-item');
        });

        it('offers the record button only on an unmet attestation, and only with ChangePhase', () => {
            const viewer = create(['ViewDashboard'], { turnout: null }, 'Campaign');
            expect(viewer.readiness()!.items.some(i => viewer.canAttest(i))).toBe(false);

            TestBed.resetTestingModule();
            const chief = create(['ViewDashboard', 'ChangePhase'], { turnout: null }, 'Campaign');
            expect(chief.readiness()!.items.filter(i => chief.canAttest(i)).map(i => i.key)).toEqual(['mock-election']);
        });

        it('records an item and reloads the list', () => {
            const page = create(['ViewDashboard', 'ChangePhase'], { turnout: null }, 'Campaign');

            page.attest(page.readiness()!.items[1]);

            expect(elections.attestReadiness).toHaveBeenCalledWith(1, 'mock-election');
            expect(elections.getReadiness).toHaveBeenCalledTimes(2);
            expect(notify.success).toHaveBeenCalled();
            expect(page.attestingKey()).toBeNull();
        });

        it('reloads on a 409 because someone else recorded it first', () => {
            const page = create(['ViewDashboard', 'ChangePhase'], { turnout: null }, 'Campaign');
            elections.attestReadiness.mockReturnValue(throwError(() => ({ status: 409 })));

            page.attest(page.readiness()!.items[1]);

            expect(elections.getReadiness).toHaveBeenCalledTimes(2);
            expect(page.attestingKey()).toBeNull();
        });

        it('does not reload on any other failure', () => {
            const page = create(['ViewDashboard', 'ChangePhase'], { turnout: null }, 'Campaign');
            elections.attestReadiness.mockReturnValue(throwError(() => ({ status: 400 })));

            page.attest(page.readiness()!.items[1]);

            expect(elections.getReadiness).toHaveBeenCalledTimes(1);
            expect(page.attestingKey()).toBeNull();
        });
    });
});
