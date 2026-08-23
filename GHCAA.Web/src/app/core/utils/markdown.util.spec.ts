import { describe, it, expect } from 'vitest';
import {
    escapeHtml, renderMarkdown, renderMarkdownWithAnchors, extractHeadings, slugify
} from './markdown.util';

describe('escapeHtml', () => {
    it('neutralises every HTML-significant character', () => {
        expect(escapeHtml(`<img src=x onerror="alert('x')">`))
            .toBe('&lt;img src=x onerror=&quot;alert(&#39;x&#39;)&quot;&gt;');
    });

    it('escapes the ampersand first so entities are not double-decoded', () => {
        expect(escapeHtml('&lt;')).toBe('&amp;lt;');
    });
});

describe('renderMarkdown', () => {
    it('never emits raw HTML from the source', () => {
        const html = renderMarkdown('Beware <script>alert(1)</script> of this');
        expect(html).not.toContain('<script>');
        expect(html).toContain('&lt;script&gt;');
    });

    it('renders headings at the right level', () => {
        expect(renderMarkdown('### Nomination')).toContain('<h3>Nomination</h3>');
    });

    it('renders a pipe table using the central table classes', () => {
        const html = renderMarkdown([
            '| Form | Purpose |',
            '| --- | --- |',
            '| ER-01 | Nomination |'
        ].join('\n'));
        expect(html).toContain('class="table-wrap"');
        expect(html).toContain('class="data-table"');
        expect(html).toContain('<th>Form</th>');
        expect(html).toContain('<td>ER-01</td>');
    });

    it('renders bullet and ordered lists', () => {
        expect(renderMarkdown('- one\n- two')).toContain('<ul><li>one</li><li>two</li></ul>');
        expect(renderMarkdown('1. first\n2. second')).toContain('<ol>');
    });

    it('renders inline emphasis and code', () => {
        const html = renderMarkdown('**bold** and *italic* and `code`');
        expect(html).toContain('<strong>bold</strong>');
        expect(html).toContain('<em>italic</em>');
        expect(html).toContain('<code>code</code>');
    });

    it('turns fill-in-the-blank runs into a visible blank', () => {
        expect(renderMarkdown('Name: ____________')).toContain('<span class="md-blank"></span>');
    });

    it('renders blockquotes and horizontal rules', () => {
        expect(renderMarkdown('> note')).toContain('<blockquote>');
        expect(renderMarkdown('---')).toContain('<hr>');
    });
});

describe('extractHeadings', () => {
    it('de-duplicates repeated headings so ids stay unique', () => {
        const headings = extractHeadings('# Date\n# Date\n# Date', 2);
        expect(headings.map(h => h.id)).toEqual(['date', 'date-2', 'date-3']);
    });

    it('ignores headings deeper than the requested level', () => {
        expect(extractHeadings('# A\n### B', 2).map(h => h.text)).toEqual(['A']);
    });
});

describe('renderMarkdownWithAnchors', () => {
    it('stamps matching ids onto the rendered headings', () => {
        const html = renderMarkdownWithAnchors('# Election Regulations\n\nBody text.', 2);
        expect(html).toContain('<h1 id="election-regulations">');
    });
});

describe('slugify', () => {
    it('falls back to a usable id when nothing survives', () => {
        expect(slugify('!!!')).toBe('section');
    });
});
