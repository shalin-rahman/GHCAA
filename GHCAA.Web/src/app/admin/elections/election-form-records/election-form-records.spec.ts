import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { createNotificationServiceMock } from '../../../core/testing/testing-utils';
import { ElectionFormRecords } from './election-form-records';
import { ElectionsService } from '../../../core/services/elections.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AdminElectionDto, ElectionRecountDto } from '../../../core/models/election.models';

// TODO 37.14m.
describe('ElectionFormRecords', () => {
    let electionsMock: any;
    let notifyMock: any;
    const recount = (over: Partial<ElectionRecountDto> = {}): ElectionRecountDto => ({
        id: 4, electionSeatId: 3, positionTitle: 'President', applicantName: 'Karim', grounds: 'Close margin',
        receivedAt: '2026-10-21T08:00:00Z', allowed: null, figures: [], ...over
    } as ElectionRecountDto);
    const election = (phase: string, permissions: string[], formDetails: any = null) => ({
        id: 7, title: 'EC 2026', phase, myPermissions: permissions, positions: [], candidates: [], formDetails
    } as unknown as AdminElectionDto);

    const create = async (value: AdminElectionDto) => {
        electionsMock = {
            updateFormDetails: vi.fn().mockReturnValue(of({ ...value, title: 'Updated' })),
            getRecounts: vi.fn().mockReturnValue(of([recount()])),
            recordRecount: vi.fn().mockReturnValue(of(recount({ id: 5 }))),
            decideRecount: vi.fn().mockReturnValue(of(undefined)),
            recordRecountFigures: vi.fn().mockReturnValue(of(undefined)),
            getPublicResults: vi.fn().mockReturnValue(of({
                seats: [
                    { seatId: 2, candidates: [{ nominationId: 90, name: 'Other', voteCount: 1 }] },
                    { seatId: 3, candidates: [{ nominationId: 11, name: 'Rina', voteCount: 40 }, { nominationId: 12, name: 'Tanvir', voteCount: 38 }] }
                ]
            }))
        };
        notifyMock = createNotificationServiceMock();
        await TestBed.configureTestingModule({
            imports: [ElectionFormRecords],
            providers: [
                { provide: ElectionsService, useValue: electionsMock },
                { provide: NotificationService, useValue: notifyMock }
            ]
        }).compileComponents();
        const fixture = TestBed.createComponent(ElectionFormRecords);
        fixture.componentRef.setInput('election', value);
        fixture.detectChanges();
        return fixture.componentInstance;
    };

    it('fills the form from the saved details, cutting day fields to yyyy-MM-dd', async () => {
        const component = await create(election('Setup', ['ManageSetup'], {
            finalListOn: '2026-11-01T00:00:00', venue: 'Main hall', silenceStartsAt: '2026-11-09T18:00:00'
        }));
        expect(component.details.finalListOn).toBe('2026-11-01');
        expect(component.details.venue).toBe('Main hall');
        expect(new Date(component.details.silenceStartsAt).toISOString()).toBe('2026-11-09T18:00:00.000Z');
    });

    it('sends blanks as null, trims text and emits the updated election', async () => {
        const component = await create(election('Setup', ['ManageSetup']));
        const emitted: AdminElectionDto[] = [];
        component.changed.subscribe(e => emitted.push(e));
        component.details.venue = '  Main hall  ';
        component.details.campaignOpensOn = '2026-11-02';
        component.saveDetails();

        expect(electionsMock.updateFormDetails).toHaveBeenCalledWith(7, expect.objectContaining({
            venue: 'Main hall', campaignOpensOn: '2026-11-02', noticeReference: null, silenceStartsAt: null, finalListOn: null
        }));
        expect(emitted[0].title).toBe('Updated');
    });

    it('loads recounts only with ViewDashboard', async () => {
        await create(election('Declared', ['ManageSetup']));
        expect(electionsMock.getRecounts).not.toHaveBeenCalled();
        TestBed.resetTestingModule();
        const component = await create(election('Declared', ['ViewDashboard']));
        expect(component.recounts().map(r => r.id)).toEqual([4]);
    });

    it('needs a seat, applicant, grounds and time before recording a recount', async () => {
        const component = await create(election('Declared', ['ViewDashboard', 'DecideAppeals']));
        expect(component.canRecord()).toBe(false);
        Object.assign(component.recountForm, { electionSeatId: 3, applicantName: ' Karim ', grounds: ' Close ', receivedAt: '2026-10-21T09:05' });
        component.recordRecount();
        expect(electionsMock.recordRecount).toHaveBeenCalledWith(7, expect.objectContaining({
            electionSeatId: 3, applicantName: 'Karim', grounds: 'Close', explanation: null,
            receivedAt: new Date(2026, 9, 21, 9, 5).toISOString()
        }));
        expect(component.recounts().map(r => r.id)).toEqual([4, 5]);
        expect(component.recountForm.applicantName).toBe('');
    });

    it('will not refuse a recount without a reason', async () => {
        const component = await create(election('Declared', ['ViewDashboard', 'DecideAppeals']));
        component.decide(recount(), false);
        expect(electionsMock.decideRecount).not.toHaveBeenCalled();
        expect(notifyMock.error).toHaveBeenCalled();

        component.refuseReasons[4] = ' Margin too wide ';
        component.decide(recount(), false);
        expect(electionsMock.decideRecount).toHaveBeenCalledWith(7, 4, { allowed: false, reason: 'Margin too wide' });
    });

    it('offers figures only for an allowed recount on a declared election with Count', async () => {
        const component = await create(election('Declared', ['ViewDashboard', 'Count']));
        expect(component.canEnterFigures(recount({ allowed: true }))).toBe(true);
        expect(component.canEnterFigures(recount({ allowed: null }))).toBe(false);
        expect(component.canEnterFigures(recount({ allowed: true, figures: [{ nominationId: 11, voteCount: 1 }] }))).toBe(false);
        TestBed.resetTestingModule();
        const counting = await create(election('Counting', ['ViewDashboard', 'Count']));
        expect(counting.canEnterFigures(recount({ allowed: true }))).toBe(false);
    });

    it('takes the figure rows from the recount seat and sends whole numbers only', async () => {
        const component = await create(election('Declared', ['ViewDashboard', 'Count']));
        const allowed = recount({ allowed: true });
        component.openFigures(allowed);
        expect(component.figureRows().map(r => r.nominationId)).toEqual([11, 12]);
        expect(component.figuresReady()).toBe(true);

        component.figureVotes[12] = 1.5;
        expect(component.figuresReady()).toBe(false);
        component.figureVotes[12] = 39;
        component.saveFigures(allowed);
        expect(electionsMock.recordRecountFigures).toHaveBeenCalledWith(7, 4, [
            { nominationId: 11, voteCount: 40 }, { nominationId: 12, voteCount: 39 }
        ]);
        expect(component.figuresOpenId()).toBeNull();
    });
});
