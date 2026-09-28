// @vitest-environment jsdom
import { TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { MemberElection } from './election';
import { ElectionsService } from '../../core/services/elections.service';
import { NotificationService } from '../../core/services/notification.service';

describe('MemberElection', () => {
    let elections: any;
    let notify: any;

    const nominations = [
        { id: 1, electionSeatId: 10, status: 'Accepted', statement: 'A' },
        { id: 2, electionSeatId: 10, status: 'Accepted', statement: 'B' },
        { id: 3, electionSeatId: 20, status: 'Accepted', statement: 'C' },
        { id: 4, electionSeatId: 20, status: 'Withdrawn', statement: 'D' }
    ];

    function create(phase = 'Polling'): MemberElection {
        elections = {
            getCurrent: vi.fn().mockReturnValue(of({ id: 5, phase })),
            getNominations: vi.fn().mockReturnValue(of(nominations)),
            castBallot: vi.fn().mockReturnValue(of({ trackingCode: 'ABCD-EFGH' }))
        };
        notify = { success: vi.fn(), error: vi.fn() };
        TestBed.configureTestingModule({
            imports: [MemberElection],
            providers: [
                { provide: ElectionsService, useValue: elections },
                { provide: NotificationService, useValue: notify }
            ]
        });
        return TestBed.createComponent(MemberElection).componentInstance;
    }

    // FR-39
    it('sends every contested seat in one ballot, with an unpicked seat left blank', () => {
        const component = create();
        component.choose(10, 2);

        component.submit();

        expect(elections.castBallot).toHaveBeenCalledWith(5, {
            seats: [
                { electionSeatId: 10, nominationIds: [2] },
                { electionSeatId: 20, nominationIds: [] }
            ]
        });
        expect(component.trackingCode()).toBe('ABCD-EFGH');
        expect(component.closed).toBe(true);
    });

    it('clears a choice when the same candidate is picked again', () => {
        const component = create();
        component.choose(10, 1);
        component.choose(10, 1);

        expect(component.isChosen(10, 1)).toBe(false);
    });

    // NFR-R5
    it('locks the ballot when the server says the member already voted', () => {
        const component = create();
        elections.castBallot.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 409 })));

        component.submit();

        expect(component.alreadyVoted()).toBe(true);
        expect(component.closed).toBe(true);
        expect(notify.error).toHaveBeenCalled();
    });

    it('does not submit outside polling', () => {
        const component = create('Scrutiny');

        component.submit();

        expect(elections.castBallot).not.toHaveBeenCalled();
    });
});
