// @vitest-environment jsdom
import { TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { MemberElection } from './election';
import { ElectionsService } from '../../core/services/elections.service';
import { NotificationService } from '../../core/services/notification.service';
import { AuthService } from '../../core/services/auth.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { NetworkingService } from '../../core/services/networking.service';
import { ExportUtil } from '../../core/utils/export.util';
import { BallotViewDto } from '../../core/models/election.models';

describe('MemberElection', () => {
    let elections: any;
    let notify: any;

    const candidate = (nominationId: number, name: string, photoUrl: string | null = null) =>
        ({ nominationId, name, photoUrl, statement: `About ${name}` });

    function paper(over: Partial<BallotViewDto> = {}): BallotViewDto {
        return {
            electionId: 5, title: 'EC 2026', phase: 'Polling', hasVoted: false, ballotKeyFingerprint: null,
            isEligible: true, ineligibilityReason: null,
            seats: [
                { seatId: 10, positionTitle: 'President', places: 1, candidates: [candidate(1, 'Amina', '/p/a.png'), candidate(2, 'Bilal')] },
                { seatId: 20, positionTitle: 'Members', places: 2, candidates: [candidate(3, 'Chitra'), candidate(4, 'Dev'), candidate(5, 'Esha')] }
            ],
            ...over
        };
    }

    function create(ballot: BallotViewDto = paper(), receiptPdfBase64: string | null = null) {
        elections = {
            getCurrent: vi.fn().mockReturnValue(of({ id: 5, phase: ballot.phase })),
            getBallot: vi.fn().mockReturnValue(of(ballot)),
            castBallot: vi.fn().mockReturnValue(of({ trackingCode: 'ABCD-EFGH', receiptPdfBase64 })),
            getSeats: vi.fn().mockReturnValue(of([])),
            getNominations: vi.fn().mockReturnValue(of([]))
        };
        notify = { success: vi.fn(), error: vi.fn() };
        TestBed.configureTestingModule({
            imports: [MemberElection],
            providers: [
                { provide: ElectionsService, useValue: elections },
                { provide: NotificationService, useValue: notify },
                { provide: AuthService, useValue: { currentUser: () => ({ memberId: 7 }) } },
                { provide: NetworkingService, useValue: { searchMembers: vi.fn() } },
                { provide: ConfirmDialogService, useValue: { confirm: vi.fn() } }
            ]
        });
        const fixture = TestBed.createComponent(MemberElection);
        fixture.detectChanges();
        return { fixture, component: fixture.componentInstance, el: fixture.nativeElement as HTMLElement };
    }

    afterEach(() => { vi.restoreAllMocks(); localStorage.clear(); });

    // FR-37.1b. The ballot is empty until the candidate list, so these phases show the nomination panel instead.
    it.each(['Nomination', 'Withdrawal'] as const)('shows the nomination panel in the %s phase', phase => {
        const { el } = create(paper({ phase, seats: [] }));

        expect(el.querySelector('app-election-nomination')).not.toBeNull();
        expect(el.textContent).not.toContain('No accepted candidates yet');
        expect(elections.getNominations).toHaveBeenCalledWith(5);
    });

    it('keeps the empty state outside the nomination phases', () => {
        const { el } = create(paper({ phase: 'Scrutiny', seats: [] }));

        expect(el.querySelector('app-election-nomination')).toBeNull();
        expect(el.textContent).toContain('No accepted candidates yet');
    });

    it('shows position titles, candidate names and the photo', () => {
        const { el } = create();

        expect(el.textContent).toContain('President');
        expect(el.textContent).toContain('Amina');
        expect(el.textContent).toContain('Chitra');
        expect(el.querySelector('img')?.getAttribute('src')).toBe('/p/a.png');
    });

    it('uses a radio for one place and checkboxes for several', () => {
        const { el } = create();

        expect(el.querySelectorAll('input[name="seat-10"][type="radio"]').length).toBe(3);
        expect(el.querySelectorAll('input[name="seat-20"][type="checkbox"]').length).toBe(3);
    });

    it('replaces the pick on a one-place seat', () => {
        const { component } = create();
        const seat = component.seats()[0];

        component.toggle(seat, 1);
        component.toggle(seat, 2);

        expect(component.choices()[10]).toEqual([2]);
    });

    it('limits picks to the places on the seat', () => {
        const { component } = create();
        const seat = component.seats()[1];

        component.toggle(seat, 3);
        component.toggle(seat, 4);
        component.toggle(seat, 5);

        expect(component.choices()[20]).toEqual([3, 4]);
        expect(component.atLimit(seat, 5)).toBe(true);
    });

    it('blocks a third pick on a two-place seat', () => {
        const { component } = create();
        const seat = component.seats()[1];

        component.toggle(seat, 3);
        component.toggle(seat, 4);
        component.toggle(seat, 5);

        expect(component.choices()[20]).toEqual([3, 4]);
    });

    it('abstain clears the picks on a seat', () => {
        const { component } = create();
        const seat = component.seats()[1];
        component.toggle(seat, 3);
        component.toggle(seat, 4);

        component.abstain(seat);

        expect(component.choices()[20]).toEqual([]);
        expect(component.isAbstained(20)).toBe(true);
    });

    it('confirm sends both ids of a two-place seat', () => {
        const { component } = create();
        component.toggle(component.seats()[0], 1);
        component.toggle(component.seats()[1], 3);
        component.toggle(component.seats()[1], 5);
        component.review();

        component.confirm();

        expect(elections.castBallot).toHaveBeenCalledWith(5, {
            seats: [
                { electionSeatId: 10, nominationIds: [1] },
                { electionSeatId: 20, nominationIds: [3, 5] }
            ]
        });
    });

    it('shows Pick up to N on a multi-place seat and lists every pick in review', () => {
        const { component, fixture, el } = create();
        component.toggle(component.seats()[0], 1);
        component.toggle(component.seats()[1], 3);
        component.toggle(component.seats()[1], 4);
        expect(el.textContent).toContain('Pick up to 2');

        component.review();
        fixture.detectChanges();

        expect(el.textContent).toContain('Members: Chitra, Dev');
    });

    it('treats abstain as a decision and a later pick replaces it', () => {
        const { component } = create();
        const seat = component.seats()[0];

        component.abstain(seat);
        expect(component.isAbstained(10)).toBe(true);
        component.toggle(seat, 1);

        expect(component.isAbstained(10)).toBe(false);
        expect(component.choices()[10]).toEqual([1]);
    });

    it('will not go to review until every seat is decided', () => {
        const { component } = create();
        component.toggle(component.seats()[0], 1);

        component.review();

        expect(component.step()).toBe('pick');
    });

    it('lists the choices on the review step and sends nothing until confirm', () => {
        const { component, fixture, el } = create();
        component.toggle(component.seats()[0], 2);
        component.abstain(component.seats()[1]);

        component.review();
        fixture.detectChanges();

        expect(component.step()).toBe('review');
        expect(el.textContent).toContain('President: Bilal');
        expect(el.textContent).toContain('Members: Abstained');
        expect(elections.castBallot).not.toHaveBeenCalled();
    });

    // FR-39
    it('confirm sends the whole paper in one request', () => {
        const { component } = create();
        component.toggle(component.seats()[0], 2);
        component.abstain(component.seats()[1]);
        component.review();

        component.confirm();

        expect(elections.castBallot).toHaveBeenCalledTimes(1);
        expect(elections.castBallot).toHaveBeenCalledWith(5, {
            seats: [
                { electionSeatId: 10, nominationIds: [2] },
                { electionSeatId: 20, nominationIds: [] }
            ]
        });
        expect(component.trackingCode()).toBe('ABCD-EFGH');
        expect(component.closed).toBe(true);
    });

    it('shows the voted state from the server on load', () => {
        const { component, el } = create(paper({ hasVoted: true }));

        expect(component.alreadyVoted()).toBe(true);
        expect(el.textContent).toContain('You have already voted');
        expect(Array.from(el.querySelectorAll('input')).every(i => i.disabled)).toBe(true);
    });

    // FR-39
    it('tells an ineligible member why before they fill in the paper', () => {
        const { component, el } = create(paper({ isEligible: false, ineligibilityReason: 'Membership dues are overdue.' }));

        expect(component.closed).toBe(true);
        expect(el.textContent).toContain('You cannot vote in this election');
        expect(el.textContent).toContain('Membership dues are overdue.');
        expect(Array.from(el.querySelectorAll('input')).every(i => i.disabled)).toBe(true);
        expect(el.textContent).not.toContain('Review ballot');
    });

    // FR-33
    it('says the member is on the roll, and adds that they have voted', () => {
        const before = create();
        expect(before.el.querySelector('#roll-status')?.textContent).toContain('You are on the roll');
        expect(before.el.querySelector('#roll-status')?.textContent).not.toContain('You have voted');

        TestBed.resetTestingModule();
        const after = create(paper({ hasVoted: true }));
        expect(after.el.querySelector('#roll-status')?.textContent).toContain('You have voted');
    });

    it('leaves the paper open when the roll is not frozen yet', () => {
        const { component, el } = create(paper({ isEligible: null }));

        expect(component.closed).toBe(false);
        expect(el.textContent).not.toContain('You cannot vote');
    });

    it('labels each seat group and describes the abstain option', () => {
        const { el } = create();
        const group = el.querySelector('[role="group"]')!;
        const abstain = el.querySelectorAll<HTMLInputElement>('input[name="seat-10"]')[2];

        expect(el.querySelector('#' + group.getAttribute('aria-labelledby'))?.textContent).toBe('President');
        expect(abstain.closest('label')?.textContent).toContain('Abstain on this seat');
        expect(el.querySelector('#' + abstain.getAttribute('aria-describedby'))?.textContent)
            .toContain('no candidate on this seat gets your vote');
    });

    it('does not count an untouched seat as an abstention', () => {
        const { component } = create();
        component.toggle(component.seats()[0], 1);

        expect(component.isAbstained(20)).toBe(false);
        expect(component.allDecided()).toBe(false);
    });

    it('shows the key fingerprint when the paper has one', () => {
        const { el } = create(paper({ ballotKeyFingerprint: 'ab12cd' }));

        expect(el.textContent).toContain('ab12cd');
    });

    // NFR-R5
    it('locks the ballot when the server says the member already voted', () => {
        const { component } = create();
        elections.castBallot.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 409 })));
        component.abstain(component.seats()[0]);
        component.abstain(component.seats()[1]);

        component.confirm();

        expect(component.alreadyVoted()).toBe(true);
        expect(component.closed).toBe(true);
        expect(notify.error).toHaveBeenCalled();
    });

    it('does not submit outside polling', () => {
        const { component } = create(paper({ phase: 'Scrutiny' }));
        component.abstain(component.seats()[0]);
        component.abstain(component.seats()[1]);

        component.confirm();

        expect(elections.castBallot).not.toHaveBeenCalled();
    });

    describe('receipt', () => {
        function voted(pdf: string | null = null) {
            const ctx = create(paper(), pdf);
            ctx.component.toggle(ctx.component.seats()[0], 1);
            ctx.component.abstain(ctx.component.seats()[1]);
            ctx.component.review();
            ctx.component.confirm();
            ctx.fixture.detectChanges();
            return ctx;
        }

        it('has the election, tracking code, chosen name and Abstained', () => {
            const { component, el } = voted();
            const receipt = component.receipt()!;

            expect(receipt.title).toBe('EC 2026');
            expect(receipt.trackingCode).toBe('ABCD-EFGH');
            expect(receipt.lines).toEqual([
                { position: 'President', names: ['Amina'] },
                { position: 'Members', names: [] }
            ]);
            const printed = el.querySelector('.vote-receipt')!.textContent!;
            expect(printed).toContain('EC 2026');
            expect(printed).toContain('President: Amina');
            expect(printed).toContain('Members: Abstained');
            expect(printed).toContain('ABCD-EFGH');
        });

        it('prints from the confirmation view and says the choices print only here', () => {
            const print = vi.spyOn(window, 'print').mockImplementation(() => {});
            const { el } = voted();

            (Array.from(el.querySelectorAll('button')).find(b => b.textContent?.includes('Print receipt')) as HTMLButtonElement).click();

            expect(print).toHaveBeenCalled();
            expect(el.textContent).toContain('printed only on this screen');
        });

        it('keeps no choices in browser storage', () => {
            const set = vi.spyOn(Storage.prototype, 'setItem');
            voted();

            expect(set).not.toHaveBeenCalled();
        });

        it('has no download button when the server sent no PDF', () => {
            const { el } = voted();

            expect(el.textContent).not.toContain('Download receipt');
        });

        it('stores the server PDF under the member and election and downloads it', () => {
            const save = vi.spyOn(ExportUtil, 'saveFile').mockImplementation(() => {});
            const { el } = voted(btoa('%PDF-1.7'));

            expect(localStorage.getItem('ghcaa_ballot_receipt_7_5')).toBe(btoa('%PDF-1.7'));
            (Array.from(el.querySelectorAll('button')).find(b => b.textContent?.includes('Download receipt')) as HTMLButtonElement).click();

            const [blob, name] = save.mock.calls[0];
            expect(name).toBe('ballot-receipt-5.pdf');
            expect((blob as Blob).type).toBe('application/pdf');
        });

        it('offers the stored PDF again after a reload', () => {
            localStorage.setItem('ghcaa_ballot_receipt_7_5', btoa('%PDF-1.7'));
            const { el } = create(paper({ hasVoted: true }));

            expect(el.textContent).toContain('You have already voted');
            expect(el.textContent).toContain('Download receipt');
        });

        it('drops the stored PDF once the election is archived and clears other elections', () => {
            localStorage.setItem('ghcaa_ballot_receipt_7_5', 'a');
            localStorage.setItem('ghcaa_ballot_receipt_7_4', 'b');
            localStorage.setItem('ghcaa_ballot_receipt_8_5', 'c');
            const { component } = create(paper({ phase: 'Archived', hasVoted: true }));

            expect(component.receiptPdf()).toBeNull();
            expect(localStorage.getItem('ghcaa_ballot_receipt_7_5')).toBeNull();
            expect(localStorage.getItem('ghcaa_ballot_receipt_7_4')).toBeNull();
            expect(localStorage.getItem('ghcaa_ballot_receipt_8_5')).toBe('c');
        });

        it('drops a damaged stored PDF instead of throwing', () => {
            localStorage.setItem('ghcaa_ballot_receipt_7_5', '%%%');
            const { component } = create(paper({ hasVoted: true }));

            expect(() => component.downloadReceipt()).not.toThrow();
            expect(component.receiptPdf()).toBeNull();
            expect(localStorage.getItem('ghcaa_ballot_receipt_7_5')).toBeNull();
            expect(notify.error).toHaveBeenCalled();
        });
    });
});
