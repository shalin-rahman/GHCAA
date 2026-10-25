// @vitest-environment jsdom
import { TestBed } from '@angular/core/testing';
import { Subject, of, throwError } from 'rxjs';
import { ElectionNomination } from './election-nomination';
import { ElectionPhase, NominationViewDto } from '../../../core/models/election.models';
import { AuthService } from '../../../core/services/auth.service';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';
import { ElectionsService } from '../../../core/services/elections.service';
import { NetworkingService } from '../../../core/services/networking.service';
import { NotificationService } from '../../../core/services/notification.service';
import { SEARCH_DEBOUNCE_MS } from '../../../core/constants/app.constants';

const SEATS = [{ seatId: 10, positionTitle: 'President', places: 1 }, { seatId: 20, positionTitle: 'Members', places: 2 }];
const hit = (id: number, fullName: string) => ({ id, fullName, membershipNumber: `M-${id}` });

// FR-37.1b. Members file nominations and candidates withdraw them from the web.
describe('ElectionNomination', () => {
    let elections: any;
    let networking: any;
    let notify: any;
    let confirm: any;

    function create(phase: ElectionPhase, nominations: NominationViewDto[] = []) {
        elections = {
            getSeats: vi.fn().mockReturnValue(of(SEATS)),
            getNominations: vi.fn().mockReturnValue(of(nominations)),
            nominate: vi.fn().mockReturnValue(of({})),
            withdrawNomination: vi.fn().mockReturnValue(of(undefined))
        };
        networking = { searchMembers: vi.fn().mockReturnValue(of({ items: [hit(3, 'Chitra Das')] })) };
        notify = { success: vi.fn(), error: vi.fn() };
        confirm = { confirm: vi.fn().mockReturnValue(of(true)) };
        TestBed.configureTestingModule({
            imports: [ElectionNomination],
            providers: [
                { provide: ElectionsService, useValue: elections },
                { provide: NetworkingService, useValue: networking },
                { provide: NotificationService, useValue: notify },
                { provide: ConfirmDialogService, useValue: confirm },
                { provide: AuthService, useValue: { currentUser: () => ({ memberId: 7 }) } }
            ]
        });
        const fixture = TestBed.createComponent(ElectionNomination);
        fixture.componentRef.setInput('electionId', 5);
        fixture.componentRef.setInput('phase', phase);
        fixture.detectChanges();
        return { fixture, component: fixture.componentInstance, el: fixture.nativeElement as HTMLElement };
    }

    const nomination = (id: number, candidateMemberId: number, status: NominationViewDto['status'], electionSeatId = 10): NominationViewDto =>
        ({ id, electionSeatId, candidateMemberId, status, statement: '' });

    afterEach(() => { vi.useRealTimers(); vi.restoreAllMocks(); });

    it('lists the seats in the nomination phase', () => {
        const { el } = create('Nomination');

        expect(elections.getSeats).toHaveBeenCalledWith(5);
        const options = Array.from(el.querySelectorAll('#nomination-seat option')).map(o => o.textContent?.trim());
        expect(options).toEqual(['Choose a seat', 'President', 'Members (2 places)']);
    });

    it('files the nomination with the signed-in member as proposer, then resets', () => {
        const { component } = create('Nomination');
        component.seatId.set(10);
        component.pick('candidate', hit(3, 'Chitra Das'));
        component.pick('seconder', hit(4, 'Dev Roy'));
        component.statement.set('  I will serve.  ');

        expect(component.canSubmit()).toBe(true);
        component.submit();

        expect(elections.nominate).toHaveBeenCalledWith(5, {
            electionSeatId: 10, candidateMemberId: 3, proposerMemberId: 7, seconderMemberId: 4, statement: 'I will serve.'
        });
        expect(notify.success).toHaveBeenCalled();
        expect(component.seatId()).toBeNull();
        expect(component.picked()).toEqual({ candidate: null, seconder: null });
        expect(elections.getNominations).toHaveBeenCalledTimes(2);
    });

    it('cannot submit without a seat, both members and a statement', () => {
        const { component } = create('Nomination');
        component.seatId.set(10);
        component.pick('candidate', hit(3, 'Chitra Das'));
        component.statement.set('   ');
        expect(component.canSubmit()).toBe(false);

        component.pick('seconder', hit(4, 'Dev Roy'));
        expect(component.canSubmit()).toBe(false);

        component.submit();
        expect(elections.nominate).not.toHaveBeenCalled();
    });

    it('keeps the form when the server rejects the nomination', () => {
        const { component } = create('Nomination');
        elections.nominate.mockReturnValue(throwError(() => new Error('400')));
        component.seatId.set(10);
        component.pick('candidate', hit(3, 'Chitra Das'));
        component.pick('seconder', hit(4, 'Dev Roy'));
        component.statement.set('Statement');

        component.submit();

        expect(component.submitting()).toBe(false);
        expect(component.seatId()).toBe(10);
        expect(notify.success).not.toHaveBeenCalled();
    });

    it('searches after the debounce and clears the pick when the text changes', () => {
        vi.useFakeTimers();
        const { component } = create('Nomination');
        component.pick('candidate', hit(9, 'Old Pick'));

        component.search('candidate', 'Chi');
        expect(component.picked().candidate).toBeNull();
        expect(networking.searchMembers).not.toHaveBeenCalled();

        vi.advanceTimersByTime(SEARCH_DEBOUNCE_MS);
        expect(networking.searchMembers).toHaveBeenCalledWith({ query: 'Chi', page: 1, pageSize: 10 });
        expect(component.hits().candidate).toEqual([hit(3, 'Chitra Das')]);
        expect(component.hits().seconder).toEqual([]);
    });

    it('skips a search under two characters', () => {
        vi.useFakeTimers();
        const { component } = create('Nomination');

        component.search('seconder', 'C');
        vi.advanceTimersByTime(SEARCH_DEBOUNCE_MS);

        expect(networking.searchMembers).not.toHaveBeenCalled();
    });

    it('drops a reply for a query the member has already changed', () => {
        vi.useFakeTimers();
        const { component } = create('Nomination');
        const reply = new Subject<any>();
        networking.searchMembers.mockReturnValue(reply);

        component.search('candidate', 'Chi');
        vi.advanceTimersByTime(SEARCH_DEBOUNCE_MS);
        component.search('candidate', 'Dev');
        reply.next({ items: [hit(3, 'Chitra Das')] });

        expect(component.hits().candidate).toEqual([]);
    });

    it('shows only the signed-in member\'s nominations, named by seat', () => {
        const { el } = create('Nomination', [nomination(1, 7, 'Submitted', 20), nomination(2, 8, 'Submitted')]);

        const rows = el.querySelectorAll('[data-nomination]');
        expect(rows.length).toBe(1);
        expect(rows[0].textContent).toContain('Members');
        expect(rows[0].textContent).toContain('Submitted');
        expect(el.querySelector('[data-nomination] button')).toBeNull();
    });

    it('hides the form and offers withdraw on accepted nominations in the withdrawal phase', () => {
        const { el } = create('Withdrawal', [nomination(1, 7, 'Accepted'), nomination(2, 7, 'Rejected', 20)]);

        expect(el.querySelector('[data-section="nominate"]')).toBeNull();
        expect(el.querySelector('[data-nomination="1"] button')?.textContent).toContain('Withdraw');
        expect(el.querySelector('[data-nomination="2"] button')).toBeNull();
    });

    it('withdraws after the member confirms', async () => {
        const { component, fixture, el } = create('Withdrawal', [nomination(1, 7, 'Accepted')]);

        await component.withdraw(component.mine()[0]);
        fixture.detectChanges();

        expect(confirm.confirm).toHaveBeenCalledWith(expect.objectContaining({ danger: true }));
        expect(elections.withdrawNomination).toHaveBeenCalledWith(1);
        expect(component.mine()[0].status).toBe('Withdrawn');
        expect(el.querySelector('[data-nomination="1"] button')).toBeNull();
    });

    it('does not withdraw when the member cancels', async () => {
        const { component } = create('Withdrawal', [nomination(1, 7, 'Accepted')]);
        confirm.confirm.mockReturnValue(of(false));

        await component.withdraw(component.mine()[0]);

        expect(elections.withdrawNomination).not.toHaveBeenCalled();
    });
});
