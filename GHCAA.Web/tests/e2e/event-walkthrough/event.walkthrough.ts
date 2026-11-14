import { Browser, BrowserContext, Locator, expect, test } from '@playwright/test';
import fs from 'node:fs';
import path from 'node:path';
import { ApiError, call, field, login, send, token } from '../election-walkthrough/api';
import { local, Person } from '../election-walkthrough/local';
import { ensureCashMethod } from '../election-walkthrough/payment-method';
import { FILES_DIR, Kind, Report, SHOTS_DIR } from '../election-walkthrough/report';
import { Actor, WINDOW_VIEWPORT, shows } from '../election-walkthrough/web';

// Event participation walkthrough. The admin creates a paid event with a seat limit and a
// waitlist; members and a guest register; the admin approves one, rejects one, sends the pass and
// checks the approved member in by ticket code. Each person works in their own browser session
// and every step lands in an HTML report with a screenshot. See README.md in this folder.

interface Scenario {
  title: string; admin: number; superAdmin: number;
  event: { title: string; description: string; location: string; fee: number; participantLimit: number; hasWaitlist: boolean;
    allowNonMembers: boolean; startsInDays: number; hours: number };
  members: { user: number; web: boolean; outcome: 'approve' | 'reject' | 'waitlist' }[];
  guest: { name: string; mobile: string };
}

const S: Scenario = JSON.parse(fs.readFileSync(process.env['WALKTHROUGH_SCENARIO'] || path.join(__dirname, 'scenario.json'), 'utf-8'));
const KEEP = !!process.env['WALKTHROUGH_KEEP'];
const SUFFIX = ' (local walkthrough)';
const STAGES = ['Event created', 'Registrations', 'Approved and rejected', 'Pass sent', 'Checked in'];
const RUN = Date.now().toString();
const two = (n: number) => String(n).padStart(2, '0');

/** Date and time in the shape the date field's placeholder asks for, and HH:mm for the time box. */
function inFormat(placeholder: string | null, d: Date): { day: string; time: string } {
  const shape = (placeholder || 'dd-mm-yyyy').toLowerCase();
  return {
    day: shape.replace('yyyy', String(d.getFullYear())).replace('mm', two(d.getMonth() + 1)).replace('dd', two(d.getDate())),
    time: `${two(d.getHours())}:${two(d.getMinutes())}`,
  };
}

function stubImage(): string {
  const file = path.join(FILES_DIR, 'receipt.jpg');
  if (!fs.existsSync(file)) {
    fs.writeFileSync(file, Buffer.from(
      '/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAAgGBgcGBQgHBwcJCQgKDBQNDAsLDBkSEw8UHRofHh0aHBwgJC4nICIsIxwcKDcpLDAxNDQ0Hyc5PTgyPC4zNDL/2wBDAQkJCQwLDBgNDRgyIRwhMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjL/wAARCAABAAEDASIAAhEBAxEB/8QAFAABAAAAAAAAAAAAAAAAAAAACf/EABQQAQAAAAAAAAAAAAAAAAAAAAD/xAAUAQEAAAAAAAAAAAAAAAAAAAAA/8QAFBEBAAAAAAAAAAAAAAAAAAAAAP/aAAwDAQACEQMRAD8AKgAB/9k=',
      'base64'));
  }
  return file;
}

test('event walkthrough', async ({ browser, baseURL }) => {
  const viaWeb = await fetch(`${baseURL}/api/config`).catch(() => null);
  if (!viaWeb?.ok) {
    throw new Error(`The web server at ${baseURL} cannot reach the API (GET /api/config gave ${viaWeb?.status ?? 'no answer'}). `
      + 'Restart it with npm run start in GHCAA.Web, then run again.');
  }
  const report = new Report(`${S.title}: local walkthrough`);
  report.setPhases(STAGES);
  const userIds = [...new Set([S.admin, S.superAdmin, ...S.members.map(m => m.user)])];
  local.takeOver(userIds);
  const people = local.who(userIds);
  const admin = people.get(S.admin)!;
  const actors: Actor[] = [];
  const contexts: BrowserContext[] = [];
  const title = S.event.title + SUFFIX;

  const note = (kind: Kind, actor: string, t: string, detail?: string,
    extra: Partial<{ table: { head: string[]; rows: (string | number)[][] }; ok: boolean }> = {}) =>
    report.add({ kind, actor, title: t, detail, ok: extra.ok ?? true, table: extra.table });

  const probe = async (who: string, bearer: string | undefined, t: string, method: string, url: string, body: unknown, expected: number[]) => {
    const r = await send(method, url, bearer, body);
    const ok = expected.includes(r.status);
    const why = typeof r.body === 'object' ? field<string>(r.body, 'Detail') ?? field<string>(r.body, 'Title') ?? '' : String(r.body ?? '');
    note('refusal', who, t, `${method} ${url} answered ${r.status}${why ? `: ${why}` : ''}. Expected ${expected.join(' or ')}.`, { ok });
    if (!ok) report.bug(`${t}: the API answered ${r.status} where ${expected.join(' or ')} was expected.`);
    return r;
  };

  const open = async (p: Person, role: string) => {
    const a = await Actor.open(browser, p, role, report);
    actors.push(a);
    return a;
  };

  const visitor = async () => {
    const context = await (browser as Browser).newContext({ viewport: WINDOW_VIEWPORT });
    contexts.push(context);
    const page = await context.newPage();
    return async (route: string, t: string) => {
      await page.goto(route);
      await page.waitForLoadState('networkidle').catch(() => {});
      const name = report.nextShotName();
      await page.screenshot({ path: path.join(SHOTS_DIR, name), fullPage: true });
      report.add({ kind: 'web', actor: 'Public visitor', title: t, shot: name, ok: true });
    };
  };

  const fillDate = async (host: Locator, when: Date) => {
    const text = host.locator('input.dpi-text');
    const v = inFormat(await text.getAttribute('placeholder'), when);
    await text.fill(v.day);
    const time = host.locator('input.dpi-time');
    if (await time.count()) await time.fill(v.time);
  };

  try {
    const ad = await token(admin);

    // 0. Clean slate
    report.section('Clean slate', 'Events from earlier runs are deleted through the admin API, with their registrations.');
    const all = (await call<any[]>('GET', '/events/admin/all', ad)).body;
    const old = all.filter(e => String(field(e, 'Title') ?? '').endsWith(SUFFIX));
    for (const e of old) await send('DELETE', `/events/admin/${field(e, 'Id')}`, ad);
    note('api', admin.name, 'Earlier walkthrough events deleted', `${old.length} deleted.`);

    const cash = await ensureCashMethod(await token(people.get(S.superAdmin)!));
    const manual = cash.method;
    note('api', people.get(S.superAdmin)!.name, `Payment method for the run: ${field(manual, 'DisplayName')} (${cash.action})`);

    const starts = new Date(Date.now() + S.event.startsInDays * 86_400_000);
    starts.setMinutes(0, 0, 0);
    const ends = new Date(starts.getTime() + S.event.hours * 3_600_000);
    const regOpens = new Date(Date.now() - 3_600_000);
    const regCloses = new Date(starts.getTime() - 3_600_000);
    report.setFacts([
      ['Run', RUN], ['Event', title], ['Starts', starts.toLocaleString()], ['Seats', String(S.event.participantLimit)],
      ['Waitlist', S.event.hasWaitlist ? 'yes' : 'no'], ['Fee', String(S.event.fee)], ['Payment method', String(field(manual, 'DisplayName'))],
    ]);
    report.setCast([
      ['Admin', admin.name, admin.username, 'Creates the event, approves, rejects, sends the pass, checks in'],
      ...S.members.map(m => { const p = people.get(m.user)!; return ['Member', p.name, p.username, `${m.web ? 'Web' : 'API'}; ${m.outcome}`]; }),
      ['Guest', S.guest.name + SUFFIX, '-', 'Registers without an account'],
    ]);

    // 1. Event
    report.section('Creating the event', 'A paid event with a seat limit and a waitlist, open to guests.');
    await probe(admin.name, ad, 'An event that ends before it starts is refused', 'POST', '/events/admin', {
      Title: `Bad dates${SUFFIX}`, Description: 'x', Location: 'x', StartDate: ends.toISOString(), EndDate: starts.toISOString(),
    }, [400]);
    const adminWeb = await open(admin, 'Admin');
    await adminWeb.goto('/admin/events');
    const page = adminWeb.page;
    await page.getByRole('button', { name: /Launch New Event/ }).click();
    await page.locator('input[formControlName="title"]').fill(title);
    await page.locator('input[formControlName="location"]').fill(S.event.location);
    await page.locator('textarea[formControlName="description"]').fill(S.event.description);
    await fillDate(page.locator('app-date-input[formcontrolname="startDate"]'), starts);
    await fillDate(page.locator('app-date-input[formcontrolname="endDate"]'), ends);
    await fillDate(page.locator('app-date-input[formcontrolname="registrationStartDate"]'), regOpens);
    await fillDate(page.locator('app-date-input[formcontrolname="registrationEndDate"]'), regCloses);
    await page.locator('input[formControlName="registrationFee"]').fill(String(S.event.fee));
    await page.locator('input[formControlName="participantLimit"]').fill(String(S.event.participantLimit));
    for (const [name, on] of [['hasWaitlist', S.event.hasWaitlist], ['requiresPayment', true], ['isActive', true], ['allowNonMembers', S.event.allowNonMembers]] as const) {
      const box = page.locator(`input[formControlName="${name}"]`);
      if (on) await box.check({ force: true }); else await box.uncheck({ force: true });
    }
    await adminWeb.shot('Event form filled');
    await page.getByRole('button', { name: 'Save', exact: true }).click();
    let event = await (async () => {
      for (let i = 0; i < 15; i++) {
        const found = (await call<any[]>('GET', '/events/admin/all', ad)).body.find(e => field(e, 'Title') === title);
        if (found) return found;
        await new Promise(r => setTimeout(r, 1000));
      }
      return null;
    })();
    if (event) {
      await adminWeb.shot('Event saved');
    } else {
      report.gap('The event form did not save; the event was created through the API.');
      event = (await call('POST', '/events/admin', ad, {
        Title: title, Description: S.event.description, Location: S.event.location, StartDate: starts.toISOString(), EndDate: ends.toISOString(),
        RegistrationStartDate: regOpens.toISOString(), RegistrationEndDate: regCloses.toISOString(), RegistrationFee: S.event.fee,
        RequiresPayment: true, IsActive: true, AllowNonMembers: S.event.allowNonMembers, ParticipantLimit: S.event.participantLimit,
        HasWaitlist: S.event.hasWaitlist, RequiresRegistration: true,
      })).body;
    }
    const eventId = field<number>(event, 'Id');
    note('check', admin.name, 'Event stored with its rules', undefined, {
      ok: field(event, 'ParticipantLimit') === S.event.participantLimit && !!field(event, 'HasWaitlist') === S.event.hasWaitlist,
      table: { head: ['Field', 'Value'], rows: ['Id', 'StartDate', 'EndDate', 'RegistrationFee', 'ParticipantLimit', 'HasWaitlist', 'AllowNonMembers', 'RequiresPayment'].map(k => [k, String(field(event, k))]) },
    });
    report.reachedPhase('Event created');

    // 2. Registrations
    report.section('Registrations', `Seats fill in order. With ${S.event.participantLimit} seats, later registrations go to the waitlist.`);
    const regs: Record<number, any> = {};
    const rows: (string | number)[][] = [];
    for (const m of S.members) {
      const p = people.get(m.user)!;
      if (m.web) {
        const w = await open(p, 'Member');
        await w.goto('/portal/events');
        const card = w.page.locator('.event-card', { hasText: title }).first();
        await expect(card).toBeVisible({ timeout: 20_000 });
        await w.shot('The event in the member list');
        await card.getByRole('button', { name: /Secure Pass|Join Event/ }).click();
        await expect(w.page.getByText('Event Registration').first()).toBeVisible({ timeout: 15_000 });
        await w.page.locator('app-payment-portal .payment-card', { hasText: String(field(manual, 'DisplayName')) }).last().click();
        if (field(manual, 'RequiresReference')) await w.page.getByPlaceholder('Enter TxnID / Reference').fill(`EVT-${RUN}`);
        if (field(manual, 'RequiresReceipt')) await w.page.locator('app-payment-portal .file-drop input[type=file]').setInputFiles(stubImage());
        await w.shot('Registration with the payment declared');
        const [reply] = await Promise.all([
          w.page.waitForResponse(r => r.url().includes('/api/events/register') && r.request().method() === 'POST', { timeout: 60_000 }),
          w.page.getByRole('button', { name: 'Confirm My Spot' }).click(),
        ]);
        regs[m.user] = await reply.json().catch(() => ({}));
        if (!reply.ok()) throw new Error(`Event registration was refused: ${reply.status()} ${JSON.stringify(regs[m.user]).slice(0, 300)}`);
        await w.page.getByRole('button', { name: /My Participations/ }).click();
        await expect(w.page.locator('.reg-card', { hasText: title })).toBeVisible({ timeout: 15_000 });
        await w.shot('Registration listed under My Participations', `Status ${field(regs[m.user], 'Status')}.`);
      } else {
        const r = await call('POST', '/events/register', await login(p), {
          EventId: eventId, PaymentReference: `EVT-${RUN}-${m.user}`, PaymentMethod: field(manual, 'Method'),
        });
        regs[m.user] = r.body;
        note('api', p.name, 'Registered through the API', `Status ${field(r.body, 'Status')}.`);
      }
      rows.push([p.name, field(regs[m.user], 'Id'), String(field(regs[m.user], 'Status')), field(regs[m.user], 'TicketCode') ?? '']);
    }
    const guestEmail = `${local.sender().split('@')[0].split('+')[0]}+guest${RUN}@${local.sender().split('@')[1]}`.toLowerCase();
    const guestReply = await send('POST', '/events/register', undefined, {
      EventId: eventId, IsNonMember: true, GuestName: S.guest.name + SUFFIX, GuestEmail: guestEmail, GuestMobile: S.guest.mobile,
      PaymentReference: `EVT-${RUN}-guest`, PaymentMethod: field(manual, 'Method'),
    });
    note('api', 'Guest', 'A guest registers without an account', `Answered ${guestReply.status}, status ${field(guestReply.body, 'Status')}.`,
      { ok: guestReply.status === 200 });
    rows.push([S.guest.name + SUFFIX, field(guestReply.body, 'Id') ?? '', String(field(guestReply.body, 'Status') ?? ''), field(guestReply.body, 'TicketCode') ?? '']);
    note('check', 'Run', 'Registrations on file', undefined, { table: { head: ['Who', 'Registration', 'Status', 'Ticket code'], rows } });
    const waitlisted = S.members.filter(m => m.outcome === 'waitlist').every(m => field(regs[m.user], 'Status') === 'Waitlisted');
    note('check', 'Run', `Registrations past ${S.event.participantLimit} seats are waitlisted`, undefined, { ok: waitlisted });

    const webMember = S.members.find(m => m.web)!;
    const webPerson = people.get(webMember.user)!;
    await probe(webPerson.name, await login(webPerson), 'A member cannot register twice (409 with the reason, 84.102)', 'POST', '/events/register', { EventId: eventId, PaymentReference: 'again' }, [409]);
    await probe('Visitor', undefined, 'A registration with no account and no guest details is refused', 'POST', '/events/register', { EventId: eventId }, [401]);
    await probe(webPerson.name, await login(webPerson), 'Registering for an event that does not exist is not found', 'POST', '/events/register', { EventId: 999999, PaymentReference: 'x' }, [404]);
    report.reachedPhase('Registrations');

    // 3. Decisions
    report.section('Approval', 'The admin works the Participation Approvals tab: one approved, one rejected.');
    const decide = async (user: number, approve: boolean) => {
      const p = people.get(user)!;
      const id = field<number>(regs[user], 'Id');
      await adminWeb.goto('/admin/events');
      await page.getByRole('button', { name: /Participation Approvals/ }).click();
      const row = page.locator('tr', { hasText: title }).filter({ hasText: p.name }).first();
      if (await shows(row, 15_000)) {
        await adminWeb.shot(`${p.name} waiting for a decision`, 'The row shows the fee, the reference and the receipt.');
        await row.getByRole('button', { name: approve ? /Approve/ : /Reject/ }).click();
        await adminWeb.confirm(`${approve ? 'Approving' : 'Rejecting'} ${p.name}`);
        await shows(page.getByText(/Registration successfully/).first(), 20_000);
        await adminWeb.shot(`${p.name} ${approve ? 'approved' : 'rejected'}`);
      } else {
        await call('POST', '/events/admin/approve-registration', ad, { RegistrationId: id, Approve: approve });
        report.gap(`${p.name}'s registration did not show on the Participation Approvals tab; the decision went through the API.`);
      }
    };
    for (const m of S.members.filter(x => x.outcome === 'approve')) await decide(m.user, true);
    for (const m of S.members.filter(x => x.outcome === 'reject')) await decide(m.user, false);
    const after = (await call('GET', `/events/admin/registrations?eventId=${eventId}&pageSize=50`, ad)).body;
    const list: any[] = field<any[]>(after, 'Items') ?? (Array.isArray(after) ? after : []);
    note('check', admin.name, 'Registrations after the decisions', undefined, {
      table: { head: ['Who', 'Status'], rows: list.map(r => [String(field(r, 'MemberName') ?? field(r, 'GuestName')), String(field(r, 'Status'))]) },
    });
    const stillWaiting = list.filter(r => field(r, 'Status') === 'Waitlisted').length;
    if (stillWaiting > 0) report.gap(`A seat was freed by the rejection, but ${stillWaiting} waitlisted registration(s) stayed waitlisted. Nothing promotes the waitlist (tracker 84.80).`);
    const approvedPerson = people.get(S.members.find(m => m.outcome === 'approve')!.user)!;
    const approvedBearer = await login(approvedPerson);
    const notes = await send('GET', '/notifications', approvedBearer);
    const notified: any[] = Array.isArray(notes.body) ? notes.body : field<any[]>(notes.body, 'Items') ?? [];
    note('check', approvedPerson.name, 'The approved member is notified', `${notified.length} notifications.`,
      { ok: notified.some(n => /approv|participat/i.test(`${field(n, 'Title')} ${field(n, 'Message')}`)) });
    report.reachedPhase('Approved and rejected');

    // 4. Pass
    report.section('Event pass', 'An approved registration gets an emailed invitation with the ticket code. Others cannot open it.');
    const approvedId = field<number>(regs[S.members.find(m => m.outcome === 'approve')!.user], 'Id');
    const rejected = S.members.find(m => m.outcome === 'reject');
    await call('POST', `/events/admin/registrations/${approvedId}/send-invitation`, ad);
    note('api', admin.name, 'Invitation emailed to the approved member');
    if (rejected) {
      const rid = field<number>(regs[rejected.user], 'Id');
      await probe(admin.name, ad, 'No invitation for a rejected registration', 'POST', `/events/admin/registrations/${rid}/send-invitation`, undefined, [400]);
      const rp = people.get(rejected.user)!;
      await probe(rp.name, await login(rp), 'A rejected member cannot open the pass', 'GET', `/events/registration/${rid}`, undefined, [400]);
      await probe(rp.name, await login(rp), "A member cannot open someone else's pass", 'GET', `/events/registration/${approvedId}`, undefined, [403]);
    }
    const memberWeb = actors.find(a => a.person.userId === approvedPerson.userId) ?? await open(approvedPerson, 'Member');
    await memberWeb.goto('/portal/events');
    await memberWeb.page.getByRole('button', { name: /My Participations/ }).click();
    await memberWeb.shot('Registration approved');
    const print = memberWeb.page.locator('.reg-card', { hasText: title }).locator('button').first();
    if (await shows(print, 5_000)) { await print.click(); await memberWeb.shot('Event pass with the ticket code'); }
    report.reachedPhase('Pass sent');

    // 5. Check-in
    report.section('Check-in', 'At the door the admin enters or scans the ticket code. A code works once.');
    const ticket = field<string>(regs[S.members.find(m => m.outcome === 'approve')!.user], 'TicketCode');
    if (!ticket) {
      note('check', 'Run', 'The registration reply carried no ticket code', undefined, { ok: false });
    } else {
      const ok = await call('POST', '/events/admin/checkin/qr', ad, { TicketCode: ticket });
      note('api', admin.name, `Ticket ${ticket} checked in`, JSON.stringify(ok.body));
      await probe(admin.name, ad, 'The same ticket cannot check in twice', 'POST', '/events/admin/checkin/qr', { TicketCode: ticket }, [404]);
      await probe(admin.name, ad, 'An unknown ticket is refused', 'POST', '/events/admin/checkin/qr', { TicketCode: 'NOSUCH00' }, [404]);
      await probe(approvedPerson.name, approvedBearer, 'A member cannot check people in', 'POST', '/events/admin/checkin/qr', { TicketCode: ticket }, [403]);
    }
    report.reachedPhase('Checked in');
    const show = await visitor();
    await show('/events', 'Public events page');
    const participants = await send('GET', `/events/${eventId}/participants`);
    note('check', 'Public visitor', 'Public participant list', `Answered ${participants.status}.`, {
      ok: participants.status === 200,
      table: { head: ['Name', 'Status'], rows: (Array.isArray(participants.body) ? participants.body : []).map((r: any) => [String(field(r, 'Name')), String(field(r, 'Status'))]) },
    });
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
