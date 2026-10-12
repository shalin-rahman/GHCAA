// @vitest-environment jsdom
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap } from '@angular/router';
import { BehaviorSubject, of, throwError } from 'rxjs';
import { ElectionsPage } from './elections';
import { ElectionsService } from '../../core/services/elections.service';
import { OrgConfigService } from '../../core/services/org-config.service';
import { ElectionBoardDto } from '../../core/models/election.models';

describe('ElectionsPage board', () => {
    function board(over: Partial<ElectionBoardDto> = {}): ElectionBoardDto {
        return {
            id: 7, title: 'EC 2027', phase: 'Declared', isTest: false,
            groups: [
                { groupName: 'Commission', members: [{ personaName: 'Chief Election Commissioner', displayName: 'Rahim Uddin', acceptedAt: '2026-10-01T00:00:00Z' }] }
            ],
            turnout: { ballotsCast: 40, rollSize: 100 },
            keyFingerprint: 'AB12 CD34', auditHeadHash: 'deadbeef',
            ...over
        };
    }

    function create(getBoard: () => any, current: any = { id: 7 }) {
        const elections = {
            getCurrent: vi.fn().mockReturnValue(of(current)),
            getBoard: vi.fn().mockImplementation(getBoard),
            getDocumentText: vi.fn().mockReturnValue(of(''))
        };
        TestBed.configureTestingModule({
            imports: [ElectionsPage],
            providers: [
                { provide: ElectionsService, useValue: elections },
                { provide: OrgConfigService, useValue: { config: () => null, localePack: () => null } },
                { provide: ActivatedRoute, useValue: { queryParamMap: new BehaviorSubject(convertToParamMap({})) } },
                { provide: Router, useValue: { navigate: vi.fn() } }
            ]
        });
        const fixture = TestBed.createComponent(ElectionsPage);
        fixture.detectChanges();
        return { elections, el: fixture.nativeElement as HTMLElement };
    }

    // FR-33
    it('lists the officials by group with no contact details', () => {
        const { el } = create(() => of(board()));

        expect(el.textContent).toContain('Who runs this election');
        expect(el.textContent).toContain('Commission');
        expect(el.textContent).toContain('Rahim Uddin');
        expect(el.textContent).toContain('Chief Election Commissioner');
        expect(el.querySelector('#election-board a[href^="mailto:"], #election-board a[href^="tel:"]')).toBeNull();
    });

    it('shows the turnout line only when the board carries one', () => {
        expect(create(() => of(board())).el.textContent).toContain('40 of 100');

        TestBed.resetTestingModule();
        expect(create(() => of(board({ turnout: null }))).el.textContent).not.toContain('Turnout');
    });

    it('marks a test election', () => {
        const { el } = create(() => of(board({ isTest: true })));

        expect(el.querySelector('#election-board .status-badge')?.textContent).toContain('Test');
    });

    it('shows the fingerprint, the audit hash and the audit chain link', () => {
        const { el } = create(() => of(board()));
        const link = Array.from(el.querySelectorAll('a')).find(a => a.textContent?.includes('Check the audit chain'));

        expect(el.textContent).toContain('AB12 CD34');
        expect(el.textContent).toContain('deadbeef');
        expect(link?.getAttribute('href')).toBe('/api/elections/7/audit/verify');
    });

    it('leaves out the audit hash before the result is declared', () => {
        const { el } = create(() => of(board({ phase: 'Polling', auditHeadHash: null })));

        expect(el.textContent).not.toContain('Check the audit chain');
    });

    it('hides the board when there is no current election', () => {
        const { el, elections } = create(() => of(board()), null);

        expect(elections.getBoard).not.toHaveBeenCalled();
        expect(el.querySelector('#election-board')).toBeNull();
    });

    it('hides the board when the call fails', () => {
        const { el } = create(() => throwError(() => new Error('404')));

        expect(el.querySelector('#election-board')).toBeNull();
        expect(el.textContent).toContain('Elections & Forms');
    });
});
