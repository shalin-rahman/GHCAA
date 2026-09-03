import { Component, OnInit, inject, signal, computed, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { OrgConfigService } from '../../core/services/org-config.service';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';
import {
    extractHeadings, renderFormMarkdown, renderMarkdownWithAnchors, MarkdownHeading
} from '../../core/utils/markdown.util';
import {
    ELECTION_DOCS, ELECTION_DOC_GROUPS, ElectionDoc, ElectionForm, formStage, splitForms
} from './election-docs';

/** Public assets written by `npm run sync:docs`. */
const ASSET_BASE = '/assets/elections/';

/** One stage of the election with the forms that belong to it. */
export interface FormStageGroup {
    stage: string;
    forms: ElectionForm[];
}

@Component({
    selector: 'app-elections',
    standalone: true,
    imports: [CommonModule, RouterLink, LogoSpinnerComponent, ImgFallbackDirective],
    templateUrl: './elections.html',
    styleUrl: './elections.scss'
})
export class ElectionsPage implements OnInit {
    private http = inject(HttpClient);
    private route = inject(ActivatedRoute);
    private destroyRef = inject(DestroyRef);
    private router = inject(Router);
    orgConfigService = inject(OrgConfigService);

    readonly groups = ELECTION_DOC_GROUPS;
    readonly docs = ELECTION_DOCS;

    selected = signal<ElectionDoc | null>(null);
    /** Raw markdown of the open document. */
    source = signal('');
    loading = signal(false);
    /** Set when the asset is missing — the page then still offers the download link. */
    loadError = signal(false);
    /** Within the forms handbook, the single form the visitor drilled into. */
    openForm = signal<ElectionForm | null>(null);

    forms = computed<ElectionForm[]>(() =>
        this.selected()?.isFormsHandbook ? splitForms(this.source()) : []
    );

    /**
     * The register is grouped by election stage and kept in handbook order, so an officer
     * finds a form by when it is used rather than by remembering its number.
     */
    formGroups = computed<FormStageGroup[]>(() => {
        const groups: FormStageGroup[] = [];
        for (const form of this.forms()) {
            const stage = formStage(form.code);
            const last = groups[groups.length - 1];
            if (last?.stage === stage) last.forms.push(form);
            else groups.push({ stage, forms: [form] });
        }
        return groups;
    });

    /** True while a completed-on-paper form is on screen, rather than a prose document. */
    isFormView = computed(() => !!this.openForm() || !!this.selected()?.isForm);

    /**
     * A form is rendered with the form vocabulary (ruled fields, tick boxes, signature
     * panels); a rules or procedure document is rendered as prose. The handbook itself
     * renders as the register above, so it produces no body.
     */
    bodyHtml = computed(() => {
        const form = this.openForm();
        if (form) return renderFormMarkdown(form.source);
        if (this.selected()?.isForm) return renderFormMarkdown(this.source());
        if (this.selected()?.isFormsHandbook) return '';
        return renderMarkdownWithAnchors(this.source(), 2);
    });

    headings = computed<MarkdownHeading[]>(() => {
        if (this.isFormView() || this.selected()?.isFormsHandbook) return [];
        return extractHeadings(this.source(), 2);
    });

    /* --- Letterhead -----------------------------------------------------------
       The pad is the association letterhead: crest, name, address block and the
       reference/date strip. Values come from OrgConfigService so the printed form
       carries the same contact details as the rest of the site, with the seeded
       fallbacks used when the API has not answered yet. */

    /** Constitution, Article I: the official founding date, printed in the pad footer. */
    get establishedOn(): string {
        return this.orgConfigService.config()?.branding?.establishedOn || '29 Nov 2025';
    }

    get orgName(): string {
        return this.orgConfigService.config()?.branding?.fullName || 'Govt. Haraganga College Alumni Association';
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

    docsIn(group: string): ElectionDoc[] {
        return this.docs.filter(d => d.group === group);
    }

    downloadUrl(doc: ElectionDoc): string {
        return ASSET_BASE + doc.file;
    }

    ngOnInit() {
        // `?doc=` keeps a specific document linkable and survives a refresh.
        this.route.queryParamMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
            const id = params.get('doc');
            const doc = this.docs.find(d => d.id === id) ?? null;
            if (doc?.id === this.selected()?.id) return;
            doc ? this.load(doc) : this.close();
        });
    }

    open(doc: ElectionDoc) {
        this.router.navigate([], { relativeTo: this.route, queryParams: { doc: doc.id } });
    }

    close() {
        this.selected.set(null);
        this.openForm.set(null);
        this.source.set('');
        this.loadError.set(false);
    }

    backToIndex() {
        this.router.navigate([], { relativeTo: this.route, queryParams: {} });
    }

    private load(doc: ElectionDoc) {
        this.selected.set(doc);
        this.openForm.set(null);
        this.loadError.set(false);
        this.loading.set(true);
        this.http.get(this.downloadUrl(doc), { responseType: 'text' }).subscribe({
            next: text => { this.source.set(text); this.loading.set(false); },
            error: () => { this.source.set(''); this.loadError.set(true); this.loading.set(false); }
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

    /**
     * A single form has no file of its own on disk, so its download is produced from the
     * section text in memory. The object URL is revoked immediately after the click — a
     * leaked blob URL pins the string for the lifetime of the tab.
     */
    downloadForm(form: ElectionForm) {
        const blob = new Blob([form.source], { type: 'text/markdown;charset=utf-8' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `FORM-${form.code}.md`;
        a.click();
        URL.revokeObjectURL(url);
    }
}
