/**
 * Minimal markdown -> HTML renderer for the repo-authored governance documents
 * (`docs/Elections/*.md`, served from `public/assets/elections/`).
 *
 * Deliberately NOT a general markdown implementation and deliberately not `marked`:
 * a feature survey of all seven election documents (TODO 36.5) found the syntax in use
 * is a small fixed subset, and the app avoids dependencies that a ~150-line utility covers.
 *
 * Supported: `#`-`####` headings, `-`/`*` unordered lists, `1.` ordered lists,
 * GFM pipe tables, `---` horizontal rules, `**bold**`, `*italic*`, `` `code` ``,
 * `> ` blockquotes, and blank-line-separated paragraphs.
 *
 * SECURITY: every source character is HTML-escaped before any markup is emitted, and raw
 * HTML in the source is never passed through. The output is therefore safe to bind with
 * `[innerHTML]` even if a document ever comes from somewhere other than the repo.
 */

/** Escape the five characters that can break out of text or an attribute context. */
export function escapeHtml(input: string): string {
    return input
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

/**
 * Inline spans. Runs AFTER escapeHtml, so the patterns below can only ever match
 * literal source characters - never markup we emitted.
 */
function renderInline(text: string): string {
    return escapeHtml(text)
        .replace(/`([^`]+)`/g, '<code>$1</code>')
        .replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>')
        .replace(/\*([^*]+)\*/g, '<em>$1</em>')
        // Long runs of underscores are fill-in-the-blank lines in the forms handbook,
        // not emphasis - render them as a visible blank rather than dropping them.
        .replace(/_{3,}/g, '<span class="md-blank"></span>');
}

/** A `| a | b |` row split into its cells, with the leading/trailing pipes dropped. */
function splitRow(line: string): string[] {
    return line.trim().replace(/^\||\|$/g, '').split('|').map(c => c.trim());
}

const TABLE_DIVIDER = /^\|?[\s:|-]+\|[\s:|-]*$/;

export function renderMarkdown(source: string): string {
    const lines = source.replace(/\r\n/g, '\n').split('\n');
    const out: string[] = [];
    let i = 0;

    while (i < lines.length) {
        const line = lines[i];
        const trimmed = line.trim();

        if (!trimmed) { i++; continue; }

        // Horizontal rule. Checked before headings so `---` never reads as setext.
        if (/^(-{3,}|\*{3,}|_{3,})$/.test(trimmed)) {
            out.push('<hr>');
            i++;
            continue;
        }

        // Heading
        const heading = /^(#{1,6})\s+(.*)$/.exec(trimmed);
        if (heading) {
            const level = Math.min(heading[1].length, 6);
            out.push(`<h${level}>${renderInline(heading[2])}</h${level}>`);
            i++;
            continue;
        }

        // Table: a pipe row immediately followed by a divider row.
        if (trimmed.startsWith('|') && i + 1 < lines.length && TABLE_DIVIDER.test(lines[i + 1].trim())) {
            const header = splitRow(trimmed);
            i += 2;
            const body: string[][] = [];
            while (i < lines.length && lines[i].trim().startsWith('|')) {
                body.push(splitRow(lines[i]));
                i++;
            }
            const head = header.map(c => `<th>${renderInline(c)}</th>`).join('');
            const rows = body
                .map(r => `<tr>${r.map(c => `<td>${renderInline(c)}</td>`).join('')}</tr>`)
                .join('');
            out.push(`<div class="table-wrap"><table class="data-table"><thead><tr>${head}</tr></thead><tbody>${rows}</tbody></table></div>`);
            continue;
        }

        // Blockquote
        if (trimmed.startsWith('> ')) {
            const quoted: string[] = [];
            while (i < lines.length && lines[i].trim().startsWith('>')) {
                quoted.push(lines[i].trim().replace(/^>\s?/, ''));
                i++;
            }
            out.push(`<blockquote>${renderInline(quoted.join(' '))}</blockquote>`);
            continue;
        }

        // Lists. Ordered and unordered share the shape; the tag is the only difference.
        const ordered = /^\d+\.\s+/.test(trimmed);
        const unordered = /^[-*]\s+/.test(trimmed);
        if (ordered || unordered) {
            const tag = ordered ? 'ol' : 'ul';
            const marker = ordered ? /^\d+\.\s+/ : /^[-*]\s+/;
            const items: string[] = [];
            while (i < lines.length) {
                const t = lines[i].trim();
                if (marker.test(t)) {
                    items.push(renderInline(t.replace(marker, '')));
                    i++;
                } else if (t && !/^(#{1,6}\s|>|\|)/.test(t) && items.length) {
                    // Continuation line: part of the previous item, not a new paragraph.
                    items[items.length - 1] += ' ' + renderInline(t);
                    i++;
                } else {
                    break;
                }
            }
            out.push(`<${tag}>${items.map(it => `<li>${it}</li>`).join('')}</${tag}>`);
            continue;
        }

        // Paragraph: consume until a blank line or the start of another block.
        const para: string[] = [];
        while (i < lines.length) {
            const t = lines[i].trim();
            if (!t || /^(#{1,6}\s|>|\||[-*]\s|\d+\.\s)/.test(t) || /^(-{3,}|\*{3,}|_{3,})$/.test(t)) break;
            para.push(t);
            i++;
        }
        if (para.length) out.push(`<p>${renderInline(para.join(' '))}</p>`);
    }

    return out.join('\n');
}

/**
 * Headings suitable for an in-page table of contents, each with a slug id that
 * `renderMarkdownWithAnchors` stamps onto the matching heading element.
 */
export interface MarkdownHeading {
    id: string;
    level: number;
    text: string;
}

export function slugify(text: string): string {
    return text
        .toLowerCase()
        .replace(/[^a-z0-9\s-]/g, '')
        .trim()
        .replace(/\s+/g, '-')
        .slice(0, 60) || 'section';
}

export function extractHeadings(source: string, maxLevel = 2): MarkdownHeading[] {
    const seen = new Map<string, number>();
    const headings: MarkdownHeading[] = [];
    for (const line of source.replace(/\r\n/g, '\n').split('\n')) {
        const m = /^(#{1,6})\s+(.*)$/.exec(line.trim());
        if (!m || m[1].length > maxLevel) continue;
        const text = m[2].trim();
        const base = slugify(text);
        // Duplicate headings are common in the forms handbook ("Date", "Signature").
        const n = (seen.get(base) ?? 0) + 1;
        seen.set(base, n);
        headings.push({ id: n === 1 ? base : `${base}-${n}`, level: m[1].length, text });
    }
    return headings;
}

/** Same output as `renderMarkdown`, with `id` attributes on headings for ToC links. */
export function renderMarkdownWithAnchors(source: string, maxLevel = 2): string {
    const ids = extractHeadings(source, maxLevel).map(h => h.id);
    let n = 0;
    return renderMarkdown(source).replace(/<h([1-6])>/g, (full, level) => {
        if (Number(level) > maxLevel || n >= ids.length) return full;
        return `<h${level} id="${ids[n++]}">`;
    });
}

/* ==========================================================================
   FORM RENDERER
   --------------------------------------------------------------------------
   An election form is not prose: it is a printed A4 sheet with ruled fields,
   tick boxes, a signature panel and a seal. Rendering it through the prose
   renderer above produced a wall of one-word paragraphs ("Name", "Signature",
   "Date") with nowhere to write — which is exactly what it looked like.

   `renderFormMarkdown` therefore reads a small, explicit field vocabulary that
   the handbook source is authored in. Nothing is guessed from the shape of a
   sentence, so a form can never half-render:

     ## Candidate Information     a section band
     Name: ____                   a ruled field (label + writing rule)
     [ ] I am not contesting      a tick-box item (consecutive lines group)
     Verified: [ ] Yes [ ] No     tick boxes inline in a field label
     :: grid / :: grid3 / :: end  2- or 3-column field block
     :: lines Ground of objection | 3      label plus 3 ruled lines
     :: sign Presiding Officer | Observer  the signature panel
     :: seal                      the seal circle
     | a | b |                    a ruled register table (.form-table)

   Anything else falls through to the prose rules (paragraphs, lists, `---`), so
   the declarations and undertakings that forms carry still read as sentences.

   SECURITY: as above — every character is escaped before any markup is emitted.
   ========================================================================== */

/** A field label may carry inline tick boxes: `Membership Verified: [ ] Yes [ ] No`. */
function renderFormInline(text: string): string {
    // Runs after renderInline (and therefore after escapeHtml), so `[ ]` here can
    // only ever be literal source characters.
    return renderInline(text).replace(/\[\s?\]/g, '<span class="box"></span>');
}

/** `Label: ____` / `Label:` — the trailing underscores are the writing rule. */
const FIELD_LINE = /^(.+?):[ \t]*_*[ \t]*$/;

/** `:: name rest-of-line` */
const DIRECTIVE = /^::[ \t]*([a-z0-9]+)[ \t]*(.*)$/i;

function fieldRow(label: string): string {
    return `<div class="f-row"><span class="f-label">${renderFormInline(label)}</span><span class="f-rule"></span></div>`;
}

/** `:: sign Chief Election Commissioner | Observer / candidate agent` */
function signPanel(spec: string): string {
    const blocks = spec.split('|').map(s => s.trim()).filter(Boolean).map(entry => {
        // `Who / qualifier` puts the qualifier on a second, lighter line.
        const [who, ...rest] = entry.split('/');
        const sub = rest.join('/').trim();
        return '<div class="sign-block">'
            + '<span class="sign-space"></span>'
            + `<span class="sign-who">${renderInline(who.trim())}</span>`
            + (sub ? `<span class="sign-sub">${renderInline(sub)}</span>` : '')
            + '</div>';
    });
    return `<div class="sign-grid">${blocks.join('')}</div>`;
}

/** `:: lines Label | 3` — a label with N ruled lines under it (default 3). */
function ruledLines(spec: string): string {
    const [labelPart, countPart] = spec.split('|');
    const count = Math.min(Math.max(parseInt((countPart ?? '').trim(), 10) || 3, 1), 12);
    const label = (labelPart ?? '').trim();
    return '<div class="f-lines">'
        + (label ? `<span class="f-label">${renderFormInline(label)}</span>` : '')
        + '<i></i>'.repeat(count)
        + '</div>';
}

/**
 * Renders one form section (the output of `splitForms`) as a printable sheet body.
 * The two leading `# FORM ER-nn` / `# Name` headings become the form's masthead.
 */
export function renderFormMarkdown(source: string): string {
    const lines = (source ?? '').replace(/\r\n/g, '\n').split('\n');
    const out: string[] = [];
    /** Open `:: grid` wrapper, so `:: end` knows whether there is one to close. */
    let gridOpen = false;
    let i = 0;

    // Masthead: `# FORM ER-01` followed by the form's name.
    const codeMatch = /^#\s+FORM\s+([A-Za-z0-9-]+)\s*$/.exec((lines[0] ?? '').trim());
    if (codeMatch) {
        i = 1;
        let name = '';
        while (i < lines.length) {
            const t = lines[i].trim();
            if (!t) { i++; continue; }
            const h = /^#{1,3}\s+(.*)$/.exec(t);
            if (h) { name = h[1].trim(); i++; }
            break;
        }
        out.push('<header class="f-head">'
            + `<span class="f-code">FORM ${escapeHtml(codeMatch[1].toUpperCase())}</span>`
            + (name ? `<h3 class="f-name">${renderInline(name)}</h3>` : '')
            + '</header>');
    }

    while (i < lines.length) {
        const trimmed = lines[i].trim();

        if (!trimmed) { i++; continue; }

        // Directives.
        const directive = DIRECTIVE.exec(trimmed);
        if (directive) {
            const [, rawName, rest] = directive;
            const name = rawName.toLowerCase();
            i++;
            if (name === 'grid' || name === 'grid3') {
                if (gridOpen) out.push('</div>');
                out.push(`<div class="f-grid${name === 'grid3' ? ' f-grid-3' : ''}">`);
                gridOpen = true;
            } else if (name === 'end') {
                if (gridOpen) { out.push('</div>'); gridOpen = false; }
            } else if (name === 'sign') {
                if (gridOpen) { out.push('</div>'); gridOpen = false; }
                out.push(signPanel(rest));
            } else if (name === 'lines') {
                out.push(ruledLines(rest));
            } else if (name === 'seal') {
                out.push(`<div class="seal-box">${escapeHtml(rest.trim() || 'Official Seal')}</div>`);
            }
            // An unknown directive is dropped rather than printed as text: a typo must
            // not leak `:: sgin` onto an official form.
            continue;
        }

        // Section band. `###` is treated the same as `##` — a form has one level of
        // grouping, and a second visual weight only makes the sheet noisier.
        const heading = /^(#{2,6})\s+(.*)$/.exec(trimmed);
        if (heading) {
            if (gridOpen) { out.push('</div>'); gridOpen = false; }
            out.push(`<h4 class="f-section">${renderInline(heading[2])}</h4>`);
            i++;
            continue;
        }

        // Tick-box run.
        if (/^\[\s?\]\s+/.test(trimmed)) {
            const items: string[] = [];
            while (i < lines.length && /^\[\s?\]\s+/.test(lines[i].trim())) {
                items.push(`<li><span class="box"></span><span>${renderInline(lines[i].trim().replace(/^\[\s?\]\s+/, ''))}</span></li>`);
                i++;
            }
            out.push(`<ul class="check-list">${items.join('')}</ul>`);
            continue;
        }

        // Ruled table (all-sides borders, writable row height).
        if (trimmed.startsWith('|') && i + 1 < lines.length && TABLE_DIVIDER.test(lines[i + 1].trim())) {
            if (gridOpen) { out.push('</div>'); gridOpen = false; }
            const header = splitRow(trimmed);
            i += 2;
            const body: string[][] = [];
            while (i < lines.length && lines[i].trim().startsWith('|')) {
                body.push(splitRow(lines[i]));
                i++;
            }
            const head = header.map(c => `<th>${renderFormInline(c)}</th>`).join('');
            const rows = body
                .map(r => `<tr>${r.map(c => `<td>${renderFormInline(c)}</td>`).join('')}</tr>`)
                .join('');
            out.push(`<table class="form-table"><thead><tr>${head}</tr></thead><tbody>${rows}</tbody></table>`);
            continue;
        }

        // Ruled field.
        const field = FIELD_LINE.exec(trimmed);
        if (field) {
            out.push(fieldRow(field[1].trim()));
            i++;
            continue;
        }

        // Horizontal rule.
        if (/^(-{3,}|\*{3,})$/.test(trimmed)) {
            if (gridOpen) { out.push('</div>'); gridOpen = false; }
            out.push('<hr>');
            i++;
            continue;
        }

        // Bulleted / numbered list.
        const ordered = /^\d+\.\s+/.test(trimmed);
        if (ordered || /^[-*]\s+/.test(trimmed)) {
            const tag = ordered ? 'ol' : 'ul';
            const marker = ordered ? /^\d+\.\s+/ : /^[-*]\s+/;
            const items: string[] = [];
            while (i < lines.length && marker.test(lines[i].trim())) {
                items.push(`<li>${renderFormInline(lines[i].trim().replace(marker, ''))}</li>`);
                i++;
            }
            out.push(`<${tag}>${items.join('')}</${tag}>`);
            continue;
        }

        // Paragraph — a declaration or an instruction on the form.
        const para: string[] = [];
        while (i < lines.length) {
            const t = lines[i].trim();
            if (!t || /^(#{1,6}\s|::|\||[-*]\s|\d+\.\s|\[\s?\]\s)/.test(t) || FIELD_LINE.test(t) || /^(-{3,}|\*{3,})$/.test(t)) break;
            para.push(t);
            i++;
        }
        if (para.length) out.push(`<p>${renderFormInline(para.join(' '))}</p>`);
    }

    if (gridOpen) out.push('</div>');
    // `splitForms` cuts the handbook at the next `# FORM` heading, so the `---` that
    // separates two forms in the source lands at the foot of the preceding one. A rule
    // hanging under the last signature panel is not part of the form.
    while (out.length && out[out.length - 1] === '<hr>') out.pop();
    return out.join('\n');
}
