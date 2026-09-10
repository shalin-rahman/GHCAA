import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConstitutionService, Constitution } from '../../core/services/constitution.service';
import { OrgConfigService } from '../../core/services/org-config.service';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
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
 * A `Section 1:` / `Section A:` label opening a paragraph. The stored constitution marks
 * its subheadings this way; without this they render as ordinary body text and an article
 * reads as one undifferentiated wall.
 */
const SECTION_LABEL = /^(Section\s+[0-9]{1,3}|Section\s+[A-Z]|Section\s+[IVXLCDM]+)\s*[:.—-]\s*([\s\S]*)$/;

/**
 * A subheading is the label plus its short title and nothing else. Anything longer is a
 * section whose body runs on from the label in the same paragraph, and only the label is
 * emphasised there — promoting a whole paragraph to a heading would hide its text.
 */
const SUBHEAD_MAX = 60;

/**
 * Renders one body paragraph. Escaping happens on each piece before any markup is added,
 * so no stored text can inject HTML.
 */
function renderParagraph(text: string): string {
    const match = SECTION_LABEL.exec(text);
    if (match) {
        const label = match[1].trim();
        const rest = match[2].trim();
        if (!rest.includes('\n') && rest.length <= SUBHEAD_MAX) {
            // `.doc-prose h4` already carries the heading weight, colour and scroll offset.
            return `<h4>${escapeHtml(rest ? `${label}: ${rest}` : label)}</h4>`;
        }
        return `<p><strong>${escapeHtml(label)}:</strong> ${escapeHtml(rest).replace(/\n/g, '<br>')}</p>`;
    }
    return `<p>${escapeHtml(text).replace(/\n/g, '<br>')}</p>`;
}

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
            .map(renderParagraph)
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
    imports: [CommonModule, LoadingPanelComponent],
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

    /** Prefer the version's own PDF, then the profile's, then the static asset shipped with the site. */
    pdfUrl = computed(() =>
        this.current()?.pdfUrl?.trim()
        || this.orgConfigService.config()?.branding?.constitutionPdfUrl?.trim()
        || CONSTITUTION_PDF_FALLBACK);

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
