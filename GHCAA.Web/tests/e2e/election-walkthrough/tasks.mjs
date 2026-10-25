// Runs local-election.py against the walkthrough's own state file, for the npm scripts:
//   restore  puts the borrowed accounts back the way they were
//   clean    deletes what the walkthrough made, then restores the accounts
// The state path must match STATE_FILE in local.ts.
import { execFileSync } from 'node:child_process';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const here = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(here, '../../../..');
const script = path.join(repoRoot, 'scripts', 'local-election.py');
const state = path.join(os.tmpdir(), 'ghcaa-local-election', 'walkthrough-state.json');
const python = process.env.WALKTHROUGH_PYTHON || 'python';

const steps = { restore: [['restore']], clean: [['purge', '--leftovers'], ['restore']] }[process.argv[2]];
if (!steps) {
  console.error('usage: node tasks.mjs restore|clean');
  process.exit(2);
}
for (const args of steps) {
  execFileSync(python, [script, ...args], {
    cwd: repoRoot,
    stdio: 'inherit',
    env: { ...process.env, GHCAA_LOCAL_STATE: state, PYTHONIOENCODING: 'utf-8' },
  });
}
