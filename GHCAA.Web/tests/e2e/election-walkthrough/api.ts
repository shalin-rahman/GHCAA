import { PASSWORD, Person, noCodeError, waitForCode } from './local';

// Steps with no web page (nominations, middle phases, declare) and the refusal probes go
// straight to the API. Each person gets one stepped-up token, reused for 25 minutes.
export const API = process.env['GHCAA_LOCAL_API'] || 'http://localhost:5087/api';

export interface Reply<T = any> {
  status: number;
  body: T;
}

export class ApiError extends Error {
  constructor(public readonly method: string, public readonly path: string, public readonly reply: Reply) {
    super(`${method} ${path} -> ${reply.status} ${JSON.stringify(reply.body).slice(0, 400)}`);
  }
}

/** Reads a field whether the API sent it PascalCase or camelCase. */
export function field<T = any>(obj: any, name: string): T {
  if (obj == null) return undefined as T;
  return (name in obj ? obj[name] : obj[name[0].toLowerCase() + name.slice(1)]) as T;
}

export async function send<T = any>(method: string, path: string, token?: string, body?: unknown): Promise<Reply<T>> {
  const res = await fetch(`${API}${path}`, {
    method,
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(body !== undefined ? { 'Content-Type': 'application/json' } : {}),
    },
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });
  const text = await res.text();
  let parsed: any = text;
  try { parsed = text ? JSON.parse(text) : null; } catch { /* not JSON, keep the text */ }
  return { status: res.status, body: parsed };
}

/** Like send, but throws unless the status is a success. */
export async function call<T = any>(method: string, path: string, token?: string, body?: unknown): Promise<Reply<T>> {
  const reply = await send<T>(method, path, token, body);
  if (reply.status < 200 || reply.status > 299) throw new ApiError(method, path, reply);
  return reply;
}

const tokens = new Map<number, { token: string; at: number }>();

export async function login(person: Person): Promise<string> {
  const r = await call('POST', '/auth/login', undefined, { Username: person.username, Password: PASSWORD });
  return field<string>(r.body, 'Token');
}

/** Drops the cached token, for when the person's roles changed after it was issued. */
export function forget(person: Person): void {
  tokens.delete(person.userId);
}

/** A stepped-up token for the person. The code is read back from EmailLogs. */
export async function token(person: Person): Promise<string> {
  const cached = tokens.get(person.userId);
  if (cached && Date.now() - cached.at < 25 * 60_000) return cached.token;
  const plain = await login(person);
  const askedAt = new Date(Date.now() - 5_000);
  // A 503 means the code email failed. noCodeError reads the reason from EmailLogs.
  const asked = await send('POST', '/auth/step-up/request', plain);
  const code = asked.status === 503 ? null : await waitForCode(person.userId, askedAt);
  if (!code) throw noCodeError(person, askedAt);
  const r = await call('POST', '/auth/step-up/verify', plain, { Code: code, Method: 'email' });
  const stepped = field<string>(r.body, 'Token');
  tokens.set(person.userId, { token: stepped, at: Date.now() });
  return stepped;
}

/**
 * Runs a step that may need a second person. When the API answers 202 the approver
 * approves it. Returns the first reply and the approval id, if there was one.
 */
export async function twoPerson(asker: Person, approver: Person, method: string, path: string, body?: unknown):
  Promise<{ reply: Reply; approvalId: number | null }> {
  const reply = await call(method, path, await token(asker), body);
  if (reply.status !== 202) return { reply, approvalId: null };
  const approvalId = field<number>(reply.body, 'Id');
  await call('POST', `/elections/approvals/${approvalId}/approve`, await token(approver));
  return { reply, approvalId };
}

/** Fetches a binary file, such as a form PDF. */
export async function download(path: string, bearer: string): Promise<{ status: number; bytes: Buffer; type: string }> {
  const res = await fetch(`${API}${path}`, { headers: { Authorization: `Bearer ${bearer}` } });
  return { status: res.status, bytes: Buffer.from(await res.arrayBuffer()), type: res.headers.get('content-type') || '' };
}
