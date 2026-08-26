import { describe, it, expect } from 'vitest';
import { parseArticles, CONSTITUTION_PDF_FALLBACK } from './constitution';

describe('parseArticles', () => {
    const sample = [
        'Adopted by the general body.',
        '',
        'Article I: Name and Office',
        'The association shall be known as GHCAA.',
        '',
        'Article II: Objectives',
        'To advance the interests of alumni.'
    ].join('\n');

    it('splits on Article headings and keeps the preamble', () => {
        const articles = parseArticles(sample);
        expect(articles.map(a => a.heading)).toEqual([
            'Preamble', 'Article I: Name and Office', 'Article II: Objectives'
        ]);
    });

    it('gives every article a unique anchor id', () => {
        const ids = parseArticles(sample).map(a => a.id);
        expect(new Set(ids).size).toBe(ids.length);
    });

    it('drops an empty preamble but keeps an empty article as an incompleteness signal', () => {
        const articles = parseArticles('Article I: Name\n');
        expect(articles).toHaveLength(1);
        expect(articles[0].bodyHtml).toBe('');
    });

    it('escapes article bodies', () => {
        const articles = parseArticles('Article I: Name\n<script>alert(1)</script>');
        expect(articles[0].bodyHtml).not.toContain('<script>');
        expect(articles[0].bodyHtml).toContain('&lt;script&gt;');
    });

    /* A `Section N:` label opening a paragraph is a subheading in the stored text; without
       this it rendered as body text and an article read as one undifferentiated wall. */
    it('promotes a short `Section N: Title` paragraph to a heading', () => {
        const html = parseArticles('Article I: Name\n\nSection 1: Official Name')[0].bodyHtml;
        expect(html).toBe('<h4>Section 1: Official Name</h4>');
    });

    it('accepts a lettered or roman-numbered section label', () => {
        expect(parseArticles('Article I: Name\n\nSection A: Membership')[0].bodyHtml)
            .toContain('<h4>Section A: Membership</h4>');
        expect(parseArticles('Article I: Name\n\nSection IV: Dues')[0].bodyHtml)
            .toContain('<h4>Section IV: Dues</h4>');
    });

    it('emphasises only the label when the body runs on from it', () => {
        const runOn = 'Section 2: The association shall maintain a register of all its members, '
            + 'updated at the close of every financial year by the general secretary.';
        const html = parseArticles(`Article I: Name\n\n${runOn}`)[0].bodyHtml;
        expect(html).toContain('<p><strong>Section 2:</strong>');
        expect(html).not.toContain('<h4>');
        expect(html).toContain('register of all its members');
    });

    it('escapes a section label and its title', () => {
        const html = parseArticles('Article I: Name\n\nSection 1: <b>Name</b>')[0].bodyHtml;
        expect(html).not.toContain('<b>');
        expect(html).toContain('&lt;b&gt;');
    });

    it('leaves an ordinary paragraph as a paragraph', () => {
        expect(parseArticles('Article I: Name\n\nThe association is a body of alumni.')[0].bodyHtml)
            .toBe('<p>The association is a body of alumni.</p>');
    });

    it('returns nothing for empty content, so the page falls back to the PDF', () => {
        expect(parseArticles('')).toEqual([]);
        expect(CONSTITUTION_PDF_FALLBACK).toContain('.pdf');
    });
});
