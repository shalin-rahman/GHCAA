import { APIRequestContext } from '@playwright/test';

const API_BASE = 'http://localhost:5087';

export async function getAuthToken(
  request: APIRequestContext,
  username: string,
  password: string
): Promise<string> {
  const res = await request.post(`${API_BASE}/api/auth/login`, {
    data: { username, password },
  });
  if (!res.ok()) {
    throw new Error(`Login failed for ${username}: ${res.status()}`);
  }
  const body = await res.json();
  return body.token as string;
}

/** Mark pending registration fee as Completed so admin approval can proceed. */
export async function completeRegistrationPayment(
  request: APIRequestContext,
  adminToken: string,
  memberEmail: string
): Promise<number> {
  const headers = { Authorization: `Bearer ${adminToken}` };

  const membersRes = await request.get(
    `${API_BASE}/api/admin/members?searchQuery=${encodeURIComponent(memberEmail)}&statusFilter=Applied&pageSize=5`,
    { headers }
  );
  if (!membersRes.ok()) {
    throw new Error(`Member lookup failed: ${membersRes.status()}`);
  }
  const members = await membersRes.json();
  const member = (members.items ?? []).find(
    (m: { email?: string; Email?: string }) =>
      (m.email ?? m.Email)?.toLowerCase() === memberEmail.toLowerCase()
  );
  if (!member) {
    throw new Error(`No Applied member found for ${memberEmail}`);
  }
  const memberId = member.id ?? member.Id;

  const historyRes = await request.get(
    `${API_BASE}/api/financials/member/${memberId}/history`,
    { headers }
  );
  if (!historyRes.ok()) {
    throw new Error(`Payment history lookup failed: ${historyRes.status()}`);
  }
  const history = await historyRes.json();
  const pending = (history as Array<{ id: number; status: string | number }>).find(
    (p) => p.status === 'Pending' || p.status === 0
  );
  if (!pending) {
    return memberId;
  }

  const patchRes = await request.patch(
    `${API_BASE}/api/financials/update-status/${pending.id}?status=Completed&notes=E2E+payment+verified`,
    { headers }
  );
  if (!patchRes.ok()) {
    throw new Error(`Payment completion failed: ${patchRes.status()}`);
  }
  return memberId;
}

/** Look up a member's numeric id by email via the admin search endpoint. */
export async function findMemberIdByEmail(
  request: APIRequestContext,
  adminToken: string,
  memberEmail: string
): Promise<number> {
  const headers = { Authorization: `Bearer ${adminToken}` };
  const membersRes = await request.get(
    `${API_BASE}/api/admin/members?searchQuery=${encodeURIComponent(memberEmail)}&statusFilter=all&pageSize=5`,
    { headers }
  );
  if (!membersRes.ok()) {
    throw new Error(`Member lookup failed: ${membersRes.status()}`);
  }
  const members = await membersRes.json();
  const member = (members.items ?? []).find(
    (m: { email?: string; Email?: string }) =>
      (m.email ?? m.Email)?.toLowerCase() === memberEmail.toLowerCase()
  );
  if (!member) {
    throw new Error(`No member found for ${memberEmail}`);
  }
  return member.id ?? member.Id;
}

/** Fully approve a member (payment + direct verify) so they can log in. */
export async function approveMember(
  request: APIRequestContext,
  adminToken: string,
  memberEmail: string
): Promise<number> {
  const headers = { Authorization: `Bearer ${adminToken}` };
  const memberId = await completeRegistrationPayment(request, adminToken, memberEmail);

  // Acting admin is resolved from the bearer token server-side; no admin id in the body.
  const approveRes = await request.post(`${API_BASE}/api/admin/members/${memberId}/approve`, {
    headers,
    data: {},
  });
  if (!approveRes.ok()) {
    throw new Error(`Member approval failed: ${approveRes.status()}`);
  }
  return memberId;
}
