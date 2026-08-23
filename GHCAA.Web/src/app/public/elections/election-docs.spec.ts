import { describe, it, expect } from 'vitest';
import { splitForms, ELECTION_DOCS } from './election-docs';

describe('ELECTION_DOCS', () => {
    it('has a unique id per document', () => {
        const ids = ELECTION_DOCS.map(d => d.id);
        expect(new Set(ids).size).toBe(ids.length);
    });

    it('marks exactly one document as the forms handbook', () => {
        expect(ELECTION_DOCS.filter(d => d.isFormsHandbook)).toHaveLength(1);
    });
});

describe('splitForms', () => {
    const handbook = [
        '# HARAGANGIAN',
        '# Forms & Templates Handbook',
        '',
        '# FORM ER-01',
        '## Nomination Paper',
        'Name: ______',
        '',
        '# FORM ER-02',
        '## Withdrawal of Candidature',
        'Reason:'
    ].join('\n');

    it('extracts one entry per FORM heading', () => {
        expect(splitForms(handbook).map(f => f.code)).toEqual(['ER-01', 'ER-02']);
    });

    it('names each form from the heading that follows its code', () => {
        expect(splitForms(handbook)[0].title).toBe('Nomination Paper');
    });

    it('keeps the form heading in the extracted source so it prints complete', () => {
        expect(splitForms(handbook)[0].source).toContain('# FORM ER-01');
    });

    it('discards the cover page before the first form', () => {
        expect(splitForms(handbook)[0].source).not.toContain('HARAGANGIAN');
    });

    it('returns nothing when there are no form headings', () => {
        expect(splitForms('# Just a document\n\nText.')).toEqual([]);
    });
});
