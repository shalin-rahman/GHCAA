import { Component, OnInit, inject, signal, computed, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { OrgConfigService } from '../../core/services/org-config.service';
import { ElectionsService } from '../../core/services/elections.service';
import { ElectionContentService } from '../../core/services/election-content.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { ElectionBoardDto, ElectionContentDto, ElectionPrintMode } from '../../core/models/election.models';
import { ADMIN_ROLES, API_ENDPOINTS, ELECTION_STAFF_ROLES, getElectionPhaseLabel } from '../../core/constants/app.constants';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';
import { ExportUtil } from '../../core/utils/export.util';
import {
    extractHeadings, renderFormMarkdown, renderMarkdownWithAnchors, MarkdownHeading
} from '../../core/utils/markdown.util';
import { ElectionForm, formNumber, formStage, splitForms } from './election-docs';

/** One stage of the election with the forms that belong to it. */
export interface FormStageGroup {
    stage: string;
    forms: ElectionForm[];
}

/** One group of pages on the index, in the order the admin set. */
export interface PageGroup {
    group: string;
    pages: ElectionContentDto[];
}

@Component({
    selector: 'app-elections',
    standalone: true,
    imports: [CommonModule, RouterLink, LoadingPanelComponent, ImgFallbackDirective],
    templateUrl: './elections.html',
    styleUrl: './elections.scss'
})
export class ElectionsPage implements OnInit {
    private electionsService = inject(ElectionsService);
    private contentService = inject(ElectionContentService);
    private auth = inject(AuthService);
    private notify = inject(NotificationService);
    private route = inject(ActivatedRoute);
    private destroyRef = inject(DestroyRef);
    private router = inject(Router);
    orgConfigService = inject(OrgConfigService);

    /** 37.14s. Published pages, without bodies. */
    pages = signal<ElectionContentDto[]>([]);
    pagesLoading = signal(true);
    pagesError = signal(false);
    /** Codes of the published forms the server can print as a PDF. */
    printableCodes = signal<ReadonlySet<string>>(new Set());

    /** The `?doc=` key on screen. Set before the page arrives, so the reader shows its spinner. */
    openKey = signal<string | null>(null);
    /** The open page with its body. */
    selected = signal<ElectionContentDto | null>(null);
    loading = signal(false);
    loadError = signal(false);
    /** Within the forms handbook, the single form the visitor drilled into. */
    openForm = signal<ElectionForm | null>(null);
    /** The current election's public board. Stays null when there is no election or the call fails. */
    board = signal<ElectionBoardDto | null>(null);
    /** Set when an election is running, so staff can print its forms filled in. */
    currentElectionId = signal<number | null>(null);
    printing = signal(false);
    getElectionPhaseLabel = getElectionPhaseLabel;

    source = computed(() => this.selected()?.body ?? '');
    isFormsHandbook = computed(() => this.selected()?.format === 'FormsHandbook');
    isSingleForm = computed(() => this.selected()?.format === 'Form');

    pageGroups = computed<PageGroup[]>(() => {
        const groups: PageGroup[] = [];
        const sorted = [...this.pages()].sort((a, b) => a.displayOrder - b.displayOrder);
        for (const page of sorted) {
            const name = page.group || 'Other';
            const existing = groups.find(g => g.group === name);
            if (existing) existing.pages.push(page);
            else groups.push({ group: name, pages: [page] });
        }
        return groups;
    });

    forms = computed<ElectionForm[]>(() =>
        this.isFormsHandbook() ? splitForms(this.source()) : []
    );

    /**
     * The register is grouped by election stage and sorted by form number, so an officer
     * finds a form by when it is used rather than by remembering its number. The handbook
     * pages keep some lettered forms away from their number, so page order is not enough.
     */
    formGroups = computed<FormStageGroup[]>(() => {
        const groups: FormStageGroup[] = [];
        const sorted = [...this.forms()].sort((a, b) => formNumber(a.code) - formNumber(b.code));
        for (const form of sorted) {
            const stage = formStage(form.code);
            const last = groups[groups.length - 1];
            if (last?.stage === stage) last.forms.push(form);
            else groups.push({ stage, forms: [form] });
        }
        return groups;
    });

    /** True while a completed-on-paper form is on screen, rather than a prose document. */
    isFormView = computed(() => !!this.openForm() || this.isSingleForm());

    /** The code of the form on screen. A single-form page uses its key, such as er-25. */
    currentFormCode = computed(() => {
        const form = this.openForm();
        if (form) return form.code;
        return this.isSingleForm() ? this.selected()!.key.toUpperCase() : null;
    });

    /**
     * A form is rendered with the form vocabulary (ruled fields, tick boxes, signature
     * panels); a rules or procedure document is rendered as prose. The handbook itself
     * renders as the register above, so it produces no body.
     */
    bodyHtml = computed(() => {
        const form = this.openForm();
        if (form) return renderFormMarkdown(form.source);
        if (this.isSingleForm()) return renderFormMarkdown(this.source());
        if (this.isFormsHandbook()) return '';
        return renderMarkdownWithAnchors(this.source(), 2);
    });

    headings = computed<MarkdownHeading[]>(() => {
        if (this.isFormView() || this.isFormsHandbook()) return [];
        return extractHeadings(this.source(), 2);
    });

    /**
     * 37.14s. The PDF modes this visitor can print, with the org's default first. A blank
     * form needs an admin; a filled one needs election staff and a running election.
     */
    printModes = computed<ElectionPrintMode[]>(() => {
        if (!this.auth.isAuthenticated()) return [];
        const modes: ElectionPrintMode[] = [];
        if (ADMIN_ROLES.some(r => this.auth.hasRole(r))) modes.push('Blank');
        if (this.currentElectionId() && ELECTION_STAFF_ROLES.some(r => this.auth.hasRole(r))) modes.push('Filled');
        const preferred = this.orgConfigService.config()?.elections?.defaultPrintMode ?? 'Blank';
        return modes.sort((a, b) => (a === preferred ? -1 : b === preferred ? 1 : 0));
    });

    /* --- Letterhead -----------------------------------------------------------
       The pad is the association letterhead: crest, name, address block and the
       reference/date strip. Values come from OrgConfigService, which is always
       populated by the time a route renders — APP_INITIALIZER blocks bootstrap on
       loadConfig(), and that falls back to the build-time profile pack fallback
       (ORG_CONFIG_FALLBACK) rather than leaving config() empty on API failure. */

    /** Constitution, Article I: the official founding date, printed in the pad footer. */
    get establishedOn(): string {
        return this.orgConfigService.config()?.branding?.establishedOn ?? '';
    }

    get orgName(): string {
        return this.orgConfigService.config()?.branding?.fullName ?? '';
    }

    get crestUrl(): string {
        return this.orgConfigService.config()?.branding?.logoUrl || '/assets/logo.png';
    }

    get address(): string {
        const contact = this.orgConfigService.config()?.contact;
        return contact?.campusAddress || contact?.registeredOffice || '';
    }

    get phone(): string {
        return this.orgConfigService.config()?.contact?.phoneNumbers?.[0] || '';
    }

    get email(): string {
        return this.orgConfigService.config()?.contact?.supportEmail || '';
    }

    get motto(): string {
        return this.orgConfigService.localePack()?.tagline || '';
    }

    /** Heading printed above the fields: the form code, or the document title. */
    get sheetTitle(): string {
        const form = this.openForm();
        return form ? `Form ${form.code}` : (this.selected()?.title ?? '');
    }

    /** Link to the audit chain check, which anyone can open. */
    get auditVerifyUrl(): string {
        const board = this.board();
        return board ? API_ENDPOINTS.ELECTIONS.AUDIT_VERIFY(board.id) : '';
    }

    canPrintPdf(code: string | null): boolean {
        return !!code && this.printModes().length > 0 && this.printableCodes().has(code.toUpperCase());
    }

    ngOnInit() {
        this.electionsService.getCurrent().subscribe({
            next: election => {
                if (!election) return;
                this.currentElectionId.set(election.id);
                this.electionsService.getBoard(election.id).subscribe({
                    next: board => this.board.set(board),
                    error: () => this.board.set(null)
                });
            },
            error: () => this.board.set(null)
        });
        this.contentService.list('Page').subscribe({
            next: pages => { this.pages.set(pages); this.pagesLoading.set(false); },
            error: () => { this.pagesError.set(true); this.pagesLoading.set(false); }
        });
        this.contentService.list('Form').subscribe({
            next: forms => this.printableCodes.set(new Set(forms.map(f => f.key.toUpperCase()))),
            error: () => this.printableCodes.set(new Set())
        });
        // `?doc=` keeps a specific document linkable and survives a refresh.
        this.route.queryParamMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
            const key = params.get('doc');
            if (key === this.openKey()) return;
            key ? this.load(key) : this.close();
        });
    }

    open(page: ElectionContentDto) {
        this.router.navigate([], { relativeTo: this.route, queryParams: { doc: page.key } });
    }

    close() {
        this.openKey.set(null);
        this.selected.set(null);
        this.openForm.set(null);
        this.loadError.set(false);
    }

    backToIndex() {
        this.router.navigate([], { relativeTo: this.route, queryParams: {} });
    }

    private load(key: string) {
        this.openKey.set(key);
        this.selected.set(null);
        this.openForm.set(null);
        this.loadError.set(false);
        this.loading.set(true);
        this.contentService.get('Page', key).subscribe({
            next: page => { this.selected.set(page); this.loading.set(false); },
            error: () => { this.loadError.set(true); this.loading.set(false); }
        });
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    showForm(form: ElectionForm) {
        this.openForm.set(form);
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    backToForms() {
        this.openForm.set(null);
    }

    scrollTo(id: string) {
        document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    print() {
        window.print();
    }

    /** The open page as a markdown file, built from the text the server sent. */
    downloadPage() {
        const page = this.selected();
        if (!page) return;
        ExportUtil.saveFile(new Blob([this.source()], { type: 'text/markdown;charset=utf-8' }), `${page.key}.md`);
    }

    downloadForm(form: ElectionForm) {
        ExportUtil.saveFile(new Blob([form.source], { type: 'text/markdown;charset=utf-8' }), `FORM-${form.code}.md`);
    }

    /** 37.14s. The server-made PDF: blank for admins, filled for staff during an election. */
    downloadPdf(code: string, mode: ElectionPrintMode) {
        const electionId = this.currentElectionId();
        const request: Observable<Blob> = mode === 'Filled' && electionId
            ? this.electionsService.getOfficialDocument(electionId, code, 'Filled')
            : this.electionsService.getBlankForm(code);
        this.printing.set(true);
        request.subscribe({
            next: blob => {
                ExportUtil.saveFile(blob, `${code}-${mode.toLowerCase()}.pdf`);
                this.printing.set(false);
            },
            error: () => {
                this.notify.error(`Could not make the ${mode.toLowerCase()} PDF of ${code}.`);
                this.printing.set(false);
            }
        });
    }
}
