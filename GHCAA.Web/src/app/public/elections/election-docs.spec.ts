import { describe, it, expect } from 'vitest';
import { splitForms, formStage, ELECTION_DOCS } from './election-docs';

describe('ELECTION_DOCS', () => {
    it('has a unique id per document', () => {
        const ids = ELECTION_DOCS.map(d => d.id);
        expect(new Set(ids).size).toBe(ids.length);
    });

    it('has a unique file per document', () => {
        const files = ELECTION_DOCS.map(d => d.file);
        expect(new Set(files).size).toBe(files.length);
    });

    /* The forms handbook is split across two files (ER-01..18 and ER-20..40); a document
       cannot be both a handbook of forms and a single form. */
    it('never marks a document as both a handbook and a single form', () => {
        expect(ELECTION_DOCS.filter(d => d.isFormsHandbook && d.isForm)).toEqual([]);
    });

    it('keeps every handbook and single form in the Forms group', () => {
        const forms = ELECTION_DOCS.filter(d => d.isFormsHandbook || d.isForm);
        expect(forms.length).toBeGreaterThan(0);
        expect(forms.every(d => d.group === 'Forms')).toBe(true);
    });
});

describe('formStage', () => {
    it('groups a form by the stage of the election it is used in', () => {
        expect(formStage('ER-01')).toBe('Announcement');
        expect(formStage('ER-12')).toBe('Nomination');
        expect(formStage('ER-19')).toBe('Polling day');
        expect(formStage('ER-30')).toBe('Result');
        expect(formStage('ER-40')).toBe('Closure and handover');
    });

    it('puts a code with no number in its own bucket rather than guessing', () => {
        expect(formStage('ER-')).toBe('Other');
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
