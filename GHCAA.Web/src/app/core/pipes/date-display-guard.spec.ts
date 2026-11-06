import { readdirSync, readFileSync } from 'node:fs';
import { join, relative, resolve } from 'node:path';
import { describe, expect, it } from 'vitest';

// Every full date shown to a user goes through appDate (dd-MM-yyyy or the org's setting).
// Angular's date pipe is still fine for a part of a date: a time, a calendar tile's day or
// month, a year, or a month and year. A copied 'fullDate' or 'medium' slipped past review
// once in news.html, so this test fails on any format outside the list below.
const PARTIAL_FORMATS = new Set([
    'shortTime', 'mediumTime', 'HH:mm', 'HH:mm:ss', 'h:mm a',
    'd', 'dd', 'MMM', 'MMMM', 'MMM d', 'yyyy', 'MMM yyyy', 'MMMM yyyy'
]);

// Vitest runs from GHCAA.Web. import.meta.url is not a file URL under jsdom.
const appRoot = resolve(process.cwd(), 'src/app');

function sourceFiles(dir: string): string[] {
    return readdirSync(dir, { withFileTypes: true }).flatMap(entry => {
        const path = join(dir, entry.name);
        if (entry.isDirectory()) return sourceFiles(path);
        return /\.(html|ts)$/.test(entry.name) && !entry.name.endsWith('.spec.ts') ? [path] : [];
    });
}

// The whole file is scanned, not line by line, so a pipe wrapped onto the next line is still caught.
// A DatePipe built or injected in code would format outside the template, so that is caught too.
const RAW_CALLS = /toLocaleDateString\(|Intl\.DateTimeFormat\(|new DatePipe\(|inject\(\s*DatePipe\s*\)|\w+\s*:\s*DatePipe\b/g;

function rawDateUses(): string[] {
    const found: string[] = [];
    for (const file of sourceFiles(appRoot)) {
        const text = readFileSync(file, 'utf8');
        const where = (index: number) => `${relative(appRoot, file)}:${text.slice(0, index).split('\n').length}`;
        for (const m of text.matchAll(/\|\s*date\b(?:\s*:\s*'([^']*)')?/g)) {
            if (!m[1] || !PARTIAL_FORMATS.has(m[1])) found.push(`${where(m.index!)} date:'${m[1] ?? ''}'`);
        }
        for (const m of text.matchAll(RAW_CALLS)) found.push(`${where(m.index!)} ${m[0]}`);
    }
    return found;
}

describe('date display guard', () => {
    it('shows full dates only through appDate or toDisplayDate', () => {
        expect(rawDateUses()).toEqual([]);
    });
});
