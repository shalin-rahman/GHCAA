import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConstitutionService, Constitution } from '../../core/services/constitution.service';
import { OrgConfigService } from '../../core/services/org-config.service';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { escapeHtml, slugify } from '../../core/utils/markdown.util';

/** One `Article N: Heading` block of the constitution body, for rendering and the in-page ToC. */
export interface ConstitutionArticle {
    id: string;
    heading: string;
    /** Escaped HTML — paragraphs of the article body. */
    bodyHtml: string;
}

/**
 * The authoritative PDF that predates the versioned repository. Used as the download target
 * whenever the active row carries no `pdfUrl`, and as the entire page content when the API
 * has no active constitution at all.
 */
export const CONSTITUTION_PDF_FALLBACK = '/assets/GHCAA Constitution V4.2.pdf';

/**
 * Splits the stored plain-text constitution into articles. The seeded format is
 * `Article I: Name and Office` on its own line, followed by body paragraphs.
 * Text before the first `Article` line becomes a preamble.
 */
export function parseArticles(content: string): ConstitutionArticle[] {
    const lines = (content ?? '').replace(/\r\n/g, '\n').split('\n');
    const articles: ConstitutionArticle[] = [];
    const seen = new Map<string, number>();
    let heading = 'Preamble';
    let buffer: string[] = [];

    const flush = () => {
        const body = buffer.join('\n').trim();
        // Drop an empty preamble; keep empty articles, since a heading with no text is
        // itself a signal that the stored content is incomplete.
        if (!body && heading === 'Preamble') { buffer = []; return; }
        const base = slugify(heading);
        const n = (seen.get(base) ?? 0) + 1;
        seen.set(base, n);
        const bodyHtml = body
            .split(/\n{2,}/)
            .map(p => p.trim())
            .filter(Boolean)
            .map(p => `<p>${escapeHtml(p).replace(/\n/g, '<br>')}</p>`)
            .join('');
        articles.push({ id: n === 1 ? base : `${base}-${n}`, heading, bodyHtml });
        buffer = [];
    };

    for (const line of lines) {
        const match = /^\s*(Article\s+[IVXLCDM\d]+\s*[:.—-]\s*.+)$/i.exec(line);
        if (match) {
            flush();
            heading = match[1].trim().replace(/\s+/g, ' ');
        } else {
            buffer.push(line);
        }
    }
    flush();
    return articles;
}

@Component({
    selector: 'app-constitution',
    standalone: true,
    imports: [CommonModule, LogoSpinnerComponent],
    templateUrl: './constitution.html',
    styleUrl: './constitution.scss'
})
export class ConstitutionPage implements OnInit {
    private service = inject(ConstitutionService);
    orgConfigService = inject(OrgConfigService);

    loading = signal(true);
    current = signal<Constitution | null>(null);
    history = signal<Constitution[]>([]);
    expandedVersionId = signal<number | null>(null);

    /** True when the API has no active constitution — the page then offers the PDF only. */
    pdfOnly = computed(() => !this.loading() && !this.current());

    articles = computed(() => parseArticles(this.current()?.content ?? ''));

    /** Prefer the version's own PDF; fall back to the static asset shipped with the site. */
    pdfUrl = computed(() => this.current()?.pdfUrl?.trim() || CONSTITUTION_PDF_FALLBACK);

    /** Superseded versions only — the active one is already rendered above the history panel. */
    pastVersions = computed(() => {
        const activeId = this.current()?.id;
        return this.history().filter(v => v.id !== activeId);
    });

    ngOnInit() {
        this.service.getCurrent().subscribe({
            next: c => { this.current.set(c); this.loading.set(false); },
            // 404 is the documented "no active version" state, not a failure worth surfacing.
            error: () => { this.current.set(null); this.loading.set(false); }
        });

        this.service.getHistory().subscribe({
            next: h => this.history.set(h ?? []),
            error: () => this.history.set([])
        });
    }

    toggleVersion(id: number) {
        this.expandedVersionId.set(this.expandedVersionId() === id ? null : id);
    }

    versionArticles(version: Constitution): ConstitutionArticle[] {
        return parseArticles(version.content ?? '');
    }

    scrollTo(id: string) {
        document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
}
