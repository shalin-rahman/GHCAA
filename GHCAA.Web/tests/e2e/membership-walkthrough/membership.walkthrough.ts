import { Browser, BrowserContext, expect, test } from '@playwright/test';
import fs from 'node:fs';
import path from 'node:path';
import { API, ApiError, call, field, send, token } from '../election-walkthrough/api';
import { local, Person } from '../election-walkthrough/local';
import { ensureCashMethod } from '../election-walkthrough/payment-method';
import { FILES_DIR, Kind, Report, SHOTS_DIR } from '../election-walkthrough/report';
import { Actor, WINDOW_VIEWPORT, shows } from '../election-walkthrough/web';

// Membership walkthrough. Two people apply, verify their email and pay; the admin verifies one
// payment, approves that applicant and rejects the other; the new member logs in for the first
// time and changes the password. Each person works in their own browser session and every step
// lands in an HTML report with a screenshot. See README.md in this folder.

interface Applicant {
  key: string; fullName: string; fatherName: string; motherName: string; dateOfBirth: string;
  gender: string; bloodGroup: string; tShirtSize: string; presentAddress: string; permanentAddress: string;
  emergency: { name: string; relation: string; phone: string };
  academic: { degree: string; subject: string; admissionYear: number; passingYear: number };
  professional: { organization: string; designation: string; startDate: string };
  web: boolean; outcome: 'approve' | 'reject'; rejectReason?: string;
}
interface Scenario { title: string; admin: number; superAdmin: number; applicants: Applicant[] }

const S: Scenario = JSON.parse(fs.readFileSync(process.env['WALKTHROUGH_SCENARIO'] || path.join(__dirname, 'scenario.json'), 'utf-8'));
const KEEP = !!process.env['WALKTHROUGH_KEEP'];
// Every applicant the run makes carries this in the name, so the next run can find and archive it.
const SUFFIX = ' (local walkthrough)';
const STAGES = ['Applied', 'Email verified', 'Payment verified', 'Approved', 'First login', 'Rejected'];
const RUN = Date.now().toString();

const sleep = (ms: number) => new Promise(r => setTimeout(r, ms));

/** Fills a date in the shape the field's placeholder asks for, so the configured format is used. */
function inFormat(placeholder: string | null, isoDay: string): string {
  const [y, m, d] = isoDay.split('-');
  const shape = (placeholder || 'dd-mm-yyyy').toLowerCase();
  return shape.replace('yyyy', y).replace('mm', m).replace('dd', d);
}

/** A 1x1 JPEG, written once, for the photo, the certificate and the payment receipt. */
function stubImage(): string {
  const file = path.join(FILES_DIR, 'stub.jpg');
  if (!fs.existsSync(file)) {
    fs.writeFileSync(file, Buffer.from(
      '/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAAgGBgcGBQgHBwcJCQgKDBQNDAsLDBkSEw8UHRofHh0aHBwgJC4nICIsIxwcKDcpLDAxNDQ0Hyc5PTgyPC4zNDL/2wBDAQkJCQwLDBgNDRgyIRwhMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjL/wAARCAABAAEDASIAAhEBAxEB/8QAFAABAAAAAAAAAAAAAAAAAAAACf/EABQQAQAAAAAAAAAAAAAAAAAAAAD/xAAUAQEAAAAAAAAAAAAAAAAAAAAA/8QAFBEBAAAAAAAAAAAAAAAAAAAAAP/aAAwDAQACEQMRAD8AKgAB/9k=',
      'base64'));
  }
  return file;
}

test('membership walkthrough', async ({ browser, baseURL }) => {
  const viaWeb = await fetch(`${baseURL}/api/config`).catch(() => null);
  if (!viaWeb?.ok) {
    throw new Error(`The web server at ${baseURL} cannot reach the API (GET /api/config gave ${viaWeb?.status ?? 'no answer'}). `
      + 'Restart it with npm run start in GHCAA.Web, then run again.');
  }
  const config = await viaWeb.json();
  const institution: string = field(field(config, 'Branding'), 'InstitutionName') || 'the college';

  const report = new Report(`${S.title}: local walkthrough`);
  report.setPhases(STAGES);
  local.takeOver([S.admin, S.superAdmin]);
  const people = local.who([S.admin, S.superAdmin]);
  const admin = people.get(S.admin)!;
  const superAdmin = people.get(S.superAdmin)!;
  const actors: Actor[] = [];
  const contexts: BrowserContext[] = [];

  const note = (kind: Kind, actor: string, title: string, detail?: string,
    extra: Partial<{ table: { head: string[]; rows: (string | number)[][] }; ok: boolean }> = {}) =>
    report.add({ kind, actor, title, detail, ok: extra.ok ?? true, table: extra.table });

  // A probe passes when the API refuses. A 2xx or a 500 is a bug in the rules.
  const probe = async (who: string, bearer: string | undefined, title: string, method: string, url: string, body: unknown, expected: number[]) => {
    const r = await send(method, url, bearer, body);
    const ok = expected.includes(r.status);
    const why = typeof r.body === 'object' ? field<string>(r.body, 'Detail') ?? field<string>(r.body, 'Title') ?? '' : String(r.body ?? '');
    note('refusal', who, title, `${method} ${url} answered ${r.status}${why ? `: ${why}` : ''}. Expected ${expected.join(' or ')}.`, { ok });
    if (!ok) report.bug(`${title}: the API answered ${r.status} where ${expected.join(' or ')} was expected.`);
    return r;
  };

  /** A browser session for someone who is not logged in yet. */
  const guest = async (label: string) => {
    const context = await (browser as Browser).newContext({ viewport: WINDOW_VIEWPORT });
    context.setDefaultTimeout(60_000);
    contexts.push(context);
    const page = await context.newPage();
    page.on('dialog', d => d.accept());
    return {
      page,
      async goto(route: string) { await page.goto(route); await page.waitForLoadState('networkidle').catch(() => {}); },
      async shot(title: string, detail?: string) {
        if (!process.env['WALKTHROUGH_DEMO']) await page.waitForLoadState('networkidle').catch(() => {});
        const name = report.nextShotName();
        await page.screenshot({ path: path.join(SHOTS_DIR, name), fullPage: true });
        report.add({ kind: 'web', actor: label, title, detail, shot: name, ok: true });
      },
    };
  };

  /** Polls EmailLogs for a code sent to the address after `since`. */
  const codeFor = async (email: string, since: Date) => {
    for (let i = 0; i < 45; i++) {
      const code = local.codeFor(email, since);
      if (code) return code;
      await sleep(1000);
    }
    throw new Error(`No code reached ${email}. ${local.mailErrorFor(email, since) ?? ''}`.trim());
  };

  const sender = local.sender();
  const [box, domain] = sender.split('@');
  const ids = (a: Applicant, i: number) => ({
    name: a.fullName + SUFFIX,
    email: `${box.split('+')[0]}+reg${RUN}${i}@${domain}`.toLowerCase(),
    // 17 digits, one of the NID lengths the API accepts.
    nid: `19${RUN.slice(-13)}${i}0`,
    mobile: `01${7 + i}${RUN.slice(-8)}`,
  });

  try {
    // 0. Archive what earlier runs made, so the queue only shows this run's applicants.
    report.section('Clean slate', 'Applicants from earlier runs are archived through the API, the same way a SuperAdmin archives a member.');
    const su = await token(superAdmin);
    const earlier = (await call('GET', `/admin/members?searchQuery=${encodeURIComponent('(local walkthrough)')}&statusFilter=all&pageSize=50`, su)).body;
    const old = (field<any[]>(earlier, 'Items') ?? []).filter(m => String(field(m, 'FullName') ?? '').endsWith(SUFFIX));
    for (const m of old) await send('DELETE', `/admin/members/${field(m, 'Id')}`, su);
    note('api', superAdmin.name, 'Earlier walkthrough applicants archived', `${old.length} archived.`);

    const cash = await ensureCashMethod(su);
    const manual = cash.method;
    note('api', superAdmin.name, `Payment method for the run: ${field(manual, 'DisplayName')} (${cash.action})`,
      `Reference ${field(manual, 'RequiresReference') ? 'required' : 'not required'}, receipt ${field(manual, 'RequiresReceipt') ? 'required' : 'not required'}.`);
    report.setFacts([
      ['Run', RUN], ['Institution', institution], ['Admin', admin.name], ['SuperAdmin (clean-up)', superAdmin.name],
      ['Payment method', String(field(manual, 'DisplayName'))], ['Codes read from', `EmailLogs, plus addresses of ${sender}`],
    ]);
    report.setCast(S.applicants.map((a, i) => ['Applicant', ids(a, i).name, ids(a, i).nid, a.outcome === 'approve' ? 'Applies on the web, is approved' : 'Applies through the API, is rejected']));

    const memberIds: Record<string, number> = {};
    const image = stubImage();

    /** The multipart body the web form sends, for an applicant who applies through the API. */
    const formFor = (a: Applicant, i: number) => {
      const id = ids(a, i);
      const form = new FormData();
      const fields: Record<string, string> = {
        FullName: id.name, FatherName: a.fatherName, MotherName: a.motherName, DateOfBirth: a.dateOfBirth, NID: id.nid,
        MobileNo: id.mobile, Email: id.email, Gender: a.gender, BloodGroup: a.bloodGroup, TShirtSize: a.tShirtSize,
        PresentAddress: a.presentAddress, PermanentAddress: a.permanentAddress, EmergencyContactName: a.emergency.name,
        EmergencyContactRelation: a.emergency.relation, EmergencyContactPhone: a.emergency.phone,
        HasAcceptedTerms: 'true', HasAcceptedGdpr: 'true', PaymentMethodId: String(field(manual, 'Id')), TransactionId: `WALK-${RUN}-${i}`,
        'AcademicHistory[0].InstitutionName': institution, 'AcademicHistory[0].Degree': a.academic.degree,
        'AcademicHistory[0].Subject': a.academic.subject, 'AcademicHistory[0].AdmissionYear': String(a.academic.admissionYear),
        'AcademicHistory[0].PassingYear': String(a.academic.passingYear), 'AcademicHistory[0].IsOrgProfile': 'true',
        'ProfessionalHistory[0].OrganizationName': a.professional.organization, 'ProfessionalHistory[0].Designation': a.professional.designation,
        'ProfessionalHistory[0].StartDate': a.professional.startDate, 'ProfessionalHistory[0].IsCurrent': 'true',
      };
      for (const [k, v] of Object.entries(fields)) form.append(k, v);
      const jpeg = new Blob([fs.readFileSync(image)], { type: 'image/jpeg' });
      form.append('photo', jpeg, 'photo.jpg');
      form.append('paymentProof', jpeg, 'receipt.jpg');
      return form;
    };

    // 1. Applications
    report.section('Application', 'The public form has three steps: identity, background and filing with payment. Submitting sends an email code.');
    for (const [i, a] of S.applicants.entries()) {
      const id = ids(a, i);
      if (a.web) {
        const g = await guest(`${id.name} (applicant)`);
        await g.goto('/register');
        const p = g.page;
        await p.locator('#reg-fullName').fill(id.name);
        await p.locator('#reg-fatherName').fill(a.fatherName);
        await p.locator('#reg-motherName').fill(a.motherName);
        const dob = p.locator('#reg-dateOfBirth');
        await dob.fill(inFormat(await dob.getAttribute('placeholder'), a.dateOfBirth));
        await dob.blur();
        await p.locator('#reg-nid').fill(id.nid);
        await p.locator('#reg-gender').selectOption(a.gender);
        await p.locator('#reg-bloodGroup').selectOption(a.bloodGroup);
        await p.locator('#reg-tShirtSize').selectOption(a.tShirtSize);
        await p.locator('#reg-email').fill(id.email);
        await p.locator('#reg-mobileNo').fill(id.mobile);
        await p.locator('#reg-presentAddress').fill(a.presentAddress);
        await p.locator('#reg-permanentAddress').fill(a.permanentAddress);
        await g.shot('Step 1: identity and contact', 'The fee shown comes from the fee settings for the default tier.');
        await p.getByRole('button', { name: /Continue Assessment/ }).click();
        await expect(p.getByRole('heading', { name: /Background & Milestones/ })).toBeVisible({ timeout: 15_000 });

        // ngModel keeps [name] to itself, so the selects are found by order on the step.
        const selects = p.locator('.step-content select');
        await selects.nth(0).selectOption(a.academic.degree);
        await selects.nth(1).selectOption(a.academic.subject);
        await selects.nth(2).selectOption(String(a.academic.admissionYear));
        await selects.nth(3).selectOption(String(a.academic.passingYear));
        await p.getByPlaceholder('Employer Title').first().fill(a.professional.organization);
        await p.getByPlaceholder(/Chief Technologist/).first().fill(a.professional.designation);
        const sector = p.locator('.step-content select').filter({ hasText: /Select Sector/ }).first();
        if (await sector.count()) await sector.selectOption({ index: 1 }).catch(() => {});
        await p.locator('label', { hasText: /I currently serve/ }).locator('input[type=checkbox]').first().check({ force: true });
        const start = p.locator('.step-content input.dpi-text').first();
        await start.fill(inFormat(await start.getAttribute('placeholder'), a.professional.startDate));
        await start.blur();
        await p.locator('#reg-emergencyContactName').fill(a.emergency.name);
        await p.locator('#reg-emergencyContactRelation').fill(a.emergency.relation);
        await p.locator('#reg-emergencyContactPhone').fill(a.emergency.phone);
        await g.shot('Step 2: academic and career record', `The first academic record is fixed to ${institution}.`);
        await p.getByRole('button', { name: /Continue Assessment/ }).click();
        await expect(p.getByRole('heading', { name: /Registry Filing/ })).toBeVisible({ timeout: 15_000 });

        await p.locator('app-payment-portal .payment-card', { hasText: String(field(manual, 'DisplayName')) }).first().click();
        if (field(manual, 'RequiresReference')) await p.getByPlaceholder('Enter TxnID / Reference').fill(`WALK-${RUN}-${i}`);
        if (field(manual, 'RequiresReceipt')) await p.locator('app-payment-portal .file-drop input[type=file]').setInputFiles(image);
        await p.locator('.form-group', { hasText: 'Formal Profile Photo' }).locator('input[type=file]').setInputFiles(image);
        await p.locator('.form-group', { hasText: 'Academic Certificate Proof' }).locator('input[type=file]').setInputFiles(image);
        for (const n of ['terms', 'gdpr', 'agree']) await p.locator(`input[name=${n}]`).check({ force: true });
        await g.shot('Step 3: payment, documents and consent');
        const since = new Date(Date.now() - 5_000);
        const [reply] = await Promise.all([
          p.waitForResponse(r => r.url().includes('/api/auth/register') && r.request().method() === 'POST', { timeout: 60_000 }),
          p.getByRole('button', { name: /Finalize Registry/ }).click(),
        ]);
        const body = await reply.json().catch(() => ({}));
        if (!reply.ok()) throw new Error(`The registration form was refused: ${reply.status()} ${JSON.stringify(body).slice(0, 300)}`);
        memberIds[a.key] = field<number>(body, 'MemberId');
        await expect(p.getByRole('heading', { name: 'Security Authentication' })).toBeVisible({ timeout: 30_000 });
        await g.shot('Email code asked for', `Application ${memberIds[a.key]} saved as Applied.`);
        const code = await codeFor(id.email, since);
        await p.locator('.otp-field input').fill(code);
        await p.getByRole('button', { name: /Confirm Security Identity/ }).click();
        await expect(p).toHaveURL(/\/login/, { timeout: 30_000 });
        await g.shot('Email verified, sent to the login page', 'The application now waits for the admin.');
      } else {
        const form = formFor(a, i);
        const since = new Date(Date.now() - 5_000);
        const res = await fetch(`${API}/auth/register`, { method: 'POST', body: form });
        const body = await res.json().catch(() => ({}));
        if (!res.ok) throw new Error(`POST /auth/register gave ${res.status}: ${JSON.stringify(body).slice(0, 300)}`);
        memberIds[a.key] = field<number>(body, 'MemberId');
        note('api', id.name, 'Application filed through the API', `Application ${memberIds[a.key]}, with photo and receipt.`);
        await probe(id.name, undefined, 'A wrong email code is refused', 'POST', '/auth/verify-email', { Email: id.email, OtpCode: '000000' }, [400]);
        const code = await codeFor(id.email, since);
        await call('POST', '/auth/verify-email', undefined, { Email: id.email, OtpCode: code });
        note('api', id.name, 'Email verified with the code from the email');
      }
      const status = await send('GET', `/auth/status/${memberIds[a.key]}?email=${encodeURIComponent(id.email)}`);
      note('check', id.name, 'Application status as the applicant sees it', JSON.stringify(status.body).slice(0, 300), { ok: status.status === 200 });
    }
    report.reachedPhase('Applied');
    report.reachedPhase('Email verified');

    const first = S.applicants[0];
    const firstId = ids(first, 0);
    // 84.102: the same person applying again is refused with 409 and the reason, before any row is written.
    {
      const again = S.applicants.findIndex(a => !a.web);
      if (again >= 0) {
        const r = await fetch(`${API}/auth/register`, { method: 'POST', body: formFor(S.applicants[again], again) });
        const body = await r.json().catch(() => ({}));
        const ok = r.status === 409;
        note('refusal', ids(S.applicants[again], again).name, 'Applying a second time with the same NID, email and mobile is refused',
          `POST /auth/register answered ${r.status}: ${field(body, 'Detail') ?? ''}. Expected 409.`, { ok });
        if (!ok) report.bug(`A duplicate application answered ${r.status}, not 409 with the reason (tracker 84.102).`);
      }
    }
    await probe(firstId.name, undefined, 'An applicant cannot log in before approval', 'POST', '/auth/login', { Username: firstId.nid, Password: firstId.nid }, [400, 401, 403]);

    // 2. Payment
    report.section('Payment', 'The application fee was declared on the form. It has to be marked as received before the member can be approved.');
    const ad = await token(admin);
    const adminWeb = await Actor.open(browser, admin, 'Admin', report);
    actors.push(adminWeb);
    const approveFirst = `/admin/members/${memberIds[first.key]}/approve`;
    await probe(admin.name, ad, 'Approval is refused while the fee is unpaid', 'POST', approveFirst, {}, [400]);
    const history = (await call<any[]>('GET', `/financials/member/${memberIds[first.key]}/history`, ad)).body;
    const pending = history.find(p => ['Pending', 0].includes(field(p, 'Status')));
    await adminWeb.goto('/admin/ledger');
    await adminWeb.shot('Ledger', 'The declared payment is Pending. There is no screen to open the proof and verify it (tracker 84.92).');
    if (pending) {
      const paymentId = field(pending, 'Id');
      const queue = (await call<any[]>('GET', '/financials/pending', ad)).body ?? [];
      const queued = queue.some(p => field(p, 'Id') === paymentId);
      note('check', admin.name, 'The declared payment is in the review queue', `GET /financials/pending lists ${queue.length} payment(s).`, { ok: queued });
      if (!queued) report.bug(`Payment ${paymentId} is Pending but GET /financials/pending does not list it.`);
      const verified = await call('POST', `/financials/${paymentId}/verify`, ad);
      note('api', admin.name, 'Payment verified through the API', `Payment ${paymentId}, ${field(pending, 'Amount')}; due ${field(verified.body, 'DueId') ?? 'none matched'}.`);
      await probe(admin.name, ad, 'A second verify is refused', 'POST', `/financials/${paymentId}/verify`, {}, [409]);
      await probe(admin.name, ad, 'Rejecting a verified payment is refused', 'POST', `/financials/${paymentId}/reject`, { Reason: 'Walkthrough check' }, [409]);
      report.gap('No admin screen verifies a declared payment yet; the walkthrough used POST /financials/{id}/verify (tracker 84.92, screen in plan S11).');
    } else {
      note('check', admin.name, 'No pending payment was found for the applicant', undefined, { ok: false });
    }
    report.reachedPhase('Payment verified');

    // 3. Approval and rejection
    report.section('Approval', 'The admin reviews each application in the approvals queue, approves one and declines the other with a reason.');
    const openRow = async (email: string) => {
      await adminWeb.goto('/admin/approvals');
      const search = adminWeb.page.locator('app-search-bar input').first();
      if (await shows(search, 5_000)) await search.fill(email);
      const row = adminWeb.page.locator('tr', { hasText: email }).first();
      await expect(row).toBeVisible({ timeout: 20_000 });
      return row;
    };
    let row = await openRow(firstId.email);
    await adminWeb.shot('Application in the approvals queue');
    await row.getByRole('button', { name: /Details/ }).click();
    await adminWeb.shot('Reviewing the application', 'Identity, academic record, career, documents and payment.');
    const approveButton = adminWeb.page.getByRole('button', { name: 'Approve Member' });
    let membershipNumber = '';
    if (await shows(approveButton)) {
      const [reply] = await Promise.all([
        adminWeb.page.waitForResponse(r => r.url().includes(approveFirst) && r.request().method() === 'POST', { timeout: 60_000 }),
        (async () => { await approveButton.click(); await adminWeb.confirm('Approving the member'); })(),
      ]);
      membershipNumber = field(await reply.json().catch(() => ({})), 'MembershipNumber') ?? '';
      await adminWeb.shot('Member approved', `Membership number ${membershipNumber}. The welcome email carries the first password.`);
    } else {
      const r = await call('POST', approveFirst, ad, {});
      membershipNumber = field(r.body, 'MembershipNumber');
      report.gap('The "Approve Member" button did not show in the review panel; approval went through the API.');
    }
    await probe(admin.name, ad, 'An approved member cannot be approved again', 'POST', approveFirst, {}, [400]);
    report.reachedPhase('Approved');

    const second = S.applicants.find(a => a.outcome === 'reject');
    if (second) {
      const secondId = ids(second, S.applicants.indexOf(second));
      row = await openRow(secondId.email);
      await row.getByRole('button', { name: /Details/ }).click();
      const decline = adminWeb.page.getByRole('button', { name: /Decline\s+Application/ });
      if (await shows(decline)) {
        await decline.click();
        await adminWeb.page.locator('textarea').last().fill(second.rejectReason ?? 'Walkthrough rejection.');
        await adminWeb.shot('Reason for declining');
        await adminWeb.page.getByRole('button', { name: 'Reject Application' }).click();
        await adminWeb.confirm('Declining the application');
        await shows(adminWeb.page.getByText(/Application declined/).first(), 30_000);
        await adminWeb.shot('Application declined', 'The applicant is emailed the reason and the record is archived.');
      } else {
        await call('POST', `/admin/members/${memberIds[second.key]}/reject`, ad, { Reason: second.rejectReason });
        report.gap('The "Decline Application" button did not show; the rejection went through the API.');
      }
      await probe(admin.name, ad, 'A rejected application cannot be rejected again', 'POST', `/admin/members/${memberIds[second.key]}/reject`, { Reason: 'Again.' }, [400, 404]);
      await probe(secondId.name, undefined, 'A rejected applicant cannot log in', 'POST', '/auth/login', { Username: secondId.nid, Password: secondId.nid }, [400, 401, 403]);
      report.reachedPhase('Rejected');
    }

    // 4. First login
    report.section('First login', 'The new member logs in with the NID as user name and first password, and has to change the password before anything else opens.');
    const newPassword = `Walk#${RUN.slice(-6)}a`;
    const member: Person = { userId: 0, username: firstId.nid, memberId: memberIds[first.key], name: firstId.name };
    const memberWeb = await Actor.open(browser, member, 'New member', report, { password: firstId.nid, changeTo: newPassword });
    actors.push(memberWeb);
    await memberWeb.goto('/portal/dashboard');
    await memberWeb.shot('Member dashboard after the first login');
    report.reachedPhase('First login');
    await probe(member.name, undefined, 'The first password no longer works', 'POST', '/auth/login', { Username: firstId.nid, Password: firstId.nid }, [400, 401]);
    const signedIn = await call('POST', '/auth/login', undefined, { Username: firstId.nid, Password: newPassword });
    const bearer = field<string>(signedIn.body, 'Token');
    const notes = await send('GET', '/notifications', bearer);
    const list: any[] = Array.isArray(notes.body) ? notes.body : field<any[]>(notes.body, 'Items') ?? [];
    note('check', member.name, 'Welcome notification received', `${list.length} notifications.`, { ok: list.some(n => /welcome/i.test(String(field(n, 'Title') ?? ''))) });
    for (const [route, title] of [['/portal/id-card', 'Digital ID card'], ['/portal/profile', 'Profile'], ['/portal/payments', 'Payments, with the fee received']] as const) {
      await memberWeb.goto(route);
      await memberWeb.shot(title);
    }
    note('check', 'Run', 'Membership number issued', membershipNumber || 'none', { ok: !!membershipNumber });
    report.finish('finished');
  } catch (err) {
    const msg = err instanceof ApiError ? err.message : String((err as Error)?.stack ?? err).slice(0, 1500);
    note('check', 'Run', 'The run stopped', msg, { ok: false });
    for (const a of actors) await a.shot('Screen when the run stopped', undefined, 'check', false).catch(() => {});
    report.finish('stopped with an error');
    throw err;
  } finally {
    for (const a of actors) await a.close().catch(() => {});
    for (const c of contexts) await c.close().catch(() => {});
    if (!KEEP) local.restore();
  }
});
