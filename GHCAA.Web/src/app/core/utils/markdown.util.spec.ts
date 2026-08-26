import { describe, it, expect } from 'vitest';
import {
    escapeHtml, renderMarkdown, renderMarkdownWithAnchors, extractHeadings, slugify,
    renderFormMarkdown
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

describe('renderFormMarkdown', () => {
    it('turns the two leading headings into the form masthead', () => {
        const html = renderFormMarkdown('# FORM ER-01\n## Nomination Paper\n');
        expect(html).toContain('<span class="f-code">FORM ER-01</span>');
        expect(html).toContain('<h3 class="f-name">Nomination Paper</h3>');
    });

    it('gives a `Label: ____` line a writing rule', () => {
        const html = renderFormMarkdown('Name of candidate: ______');
        expect(html).toContain('<span class="f-label">Name of candidate</span>');
        expect(html).toContain('<span class="f-rule"></span>');
    });

    it('groups consecutive tick-box lines into one list', () => {
        const html = renderFormMarkdown('[ ] Yes\n[ ] No');
        expect(html.match(/<ul class="check-list">/g)).toHaveLength(1);
        expect(html.match(/<span class="box"><\/span>/g)).toHaveLength(2);
    });

    it('renders inline tick boxes inside a field label', () => {
        expect(renderFormMarkdown('Verified: [ ] Yes [ ] No'))
            .toContain('Verified: <span class="box"></span> Yes <span class="box"></span> No');
    });

    it('closes an open grid when a signature panel starts', () => {
        const html = renderFormMarkdown(':: grid\nA:\n:: sign Returning Officer | Observer');
        expect(html).toContain('<div class="f-grid">');
        expect(html).toContain('<div class="sign-grid">');
        // The grid must be closed before the panel, or the panel renders inside a column.
        expect(html.indexOf('</div>')).toBeLessThan(html.indexOf('<div class="sign-grid">'));
    });

    it('puts the qualifier of a signature block on its own line', () => {
        const html = renderFormMarkdown(':: sign Presiding Officer / on polling day');
        expect(html).toContain('<span class="sign-who">Presiding Officer</span>');
        expect(html).toContain('<span class="sign-sub">on polling day</span>');
    });

    it('clamps the number of ruled lines to a sane range', () => {
        expect((renderFormMarkdown(':: lines Grounds | 99').match(/<i><\/i>/g) ?? [])).toHaveLength(12);
        expect((renderFormMarkdown(':: lines Grounds').match(/<i><\/i>/g) ?? [])).toHaveLength(3);
    });

    it('drops an unknown directive rather than printing it on an official form', () => {
        expect(renderFormMarkdown(':: sgin Officer')).not.toContain('sgin');
    });

    it('renders a pipe table as a ruled register', () => {
        const html = renderFormMarkdown('| Sl | Name |\n| --- | --- |\n| 1 | Rahim |');
        expect(html).toContain('<table class="form-table">');
        expect(html).toContain('<th>Sl</th>');
        expect(html).toContain('<td>Rahim</td>');
    });

    it('trims the separator rule left behind by the split from the handbook', () => {
        expect(renderFormMarkdown('Declaration text.\n\n---')).not.toContain('<hr>');
    });

    it('escapes form source, so no stored text can inject markup', () => {
        const html = renderFormMarkdown('Name: <img src=x onerror=alert(1)>\n');
        expect(html).not.toContain('<img');
        expect(html).toContain('&lt;img');
    });
});

describe('slugify', () => {
    it('falls back to a usable id when nothing survives', () => {
        expect(slugify('!!!')).toBe('section');
    });
});
