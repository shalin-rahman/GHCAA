import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { createNotificationServiceMock } from '../../../core/testing/testing-utils';
import { NominationRecords } from './nomination-records';
import { ElectionsService } from '../../../core/services/elections.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AdminElectionDto, NominationViewDto, ScrutinyCheckDto } from '../../../core/models/election.models';

// TODO 37.14m.
describe('NominationRecords', () => {
    let electionsMock: any;
    let notifyMock: any;
    const nomination = { id: 21, electionSeatId: 3, candidateMemberId: 9, status: 'Submitted', statement: '', consentAt: '2026-10-20T06:00:00' } as NominationViewDto;
    const tick = (item: ScrutinyCheckDto['item'], isChecked = true): ScrutinyCheckDto =>
        ({ item, isChecked, setByUserId: 1, setAt: '2026-10-21T08:00:00Z' });

    const create = async (phase: string, permissions: string[], status: NominationViewDto['status'] = 'Submitted') => {
        electionsMock = {
            getScrutinyChecks: vi.fn().mockReturnValue(of([])),
            setScrutinyCheck: vi.fn().mockImplementation((_e: number, _n: number, item: any, isChecked: boolean) => of([tick(item, isChecked)])),
            recordConsent: vi.fn().mockReturnValue(of(undefined))
        };
        notifyMock = createNotificationServiceMock();
        await TestBed.configureTestingModule({
            imports: [NominationRecords],
            providers: [
                { provide: ElectionsService, useValue: electionsMock },
                { provide: NotificationService, useValue: notifyMock }
            ]
        }).compileComponents();
        const fixture = TestBed.createComponent(NominationRecords);
        fixture.componentRef.setInput('election', { id: 7, phase, myPermissions: permissions } as unknown as AdminElectionDto);
        fixture.componentRef.setInput('nomination', { ...nomination, status });
        fixture.detectChanges();
        return fixture.componentInstance;
    };

    it('reads the consent time as UTC and fills the input in local time', async () => {
        const component = await create('Scrutiny', ['ManageSetup']);
        expect(component.consentShown()?.toISOString()).toBe('2026-10-20T06:00:00.000Z');
        expect(new Date(component.consentInput).toISOString()).toBe('2026-10-20T06:00:00.000Z');
    });

    it('lets the setup official edit the consent time on a live nomination', async () => {
        expect((await create('Scrutiny', ['ManageSetup'])).canEditConsent()).toBe(true);
    });

    it('shows the consent time read-only without ManageSetup', async () => {
        expect((await create('Scrutiny', ['DecideNominations'])).canEditConsent()).toBe(false);
    });

    // A withdrawn or rejected candidate is off the ballot, so the consent form goes.
    it.each(['Withdrawn', 'Rejected'] as const)('hides the consent form on a %s nomination', async status => {
        expect((await create('Scrutiny', ['ManageSetup'], status)).canEditConsent()).toBe(false);
    });

    it('skips the scrutiny sheet without DecideNominations', async () => {
        await create('Scrutiny', ['ManageSetup']);
        expect(electionsMock.getScrutinyChecks).not.toHaveBeenCalled();
    });

    it('sends the consent time in UTC and does nothing on an empty input', async () => {
        const component = await create('Scrutiny', ['ManageSetup']);
        component.consentInput = '';
        component.saveConsent();
        expect(electionsMock.recordConsent).not.toHaveBeenCalled();

        component.consentInput = '2026-10-21T09:05';
        component.saveConsent();
        const sent = new Date(2026, 9, 21, 9, 5).toISOString();
        expect(electionsMock.recordConsent).toHaveBeenCalledWith(7, 21, sent);
        expect(component.consentAt()).toBe(sent);
        expect(notifyMock.success).toHaveBeenCalled();
    });

    it('ticks a scrutiny item during scrutiny', async () => {
        const component = await create('Scrutiny', ['DecideNominations']);
        expect(electionsMock.getScrutinyChecks).toHaveBeenCalledWith(7, 21);
        component.setCheck('MembershipVerified', true);
        expect(electionsMock.setScrutinyCheck).toHaveBeenCalledWith(7, 21, 'MembershipVerified', true);
        expect(component.isChecked('MembershipVerified')).toBe(true);
    });

    it('will not tick outside the scrutiny phase', async () => {
        const component = await create('Campaign', ['DecideNominations']);
        expect(component.canTick()).toBe(false);
        component.setCheck('MembershipVerified', true);
        expect(electionsMock.setScrutinyCheck).not.toHaveBeenCalled();
    });
});
