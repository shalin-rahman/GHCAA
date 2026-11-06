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

function rawDateUses(): string[] {
    const found: string[] = [];
    for (const file of sourceFiles(appRoot)) {
        const lines = readFileSync(file, 'utf8').split('\n');
        lines.forEach((line, i) => {
            const where = `${relative(appRoot, file)}:${i + 1}`;
            for (const m of line.matchAll(/\|\s*date\b(?:\s*:\s*'([^']*)')?/g)) {
                if (!m[1] || !PARTIAL_FORMATS.has(m[1])) found.push(`${where} date:'${m[1] ?? ''}'`);
            }
            if (line.includes('toLocaleDateString(')) found.push(`${where} toLocaleDateString`);
        });
    }
    return found;
}

describe('date display guard', () => {
    it('shows full dates only through appDate or toDisplayDate', () => {
        expect(rawDateUses()).toEqual([]);
    });
});
