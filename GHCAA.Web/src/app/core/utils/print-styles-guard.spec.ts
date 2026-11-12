import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { describe, expect, it } from 'vitest';

// jsdom cannot apply print CSS, so these tests read the stylesheet text. Vitest runs from GHCAA.Web.
const styles = readFileSync(resolve(process.cwd(), 'src/styles.scss'), 'utf8');

function escapeRegex(text: string): string {
    return text.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

function visibilityTargets(css: string): string[] {
    return [...css.matchAll(/body:has\(\.([\w-]+)\)\s*\*\s*\{[^}]*visibility:\s*hidden/g)].map(m => m[1]);
}

function showsWithPrefix(css: string, cls: string): boolean {
    const prefix = escapeRegex(`body:has(.${cls}) .${cls}`);
    return new RegExp(`${prefix},\\s*${prefix} \\*\\s*\\{[^}]*visibility:\\s*visible !important`).test(css);
}

function ruleBody(css: string, selector: string): string {
    const start = css.indexOf(selector);
    if (start < 0) throw new Error(`selector not found in styles.scss: ${selector}`);
    return css.slice(start, css.indexOf('}', start));
}

function insidePrintMedia(css: string, selector: string): boolean {
    const at = css.indexOf(selector);
    const media = css.lastIndexOf('@media', at);
    return at >= 0 && media >= 0 && css.slice(media, at).startsWith('@media print');
}

const hideRule = 'body:has(.vote-receipt) *:not(:has(.vote-receipt), .vote-receipt, .vote-receipt *)';
const ancestorRule = 'body:has(.vote-receipt) :has(.vote-receipt)';
const inkRule = 'body:has(.vote-receipt) .vote-receipt,';

describe('print-only styles', () => {
    // FR-37.1m
    it('a visibility show rule carries the same body:has() prefix as its hide rule', () => {
        const targets = visibilityTargets(styles);
        expect(targets.length).toBeGreaterThan(0);
        expect(targets.filter(cls => !showsWithPrefix(styles, cls))).toEqual([]);
    });

    // FR-37.1m
    it('flags a show rule without the prefix', () => {
        const bad = 'body:has(.x) * { visibility: hidden !important; } .x, .x * { visibility: visible !important; }';
        expect(visibilityTargets(bad)).toEqual(['x']);
        expect(showsWithPrefix(bad, 'x')).toBe(false);
    });

    // FR-37.1m
    it('the vote receipt rules sit inside @media print', () => {
        for (const selector of [hideRule, ancestorRule, inkRule]) expect(insidePrintMedia(styles, selector)).toBe(true);
    });

    // FR-37.1m
    it('removes everything outside the receipt and its ancestors', () => {
        expect(ruleBody(styles, hideRule)).toContain('display: none !important');
    });

    // FR-37.1m
    it('the receipt ancestors print in normal flow, so a long receipt is not clipped', () => {
        const body = ruleBody(styles, ancestorRule);
        expect(body).toContain('position: static !important');
        expect(body).toContain('overflow: visible !important');
        expect(body).toContain('height: auto !important');
    });

    // FR-37.1m
    it('the printed receipt uses paper ink and stops the fade', () => {
        const body = ruleBody(styles, inkRule);
        expect(body).toContain('color: var(--paper-ink) !important');
        expect(body).toContain('transition: none !important');
    });

    // FR-37.1m
    it('the printed receipt has a paper background and no glass effect', () => {
        const body = ruleBody(styles, 'background: var(--paper-bg) !important');
        expect(body).toContain('backdrop-filter: none !important');
        expect(body).toContain('border-color: var(--paper-hairline) !important');
    });
});
