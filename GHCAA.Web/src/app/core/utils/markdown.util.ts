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
