// 37.14s. The pages themselves now come from api/election-content. These helpers turn a
// FormsHandbook page body into a register of single forms.

/**
 * Sort key for a form code. Codes follow the Election Regulations, and a form the
 * Regulations do not list carries a letter after the form it follows, so ER-17A sorts
 * as 17.1 and ER-27C as 27.3. Returns 0 when the code has no number.
 */
export function formNumber(code: string): number {
    const m = /(\d+)([A-Z])?\s*$/i.exec(code);
    if (!m) return 0;
    const letter = m[2] ? m[2].toUpperCase().charCodeAt(0) - 64 : 0;
    return parseInt(m[1], 10) + letter / 10;
}

/**
 * Which stage of an election a form belongs to, derived from its ER number. The register
 * is grouped by this so an officer looks under "Polling day" rather than scanning fifty
 * near-identical cards. The ranges follow the Regulations numbering and live only here.
 */
export function formStage(code: string): string {
    const n = formNumber(code);
    if (!n) return 'Other';
    if (n <= 2.9) return 'Announcement';
    if (n <= 7.9) return 'Commission appointment';
    if (n <= 9.9) return 'Voter roll';
    if (n <= 19.9) return 'Nomination';
    if (n <= 22.9) return 'Campaign and polling preparation';
    if (n <= 25) return 'Polling day';
    if (n <= 27.9) return 'Counting';
    if (n <= 28.9) return 'Recount';
    if (n <= 30.9) return 'Result';
    if (n <= 32.9) return 'Complaints and appeals';
    return 'Closure and handover';
}

/** One `# FORM ER-nn` section of the handbook, extractable and printable on its own. */
export interface ElectionForm {
    id: string;
    /** e.g. `ER-04`. */
    code: string;
    title: string;
    /** The raw markdown of this section, including its own heading. */
    source: string;
}

/**
 * Splits the handbook on its `# FORM ER-nn` headings so a returning officer can open,
 * print or download the single form they need rather than the whole 500-line document.
 * Text before the first FORM heading (cover page, index) is discarded — it is chrome,
 * not a form.
 */
export function splitForms(markdown: string): ElectionForm[] {
    const lines = (markdown ?? '').replace(/\r\n/g, '\n').split('\n');
    const forms: ElectionForm[] = [];
    let current: { code: string; title: string; buffer: string[] } | null = null;

    const flush = () => {
        if (!current) return;
        forms.push({
            id: current.code.toLowerCase().replace(/[^a-z0-9]+/g, '-'),
            code: current.code,
            // The line after the FORM heading is the form's name; fall back to the code.
            title: current.title || current.code,
            source: current.buffer.join('\n').trim()
        });
        current = null;
    };

    for (const line of lines) {
        const m = /^#\s+FORM\s+([A-Z0-9-]+)\s*$/i.exec(line.trim());
        if (m) {
            flush();
            current = { code: m[1].toUpperCase(), title: '', buffer: [line] };
            continue;
        }
        if (!current) continue;
        // First heading after the FORM code names the form.
        const h = /^#{1,3}\s+(.*)$/.exec(line.trim());
        if (h && !current.title) current.title = h[1].trim();
        current.buffer.push(line);
    }
    flush();
    return forms;
}
