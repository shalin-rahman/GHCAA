import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { createAuthServiceMock, createNotificationServiceMock } from '../../core/testing/testing-utils';
import { AuthService } from '../../core/services/auth.service';
import { AdminElections } from './admin-elections';
import { ElectionsService } from '../../core/services/elections.service';
import { NotificationService } from '../../core/services/notification.service';
import { AdminService } from '../../core/services/admin.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { AdminElectionDto, AdminElectionStepResult, ElectionApprovalDto, ElectionReadinessDto, NominationViewDto, ELECTION_TOO_FEW_APPROVERS_WARNING } from '../../core/models/election.models';
import { ExportUtil } from '../../core/utils/export.util';

describe('AdminElections ballot key', () => {
    let component: AdminElections;
    let electionsMock: any;
    let notifyMock: any;
    let confirmMock: any;
    let adminMock: any;
    let authMock: any;
    let asAdmin: boolean;
    const election = { id: 7, title: 'EC 2026', phase: 'Campaign', ballotKeyFingerprint: null, positions: [], candidates: [] } as unknown as AdminElectionDto;
    const ran = (updated: AdminElectionDto): AdminElectionStepResult => ({ election: updated, pending: null });
    const readiness: ElectionReadinessDto = {
        ready: false,
        items: [
            { key: 'ballot-key', met: false, attestation: false, advisory: false },
            { key: 'approvers', met: false, attestation: false, advisory: true }
        ]
    };
    const request: ElectionApprovalDto = {
        id: 30, electionId: 7, action: 'Publish', requestedByUserId: 1, requestedBy: 'alice',
        requestedAt: '2026-10-01T10:00:00Z', expiresAt: '2026-10-03T10:00:00Z'
    };

    beforeEach(async () => {
        electionsMock = {
            getAdminElections: vi.fn().mockReturnValue(of([election])),
            setBallotKey: vi.fn().mockReturnValue(of(ran({ ...election, ballotKeyFingerprint: 'ab12' }))),
            publish: vi.fn(),
            declare: vi.fn().mockReturnValue(of(null)),
            approve: vi.fn().mockReturnValue(of(undefined)),
            reject: vi.fn().mockReturnValue(of(undefined)),
            runCount: vi.fn().mockReturnValue(of({ results: [], pending: null })),
            getReadiness: vi.fn().mockReturnValue(of(readiness))
        };
        asAdmin = true;
        authMock = createAuthServiceMock();
        authMock.hasRole = vi.fn(() => asAdmin);
        notifyMock = createNotificationServiceMock();
        confirmMock = { confirm: vi.fn().mockReturnValue(of(true)) };
        adminMock = { getGovernancePeriods: vi.fn().mockReturnValue(of([{ id: 4, title: 'EC 2027', startDate: '2027-01-01', endDate: '2028-12-31', isActive: false }])) };

        await TestBed.configureTestingModule({
            imports: [AdminElections],
            providers: [
                { provide: ElectionsService, useValue: electionsMock },
                { provide: NotificationService, useValue: notifyMock },
                { provide: ConfirmDialogService, useValue: confirmMock },
                { provide: AdminService, useValue: adminMock },
                { provide: AuthService, useValue: authMock }
            ]
        }).compileComponents();

        component = TestBed.createComponent(AdminElections).componentInstance;
    });

    afterEach(() => vi.restoreAllMocks());

    // 37.1t
    describe('governance period picker', () => {
        const fillForm = () => {
            component.form.title = 'EC 2027';
            component.form.positions = [{ title: 'President', seats: 1 }];
            component.form.nominationOpensOn = '2027-03-01';
            component.form.nominationClosesOn = '2027-03-08';
            component.form.pollingOpensOn = '2027-03-15';
            component.form.pollingClosesOn = '2027-03-16';
        };

        it('lists the governance periods for the select', () => {
            expect(component.periods()).toEqual([{ id: 4, label: 'EC 2027 (2027 - 2028)' }]);
        });

        it('does not create an election until a period is chosen', () => {
            electionsMock.create = vi.fn();
            fillForm();

            component.create();

            expect(electionsMock.create).not.toHaveBeenCalled();
            expect(notifyMock.warning).toHaveBeenCalled();
        });

        it('sends the chosen period id', () => {
            electionsMock.create = vi.fn().mockReturnValue(of(election));
            fillForm();
            component.selectedPeriodId = 4;

            component.create();

            expect(electionsMock.create).toHaveBeenCalledWith(expect.objectContaining({ ecPeriodId: 4 }));
            expect(component.selectedPeriodId).toBeNull();
        });

        it('sends the test-run flag and clears it after', () => {
            electionsMock.create = vi.fn().mockReturnValue(of(election));
            fillForm();
            component.selectedPeriodId = 4;
            component.form.isTest = true;

            component.create();

            expect(electionsMock.create).toHaveBeenCalledWith(expect.objectContaining({ isTest: true }));
            expect(component.form.isTest).toBe(false);
        });

        it('sends the tie rule and both dates, then resets them', () => {
            electionsMock.create = vi.fn().mockReturnValue(of(election));
            fillForm();
            component.selectedPeriodId = 4;
            component.form.tieRule = 'RunOff';
            component.form.scrutinyOn = '2027-03-10';
            component.form.withdrawalClosesOn = '2027-03-12';

            component.create();

            expect(electionsMock.create).toHaveBeenCalledWith(expect.objectContaining({
                tieRule: 'RunOff', scrutinyOn: '2027-03-10', withdrawalClosesOn: '2027-03-12'
            }));
            expect(component.form.tieRule).toBe('DrawingLots');
            expect(component.form.scrutinyOn).toBeNull();
            expect(component.form.withdrawalClosesOn).toBeNull();
        });

        it('does not send the request when a main date is empty', () => {
            electionsMock.create = vi.fn();
            fillForm();
            component.selectedPeriodId = 4;
            component.form.pollingClosesOn = '';

            component.create();

            expect(electionsMock.create).not.toHaveBeenCalled();
        });

        it('sends null for a date left empty', () => {
            electionsMock.create = vi.fn().mockReturnValue(of(election));
            fillForm();
            component.selectedPeriodId = 4;
            component.form.scrutinyOn = '';

            component.create();

            expect(electionsMock.create).toHaveBeenCalledWith(expect.objectContaining({ scrutinyOn: null, withdrawalClosesOn: null }));
        });

        it('keeps the form open and the choice when the server rejects the period', () => {
            electionsMock.create = vi.fn().mockReturnValue(throwError(() => new HttpErrorResponse({ status: 400, error: { detail: 'The period already has members.' } })));
            fillForm();
            component.selectedPeriodId = 4;

            component.create();

            expect(component.saving()).toBe(false);
            expect(component.selectedPeriodId).toBe(4);
        });
    });

    // jsdom's File has no text(), which browsers do have.
    const keyFile = (text: string) => ({ text: () => Promise.resolve(text) }) as unknown as File;

    // FR-39: the key can be set up to the campaign and not once voting has opened.
    it('offers the key only before polling', () => {
        expect(component.canSetBallotKey(election)).toBe(true);
        expect(component.canSetBallotKey({ ...election, phase: 'Polling' })).toBe(false);
        expect(component.canSetBallotKey({ ...election, phase: 'Counting' })).toBe(false);
    });

    // 37.12w. The full readiness list replaces the single approver warning on the row.
    it('loads the readiness list only for rows before polling the official may view', () => {
        const before = { ...election, id: 8, myPermissions: ['ViewDashboard'] } as AdminElectionDto;
        const polling = { ...election, id: 9, phase: 'Polling', myPermissions: ['ViewDashboard'] } as AdminElectionDto;
        const noView = { ...election, id: 10, myPermissions: ['ChangePhase'] } as AdminElectionDto;
        electionsMock.getAdminElections.mockReturnValue(of([before, polling, noView]));
        electionsMock.getReadiness.mockClear();

        component.load();

        expect(electionsMock.getReadiness).toHaveBeenCalledTimes(1);
        expect(electionsMock.getReadiness).toHaveBeenCalledWith(8);
        expect(component.readiness()[8]).toEqual(readiness);
        expect(component.readinessLabel(readiness.items[0])).toBe('The ballot key is set');
        expect(component.readinessLabel({ ...readiness.items[0], key: 'new-item' })).toBe('new-item');
    });

    // FR-37.13t
    it('shows the approver warning only while that item is unmet', () => {
        const approvers = readiness.items.find(i => i.key === 'approvers')!;
        expect(component.readinessLabel(approvers)).toBe(ELECTION_TOO_FEW_APPROVERS_WARNING);
        expect(component.readinessLabel({ ...approvers, met: true })).toBe('At least two officials can approve on this election');
    });

    // FR-39
    it('names the candidate and the seat on a scrutiny card, falling back to ids', () => {
        const fixture = TestBed.createComponent(AdminElections);
        fixture.componentInstance.activeTab.set('nominations');
        fixture.componentInstance.nominations.set([
            { id: 1, electionSeatId: 49, candidateMemberId: 217, status: 'Submitted', statement: '', candidateName: 'Rina Akter', positionTitle: 'General Secretary' },
            { id: 2, electionSeatId: 50, candidateMemberId: 218, status: 'Submitted', statement: '' }
        ] as NominationViewDto[]);
        fixture.detectChanges();

        const cards = Array.from(fixture.nativeElement.querySelectorAll('[data-nomination] > div > div > p:first-child') as NodeListOf<HTMLElement>).map(p => p.textContent?.trim());
        expect(cards).toEqual(['General Secretary · Rina Akter', 'Seat 50 · Member 218']);
    });

    // FR-050: a row with myPermissions shows only the buttons those permissions allow.
    it('hides a button the official has no permission for', () => {
        const row = { ...election, myPermissions: ['ViewDashboard', 'ChangePhase'] } as AdminElectionDto;

        expect(component.can(row, 'ChangePhase')).toBe(true);
        expect(component.can(row, 'SetBallotKey')).toBe(false);
        expect(component.can(row, 'Approve')).toBe(false);
    });

    // 37.12q
    describe('voter roll freeze and unfreeze', () => {
        const rolled = { ...election, phase: 'Nomination', eligibleVoterCount: 40, myPermissions: ['ManageVoterRoll'] } as AdminElectionDto;

        beforeEach(() => {
            electionsMock.unfreezeVoterRoll = vi.fn().mockReturnValue(of({ removed: 40 }));
            electionsMock.freezeVoterRoll = vi.fn().mockReturnValue(of({ count: 40 }));
        });

        it('offers the roll buttons only before scrutiny and with ManageVoterRoll', () => {
            expect(component.canEditRoll(rolled)).toBe(true);
            expect(component.canEditRoll({ ...rolled, phase: 'Scrutiny' })).toBe(false);
            expect(component.canEditRoll({ ...rolled, myPermissions: ['ViewDashboard'] })).toBe(false);
        });

        it('does not send an unfreeze with a blank reason', () => {
            component.openUnfreeze(rolled);
            component.unfreezeReason = '   ';

            component.unfreezeRoll(rolled);

            expect(electionsMock.unfreezeVoterRoll).not.toHaveBeenCalled();
            expect(notifyMock.error).toHaveBeenCalled();
        });

        it('sends the trimmed reason and closes the form', () => {
            component.openUnfreeze(rolled);
            component.unfreezeReason = ' Wrong roll ';

            component.unfreezeRoll(rolled);

            expect(electionsMock.unfreezeVoterRoll).toHaveBeenCalledWith(7, 'Wrong roll');
            expect(component.unfreezeOpenId()).toBeNull();
        });

        it('shows the server message when the unfreeze is refused', () => {
            electionsMock.unfreezeVoterRoll.mockReturnValue(throwError(() => ({ error: { detail: 'The unfreeze window has closed.' } })));
            component.openUnfreeze(rolled);
            component.unfreezeReason = 'Mistake';

            component.unfreezeRoll(rolled);

            expect(notifyMock.error).toHaveBeenCalledWith('The unfreeze window has closed.');
            expect(component.unfreezeOpenId()).toBe(7);
        });
    });

    it('shows no button when the reply carries no permission list', () => {
        expect(component.can(election, 'SetBallotKey')).toBe(false);
    });

    describe('for an official who is not an admin', () => {
        beforeEach(() => {
            asAdmin = false;
            adminMock.getGovernancePeriods.mockClear();
            TestBed.createComponent(AdminElections);
        });

        it('does not call the admin-only governance period endpoint', () => {
            expect(adminMock.getGovernancePeriods).not.toHaveBeenCalled();
        });

        it('hides the New election button', () => {
            const fixture = TestBed.createComponent(AdminElections);
            fixture.detectChanges();

            expect(fixture.nativeElement.textContent).not.toContain('New election');
        });
    });

    it('FR-39: the new election form explains each date and links the hint to its field', () => {
        const fixture = TestBed.createComponent(AdminElections);
        fixture.componentInstance.showForm.set(true);
        fixture.detectChanges();
        const el: HTMLElement = fixture.nativeElement;

        const polling = el.querySelector('#election-polling-opens-on')!;
        expect(polling.getAttribute('aria-describedby')).toBe('election-polling-opens-on-hint');
        expect(el.querySelector('#election-polling-opens-on-hint')!.textContent).toContain('returning officer key');
        expect(el.querySelector('#election-is-test-hint')!.textContent).toContain('Puts nobody on the committee');
        expect(el.querySelector('#seats-hint-0')!.textContent).toContain('once nominations open');
    });

    it('shows the New election button to an admin', () => {
        const fixture = TestBed.createComponent(AdminElections);
        fixture.detectChanges();

        expect(fixture.nativeElement.textContent).toContain('New election');
    });

    it('records the key as destroyed and tells the official', () => {
        electionsMock.keyDestroyed = vi.fn().mockReturnValue(of(undefined));

        component.keyDestroyed(election, true);

        expect(electionsMock.keyDestroyed).toHaveBeenCalledWith(7);
        expect(notifyMock.success).toHaveBeenCalled();
    });

    it('sends nothing when the box is cleared', () => {
        electionsMock.keyDestroyed = vi.fn();

        component.keyDestroyed(election, false);

        expect(electionsMock.keyDestroyed).not.toHaveBeenCalled();
    });

    it('saves the private key before the public key is sent', async () => {
        const order: string[] = [];
        vi.spyOn(ExportUtil, 'saveFile').mockImplementation(() => { order.push('save'); });
        electionsMock.setBallotKey.mockImplementation(() => { order.push('upload'); return of(ran({ ...election, ballotKeyFingerprint: 'ab12' })); });

        await component.createBallotKey(election);

        expect(order).toEqual(['save', 'upload']);
        expect(electionsMock.setBallotKey).toHaveBeenCalledWith(7, expect.stringMatching(/^[A-Za-z0-9+/=]{300,}$/));
        expect(component.elections()[0].ballotKeyFingerprint).toBe('ab12');
    }, 30000);

    it('does nothing when the officer cancels', async () => {
        confirmMock.confirm.mockReturnValue(of(false));
        const save = vi.spyOn(ExportUtil, 'saveFile').mockImplementation(() => {});

        await component.createBallotKey(election);

        expect(save).not.toHaveBeenCalled();
        expect(electionsMock.setBallotKey).not.toHaveBeenCalled();
    });

    it('sends the key file text to the count', async () => {
        const file = keyFile('-----BEGIN PRIVATE KEY-----\nAB\nCD\n-----END PRIVATE KEY-----\n');
        const input = { files: [file], value: 'key.pem' } as unknown as HTMLInputElement;

        await component.countWithKeyFile({ ...election, phase: 'Counting' }, { target: input } as unknown as Event);

        expect(electionsMock.runCount).toHaveBeenCalledWith(7, 'ABCD');
        expect(input.value).toBe('');
        expect(notifyMock.success).toHaveBeenCalled();
    });

    it('leaves a failed count message to the HTTP interceptor', async () => {
        electionsMock.runCount.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 400 })));
        const input = { files: [keyFile('ABCD')], value: '' } as unknown as HTMLInputElement;

        await component.countWithKeyFile({ ...election, phase: 'Counting' }, { target: input } as unknown as Event);

        expect(notifyMock.error).not.toHaveBeenCalled();
        expect(component.countingId()).toBeNull();
    });

    // FR-37.1a. Publish, close and count have their own buttons, so advance skips those phases.
    it('offers the next phase only where no other button moves the election', () => {
        expect(component.nextPhase({ ...election, phase: 'Nomination' })).toBe('Scrutiny');
        expect(component.nextPhase({ ...election, phase: 'Campaign' })).toBe('Polling');
        expect(component.nextPhase({ ...election, phase: 'Declared' })).toBe('Archived');
        for (const phase of ['Announced', 'Polling', 'Counting', 'Archived'] as const) {
            expect(component.nextPhase({ ...election, phase })).toBeNull();
        }
    });

    it('moves the election on when the phase change runs at once', async () => {
        electionsMock.setPhase = vi.fn().mockReturnValue(of(null));
        component.elections.set([{ ...election, phase: 'Nomination' }]);

        await component.advance(component.elections()[0]);

        expect(electionsMock.setPhase).toHaveBeenCalledWith(7, 'Scrutiny');
        expect(component.elections()[0].phase).toBe('Scrutiny');
        expect(component.advancingId()).toBeNull();
    });

    it('keeps the phase and lists the request when opening polling waits for approval', async () => {
        electionsMock.setPhase = vi.fn().mockReturnValue(of({ ...request, action: 'OpenPolling' }));
        component.elections.set([{ ...election, tooFewApprovers: true }]);

        await component.advance(component.elections()[0]);

        expect(confirmMock.confirm.mock.calls[0][0].danger).toBe(true);
        expect(component.elections()[0].phase).toBe('Campaign');
        expect(component.approvals()[7]?.[0].action).toBe('OpenPolling');
    });

    it('FR-017: declares counted results through the two-person flow', async () => {
        const counting = { ...election, phase: 'Counting', myPermissions: ['Declare'] } as AdminElectionDto;
        const pending: ElectionApprovalDto = { ...request, action: 'Declare' };
        electionsMock.declare.mockReturnValue(of(pending));
        component.elections.set([counting]);

        await component.declareResults(counting);

        expect(confirmMock.confirm.mock.calls[0][0].confirmLabel).toBe('Declare results');
        expect(electionsMock.declare).toHaveBeenCalledWith(counting.id);
        expect(component.approvals()[counting.id]?.[0].action).toBe('Declare');
        expect(component.declaringId()).toBeNull();
    });

    it('shows the server reason when declaring results fails', async () => {
        const counting = { ...election, phase: 'Counting', myPermissions: ['Declare'] } as AdminElectionDto;
        electionsMock.declare.mockReturnValue(throwError(() => new HttpErrorResponse({
            status: 400,
            error: { detail: 'The election is missing a verified count.' }
        })));
        component.elections.set([counting]);

        await component.declareResults(counting);

        expect(notifyMock.error).toHaveBeenCalledWith('The election is missing a verified count.');
        expect(component.declaringId()).toBeNull();
    });

    it('shows the declare button only while counting with Declare permission', () => {
        const fixture = TestBed.createComponent(AdminElections);
        fixture.componentInstance.elections.set([
            { ...election, phase: 'Counting', myPermissions: ['Declare'] } as AdminElectionDto,
            { ...election, id: 8, phase: 'Counting', myPermissions: ['Count'] } as AdminElectionDto
        ]);
        fixture.detectChanges();

        const buttons = Array.from(fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>);
        expect(buttons.filter(button => button.textContent?.trim() === 'Declare results')).toHaveLength(1);
    });

    it('does nothing when the advance is not confirmed', async () => {
        electionsMock.setPhase = vi.fn();
        confirmMock.confirm.mockReturnValue(of(false));

        await component.advance({ ...election, phase: 'Nomination' });

        expect(electionsMock.setPhase).not.toHaveBeenCalled();
    });

    // FR-39 (spec 023, 37.12f): a stored step leaves the election as it was and lists the request.
    it('keeps the election and lists the request when a second person is needed', () => {
        electionsMock.publish.mockReturnValue(of({ election: null, pending: request }));
        component.elections.set([{ ...election, phase: 'Announced' }]);

        component.publish(component.elections()[0]);

        expect(component.elections()[0].phase).toBe('Announced');
        expect(component.approvals()[7]).toEqual([request]);
        expect(notifyMock.info).toHaveBeenCalled();
    });

    it('keeps the current key when a replacement waits for approval', async () => {
        vi.spyOn(ExportUtil, 'saveFile').mockImplementation(() => {});
        electionsMock.setBallotKey.mockReturnValue(of({ election: null, pending: { ...request, action: 'ReplaceBallotKey', keyFingerprint: 'cd34' } }));
        component.elections.set([{ ...election, ballotKeyFingerprint: 'ab12' }]);

        await component.createBallotKey(component.elections()[0]);

        expect(component.elections()[0].ballotKeyFingerprint).toBe('ab12');
        expect(component.approvals()[7]?.[0].keyFingerprint).toBe('cd34');
        expect(notifyMock.success).not.toHaveBeenCalled();
    }, 30000);

    it('drops the request and reloads once approved', () => {
        component.approvals.set({ 7: [request] });

        component.approve(request);

        expect(electionsMock.approve).toHaveBeenCalledWith(30);
        expect(component.approvals()[7]).toEqual([]);
        expect(electionsMock.getAdminElections).toHaveBeenCalledTimes(2);
    });

    // FR-39 (37.13h): the count can wait for a second person, then runs with the same key file.
    const countRequest: ElectionApprovalDto = { ...request, id: 31, action: 'Count' };

    it('lists the count request when a second person must approve it', async () => {
        electionsMock.runCount.mockReturnValue(of({ results: null, pending: countRequest }));
        const input = { files: [keyFile('ABCD')], value: '' } as unknown as HTMLInputElement;

        await component.countWithKeyFile({ ...election, phase: 'Counting' }, { target: input } as unknown as Event);

        expect(component.approvals()[7]).toEqual([countRequest]);
        expect(notifyMock.success).not.toHaveBeenCalled();
        expect(notifyMock.info).toHaveBeenCalled();
    });

    it('keeps an approved count listed as ready, without reloading', () => {
        component.approvals.set({ 7: [countRequest] });

        component.approve(countRequest);

        expect(component.approvals()[7]?.[0].approvedAt).toBeTruthy();
        expect(electionsMock.getAdminElections).toHaveBeenCalledTimes(1);
    });

    it('clears the count request once the count runs', async () => {
        component.approvals.set({ 7: [request, { ...countRequest, approvedAt: '2026-10-02T10:00:00Z' }] });
        const input = { files: [keyFile('ABCD')], value: '' } as unknown as HTMLInputElement;

        await component.countWithKeyFile({ ...election, phase: 'Counting' }, { target: input } as unknown as Event);

        expect(component.approvals()[7]).toEqual([request]);
        expect(notifyMock.success).toHaveBeenCalled();
    });

    it('rejects only after the confirm dialog', async () => {
        component.approvals.set({ 7: [request] });
        confirmMock.confirm.mockReturnValue(of(false));
        await component.reject(request);
        expect(electionsMock.reject).not.toHaveBeenCalled();

        confirmMock.confirm.mockReturnValue(of(true));
        await component.reject(request);
        expect(electionsMock.reject).toHaveBeenCalledWith(30, null);
        expect(component.approvals()[7]).toEqual([]);
    });

    describe('scrutiny decisions', () => {
        const nomination = { id: 41, status: 'Submitted' } as unknown as NominationViewDto;
        const inScrutiny = { ...election, phase: 'Scrutiny', myPermissions: ['DecideNominations'] } as unknown as AdminElectionDto;

        beforeEach(() => {
            electionsMock.scrutinise = vi.fn().mockReturnValue(of(undefined));
            electionsMock.getAdminNominations = vi.fn().mockReturnValue(of([nomination]));
            component.elections.set([inScrutiny]);
            component.selectedElectionId.set(7);
        });

        // FR-37.1a. The server only takes a decision in Scrutiny.
        it('offers a decision only in the scrutiny phase', () => {
            expect(component.canDecide(nomination)).toBe(true);
            expect(component.canDecide({ ...nomination, status: 'Accepted' })).toBe(false);

            component.elections.set([{ ...inScrutiny, phase: 'Nomination' }]);
            expect(component.canDecide(nomination)).toBe(false);

            component.elections.set([{ ...inScrutiny, myPermissions: [] }]);
            expect(component.canDecide(nomination)).toBe(false);
        });

        it('sends nothing on reject until a reason is given', () => {
            component.setScrutinyReason(41, '   ');
            component.scrutinise(nomination, false);
            expect(electionsMock.scrutinise).not.toHaveBeenCalled();
        });

        it('sends the trimmed reason with a rejection and reloads the list', () => {
            component.setScrutinyReason(41, '  Seconder not on the roll  ');
            component.scrutinise(nomination, false);

            expect(electionsMock.scrutinise).toHaveBeenCalledWith(41, { accepted: false, reason: 'Seconder not on the roll' });
            expect(electionsMock.getAdminNominations).toHaveBeenCalledWith(7);
            expect(component.scrutinisingId()).toBeNull();
        });

        it('accepts without a reason when none is given', () => {
            component.scrutinise(nomination, true);
            expect(electionsMock.scrutinise).toHaveBeenCalledWith(41, { accepted: true });
        });

        it('reports a failed decision and frees the buttons', () => {
            electionsMock.scrutinise.mockReturnValue(throwError(() => new Error('409')));
            component.scrutinise(nomination, true);

            expect(notifyMock.error).toHaveBeenCalled();
            expect(component.scrutinisingId()).toBeNull();
        });
    });
});
