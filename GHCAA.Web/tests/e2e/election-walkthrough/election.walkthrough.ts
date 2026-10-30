import { Browser, Locator, Page, expect, test } from '@playwright/test';
import fs from 'node:fs';
import path from 'node:path';
import { ApiError, call, download, field, forget, login, send, token, twoPerson } from './api';
import { local, Person, STATE_FILE } from './local';
import { FILES_DIR, Kind, Report, SHOTS_DIR } from './report';
import { Actor, WINDOW_VIEWPORT, shows } from './web';

// 84.52. One full election on the local stack, from setup to archive, with a screenshot
// for every step a person takes on the web and a report entry for every API step.
//   WALKTHROUGH_STAGE=setup   stop after the campaign phase and save a checkpoint
//   WALKTHROUGH_FRESH=1       ignore the checkpoint and start again
//   WALKTHROUGH_KEEP=1        leave the borrowed accounts on the test password too
//   WALKTHROUGH_DEMO=1        move the polling dates to now in the database instead of waiting
//   WALKTHROUGH_SCENARIO=...  another scenario file in place of scenario.json

interface Candidate { member: number; proposer: number; seconder: number; statement: string; withdraws?: boolean; rejectedBecause?: string }
interface Scenario {
  title: string;
  description: string;
  periodTitle: string;
  periodYears: number;
  timing: { pollingOpensInMinutes: number; pollingMinutes: number; recountWindowMinutes: number; silenceMinutesBeforePolling: number };
  details: { venue: string; noticeReference: string };
  admin: number;
  officials: {
    returningOfficer: number; chiefCommissioner: number; commissioner: number; tribunal: number;
    scrutineerWhoDeclines: number; replacementScrutineer: number; observerWhoIsRevoked: number;
  };
  seats: { positionId: number; title: string; candidates: Candidate[] }[];
  selfProposalProbe: { member: number; positionId: number };
  votes: { voter: number; web?: boolean; choices: Record<string, number> }[];
  recounts: { positionId: number; applicant: number; grounds: string; explanation: string; allow: boolean; refusedBecause?: string }[];
}

interface Checkpoint {
  electionId: number;
  periodId: number;
  title: string;
  seatIds: Record<string, number>;
  seatTitles: Record<string, string>;
  nominations: Record<string, number>;
  keyFile: string;
  opens: string;
  closes: string;
  recountEnds: string;
}

const SCENARIO_FILE = process.env['WALKTHROUGH_SCENARIO']
  ? path.resolve(process.env['WALKTHROUGH_SCENARIO'])
  : path.join(__dirname, 'scenario.json');
const S: Scenario = JSON.parse(fs.readFileSync(SCENARIO_FILE, 'utf-8'));
const CHECKPOINT = path.join(path.dirname(STATE_FILE), 'walkthrough-checkpoint.json');
const SETUP_ONLY = process.env['WALKTHROUGH_STAGE'] === 'setup';
const FRESH = !!process.env['WALKTHROUGH_FRESH'];
const KEEP = !!process.env['WALKTHROUGH_KEEP'];
const DEMO = !!process.env['WALKTHROUGH_DEMO'];

// purge finds anything left by a crashed run through this suffix, so both titles carry it.
const SUFFIX = ' (local walkthrough)';

const PHASES = ['Announced', 'Nomination', 'Scrutiny', 'Withdrawal', 'CandidateList', 'Campaign', 'Polling', 'Counting', 'Declared', 'Archived'];
const SCRUTINY_ITEMS = ['MembershipVerified', 'VotingMember', 'MinimumMembershipPeriod', 'FoundingMemberRequirement',
  'DocumentsComplete', 'NominationFeePaid', 'DeclarationSigned'];

const O = S.officials;
const allUsers = [...new Set([
  S.admin, ...Object.values(O),
  ...S.seats.flatMap(s => s.candidates.flatMap(c => [c.member, c.proposer, c.seconder])),
  ...S.votes.map(v => v.voter), ...S.recounts.map(r => r.applicant),
])];

const sleep = (ms: number) => new Promise(r => setTimeout(r, ms));
const iso = (d: Date) => d.toISOString().replace(/\.\d{3}Z$/, 'Z');
const utcDay = (d: Date) => d.toISOString().slice(0, 10);
// datetime-local inputs take the browser's local time, and the browser runs on this machine.
const localInput = (d: Date) => {
  const p = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}T${p(d.getHours())}:${p(d.getMinutes())}`;
};
const escapeRe = (s: string) => s.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');

test('election walkthrough', async ({ browser, baseURL }) => {
  // A web dev server left running across an API restart can answer every /api call with
  // a 500 from its proxy. The browser login then waits forever with nothing in the report,
  // so check the proxy before any account is taken over.
  const viaWeb = await fetch(`${baseURL}/api/config`).catch(() => null);
  if (!viaWeb?.ok) {
    throw new Error(`The web server at ${baseURL} cannot reach the API (GET /api/config gave ${viaWeb?.status ?? 'no answer'}). `
      + 'Restart it with npm run start in GHCAA.Web, then run again.');
  }
  if (FRESH) fs.rmSync(CHECKPOINT, { force: true });
  const resume = !SETUP_ONLY && fs.existsSync(CHECKPOINT);
  if (!resume) local.purge(true);

  const report = new Report(`${S.title}: local walkthrough`, resume);
  local.takeOver(allUsers);
  const people = local.who(allUsers);
  const P = (id: number) => {
    const p = people.get(id);
    if (!p) throw new Error(`user ${id} from the scenario was not found`);
    return p;
  };
  const admin = P(S.admin);
  const ro = P(O.returningOfficer);
  const cc = P(O.chiefCommissioner);
  const comm = P(O.commissioner);
  const tribunal = P(O.tribunal);
  const actors: Actor[] = [];
  let failed = false;

  const open = async (person: Person, role: string) => {
    const a = await Actor.open(browser, person, role, report);
    actors.push(a);
    return a;
  };
  const closeActor = async (a: Actor) => {
    actors.splice(actors.indexOf(a), 1);
    await a.close().catch(() => {});
  };

  const note = (kind: Kind, actor: string, title: string, detail?: string, extra: Partial<{ table: { head: string[]; rows: (string | number)[][] }; links: { label: string; href: string }[]; ok: boolean }> = {}) =>
    report.add({ kind, actor, title, detail, ok: extra.ok ?? true, table: extra.table, links: extra.links });

  // A probe passes when the API refuses. A 2xx answer is a bug in the rules.
  const probe = async (person: Person, title: string, method: string, url: string, body: unknown, expected: number[], stepped = true) => {
    const bearer = stepped ? await token(person) : await login(person);
    const r = await send(method, url, bearer, body);
    const ok = expected.includes(r.status);
    const why = typeof r.body === 'object' ? field<string>(r.body, 'Detail') ?? field<string>(r.body, 'Title') ?? '' : String(r.body ?? '');
    note('refusal', person.name, title, `${method} ${url} answered ${r.status}${why ? `: ${why}` : ''}. Expected ${expected.join(' or ')}.`, { ok });
    if (!ok) report.bug(`${title}: the API answered ${r.status} where ${expected.join(' or ')} was expected.`);
    return r;
  };

  const adminElection = async (eid: number) => {
    const list = (await call<any[]>('GET', '/admin/elections', await token(admin))).body;
    const e = list.find(x => field(x, 'Id') === eid);
    if (!e) throw new Error(`election ${eid} is not in the admin list`);
    return e;
  };
  const phaseOf = async (eid: number) => String(field(await adminElection(eid), 'Phase'));

  const waitUntil = async (when: Date, what: string) => {
    const left = when.getTime() - Date.now();
    if (left <= 0) return;
    const about = left < 60_000 ? `${Math.ceil(left / 1000)} seconds` : `${Math.ceil(left / 60_000)} minutes`;
    note('wait', 'Run', `Waiting for ${what}`, `About ${about}, until ${when.toLocaleTimeString()}.`);
    await sleep(left + 3_000);
  };

  /** The card for this election on the admin or officials elections page. */
  const card = (page: Page, title: string): Locator =>
    page.locator('.glass-card').filter({ has: page.getByRole('heading', { name: title, exact: true }) }).first();

  const toast = async (page: Page, text: RegExp, timeout = 15_000) =>
    page.getByText(text).first().waitFor({ timeout }).then(() => true, () => false);

  /**
   * The approver approves the newest open request on the officials dashboard. When the
   * dashboard shows nothing to approve, the API list is used and the report says so.
   */
  const approveOnDashboard = async (approver: Actor, eid: number, what: string) => {
    const page = approver.page;
    await approver.goto('/officials');
    const pick = page.locator('#officials-election');
    const box = page.locator('[data-card=approvals]');
    // The picker shows only with more than one election, so wait for whichever comes first.
    await shows(pick.or(box).first(), 5_000);
    if (await pick.isVisible()) await pick.selectOption(String(eid));
    const load = box.getByRole('button', { name: 'Load approvals' });
    if (await shows(load)) await load.click();
    const row = box.locator('[data-row=approval]').filter({ has: page.getByRole('button', { name: 'Approve' }) }).first();
    if (await shows(row)) {
      await approver.shot(`${what}: waiting for a second person`, 'The request shows on the dashboard of an official who can approve it.');
      await row.getByRole('button', { name: 'Approve' }).click();
      const gone = await expect(row).toBeHidden({ timeout: 20_000 }).then(() => true, () => false);
      if (!gone) note('check', approver.person.name, `${what}: request still on the dashboard after Approve`, undefined, { ok: false });
      await approver.shot(`${what}: approved`);
      return;
    }
    const bearer = await token(approver.person);
    const open = (await call<any[]>('GET', `/elections/${eid}/approvals`, bearer)).body
      .filter(a => !field(a, 'ApprovedAt') && !field(a, 'RequestedByMe'));
    if (open.length === 0) throw new Error(`${what}: nothing is waiting for approval`);
    await call('POST', `/elections/approvals/${field(open[open.length - 1], 'Id')}/approve`, bearer);
    note('api', approver.label, `${what}: approved through the API`, 'The dashboard did not show the request, so the approval went through the API.');
    report.gap(`${what}: the request did not show on the officials dashboard for ${approver.label}.`);
  };

  /**
   * Moves the election on with the asker's "Advance to" button. Two-person phases wait for
   * the approver. Without the button the phase goes through the API and the report says so.
   */
  const toPhase = async (eid: number, title: string, n: number, asker: Actor, approver: Actor) => {
    await asker.goto(`/officials/elections/${eid}`);
    const button = card(asker.page, title).locator('[data-action=advance-phase]');
    if (await shows(button)) {
      await button.click();
      await asker.confirm(`Moving to ${PHASES[n]}`);
      await shows(asker.page.getByText(/second person must approve|Moved to/).first(), 60_000);
      await asker.shot(`Move to ${PHASES[n]} requested`);
    } else {
      const r = await call('POST', `/elections/${eid}/phase`, await token(asker.person), n);
      note('api', asker.label, `Phase ${n}, ${PHASES[n]}, requested`, `POST /elections/${eid}/phase answered ${r.status}.`);
      report.gap(`${asker.label} had no "Advance to" button for ${PHASES[n]}. The phase was changed through the API.`);
    }
    if (await phaseOf(eid) !== PHASES[n]) await approveOnDashboard(approver, eid, `Move to ${PHASES[n]}`);
    expect(await phaseOf(eid), `phase after asking for ${PHASES[n]}`).toBe(PHASES[n]);
    report.reachedPhase(PHASES[n]);
  };

  report.setPhases(PHASES);
  let cp: Checkpoint;

  try {
    if (resume) {
      cp = JSON.parse(fs.readFileSync(CHECKPOINT, 'utf-8'));
      report.section('Resumed from the setup checkpoint', `Election ${cp.electionId} was set up by an earlier run. Polling opens ${new Date(cp.opens).toLocaleString()}.`);
    } else {
      cp = await setup();
      fs.writeFileSync(CHECKPOINT, JSON.stringify(cp, null, 2), 'utf-8');
      if (SETUP_ONLY) {
        report.section('Checkpoint saved', `Run again without WALKTHROUGH_STAGE to carry on from the campaign phase. Polling opens ${new Date(cp.opens).toLocaleString()}.`);
        report.finish('setup done, checkpoint saved');
        return;
      }
    }
    await polling(cp);
    report.finish('finished');
  } catch (err) {
    failed = true;
    const msg = err instanceof ApiError ? err.message : String((err as Error)?.stack ?? err).slice(0, 1500);
    note('check', 'Run', 'The run stopped', msg, { ok: false });
    for (const a of actors) await a.shot('Screen when the run stopped', undefined, 'check', false).catch(() => {});
    report.finish('stopped with an error');
    throw err;
  } finally {
    for (const a of [...actors]) await closeActor(a);
    // The election stays in the database to look at afterwards. The next fresh run deletes it,
    // because EC periods cannot overlap, and so does scripts\clean-election-test-data.bat.
    const keepForResume = SETUP_ONLY && !failed;
    if (!KEEP && !keepForResume) {
      try { local.restore(); } catch (e) { report.bug(`Account restore failed: ${(e as Error).message}`); }
      fs.rmSync(CHECKPOINT, { force: true });
    }
  }

  // ---------------------------------------------------------------------------
  // Sections 1 to 8: from an empty slate to the campaign phase with a ballot key.
  async function setup(): Promise<Checkpoint> {
    const now = new Date();
    const opens = new Date(now.getTime() + S.timing.pollingOpensInMinutes * 60_000);
    const closes = new Date(opens.getTime() + S.timing.pollingMinutes * 60_000);
    const recountEnds = new Date(closes.getTime() + S.timing.recountWindowMinutes * 60_000);
    const title = `${S.title} ${now.getFullYear()}${SUFFIX}`;

    report.setFacts([
      ['Election', title],
      ['Scenario file', path.basename(SCENARIO_FILE)],
      ['Nominations open', now.toLocaleString()],
      ['Polling opens', opens.toLocaleString()],
      ['Polling closes', closes.toLocaleString()],
      ['Recount window ends', recountEnds.toLocaleString()],
      ['Seats', S.seats.map(s => s.title).join(', ')],
      ['Ballots planned', `${S.votes.length}, of which ${S.votes.filter(v => v.web).length} on the web`],
    ]);
    const castRows: (string | number)[][] = [
      ['Administrator', admin.name, admin.username, 'Creates the period and the election, freezes the roll, appoints the officials'],
      ['Returning Officer', ro.name, ro.username, 'Makes the ballot key, counts, enters recount figures'],
      ['Chief Commissioner', cc.name, cc.username, 'Publishes, moves the phases, closes polling, appoints a replacement'],
      ['Commissioner', comm.name, comm.username, 'Second person on every two-person step'],
      ['Tribunal', tribunal.name, tribunal.username, 'Decides the recount requests'],
      ['Scrutineer', P(O.scrutineerWhoDeclines).name, P(O.scrutineerWhoDeclines).username, 'Declines the appointment'],
      ['Scrutineer', P(O.replacementScrutineer).name, P(O.replacementScrutineer).username, 'Replaces the one who declined'],
      ['Observer', P(O.observerWhoIsRevoked).name, P(O.observerWhoIsRevoked).username, 'Is appointed, then revoked'],
    ];
    for (const seat of S.seats) for (const c of seat.candidates) {
      const fate = c.withdraws ? ', withdraws' : c.rejectedBecause ? ', rejected at scrutiny' : '';
      castRows.push([`Candidate, ${seat.title}`, P(c.member).name, P(c.member).username, `Proposed by ${P(c.proposer).name}, seconded by ${P(c.seconder).name}${fate}`]);
    }
    for (const v of S.votes) castRows.push(['Voter', P(v.voter).name, P(v.voter).username, v.web ? 'Votes on the web' : 'Votes through the API']);
    report.setCast(castRows);

    // 1. Period and election
    report.section('Executive committee period and the election', 'The election fills the seats of a committee period, so the period comes first. Both are made through the API because the run needs their ids.');
    const a = await token(admin);
    let start = new Date(now.getTime() + 30 * 86_400_000);
    for (const p of (await call<any[]>('GET', '/admin/governance/periods', a)).body) {
      const begins = new Date(field<string>(p, 'StartDate'));
      let ends = field<string | null>(p, 'EndDate');
      if (ends == null) {
        if (!String(field(p, 'Title')).endsWith(SUFFIX)) throw new Error(`period ${field(p, 'Id')} has no end date, so no new period can follow it`);
        ends = utcDay(new Date(begins.getTime() + 86_400_000));
        await call('PUT', `/admin/governance/periods/${field(p, 'Id')}`, a,
          { Title: field(p, 'Title'), StartDate: utcDay(begins), EndDate: ends, IsActive: field(p, 'IsActive') });
      }
      const after = new Date(new Date(ends).getTime() + 2 * 86_400_000);
      if (after > start) start = after;
    }
    const end = new Date(start);
    end.setUTCFullYear(end.getUTCFullYear() + S.periodYears);
    const period = (await call('POST', '/admin/governance/periods', a,
      { Title: `${S.periodTitle} ${start.getUTCFullYear()}-${end.getUTCFullYear()}${SUFFIX}`, StartDate: utcDay(start), EndDate: utcDay(end) })).body;
    const periodId = field<number>(period, 'Id');
    local.record('period', periodId);
    note('api', admin.name, 'Committee period created', `${S.periodTitle}, ${utcDay(start)} to ${utcDay(end)}. Periods may not overlap, so it starts after the last one on record.`);

    const created = (await call('POST', '/admin/elections', a, {
      Title: title,
      ECPeriodId: periodId,
      NominationOpensOn: iso(new Date(now.getTime() - 60 * 60_000)),
      NominationClosesOn: iso(opens),
      PollingOpensOn: iso(opens),
      PollingClosesOn: iso(closes),
      Description: S.description,
      Positions: S.seats.map(s => ({ Title: '', Seats: 1, PositionId: s.positionId })),
      IsTest: false,
    })).body;
    const eid = field<number>(created, 'Id');
    local.record('election', eid);
    const listed = await adminElection(eid);
    const positions = field<any[]>(listed, 'Positions');
    const seatIds: Record<string, number> = {};
    const seatTitles: Record<string, string> = {};
    S.seats.forEach((s, i) => {
      seatIds[s.positionId] = field<number>(positions[i], 'Id');
      seatTitles[s.positionId] = field<string>(positions[i], 'Title') || s.title;
    });
    note('api', admin.name, 'Election created', `Election ${eid} in the Announced phase.`, {
      table: { head: ['Seat', 'Places', 'Seat id'], rows: S.seats.map(s => [seatTitles[s.positionId], 1, seatIds[s.positionId]]) },
    });
    report.reachedPhase('Announced');

    const adminWeb = await open(admin, 'Administrator');
    await adminWeb.goto('/admin/elections');
    const adminCard = card(adminWeb.page, title);
    await expect(adminCard).toBeVisible({ timeout: 20_000 });
    await adminWeb.shot('The new election on the admin elections page', 'No ballot key yet, so the card says voting cannot open.');

    // 2. Voter roll and officials
    report.section('Voter roll and officials', 'The roll is frozen so the electorate is fixed. Officials are appointed by persona; each one has to accept before they can act.');
    await adminCard.getByRole('button', { name: 'Freeze voter roll' }).click();
    await toast(adminWeb.page, /voters frozen onto the roll/);
    await adminWeb.shot('Voter roll frozen', 'The toast gives the number of voters on the roll.');

    const personas = (await call<any[]>('GET', '/admin/election-personas', a)).body;
    const persona = (re: RegExp, not: RegExp | null, fallback: number) =>
      personas.find(p => re.test(field(p, 'Name')) && !(not && not.test(field(p, 'Name'))))
      ?? personas.find(p => field(p, 'Id') === fallback);
    const plan: { who: Person; persona: any; returning?: boolean }[] = [
      { who: ro, persona: persona(/returning/i, null, 4), returning: true },
      { who: cc, persona: persona(/chief/i, null, 2) },
      { who: comm, persona: persona(/commissioner/i, /chief/i, 3) },
      { who: tribunal, persona: persona(/tribunal/i, null, 14) },
      { who: P(O.scrutineerWhoDeclines), persona: persona(/scrutineer/i, null, 8) },
      { who: P(O.observerWhoIsRevoked), persona: persona(/observer/i, null, 13) },
    ];
    await adminCard.getByRole('button', { name: 'Officials', exact: true }).click();
    for (const step of plan) await appoint(adminWeb, eid, step.who, field(step.persona, 'Name'), !!step.returning);
    await adminWeb.shot('Officials appointed', 'Each one shows "Not accepted yet" until they accept.');

    // 3. Officials accept or decline
    report.section('Officials accept or decline', 'Each official signs the declaration on their own appointments page. One scrutineer declines, so the Chief Commissioner appoints another. The observer is revoked.');
    const ccWeb = await open(cc, 'Chief Commissioner');
    const commWeb = await open(comm, 'Commissioner');
    const roWeb = await open(ro, 'Returning Officer');
    const tribunalWeb = await open(tribunal, 'Tribunal');
    for (const w of [roWeb, ccWeb, commWeb, tribunalWeb]) await answerAppointment(w, eid, true);
    const observerWeb = await open(P(O.observerWhoIsRevoked), 'Observer');
    await answerAppointment(observerWeb, eid, true);
    const declinerWeb = await open(P(O.scrutineerWhoDeclines), 'Scrutineer');
    await answerAppointment(declinerWeb, eid, false, 'I am travelling during the polling week.');
    await closeActor(declinerWeb);

    await probe(P(O.observerWhoIsRevoked), 'An observer cannot change the phase', 'POST', `/elections/${eid}/phase`, 1, [403]);

    await ccWeb.goto(`/officials/elections/${eid}`);
    const ccCard = card(ccWeb.page, title);
    const officialsButton = ccCard.getByRole('button', { name: 'Officials', exact: true });
    const replacement = P(O.replacementScrutineer);
    const appointer = await shows(officialsButton) ? ccWeb : adminWeb;
    if (appointer === adminWeb) report.gap('The Chief Commissioner has no Officials button, so the administrator appointed the replacement scrutineer.');
    else await officialsButton.click();
    await appoint(appointer, eid, replacement, field(persona(/scrutineer/i, null, 8), 'Name'), false);
    const replacementWeb = await open(replacement, 'Scrutineer');
    await answerAppointment(replacementWeb, eid, true);
    await closeActor(replacementWeb);

    const row = appointer.page.locator('.stack.gap-2').filter({ hasText: P(O.observerWhoIsRevoked).name })
      .filter({ has: appointer.page.getByRole('button', { name: 'Revoke' }) }).last();
    await row.getByRole('button', { name: 'Revoke' }).click();
    await appointer.confirm('Revoking the observer');
    await toast(appointer.page, /Appointment revoked/);
    await appointer.shot('Observer revoked', 'The revoked appointment moves to the "Revoked" list.');
    await probe(P(O.observerWhoIsRevoked), 'A revoked observer cannot see the approvals', 'GET', `/elections/${eid}/approvals`, undefined, [401, 403]);
    await closeActor(observerWeb);

    // 4. Publish
    report.section('Publishing the election', 'Publishing opens nominations. It is a two-person step: the Chief Commissioner asks, the Commissioner approves.');
    await ccWeb.goto(`/officials/elections/${eid}`);
    const publish = card(ccWeb.page, title).getByRole('button', { name: 'Publish' });
    if (await shows(publish)) {
      await ccWeb.shot('Before publishing', 'The election is in the Announced phase.');
      await publish.click();
      await toast(ccWeb.page, /second person must approve|published/i);
      await ccWeb.shot('Publish requested');
    } else {
      report.gap('The Chief Commissioner has no Publish button, so the administrator published.');
      await adminWeb.goto('/admin/elections');
      await card(adminWeb.page, title).getByRole('button', { name: 'Publish' }).click();
      await toast(adminWeb.page, /second person must approve|published/i);
      await adminWeb.shot('Publish requested');
    }
    if (await phaseOf(eid) !== 'Nomination') await approveOnDashboard(commWeb, eid, 'Publish');
    expect(await phaseOf(eid)).toBe('Nomination');
    report.reachedPhase('Nomination');
    await (await anonymous()).show('/elections', 'The public elections page after publishing');

    // 5. Nominations
    report.section('Nominations', 'Each proposer files a nomination on the member election page, naming the candidate and a seconder.');
    for (const seat of S.seats) for (const c of seat.candidates) {
      const proposerWeb = await open(P(c.proposer), 'Proposer');
      await fileNomination(proposerWeb, seatTitles[seat.positionId], P(c.member), P(c.seconder), c.statement);
      await closeActor(proposerWeb);
    }
    const filed = (await call<any[]>('GET', `/elections/${eid}/nominations`, await token(admin))).body;
    const nominations: Record<string, number> = {};
    const nominationRows: (string | number)[][] = [];
    for (const seat of S.seats) for (const c of seat.candidates) {
      const n = filed.find(x => field(x, 'CandidateMemberId') === P(c.member).memberId && field(x, 'ElectionSeatId') === seatIds[seat.positionId]);
      expect(n, `nomination for ${P(c.member).name}`).toBeTruthy();
      nominations[c.member] = field<number>(n, 'Id');
      nominationRows.push([seat.title, P(c.member).name, P(c.proposer).name, P(c.seconder).name, nominations[c.member], String(field(n, 'Status'))]);
    }
    note('check', 'Run', 'Nominations on file', undefined, { table: { head: ['Seat', 'Candidate', 'Proposer', 'Seconder', 'Nomination', 'Status'], rows: nominationRows } });

    const self = P(S.selfProposalProbe.member);
    await probe(self, 'A member cannot propose themselves', 'POST', `/elections/${eid}/nominations`, {
      ElectionSeatId: seatIds[S.selfProposalProbe.positionId], CandidateMemberId: self.memberId,
      ProposerMemberId: self.memberId, SeconderMemberId: P(S.seats[0].candidates[0].seconder).memberId,
      Statement: 'Self proposal check.', PhotoPath: null,
    }, [400, 409], false);

    // The input has no seconds, so a time this minute could land after the server's "now".
    const consentAt = new Date(Date.now() - 120_000);
    await openScrutiny(adminWeb, '/admin/elections', title);
    for (const id of Object.values(nominations)) {
      const box = adminWeb.page.locator(`[data-nomination="${id}"]`);
      await box.locator(`#consent-at-${id}`).fill(localInput(consentAt));
      await box.getByRole('button', { name: 'Save consent time' }).click();
      await adminWeb.page.waitForLoadState('networkidle');
    }
    await adminWeb.shot('Candidate consent recorded', 'Each nomination shows the time the candidate signed the consent form.');
    const consentSaved = async () => {
      const rows = (await call<any[]>('GET', `/admin/elections/${eid}/nominations`, await token(admin))).body;
      return Object.values(nominations).every(id => !!field(rows.find(x => field(x, 'Id') === id), 'ConsentAt'));
    };
    await expect.poll(consentSaved, { timeout: 30_000 }).toBe(true).catch(() => {});
    note('check', admin.name, 'Consent saved on every nomination', undefined, { ok: await consentSaved() });
    await probe(admin, 'Consent cannot be dated in the future', 'PUT', `/admin/elections/${eid}/nominations/${Object.values(nominations)[0]}/consent`,
      { ConsentAt: iso(new Date(Date.now() + 86_400_000)) }, [400]);

    await fillFormDetails(adminWeb, title, eid, opens, closes, recountEnds);

    // 6. Scrutiny
    report.section('Scrutiny', 'The scrutiny checklist is ticked for each nomination, then each one is accepted or rejected with a reason.');
    await toPhase(eid, title, 2, ccWeb, commWeb);
    let decider: Actor | null = null;
    const firstId = Object.values(nominations)[0];
    for (const [who, route] of [[roWeb, `/officials/elections/${eid}`], [ccWeb, `/officials/elections/${eid}`], [adminWeb, '/admin/elections']] as const) {
      await openScrutiny(who, route, title);
      if (await shows(who.page.locator(`[data-nomination="${firstId}"]`).getByRole('button', { name: 'Accept', exact: true }), 5_000)) {
        decider = who;
        break;
      }
    }
    if (decider !== roWeb) report.gap(`The Returning Officer had no Accept or Reject buttons on the scrutiny tab${decider ? `, so ${decider.label} decided` : ''}.`);

    const checkRows: (string | number)[][] = [];
    const decisions: (string | number)[][] = [];
    if (decider) {
      for (const seat of S.seats) for (const c of seat.candidates) {
        if (c.rejectedBecause) continue;
        const box = decider.page.locator(`[data-nomination="${nominations[c.member]}"]`);
        await box.locator('details summary').click();
        // Each tick saves on its own and the boxes stay disabled until it has, so check() waits.
        const ticks = box.locator('details input[type=checkbox]');
        const count = await ticks.count();
        for (let i = 0; i < count; i++) await ticks.nth(i).check({ timeout: 60_000 });
        await decider.page.waitForLoadState('networkidle');
        checkRows.push([P(c.member).name, count]);
      }
      await decider.shot('Scrutiny checklist ticked', 'Each nomination that passes has every item ticked.');

      for (const seat of S.seats) for (const c of seat.candidates) {
        const accepted = !c.rejectedBecause;
        const reason = c.rejectedBecause ?? 'Papers complete and in order.';
        const box = decider.page.locator(`[data-nomination="${nominations[c.member]}"]`);
        await box.locator(`#scrutiny-reason-${nominations[c.member]}`).fill(reason);
        if (!accepted) await decider.shot(`Rejecting ${P(c.member).name}`, 'Reject stays disabled until a reason is typed.');
        await box.getByRole('button', { name: accepted ? 'Accept' : 'Reject', exact: true }).click();
        await expect(box.getByRole('button', { name: 'Accept', exact: true })).toHaveCount(0, { timeout: 60_000 });
        decisions.push([P(c.member).name, accepted ? 'Accepted' : 'Rejected', reason, decider.label]);
      }
      await decider.shot('Nominations decided');
    } else {
      for (const seat of S.seats) for (const c of seat.candidates) {
        if (c.rejectedBecause) continue;
        for (const item of SCRUTINY_ITEMS) {
          await call('PUT', `/admin/elections/${eid}/nominations/${nominations[c.member]}/scrutiny-checks`, await token(admin), { Item: item, IsChecked: true });
        }
        checkRows.push([P(c.member).name, SCRUTINY_ITEMS.length]);
      }
      for (const seat of S.seats) for (const c of seat.candidates) {
        const accepted = !c.rejectedBecause;
        const reason = c.rejectedBecause ?? 'Papers complete and in order.';
        const by = await firstAllowed([ro, cc, admin], p => call('POST', `/elections/nominations/${nominations[c.member]}/scrutiny`, p, { Accepted: accepted, Reason: reason }));
        decisions.push([P(c.member).name, accepted ? 'Accepted' : 'Rejected', reason, `${by.name}, through the API`]);
      }
    }
    note('check', 'Scrutiny', 'Scrutiny checklist ticked', undefined, { table: { head: ['Candidate', 'Items ticked'], rows: checkRows } });
    const decided = (await call<any[]>('GET', `/admin/elections/${eid}/nominations`, await token(admin))).body;
    note('check', 'Scrutiny', 'Nominations decided', undefined, {
      ok: S.seats.every(seat => seat.candidates.every(c =>
        field(decided.find(x => field(x, 'Id') === nominations[c.member]), 'Status') === (c.rejectedBecause ? 'Rejected' : 'Accepted'))),
      table: { head: ['Candidate', 'Decision', 'Reason', 'Decided by'], rows: decisions },
    });
    await commWeb.goto('/officials');
    await commWeb.shot('Officials dashboard after scrutiny', 'Nothing should be waiting in the nominations card.');

    // 7. Withdrawal, candidate list, campaign
    report.section('Withdrawal, candidate list and campaign', 'A candidate may withdraw while the withdrawal phase is open. The list is then final.');
    await toPhase(eid, title, 3, ccWeb, commWeb);
    for (const seat of S.seats) for (const c of seat.candidates) {
      if (!c.withdraws) continue;
      const candidateWeb = await open(P(c.member), 'Candidate');
      await candidateWeb.goto('/portal/election');
      const row = candidateWeb.page.locator(`[data-section=my-nominations] [data-nomination="${nominations[c.member]}"]`);
      const withdraw = row.getByRole('button', { name: 'Withdraw' });
      if (await shows(withdraw, 20_000)) {
        await candidateWeb.shot(`${P(c.member).name} before withdrawing`, 'The candidate sees their own nominations with a Withdraw button.');
        await withdraw.click();
        await candidateWeb.confirm(`${P(c.member).name} confirms the withdrawal`);
        await shows(candidateWeb.page.getByText(/nomination was withdrawn/).first(), 60_000);
        await candidateWeb.shot(`${P(c.member).name} withdrew from ${seat.title}`);
      } else {
        await call('POST', `/elections/nominations/${nominations[c.member]}/withdraw`, await token(P(c.member)));
        note('api', P(c.member).name, `${P(c.member).name} withdraws from ${seat.title}`);
        report.gap(`${P(c.member).name} had no Withdraw button on the election page. The withdrawal went through the API.`);
      }
      await closeActor(candidateWeb);
    }
    await toPhase(eid, title, 4, ccWeb, commWeb);
    await toPhase(eid, title, 5, ccWeb, commWeb);
    await (await anonymous()).show('/elections', 'The public elections page during the campaign');

    // 8. Ballot key and readiness
    report.section('Ballot key and readiness', 'The Returning Officer makes the key pair in the browser. Only the public half goes to the server; the private half is the file the count needs.');
    const keyFile = path.join(FILES_DIR, 'officer-key.pem');
    await probe(cc, 'Polling cannot open without a ballot key', 'POST', `/elections/${eid}/phase`, 6, [400, 409]);
    await roWeb.goto(`/officials/elections/${eid}`);
    const roCard = card(roWeb.page, title);
    await roCard.getByRole('button', { name: /Make officer key/ }).click();
    await roWeb.download(() => roWeb.confirm('The key warning'), keyFile);
    await toast(roWeb.page, /Key stored|second person must approve/i);
    await roWeb.shot('Officer key made', 'The private key file was saved on the officer\'s machine.');
    if (!field(await adminElection(eid), 'BallotKeyFingerprint')) await approveOnDashboard(ccWeb, eid, 'Ballot key');
    const fingerprint = field<string>(await adminElection(eid), 'BallotKeyFingerprint');
    expect(fingerprint, 'ballot key fingerprint').toBeTruthy();
    note('check', 'Run', 'Ballot key stored', `Fingerprint ${fingerprint}.`);

    await ccWeb.goto('/officials');
    const pick = ccWeb.page.locator('#officials-election');
    if (await shows(pick, 5_000)) await pick.selectOption(String(eid));
    await ccWeb.shot('Readiness before the attestations');
    const attest = ccWeb.page.locator('[data-card=readiness]').getByRole('button', { name: 'Record as done' });
    // Each click reloads the list. Clicking before that reload lands can catch a button that a
    // late reply then takes away, so wait for it, and stop when the button has gone.
    for (let i = 0; i < 10 && await shows(attest.first(), i === 0 ? 15_000 : 5_000); i++) {
      const reloaded = ccWeb.page.waitForResponse(
        r => r.request().method() === 'GET' && r.url().endsWith(`/elections/${eid}/readiness`), { timeout: 15_000 }).catch(() => {});
      if (!await attest.first().click({ timeout: 10_000 }).then(() => true, () => false)) break;
      await reloaded;
    }
    const ready = (await call('GET', `/elections/${eid}/readiness`, await token(admin))).body;
    for (const item of field<any[]>(ready, 'Items')) {
      if (field(item, 'Met') || !field(item, 'Attestation')) continue;
      await send('POST', `/elections/${eid}/readiness/attest`, await token(admin), { Condition: field(item, 'Key') });
      report.gap(`Readiness item ${field(item, 'Key')} had no "Record as done" button for the Chief Commissioner. The administrator recorded it through the API.`);
    }
    await ccWeb.goto('/officials');
    await ccWeb.shot('Readiness after the attestations', 'Every item should now be met.');
    const after = (await call('GET', `/elections/${eid}/readiness`, await token(admin))).body;
    note('check', 'Run', 'Readiness for polling', undefined, {
      ok: !!field(after, 'Ready'),
      table: { head: ['Item', 'Met', 'Advisory'], rows: field<any[]>(after, 'Items').map(i => [field(i, 'Key'), field(i, 'Met') ? 'yes' : 'no', field(i, 'Advisory') ? 'yes' : 'no']) },
    });

    return {
      electionId: eid, periodId, title, seatIds, seatTitles, nominations, keyFile,
      opens: opens.toISOString(), closes: closes.toISOString(), recountEnds: recountEnds.toISOString(),
    };
  }

  // ---------------------------------------------------------------------------
  // Sections 9 to 16: polling, count, declare, recounts, forms, archive.
  async function polling(c: Checkpoint): Promise<void> {
    const eid = c.electionId;
    let opens = new Date(c.opens);
    let closes = new Date(c.closes);
    let recountEnds = new Date(c.recountEnds);
    const nominationOf = (member: number) => c.nominations[member];

    // The server takes a ballot only after the opening time and closes the poll only after the
    // closing time, so a demo cannot skip the waits. It moves the dates up to now instead.
    const move = (to: { opens?: Date; closes?: Date; recountEnds?: Date }) => {
      opens = to.opens ?? opens;
      closes = to.closes ?? closes;
      recountEnds = to.recountEnds ?? recountEnds;
      const silence = new Date(opens.getTime() - S.timing.silenceMinutesBeforePolling * 60_000);
      local.window(eid, { opens, closes, 'recount-ends': recountEnds, silence });
    };
    if (DEMO) {
      // Far enough ahead that opening the poll and casting the ballots fit inside the window.
      move({ opens: new Date(Date.now() + 60_000), closes: new Date(Date.now() + 30 * 60_000), recountEnds: new Date(Date.now() + 40 * 60_000) });
      note('wait', 'Run', 'Demo mode: the polling dates are moved to now',
        'At each wait the run writes the polling dates straight to the database, so the demo does not sit through them. The audit trail does not show these moves.');
    }

    const ccWeb = await open(cc, 'Chief Commissioner');
    const commWeb = await open(comm, 'Commissioner');
    const roWeb = await open(ro, 'Returning Officer');

    // 9. Polling opens
    report.section('Polling opens', 'Opening the poll is a two-person step. Ballots are refused until the opening time.');
    await toPhase(eid, c.title, 6, ccWeb, commWeb);
    if (DEMO) move({ opens: new Date(Date.now() + 3_000) });
    const early = S.votes.find(v => !v.web)!;
    if (Date.now() < opens.getTime() - (DEMO ? 5_000 : 30_000)) {
      await probe(P(early.voter), 'A ballot before the opening time is refused', 'POST', `/elections/${eid}/vote`, { Seats: [] }, [400, 403, 409]);
    }
    await waitUntil(opens, 'polling to open');

    // 10. Voting
    report.section('Voting', 'Some members vote on the web and the rest through the API. Each ballot gets a tracking code.');
    const tracking: (string | number)[][] = [];
    for (const v of S.votes.filter(x => x.web)) {
      const voter = await open(P(v.voter), 'Voter');
      await voter.goto('/portal/election');
      const page = voter.page;
      await expect(page.getByRole('button', { name: 'Review ballot' })).toBeVisible({ timeout: 20_000 });
      await voter.shot('The ballot', 'Every accepted candidate with their statement. The withdrawn and rejected ones are not listed.');
      for (const seat of S.seats) {
        const group = page.locator('.glass-card[role=group]').filter({ has: page.getByRole('heading', { name: c.seatTitles[seat.positionId], exact: true }) });
        const pickId = v.choices[String(seat.positionId)];
        const label = pickId ? P(pickId).name : 'Abstain on this seat';
        await group.locator('label').filter({ hasText: label }).first().locator('input').click();
      }
      await voter.shot('Choices made');
      await page.getByRole('button', { name: 'Review ballot' }).click();
      await voter.shot('Reviewing the ballot before casting');
      await page.getByRole('button', { name: 'Confirm and cast ballot' }).click();
      // The toast carries the same words, so match the heading only.
      await expect(page.getByRole('heading', { name: 'Your ballot was recorded' })).toBeVisible({ timeout: 30_000 });
      const code = (await page.locator('p', { hasText: 'Tracking code:' }).first().locator('strong').textContent())?.trim() ?? '';
      await voter.shot('Ballot recorded', `Tracking code ${code}.`);
      tracking.push([voter.person.name, 'web', code]);
      await voter.goto('/portal/election');
      await voter.shot('The same member opens the ballot again', 'The page says the member has already voted.');
      await closeActor(voter);
    }
    // One at a time, as the owner asked to keep it. The API now retries two ballots that clash
    // (item 84.53 in docs/TODO.md, the concurrent-vote fix), but that is not tested on PostgreSQL yet.
    for (const v of S.votes.filter(x => !x.web)) tracking.push(await (async () => {
      const bearer = await token(P(v.voter));
      const ballot = (await call('GET', `/elections/${eid}/ballot`, bearer)).body;
      expect(field(ballot, 'IsEligible'), `${P(v.voter).name} on the roll`).not.toBe(false);
      const seats = S.seats.map(s => ({
        ElectionSeatId: c.seatIds[s.positionId],
        NominationIds: v.choices[String(s.positionId)] ? [nominationOf(v.choices[String(s.positionId)])] : [],
      }));
      const r = await call('POST', `/elections/${eid}/vote`, bearer, { Seats: seats });
      return [P(v.voter).name, 'API', field<string>(r.body, 'TrackingCode')];
    })());
    note('api', 'Voters', 'Ballots cast', undefined, { table: { head: ['Voter', 'Channel', 'Tracking code'], rows: tracking } });
    await probe(P(early.voter), 'A second ballot from the same member is refused', 'POST', `/elections/${eid}/vote`,
      { Seats: S.seats.map(s => ({ ElectionSeatId: c.seatIds[s.positionId], NominationIds: [] })) }, [409]);
    await ccWeb.goto('/officials');
    await ccWeb.shot('Turnout on the officials dashboard');

    // 11. Close
    report.section('Polling closes', 'The Chief Commissioner closes the poll after the closing time. The Commissioner approves.');
    if (DEMO) move({ closes: new Date() });
    await waitUntil(closes, 'polling to close');
    await ccWeb.goto(`/officials/elections/${eid}`);
    const closeButton = card(ccWeb.page, c.title).getByRole('button', { name: 'Close', exact: true });
    await closeButton.click();
    await ccWeb.confirm('Closing the poll');
    await toast(ccWeb.page, /second person must approve|closed/i);
    await ccWeb.shot('Close requested');
    if (await phaseOf(eid) !== 'Counting') await approveOnDashboard(commWeb, eid, 'Close polling');
    expect(await phaseOf(eid)).toBe('Counting');
    report.reachedPhase('Counting');

    // 12. Count
    report.section('Count', 'The Returning Officer counts with the private key file. When the settings ask for a second person, the count waits for their approval and the officer counts again with the same file.');
    const countWithFile = async () => {
      await roWeb.goto(`/officials/elections/${eid}`);
      const roCard = card(roWeb.page, c.title);
      await expect(roCard.getByRole('button', { name: 'Count with officer key' })).toBeVisible({ timeout: 15_000 });
      await roCard.locator('input[type=file]').setInputFiles(c.keyFile);
      await toast(roWeb.page, /Count done|second person must approve/i, 60_000);
      return roWeb.page.getByText(/Count done/i).first().isVisible();
    };
    // The server counts at once when no second person is needed, and then no approval row exists.
    const doneAtOnce = await countWithFile();
    await roWeb.shot(doneAtOnce ? 'Count done' : 'First count request', 'The key file was read in the browser and sent with the request.');
    const counted = async () => (await call('GET', `/elections/${eid}/approvals`, await token(ro))).body
      .some((x: any) => field(x, 'Action') === 'Count' && field(x, 'ExecutedByUserId'));
    if (!doneAtOnce && !await counted()) {
      await approveOnDashboard(ccWeb, eid, 'Count');
      await countWithFile();
      await roWeb.shot('Count done', 'The results are stored but not public until the declare.');
    }

    // 13. Declare and results
    report.section('Declare and results', 'Declaring publishes the results. It is a two-person step.');
    await ccWeb.goto(`/officials/elections/${eid}`);
    const declareCard = card(ccWeb.page, c.title);
    const declareButton = declareCard.getByRole('button', { name: 'Declare results', exact: true });
    await expect(declareButton).toBeVisible({ timeout: 15_000 });
    await declareButton.click();
    await ccWeb.confirm('Declare results');
    await expect(ccWeb.page.getByText(/second person must approve|Election results declared/i).first()).toBeVisible({ timeout: 60_000 });
    await ccWeb.shot('Declare requested');
    if (await phaseOf(eid) !== 'Declared') await approveOnDashboard(commWeb, eid, 'Declare results');
    expect(await phaseOf(eid)).toBe('Declared');
    report.reachedPhase('Declared');

    const results = (await call('GET', `/elections/${eid}/results`)).body;
    const expected = new Map<number, number>();
    for (const v of S.votes) for (const member of Object.values(v.choices)) {
      const nid = nominationOf(member);
      expected.set(nid, (expected.get(nid) ?? 0) + 1);
    }
    const resultRows: (string | number)[][] = [];
    let match = true;
    const voteCount = new Map<number, number>();
    for (const seat of field<any[]>(results, 'Seats')) {
      for (const cand of field<any[]>(seat, 'Candidates')) {
        const nid = field<number>(cand, 'NominationId');
        const got = field<number>(cand, 'VoteCount');
        voteCount.set(nid, got);
        const want = expected.get(nid) ?? 0;
        if (got !== want) match = false;
        resultRows.push([field(seat, 'PositionTitle'), field(cand, 'Name'), got, want, field(cand, 'IsElected') ? 'elected' : field(cand, 'IsTie') ? 'tie' : '']);
      }
      resultRows.push([field(seat, 'PositionTitle'), 'Abstentions', field(seat, 'Abstentions'), '', '']);
    }
    note('check', 'Run', 'Declared results match the scenario', `${field(results, 'BallotsCast')} ballots cast from a roll of ${field(results, 'EligibleVoters')}.`, {
      ok: match, table: { head: ['Seat', 'Candidate', 'Counted', 'Expected', 'Outcome'], rows: resultRows },
    });
    if (!match) report.bug('The declared counts differ from the ballots the scenario cast.');
    const visitor = await anonymous();
    await visitor.show('/elections/results', 'Public results page');

    // 14. Recounts
    report.section('Recount requests', 'Requests are recorded inside the recount window. The Tribunal allows or refuses each one, and the Returning Officer enters the recount figures for an allowed one.');
    const tribunalWeb = await open(tribunal, 'Tribunal');
    await openFormRecords(tribunalWeb, eid, c.title);
    for (const rc of S.recounts) {
      const applicant = P(rc.applicant);
      const page = tribunalWeb.page;
      await page.locator(`#recount-seat-${eid}`).selectOption({ label: c.seatTitles[rc.positionId] });
      await page.locator(`#recount-applicant-${eid}`).fill(applicant.name);
      if (applicant.memberId) await page.locator(`#recount-member-${eid}`).fill(String(applicant.memberId));
      await page.locator(`#recount-grounds-${eid}`).fill(rc.grounds);
      await page.locator(`#recount-explanation-${eid}`).fill(rc.explanation);
      await page.locator(`#recount-received-${eid}`).fill(localInput(new Date(Date.now() - 60_000)));
      await tribunalWeb.shot(`Recount request from ${applicant.name}`);
      await page.getByRole('button', { name: 'Record request' }).click();
      const block = page.locator('.stack.gap-2').filter({ hasText: applicant.name })
        .filter({ has: page.getByRole('button', { name: 'Allow' }) }).last();
      await expect(block).toBeVisible({ timeout: 15_000 });
      if (rc.allow) {
        await block.getByRole('button', { name: 'Allow' }).click();
      } else {
        await block.locator('textarea').fill(rc.refusedBecause ?? 'Refused.');
        await block.getByRole('button', { name: 'Refuse' }).click();
      }
      const gone = await expect(block).toBeHidden({ timeout: 15_000 }).then(() => true, () => false);
      if (!gone) note('check', tribunalWeb.person.name, `Recount request from ${applicant.name} still open after the decision`, undefined, { ok: false });
      await tribunalWeb.shot(`Recount request from ${applicant.name} ${rc.allow ? 'allowed' : 'refused'}`);
    }
    await closeActor(tribunalWeb);

    let figuresBy: Actor | null = null;
    for (const w of [roWeb, ccWeb]) {
      await openFormRecords(w, eid, c.title);
      if (await shows(w.page.getByRole('button', { name: 'Enter recount figures' }).first(), 8_000)) { figuresBy = w; break; }
    }
    if (!figuresBy) throw new Error('nobody could enter the recount figures');
    if (figuresBy !== roWeb) report.gap('The Returning Officer had no "Enter recount figures" button.');
    await figuresBy.page.getByRole('button', { name: 'Enter recount figures' }).first().click();
    for (const input of await figuresBy.page.locator('input[id^="figure-"]').all()) {
      const nid = Number((await input.getAttribute('id'))!.split('-')[2]);
      await input.fill(String(voteCount.get(nid) ?? 0));
    }
    await figuresBy.shot('Recount figures entered', 'The recount agreed with the original count.');
    await figuresBy.page.getByRole('button', { name: 'Save figures' }).click();
    await figuresBy.page.waitForLoadState('networkidle').catch(() => {});
    await figuresBy.shot('Recount figures saved');
    await probe(admin, 'A recount dated after the window is refused', 'POST', `/admin/elections/${eid}/recounts`, {
      ElectionSeatId: c.seatIds[S.recounts[0].positionId], ApplicantName: 'Late applicant', ApplicantMemberId: null,
      Grounds: 'Late', Explanation: 'Received after the window.', ReceivedAt: iso(new Date(recountEnds.getTime() + 86_400_000)),
    }, [400]);

    // 15. Key, forms and audit
    report.section('Key destroyed, forms and audit', 'Once the recount window ends the officer records that the key file was destroyed. Every statutory form is produced filled and blank, and the audit chain is checked.');
    await probe(ro, 'The key cannot be marked destroyed while the recount window is open', 'POST', `/elections/${eid}/key-destroyed`, undefined, [400]);
    if (DEMO) move({ recountEnds: new Date() });
    await waitUntil(recountEnds, 'the recount window to end');
    await roWeb.goto(`/officials/elections/${eid}`);
    const destroyed = card(roWeb.page, c.title).locator('xpath=..').getByLabel(/I destroyed the key file/).first();
    if (await shows(destroyed)) {
      await destroyed.check();
      await roWeb.page.waitForLoadState('networkidle').catch(() => {});
      await roWeb.shot('Key file destroyed', 'The audit log records when the officer destroyed the file.');
    } else {
      await call('POST', `/elections/${eid}/key-destroyed`, await token(ro));
      note('api', ro.name, 'Key file destroyed, recorded through the API');
      report.gap('The "I destroyed the key file" box did not show for the Returning Officer.');
    }
    fs.rmSync(c.keyFile, { force: true });

    const forms = (await call<any[]>('GET', '/election-content?kind=Form')).body.filter(x => field(x, 'Kind') === 'Form');
    const formRows: (string | number)[][] = [];
    const links: { label: string; href: string }[] = [];
    const bearer = await token(admin);
    for (const f of forms) {
      const code = field<string>(f, 'Key');
      for (const mode of ['Filled', 'Blank']) {
        const d = await download(`/elections/${eid}/documents/${encodeURIComponent(code)}?mode=${mode}`, bearer);
        const ok = d.status === 200 && d.type.includes('pdf');
        const name = `${code}-${mode.toLowerCase()}.pdf`;
        if (ok) { fs.writeFileSync(path.join(FILES_DIR, name), d.bytes); links.push({ label: name, href: `files/${name}` }); }
        formRows.push([code, field(f, 'Title'), mode, d.status, ok ? `${Math.round(d.bytes.length / 1024)} KB` : 'not produced']);
      }
    }
    note('check', admin.name, 'Statutory forms produced', `${forms.length} forms, each filled with this election's data and blank.`, {
      ok: formRows.every(r => r[3] === 200), table: { head: ['Code', 'Form', 'Mode', 'Status', 'Size'], rows: formRows }, links,
    });
    // The verify route shares the credential-check rate limit, so only a few forms are opened.
    for (const f of forms.slice(0, 3)) await visitor.show(`/verify/election/${eid}/${encodeURIComponent(field<string>(f, 'Key'))}`, `QR check for form ${field(f, 'Key')}`);

    const audit = (await call('GET', `/elections/${eid}/audit/verify`, await token(admin))).body;
    note('check', admin.name, 'Audit chain verified', `${field(audit, 'Entries')} entries${field(audit, 'Ok') ? ', unbroken' : `, broken at ${field(audit, 'FirstBrokenSeq')}`}.`, { ok: !!field(audit, 'Ok') });
    const board = (await call('GET', `/elections/${eid}/board`, await token(admin))).body;
    note('check', 'Run', 'Election board', `Turnout ${field(field(board, 'Turnout'), 'BallotsCast')} of ${field(field(board, 'Turnout'), 'RollSize')}.`, {
      table: { head: ['Group', 'Official', 'Accepted'], rows: field<any[]>(board, 'Groups').flatMap(g => field<any[]>(g, 'Members').map(m => [field(g, 'GroupName'), `${field(m, 'DisplayName')}, ${field(m, 'PersonaName')}`, field(m, 'AcceptedAt') ? new Date(field(m, 'AcceptedAt')).toLocaleString() : 'no'])) },
    });
    await ccWeb.goto('/officials');
    await ccWeb.shot('Officials dashboard after the declare');

    // 16. Archive
    report.section('Archive', 'Archiving closes the election for good. It is a two-person step.');
    await toPhase(eid, c.title, 9, ccWeb, commWeb);
    await visitor.show('/elections/results', 'Public results after archiving');
  }

  // ---------------------------------------------------------------------------
  async function appoint(by: Actor, eid: number, who: Person, personaName: string, returning: boolean) {
    const page = by.page;
    await page.locator(`#persona-${eid}`).selectOption({ label: personaName });
    await page.locator(`#member-search-${eid}`).fill(who.name);
    // Some names belong to two members. The button shows the membership number, which is the
  // login name with a prefix (GHC-2512047), so match on that to pick the one who logs in.
  await page.getByRole('button', { name: new RegExp(`^${escapeRe(who.name)} \\([^)]*${escapeRe(who.username)}\\)$`) }).click();
    if (returning) await page.getByLabel('Returning Officer (one per election)').check();
    await by.shot(`Appointing ${who.name} as ${personaName}`);
    await page.getByRole('button', { name: 'Appoint', exact: true }).click();
    await expect(page.locator('strong', { hasText: who.name }).first()).toBeVisible({ timeout: 15_000 });
  }

  async function answerAppointment(official: Actor, eid: number, accept: boolean, reason?: string) {
    const mine = (await call<any[]>('GET', '/me/election-appointments', await login(official.person))).body
      .filter(x => field(x, 'ElectionId') === eid && !field(x, 'IsLive'));
    const id = field<number>(mine[mine.length - 1], 'Id');
    await official.goto('/officials/my-appointments');
    // ngModel takes [name] as its own input and never writes it to the page, so find the
    // card through the reason box, which has a real id.
    const reasonBox = official.page.locator(`#reason-${id}`);
    await expect(reasonBox).toBeVisible({ timeout: 15_000 });
    const box = reasonBox.locator('xpath=ancestor::*[.//button[normalize-space()="Accept"]][1]');
    const agree = box.getByRole('checkbox');
    if (accept) {
      await agree.check();
      await official.shot(`${official.person.name} reads the declaration`, 'The official ticks the declaration before accepting.');
      await box.getByRole('button', { name: 'Accept' }).click();
    } else {
      await official.page.locator(`#reason-${id}`).fill(reason ?? '');
      await official.shot(`${official.person.name} gives a reason for declining`);
      await box.getByRole('button', { name: 'Decline' }).click();
    }
    // The click can open the step-up dialog first. Wait for the page's own answer, so the
    // shot comes after the server has saved it.
    await expect(official.page.getByText(accept ? /You are now/ : /Appointment declined/).first()).toBeVisible({ timeout: 60_000 });
    // The page reloads the session after an accept. If that reload fails the official is
    // logged out, so wait for the card to settle before the next step.
    if (accept) await expect(box.getByRole('button', { name: 'Accept' })).toBeHidden({ timeout: 30_000 });
    await official.shot(`${official.person.name} ${accept ? 'accepted' : 'declined'}`);
    if (!accept) return;
    // Accepting adds the ElectionOfficial role. The page refreshes its own session, but the
    // API helper's cached login was issued before the role and has to be dropped.
    forget(official.person);
  }

  async function fileNomination(by: Actor, seatTitle: string, candidate: Person, seconder: Person, statement: string) {
    const page = by.page;
    await by.goto('/portal/election');
    const form = page.locator('form[data-section=nominate]');
    await expect(form).toBeVisible({ timeout: 20_000 });
    // A seat with more than one place shows " (N places)" after its title.
    const option = form.locator('#nomination-seat option').filter({ hasText: new RegExp(`^\\s*${escapeRe(seatTitle)}( \\(|\\s*$)`) }).first();
    await form.locator('#nomination-seat').selectOption({ label: (await option.textContent())!.trim() });
    for (const [role, who] of [['candidate', candidate], ['seconder', seconder]] as const) {
      await form.locator(`#nomination-${role}`).fill(who.name);
      await form.locator(`[data-hits=${role}]`).getByRole('button', { name: new RegExp(`^${escapeRe(who.name)} \\([^)]*${escapeRe(who.username)}\\)$`) }).click();
    }
    await form.locator('#nomination-statement').fill(statement);
    await by.shot(`${by.person.name} nominates ${candidate.name} for ${seatTitle}`, `${seconder.name} seconds.`);
    await form.getByRole('button', { name: 'File nomination' }).click();
    await expect(page.getByText(/Nomination filed/).first()).toBeVisible({ timeout: 60_000 });
    await by.shot(`Nomination for ${candidate.name} filed`);
  }

  /** Opens the Nominations & scrutiny tab on this election and waits for its list. */
  async function openScrutiny(by: Actor, route: string, title: string) {
    await by.goto(route);
    await by.page.getByRole('button', { name: 'Nominations & scrutiny' }).click();
    await by.page.locator('#scrutiny-election').selectOption({ label: title });
    await expect(by.page.locator('[data-nomination]').first()).toBeVisible({ timeout: 30_000 });
  }

  async function fillFormDetails(by: Actor, title: string, eid: number, opens: Date, closes: Date, recountEnds: Date) {
    const day = utcDay(opens);
    const silence = new Date(opens.getTime() - S.timing.silenceMinutesBeforePolling * 60_000);
    await by.goto('/admin/elections');
    await card(by.page, title).getByRole('button', { name: 'Form records' }).click();
    const page = by.page;
    await page.locator(`#final-list-on-${eid}`).fill(day);
    await page.locator(`#campaign-opens-on-${eid}`).fill(day);
    await page.locator(`#campaign-closes-on-${eid}`).fill(day);
    await page.locator(`#silence-starts-at-${eid}`).fill(localInput(silence));
    await page.locator(`#venue-${eid}`).fill(S.details.venue);
    await page.locator(`#notice-reference-${eid}`).fill(S.details.noticeReference);
    await page.locator(`#recount-window-ends-on-${eid}`).fill(localInput(recountEnds));
    await by.shot('Form details', 'These dates and the venue are printed on the statutory forms.');
    await page.getByRole('button', { name: 'Save details' }).click();
    await page.waitForLoadState('networkidle').catch(() => {});
    const saved = field(await adminElection(eid), 'FormDetails');
    note('check', by.label, 'Form details saved', undefined, {
      ok: !!saved && field(saved, 'Venue') === S.details.venue,
      table: { head: ['Field', 'Value'], rows: saved ? Object.entries(saved).map(([k, v]) => [k, String(v ?? '')]) : [] },
    });
    void closes;
  }

  async function openFormRecords(by: Actor, eid: number, title: string) {
    await by.goto(`/officials/elections/${eid}`);
    const button = card(by.page, title).getByRole('button', { name: 'Form records' });
    await button.click();
    await expect(by.page.getByRole('heading', { name: 'Form records' })).toBeVisible({ timeout: 15_000 });
  }

  /** Tries each person in turn and moves on only when the API answers 403. */
  async function firstAllowed(who: Person[], run: (bearer: string) => Promise<unknown>): Promise<Person> {
    for (const p of who) {
      try { await run(await token(p)); return p; } catch (e) {
        if (!(e instanceof ApiError) || e.reply.status !== 403) throw e;
      }
    }
    throw new Error(`none of ${who.map(p => p.name).join(', ')} was allowed`);
  }

  /** A visitor with no login, for the public pages. */
  async function anonymous() {
    const context = await (browser as Browser).newContext({ viewport: WINDOW_VIEWPORT });
    const page = await context.newPage();
    return {
      async show(route: string, title: string) {
        await page.goto(route);
        await page.waitForLoadState('networkidle').catch(() => {});
        const name = report.nextShotName();
        await page.screenshot({ path: path.join(SHOTS_DIR, name), fullPage: true });
        report.add({ kind: 'web', actor: 'Public visitor', title, shot: name, ok: true });
      },
    };
  }
});
