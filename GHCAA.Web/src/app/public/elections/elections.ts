import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { OrgConfigService } from '../../core/services/org-config.service';
import { extractHeadings, renderMarkdownWithAnchors, MarkdownHeading } from '../../core/utils/markdown.util';
import {
    ELECTION_DOCS, ELECTION_DOC_GROUPS, ElectionDoc, ElectionForm, splitForms
} from './election-docs';

/** Public assets written by `npm run sync:docs`. */
const ASSET_BASE = '/assets/elections/';

@Component({
    selector: 'app-elections',
    standalone: true,
    imports: [CommonModule, RouterLink, LogoSpinnerComponent],
    templateUrl: './elections.html',
    styleUrl: './elections.scss'
})
export class ElectionsPage implements OnInit {
    private http = inject(HttpClient);
    private route = inject(ActivatedRoute);
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

    /** The handbook renders as a form index, so only whole documents render as prose. */
    bodyHtml = computed(() => {
        const form = this.openForm();
        if (form) return renderMarkdownWithAnchors(form.source, 3);
        if (this.selected()?.isFormsHandbook) return '';
        return renderMarkdownWithAnchors(this.source(), 2);
    });

    headings = computed<MarkdownHeading[]>(() => {
        if (this.openForm() || this.selected()?.isFormsHandbook) return [];
        return extractHeadings(this.source(), 2);
    });

    docsIn(group: string): ElectionDoc[] {
        return this.docs.filter(d => d.group === group);
    }

    downloadUrl(doc: ElectionDoc): string {
        return ASSET_BASE + doc.file;
    }

    ngOnInit() {
        // `?doc=` keeps a specific document linkable and survives a refresh.
        this.route.queryParamMap.subscribe(params => {
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
