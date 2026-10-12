// @vitest-environment jsdom
import { TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { ElectionResults } from './election-results';
import { ElectionsService } from '../../core/services/elections.service';
import { PublicElectionResultsDto } from '../../core/models/election.models';

describe('ElectionResults', () => {
    const results: PublicElectionResultsDto = {
        electionId: 5, title: 'EC 2026', phase: 'Declared', declaredOn: '2026-10-05T00:00:00Z',
        eligibleVoters: 120, ballotsCast: 80, spoiledBallots: 2,
        seats: [{
            seatId: 10, positionTitle: 'President', places: 1, abstentions: 7,
            candidates: [
                { nominationId: 1, name: 'Amina', voteCount: 40, isElected: true, isTie: false },
                { nominationId: 2, name: 'Bilal', voteCount: 31, isElected: false, isTie: false }
            ]
        }]
    };
    const notFound = () => throwError(() => new HttpErrorResponse({ status: 404 }));
    let elections: any;

    function create(resultCall: any = of(results), codesCall: any = of(['AAAA-1111', 'BBBB-2222', 'CCCC-3333']), latest: any = of({ id: 5 })) {
        elections = {
            getLatestDeclared: vi.fn().mockReturnValue(latest),
            getPublicResults: vi.fn().mockReturnValue(resultCall),
            getTrackingCodes: vi.fn().mockReturnValue(codesCall)
        };
        TestBed.configureTestingModule({ imports: [ElectionResults], providers: [{ provide: ElectionsService, useValue: elections }] });
        const fixture = TestBed.createComponent(ElectionResults);
        fixture.detectChanges();
        return { fixture, component: fixture.componentInstance, el: fixture.nativeElement as HTMLElement };
    }

    it('shows turnout, spoiled ballots, votes, elected marker and abstentions', () => {
        const { el } = create();
        const text = el.textContent!;

        expect(text).toContain('80 of 120 eligible voters');
        expect(text).toContain('Spoiled ballots: 2');
        expect(text).toContain('Amina');
        expect(text).toContain('40 votes');
        expect(text).toContain('Elected');
        expect(text).toContain('Abstentions');
        expect(text).toContain('7');
    });

    it('marks a tie', () => {
        const tied = { ...results, seats: [{ ...results.seats[0], candidates: [{ ...results.seats[0].candidates[1], isTie: true }] }] };
        const { el } = create(of(tied));

        expect(el.textContent).toContain('Tie');
    });

    it('says results are not declared yet on a 404', () => {
        const { el, component } = create(notFound());

        expect(component.notDeclared()).toBe(true);
        expect(el.textContent).toContain('Results are not declared yet');
    });

    it('filters the tracking codes', () => {
        const { component, fixture, el } = create();
        expect(el.querySelectorAll('li').length).toBe(3);

        component.codeFilter.set('bbbb');
        fixture.detectChanges();

        expect(component.matchingCodes()).toEqual(['BBBB-2222']);
        expect(el.querySelectorAll('li').length).toBe(1);
    });

    it('says a code was not found', () => {
        const { component, fixture, el } = create();

        component.codeFilter.set('zzzz');
        fixture.detectChanges();

        expect(el.textContent).toContain('No code matches');
    });

    it('says the codes are not published yet on a 404', () => {
        const { el, component } = create(of(results), notFound());

        expect(component.codesNotPublished()).toBe(true);
        expect(el.textContent).toContain('not published yet');
    });

    it('loads the results of the latest declared election', () => {
        create();

        expect(elections.getLatestDeclared).toHaveBeenCalled();
        expect(elections.getPublicResults).toHaveBeenCalledWith(5);
    });

    it('says results are not declared yet when there is no declared election', () => {
        const { el, component } = create(of(results), of([]), notFound());

        expect(component.notDeclared()).toBe(true);
        expect(component.error()).toBe(false);
        expect(el.textContent).toContain('Results are not declared yet');
        expect(elections.getPublicResults).not.toHaveBeenCalled();
    });

    it('shows the error state when the latest-declared call fails', () => {
        const { component } = create(of(results), of([]), throwError(() => new HttpErrorResponse({ status: 500 })));

        expect(component.error()).toBe(true);
    });
});
