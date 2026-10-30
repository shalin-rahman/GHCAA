// @vitest-environment jsdom
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap } from '@angular/router';
import { BehaviorSubject, of, throwError } from 'rxjs';
import { ElectionsPage } from './elections';
import { ElectionsService } from '../../core/services/elections.service';
import { ElectionContentService } from '../../core/services/election-content.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { OrgConfigService } from '../../core/services/org-config.service';
import { ElectionBoardDto, ElectionContentDto, PublicCandidateSeatDto } from '../../core/models/election.models';
import { ExportUtil } from '../../core/utils/export.util';

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

function page(over: Partial<ElectionContentDto> = {}): ElectionContentDto {
    return {
        id: 1, key: 'regulations', kind: 'Page', format: 'Prose', group: 'Rules',
        title: 'Election Regulations', summary: 'The rules.', body: null,
        displayOrder: 1, isPublished: true, lastModified: null,
        ...over
    };
}

const handbook = page({
    id: 5, key: 'forms', format: 'FormsHandbook', group: 'Forms', title: 'Forms Handbook', displayOrder: 5,
    body: '# FORM ER-01\n\n## Nomination Paper\n\nName: ____\n\n# FORM ER-02\n\n## Withdrawal\n\nName: ____'
});

interface Options {
    getBoard?: () => any;
    getCandidateList?: () => any;
    current?: any;
    pages?: () => any;
    forms?: ElectionContentDto[];
    getPage?: () => any;
    doc?: string | null;
    signedIn?: boolean;
    roles?: string[];
    defaultPrintMode?: 'Blank' | 'Filled';
}

function create(opts: Options = {}) {
    const elections = {
        getCurrent: vi.fn().mockReturnValue(of(opts.current === undefined ? { id: 7 } : opts.current)),
        getBoard: vi.fn().mockImplementation(opts.getBoard ?? (() => of(board()))),
        getPublicCandidateList: vi.fn().mockImplementation(opts.getCandidateList ?? (() => of([
            { seatId: 1, positionTitle: 'President', places: 1, candidates: [{ nominationId: 12, name: 'Candidate One', photoUrl: null, statement: 'A public statement.' }] }
        ] as PublicCandidateSeatDto[]))),
        getBlankForm: vi.fn().mockReturnValue(of(new Blob(['%PDF']))),
        getOfficialDocument: vi.fn().mockReturnValue(of(new Blob(['%PDF'])))
    };
    const content = {
        list: vi.fn().mockImplementation((kind: string) => kind === 'Form'
            ? of(opts.forms ?? [page({ key: 'ER-01', kind: 'Form', format: null })])
            : (opts.pages ?? (() => of([page()])))()),
        get: vi.fn().mockImplementation(opts.getPage ?? (() => of(handbook)))
    };
    const roles = opts.roles ?? [];
    const auth = { isAuthenticated: () => !!opts.signedIn, hasRole: (r: string) => roles.includes(r) };
    const notify = { error: vi.fn(), success: vi.fn() };
    const config = { elections: { defaultPrintMode: opts.defaultPrintMode ?? 'Blank' } };
    const router = { navigate: vi.fn() };
    TestBed.configureTestingModule({
        imports: [ElectionsPage],
        providers: [
            { provide: ElectionsService, useValue: elections },
            { provide: ElectionContentService, useValue: content },
            { provide: AuthService, useValue: auth },
            { provide: NotificationService, useValue: notify },
            { provide: OrgConfigService, useValue: { config: () => config, localePack: () => null } },
            { provide: ActivatedRoute, useValue: { queryParamMap: new BehaviorSubject(convertToParamMap(opts.doc ? { doc: opts.doc } : {})) } },
            { provide: Router, useValue: router }
        ]
    });
    const fixture = TestBed.createComponent(ElectionsPage);
    fixture.detectChanges();
    return { elections, content, notify, router, fixture, component: fixture.componentInstance, el: fixture.nativeElement as HTMLElement };
}

function buttons(el: HTMLElement, text: string) {
    return Array.from(el.querySelectorAll('button')).filter(b => b.textContent?.trim() === text);
}

describe('ElectionsPage board', () => {
    beforeEach(() => vi.spyOn(window, 'scrollTo').mockImplementation(() => { }));

    // FR-33
    it('lists the officials by group with no contact details', () => {
        const { el } = create();

        expect(el.textContent).toContain('Who runs this election');
        expect(el.textContent).toContain('Commission');
        expect(el.textContent).toContain('Rahim Uddin');
        expect(el.textContent).toContain('Chief Election Commissioner');
        expect(el.querySelector('#election-board a[href^="mailto:"], #election-board a[href^="tel:"]')).toBeNull();
    });

    it('shows the turnout line only when the board carries one', () => {
        expect(create().el.textContent).toContain('40 of 100');

        TestBed.resetTestingModule();
        expect(create({ getBoard: () => of(board({ turnout: null })) }).el.textContent).not.toContain('Turnout');
    });

    it('marks a test election', () => {
        const { el } = create({ getBoard: () => of(board({ isTest: true })) });

        expect(el.querySelector('#election-board .status-badge')?.textContent).toContain('Test');
    });

    it('shows the fingerprint, the audit hash and the audit chain link', () => {
        const { el } = create();
        const link = Array.from(el.querySelectorAll('a')).find(a => a.textContent?.includes('Check the audit chain'));

        expect(el.textContent).toContain('AB12 CD34');
        expect(el.textContent).toContain('deadbeef');
        expect(link?.getAttribute('href')).toBe('/api/elections/7/audit/verify');
    });

    it('leaves out the audit hash before the result is declared', () => {
        const { el } = create({ getBoard: () => of(board({ phase: 'Polling', auditHeadHash: null })) });

        expect(el.textContent).not.toContain('Check the audit chain');
    });

    it('hides the board when there is no current election', () => {
        const { el, elections } = create({ current: null });

        expect(elections.getBoard).not.toHaveBeenCalled();
        expect(el.querySelector('#election-board')).toBeNull();
    });

    it('hides the board when the call fails', () => {
        const { el } = create({ getBoard: () => throwError(() => new Error('404')) });

        expect(el.querySelector('#election-board')).toBeNull();
        expect(el.textContent).toContain('Elections & Forms');
    });

    it('FR-051: publishes the finalized candidate list on the public board', () => {
        const { el, elections } = create({ getBoard: () => of(board({ phase: 'CandidateList' })) });

        expect(elections.getPublicCandidateList).toHaveBeenCalledWith(7);
        expect(el.textContent).toContain('Final candidate list');
        expect(el.textContent).toContain('President');
        expect(el.textContent).toContain('Candidate One');
        expect(el.textContent).toContain('A public statement.');
    });

    it('FR-051: does not request or show candidates before the list is finalized', () => {
        const { el, elections } = create({ getBoard: () => of(board({ phase: 'Withdrawal' })) });

        expect(elections.getPublicCandidateList).not.toHaveBeenCalled();
        expect(el.textContent).not.toContain('Final candidate list');
    });

    it('FR-051: does not publish candidates from test elections', () => {
        const { el, elections } = create({ getBoard: () => of(board({ phase: 'CandidateList', isTest: true })) });

        expect(elections.getPublicCandidateList).not.toHaveBeenCalled();
        expect(el.textContent).not.toContain('Final candidate list');
    });

    it('FR-051: clears the previous candidate list when a reload finds an earlier phase', () => {
        let reloaded = false;
        const { component, el, fixture } = create({ getBoard: () => of(board({ phase: reloaded ? 'Withdrawal' : 'CandidateList' })) });

        expect(component.publicCandidateList()).not.toBeNull();
        reloaded = true;
        component.ngOnInit();
        fixture.detectChanges();

        expect(component.publicCandidateList()).toBeNull();
        expect(el.textContent).not.toContain('Candidate One');
    });
});

// FR-25
describe('ElectionsPage documents from the API', () => {
    beforeEach(() => vi.spyOn(window, 'scrollTo').mockImplementation(() => { }));
    afterEach(() => vi.restoreAllMocks());

    it('groups the published pages in display order', () => {
        const { el, content } = create({
            pages: () => of([
                page({ id: 3, key: 'forms', group: 'Forms', title: 'Forms Handbook', displayOrder: 5 }),
                page({ id: 1, key: 'regulations', group: 'Rules', title: 'Election Regulations', displayOrder: 1 }),
                page({ id: 2, key: 'conduct', group: 'Rules', title: 'Code of Conduct', displayOrder: 2, summary: null })
            ])
        });
        const groups = Array.from(el.querySelectorAll('.doc-group .group-title')).map(h => h.textContent?.trim());

        expect(content.list).toHaveBeenCalledWith('Page');
        expect(groups).toEqual(['EC 2027', 'Rules', 'Forms']);
        expect(el.textContent).toContain('The rules.');
    });

    it('says so when the page list cannot be loaded', () => {
        const { el } = create({ pages: () => throwError(() => new Error('500')) });

        expect(el.textContent).toContain('could not be loaded');
    });

    it('opens a page by its key through the query string', () => {
        const { component, router } = create();

        component.open(page({ key: 'regulations' }));

        expect(router.navigate).toHaveBeenCalledWith([], expect.objectContaining({ queryParams: { doc: 'regulations' } }));
    });

    it('loads the page named in ?doc= and shows its register of forms', () => {
        const { el, content } = create({ doc: 'forms' });

        expect(content.get).toHaveBeenCalledWith('Page', 'forms');
        expect(el.querySelectorAll('.register-row').length).toBe(2);
        expect(el.textContent).toContain('Nomination Paper');
    });

    it('shows a not-found card when the page cannot be fetched', () => {
        const { el } = create({ doc: 'missing', getPage: () => throwError(() => new Error('404')) });

        expect(el.querySelector('.error-card')?.textContent).toContain('could not be found');
    });

    it('downloads the open page as markdown named by its key', () => {
        const save = vi.spyOn(ExportUtil, 'saveFile').mockImplementation(() => { });
        const { component } = create({ doc: 'forms' });

        component.downloadPage();

        expect(save).toHaveBeenCalledWith(expect.any(Blob), 'forms.md');
    });
});

// FR-25
describe('ElectionsPage form PDFs', () => {
    beforeEach(() => vi.spyOn(window, 'scrollTo').mockImplementation(() => { }));
    afterEach(() => vi.restoreAllMocks());

    it('offers no PDF to a visitor who is not signed in', () => {
        const { el } = create({ doc: 'forms' });

        expect(buttons(el, 'Blank PDF')).toEqual([]);
    });

    it('offers no blank PDF to a signed-in member who is not an admin', () => {
        const { el } = create({ doc: 'forms', signedIn: true });

        expect(buttons(el, 'Blank PDF')).toEqual([]);
    });

    it('offers a blank PDF to an admin, only for forms the server can print', () => {
        const { el } = create({ doc: 'forms', signedIn: true, roles: ['Admin'], current: null });

        // ER-01 has a server layout; ER-02 does not.
        expect(buttons(el, 'Blank PDF').length).toBe(1);
        expect(buttons(el, 'Filled PDF')).toEqual([]);
    });

    it('offers a filled PDF to election staff while an election is running', () => {
        const { component } = create({ doc: 'forms', signedIn: true, roles: ['ElectionOfficial'] });

        expect(component.printModes()).toEqual(['Filled']);
    });

    it('drops the filled PDF when there is no current election', () => {
        const { component } = create({ doc: 'forms', signedIn: true, roles: ['Admin'], current: null });

        expect(component.printModes()).toEqual(['Blank']);
    });

    it('puts the org default print mode first', () => {
        const { component } = create({ signedIn: true, roles: ['Admin'], defaultPrintMode: 'Filled' });

        expect(component.printModes()).toEqual(['Filled', 'Blank']);
    });

    it('fetches a blank PDF without an election and saves it under the form code', () => {
        const save = vi.spyOn(ExportUtil, 'saveFile').mockImplementation(() => { });
        const { component, elections } = create({ signedIn: true, roles: ['Admin'] });

        component.downloadPdf('ER-01', 'Blank');

        expect(elections.getBlankForm).toHaveBeenCalledWith('ER-01');
        expect(save).toHaveBeenCalledWith(expect.any(Blob), 'ER-01-blank.pdf');
    });

    it('fetches a filled PDF for the current election', () => {
        vi.spyOn(ExportUtil, 'saveFile').mockImplementation(() => { });
        const { component, elections } = create({ signedIn: true, roles: ['Admin'] });

        component.downloadPdf('ER-01', 'Filled');

        expect(elections.getOfficialDocument).toHaveBeenCalledWith(7, 'ER-01', 'Filled');
    });

    it('tells the user when the PDF cannot be made', () => {
        const { component, elections, notify } = create({ signedIn: true, roles: ['Admin'] });
        elections.getBlankForm.mockReturnValue(throwError(() => new Error('500')));

        component.downloadPdf('ER-01', 'Blank');

        expect(notify.error).toHaveBeenCalled();
        expect(component.printing()).toBe(false);
    });

    it('uses the page key as the form code on a single-form page', () => {
        const { component } = create({
            doc: 'er-25', signedIn: true, roles: ['Admin'], forms: [page({ key: 'ER-25', kind: 'Form', format: null })],
            getPage: () => of(page({ key: 'er-25', format: 'Form', group: 'Forms', body: '# FORM ER-25\n\nName: ____' }))
        });

        expect(component.currentFormCode()).toBe('ER-25');
        expect(component.canPrintPdf(component.currentFormCode())).toBe(true);
    });
});
