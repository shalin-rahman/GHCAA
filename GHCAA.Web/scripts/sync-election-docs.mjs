/**
 * Copies the election governance documents from `docs/Elections/` (the source of truth)
 * into `public/assets/elections/` so the public /elections page can fetch and download them.
 *
 * Runs as part of `npm run build` (TODO 36.4). Copying rather than moving keeps `docs/` the
 * canonical location; running it on every build is what stops the two from silently drifting.
 */
import { readdirSync, mkdirSync, copyFileSync, rmSync, existsSync } from 'node:fs';
import { join, dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const webRoot = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const source = resolve(webRoot, '..', 'docs', 'Elections');
const target = join(webRoot, 'public', 'assets', 'elections');

if (!existsSync(source)) {
    console.error(`[sync-election-docs] source not found: ${source}`);
    process.exit(1);
}

// Wipe first so a document renamed or deleted in docs/ does not linger as a public asset.
rmSync(target, { recursive: true, force: true });
mkdirSync(target, { recursive: true });

const files = readdirSync(source).filter(f => f.endsWith('.md')).sort();
for (const file of files) {
    copyFileSync(join(source, file), join(target, file));
}

console.log(`[sync-election-docs] copied ${files.length} document(s) to public/assets/elections/`);
