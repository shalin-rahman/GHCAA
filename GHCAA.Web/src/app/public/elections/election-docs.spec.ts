import { describe, it, expect } from 'vitest';
import { splitForms, formNumber, formStage, ELECTION_DOCS } from './election-docs';

describe('ELECTION_DOCS', () => {
    it('has a unique id per document', () => {
        const ids = ELECTION_DOCS.map(d => d.id);
        expect(new Set(ids).size).toBe(ids.length);
    });

    it('has a unique file per document', () => {
        const files = ELECTION_DOCS.map(d => d.file);
        expect(new Set(files).size).toBe(files.length);
    });

    /* The forms handbook is split across two files (ER-01..24 and the lettered forms after
       ER-25); a document cannot be both a handbook of forms and a single form. */
    it('never marks a document as both a handbook and a single form', () => {
        expect(ELECTION_DOCS.filter(d => d.isFormsHandbook && d.isForm)).toEqual([]);
    });

    it('keeps every handbook and single form in the Forms group', () => {
        const forms = ELECTION_DOCS.filter(d => d.isFormsHandbook || d.isForm);
        expect(forms.length).toBeGreaterThan(0);
        expect(forms.every(d => d.group === 'Forms')).toBe(true);
    });
});

// FR-39
describe('formNumber', () => {
    it('sorts a lettered form just after the form it follows', () => {
        expect(formNumber('ER-17')).toBe(17);
        expect(formNumber('ER-17A')).toBeCloseTo(17.1);
        expect(formNumber('ER-27C')).toBeCloseTo(27.3);
        expect(formNumber('ER-27C')).toBeLessThan(formNumber('ER-28'));
    });

    it('returns 0 for a code with no number', () => {
        expect(formNumber('ER-')).toBe(0);
    });
});

// FR-39
describe('formStage', () => {
    it('groups a form by the stage of the election it is used in', () => {
        expect(formStage('ER-01')).toBe('Announcement');
        expect(formStage('ER-12')).toBe('Nomination');
        expect(formStage('ER-22A')).toBe('Campaign and polling preparation');
        expect(formStage('ER-25')).toBe('Polling day');
        expect(formStage('ER-25A')).toBe('Counting');
        expect(formStage('ER-27C')).toBe('Counting');
        expect(formStage('ER-28B')).toBe('Recount');
        expect(formStage('ER-30')).toBe('Result');
        expect(formStage('ER-35A')).toBe('Closure and handover');
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
