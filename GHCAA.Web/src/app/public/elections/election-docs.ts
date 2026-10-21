/**
 * The election document set, mirrored from `docs/Elections/` into
 * `public/assets/elections/` by `scripts/sync-election-docs.mjs` at build time.
 *
 * The manifest is hand-maintained rather than generated because the display title,
 * blurb and grouping are editorial, not derivable from the filename. Adding a document
 * therefore means two edits: drop the .md in docs/Elections/ and add an entry here.
 */
export interface ElectionDoc {
    /** Slug used in the URL fragment and as the `track` key. */
    id: string;
    /** File name inside `assets/elections/`. */
    file: string;
    title: string;
    blurb: string;
    group: 'Rules' | 'Procedure' | 'Forms';
    /** Split into individual FORM sections when opened — see `splitForms`. */
    isFormsHandbook?: boolean;
    /** A single form: renders on the letterhead pad rather than as prose. */
    isForm?: boolean;
}

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

export const ELECTION_DOCS: ElectionDoc[] = [
    {
        id: 'regulations',
        file: '01-Election-Regulations.md',
        title: 'Election Regulations',
        blurb: 'The governing rules for every association election: eligibility, the Election Commission, nomination, campaigning, polling and dispute resolution.',
        group: 'Rules'
    },
    {
        id: 'code-of-conduct',
        file: '03-Election-Code-Of-Conduct.md',
        title: 'Election Code of Conduct',
        blurb: 'What candidates, proposers, voters and officials may and may not do during an election period, and the penalties for breaching it.',
        group: 'Rules'
    },
    {
        id: 'operations-manual',
        file: '02-Election-Operational-Manual.md',
        title: 'Election Operations Manual',
        blurb: 'The Election Commission’s working manual — the full timeline from announcement to declaration of results, step by step.',
        group: 'Procedure'
    },
    {
        id: 'polling-manual',
        file: '04-Election-Manual-Ballot.md',
        title: 'Polling & Counting Manual',
        blurb: 'How a manual ballot is run on polling day: booth setup, ballot issue, sealing, counting, recounts and the result sheet.',
        group: 'Procedure'
    },
    {
        id: 'forms',
        file: '05-Election-Forms-and-Templates.md',
        title: 'Forms Handbook — Part I (ER-01 to ER-24)',
        blurb: 'Announcement, Commission appointment, voter roll, nomination and polling-day forms. Each one opens as a ready-to-use sheet on the Association letterhead and prints on A4.',
        group: 'Forms',
        isFormsHandbook: true
    },
    {
        id: 'er-25',
        file: '06-Election-Ballot-Seal-and-Poll-Integrity-Certificate.md',
        title: 'Form ER-25 — Ballot Box Sealing & Poll Integrity Certificate',
        blurb: 'Signed at the close of poll to certify that the ballot box was sealed in the presence of the agents present.',
        group: 'Forms',
        isForm: true
    },
    {
        id: 'forms-2',
        file: '07-Election-Vote-Counting-Authorisation.md',
        title: 'Forms Handbook — Part II (ER-22A, ER-25A to ER-35A)',
        blurb: 'Counting authorisation, tally, recount, result certification, complaints, appeals, handover and closure forms, plus the signature matrix and retention schedule.',
        group: 'Forms',
        isFormsHandbook: true
    },
    {
        id: 'forms-appendices',
        file: '08-Election-Forms-Appendices.md',
        title: 'Forms Appendices — Signatures, Custody & Retention',
        blurb: 'Which officers must sign which form, the chain of custody for election material, and how long each record is kept.',
        group: 'Forms'
    }
];

export const ELECTION_DOC_GROUPS: Array<ElectionDoc['group']> = ['Rules', 'Procedure', 'Forms'];

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
