import { execFileSync } from 'node:child_process';
import os from 'node:os';
import path from 'node:path';

// The walkthrough leaves account takeover, code reading and clean-up to
// scripts/local-election.py, so the rules for what may be deleted live in one place.
const REPO_ROOT = path.resolve(__dirname, '../../../..');
const SCRIPT = path.join(REPO_ROOT, 'scripts', 'local-election.py');
const PYTHON = process.env['WALKTHROUGH_PYTHON'] || 'python';

// A fixed path, so the next run finds and removes what an interrupted run left behind.
export const STATE_FILE = path.join(os.tmpdir(), 'ghcaa-local-election', 'walkthrough-state.json');

export const PASSWORD = 'LocalVote#2026';

export interface Person {
  userId: number;
  username: string;
  memberId: number | null;
  name: string;
}

function run(args: string[], allowFail = false): string {
  try {
    return execFileSync(PYTHON, [SCRIPT, ...args], {
      cwd: REPO_ROOT,
      env: { ...process.env, GHCAA_LOCAL_STATE: STATE_FILE, PYTHONIOENCODING: 'utf-8' },
      encoding: 'utf-8',
      stdio: ['ignore', 'pipe', 'pipe'],
      timeout: 120_000,
    }).trim();
  } catch (err) {
    if (allowFail) return '';
    const e = err as { stdout?: string; stderr?: string };
    throw new Error(`local-election.py ${args.join(' ')} failed: ${(e.stdout || '') + (e.stderr || '')}`.trim());
  }
}

export const local = {
  purge: (leftovers = false) => run(leftovers ? ['purge', '--leftovers'] : ['purge']),
  restore: () => run(['restore']),
  takeOver: (userIds: number[]) => run(['accounts', userIds.join(',')]),
  record: (kind: 'election' | 'period', id: number) => run(['record', kind, String(id)]),
  /** Writes new polling dates straight to the election row. Demo mode only. */
  window: (electionId: number, times: Partial<Record<'opens' | 'closes' | 'recount-ends' | 'silence', Date>>) =>
    run(['window', String(electionId), ...Object.entries(times).flatMap(([k, v]) => [`--${k}`, v!.toISOString()])]),

  /** The newest emailed code for the user, sent after `since`, or null. */
  code(userId: number, since: Date): string | null {
    const out = run(['code', String(userId), '--since', since.toISOString()], true);
    return /^\d{6}$/.test(out) ? out : null;
  },

  /** Why the newest email to the user failed after `since`, such as Brevo refusing this IP. */
  mailError(userId: number, since: Date): string | null {
    return run(['mail-error', String(userId), '--since', since.toISOString()], true) || null;
  },

  who(userIds: number[]): Map<number, Person> {
    const rows = JSON.parse(run(['who', userIds.join(',')])) as Record<string, { username: string; member_id: number | null; name: string }>;
    return new Map(Object.entries(rows).map(([id, r]) => [Number(id), { userId: Number(id), username: r.username, memberId: r.member_id, name: r.name }]));
  },
};

/** Polls for a code sent after `since`, for up to `seconds`. */
export async function waitForCode(userId: number, since: Date, seconds = 45): Promise<string | null> {
  for (let i = 0; i < seconds; i++) {
    const code = local.code(userId, since);
    if (code) return code;
    await new Promise(r => setTimeout(r, 1000));
  }
  return null;
}

/** The error to stop on when no code came, with the email failure reason when there is one. */
export function noCodeError(person: Person, since: Date): Error {
  const why = local.mailError(person.userId, since);
  return new Error(`No step-up code arrived for ${person.name}.${why ? ` ${why}` : ''}`);
}
