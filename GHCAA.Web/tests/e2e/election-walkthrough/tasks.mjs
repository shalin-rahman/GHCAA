// Runs local-election.py against the walkthrough's own state file, for the npm scripts:
//   restore  puts the borrowed accounts back the way they were
//   clean    deletes what the walkthrough made, then restores the accounts
//   restore-workflows  puts back the accounts the membership and event walkthroughs borrowed
// The state path must match STATE_FILE in local.ts.
import { execFileSync } from 'node:child_process';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const here = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(here, '../../../..');
const script = path.join(repoRoot, 'scripts', 'local-election.py');
const stateDir = path.join(os.tmpdir(), 'ghcaa-local-election');
const state = path.join(stateDir, 'walkthrough-state.json');
const python = process.env.WALKTHROUGH_PYTHON || 'python';

const workflowStates = ['membership', 'event'].map(f => path.join(stateDir, `${f}-walkthrough-state.json`));
const runs = {
  restore: [[state, ['restore']]],
  clean: [[state, ['purge', '--leftovers']], [state, ['restore']]],
  'restore-workflows': workflowStates.map(s => [s, ['restore']]),
}[process.argv[2]];
if (!runs) {
  console.error('usage: node tasks.mjs restore|clean|restore-workflows');
  process.exit(2);
}
for (const [stateFile, args] of runs) {
  execFileSync(python, [script, ...args], {
    cwd: repoRoot,
    stdio: 'inherit',
    env: { ...process.env, GHCAA_LOCAL_STATE: stateFile, PYTHONIOENCODING: 'utf-8' },
  });
}
