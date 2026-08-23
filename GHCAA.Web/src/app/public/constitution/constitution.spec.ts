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

    it('returns nothing for empty content, so the page falls back to the PDF', () => {
        expect(parseArticles('')).toEqual([]);
        expect(CONSTITUTION_PDF_FALLBACK).toContain('.pdf');
    });
});
