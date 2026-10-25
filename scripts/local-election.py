"""Runs a full election against the LOCAL API and database, for the phone checks in 84.50.

Never point this at preprod or live. It changes passwords and emails in the local
database and puts them back on `restore`.

Step-up codes are emailed for real. So no real member gets one, `prepare` moves every
account it uses onto plus addresses of the Gmail sender in GHCAA.API/.env
(sender+v201@gmail.com and so on), and reads the codes back from EmailLogs. The real
emails and password hashes are kept in a state file under the system temp folder,
outside the repo, until `restore` writes them back.

Run from the repo root, with the API up on http://localhost:5087:
    python scripts/local-election.py prepare            # election up to Campaign, key set, readiness attested
    python scripts/local-election.py prepare --no-key   # leave the key for the phone to set (84.44)
    python scripts/local-election.py open               # open polling (waits for the opening time)
    python scripts/local-election.py vote --n 10        # ten scripted voters
    python scripts/local-election.py vote --together    # all ballots at the same moment, then check the database (84.53)
    python scripts/local-election.py close              # waits for the closing time, then Counting
    python scripts/local-election.py count              # count with the saved key, then declare
    python scripts/local-election.py status
    python scripts/local-election.py code 200           # newest emailed code for a user id
    python scripts/local-election.py restore            # put emails and passwords back
    python scripts/local-election.py purge              # delete what the runs made (see purge below)

The web walkthrough (GHCAA.Web/tests/e2e/election-walkthrough) drives this script too:
    accounts 1,2,201    take over just these users
    code 201 --since T  a code sent after T (ISO, UTC)
    record period 9     note an id so purge removes it even if the run dies
    window 12 --opens T move the polling dates for the demo mode (also --closes, --recount-ends, --silence)
    who 1,2,201         usernames and member names as JSON
GHCAA_LOCAL_STATE points it at its own state file, so it never mixes with a phone check.

All accounts it touches use the password in PASSWORD below.
"""
import argparse
import base64
import json
import os
import re
import sys
import tempfile
import threading
import time
from datetime import datetime, timedelta, timezone

import bcrypt
import psycopg2
import requests

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
API = os.environ.get("GHCAA_LOCAL_API", "http://localhost:5087/api")
DB = dict(host="localhost", dbname="GHCAADB_v2", user="postgres", password="postgres")
STATE_DIR = os.path.join(tempfile.gettempdir(), "ghcaa-local-election")
STATE = os.environ.get("GHCAA_LOCAL_STATE") or os.path.join(STATE_DIR, "state.json")
STATE_DIR = os.path.dirname(os.path.abspath(STATE))
KEY_FILE = os.path.join(STATE_DIR, "ballot-private-key.b64")
PASSWORD = "LocalVote#2026"

ADMIN_A = 2      # shalin, asks
ADMIN_B = 1      # superadmin, approves
APP_VOTER = 200  # left for the phone to vote with
VOTERS = list(range(201, 211))
SEATS = [  # (candidates, proposers, seconders), each a member id with a user of the same id
    ([211, 212], [215, 216], [219, 220]),
    ([213, 214], [217, 218], [221, 222]),
]
TOUCHED = sorted({ADMIN_A, ADMIN_B, APP_VOTER, *VOTERS,
                  *(m for seat in SEATS for group in seat[1:] for m in group)})


def say(msg):
    print(msg, flush=True)


def fail(msg):
    say(f"FAIL: {msg}")
    sys.exit(1)


def db():
    conn = psycopg2.connect(**DB)
    conn.autocommit = True
    return conn


def load_state():
    if not os.path.exists(STATE):
        return {}
    with open(STATE, encoding="utf-8") as f:
        return json.load(f)


def save_state(state):
    os.makedirs(STATE_DIR, exist_ok=True)
    with open(STATE, "w", encoding="utf-8") as f:
        json.dump(state, f, indent=2)


def sender_address():
    # Read without printing. The local .env often holds a localhost placeholder, so
    # .env.preprod is tried next. The codes must reach a mailbox the owner can open.
    for name in (".env", ".env.preprod"):
        path = os.path.join(ROOT, "GHCAA.API", name)
        if not os.path.exists(path):
            continue
        with open(path, encoding="utf-8") as f:
            for line in f:
                if line.strip().startswith("GmailSettings__Email="):
                    value = line.split("=", 1)[1].strip().strip('"').strip("'")
                    if "@" in value and not value.endswith("@localhost"):
                        return value
    fail("No real GmailSettings__Email in GHCAA.API/.env or .env.preprod")


def plus_address(user_id):
    local, domain = sender_address().split("@", 1)
    return f"{local.split('+')[0]}+v{user_id}@{domain}"


# Tables that a run adds rows to as a side effect. Their highest id is noted before the
# first takeover, so purge can remove only the rows added since, and only for touched users.
MARKED = ("EmailLogs", "Otps", "Notifications", "RefreshTokens", "ActivityLogs")


def take_over_accounts(state, users=TOUCHED):
    """Saves each touched account's email and login fields once, then sets the test values."""
    saved = state.setdefault("accounts", {})
    hashed = bcrypt.hashpw(PASSWORD.encode(), bcrypt.gensalt(11)).decode()
    with db() as conn, conn.cursor() as c:
        if "marks" not in state:
            marks = {}
            for table in (*MARKED, "ECPeriods", "Elections"):
                c.execute(f'select coalesce(max("Id"), 0) from "{table}"')
                marks[table] = c.fetchone()[0]
            state["marks"] = marks
            save_state(state)
        for uid in users:
            c.execute('select u."MemberId", u."PasswordHash", u."MustChangePassword", u."FailedLoginAttempts", '
                      'u."LockoutUntil", m."Email", u."Email" from "Users" u left join "Members" m on m."Id" = u."MemberId" '
                      'where u."Id" = %s', (uid,))
            row = c.fetchone()
            if row is None:
                fail(f"user {uid} not found")
            if str(uid) not in saved:
                saved[str(uid)] = dict(member_id=row[0], password_hash=row[1], must_change=row[2],
                                       failed=row[3], lockout=row[4].isoformat() if row[4] else None, email=row[5])
                save_state(state)
            # An account with no member, such as the system admin, gets step-up codes at the user's
            # own email. State saved before this field existed still holds the real one, so read it now.
            if row[0] is None and "user_email" not in saved[str(uid)]:
                saved[str(uid)]["user_email"] = row[6]
                save_state(state)
            c.execute('update "Users" set "PasswordHash" = %s, "MustChangePassword" = false, '
                      '"FailedLoginAttempts" = 0, "LockoutUntil" = null where "Id" = %s', (hashed, uid))
            if row[0] is not None:
                c.execute('update "Members" set "Email" = %s where "Id" = %s', (plus_address(uid), row[0]))
            else:
                c.execute('update "Users" set "Email" = %s where "Id" = %s', (plus_address(uid), uid))
    say(f"accounts ready: {len(users)} users, password {PASSWORD}, emails on plus addresses")


def accounts_cmd(args):
    take_over_accounts(load_state(), [int(x) for x in args.users.split(",") if x.strip()])


def restore(_args):
    state = load_state()
    saved = state.get("accounts", {})
    if not saved:
        say("nothing to restore")
        return
    with db() as conn, conn.cursor() as c:
        for uid, s in saved.items():
            c.execute('update "Users" set "PasswordHash" = %s, "MustChangePassword" = %s, "FailedLoginAttempts" = %s, '
                      '"LockoutUntil" = %s where "Id" = %s',
                      (s["password_hash"], s["must_change"], s["failed"], s["lockout"], int(uid)))
            if s["member_id"] is not None:
                c.execute('update "Members" set "Email" = %s where "Id" = %s', (s["email"], s["member_id"]))
            elif "user_email" in s:
                c.execute('update "Users" set "Email" = %s where "Id" = %s', (s["user_email"], int(uid)))
    state.pop("accounts")
    state.pop("tokens", None)
    # The marks stay while something is left to purge, which needs them.
    if not state.get("elections") and not state.get("periods") and not state.get("election_id"):
        state.pop("marks", None)
        state.pop("touched", None)
    else:
        # A later purge still has to find the notifications and logs these accounts got.
        touched = state.setdefault("touched", {"users": [], "members": []})
        touched["users"] = sorted(set(touched["users"]) | {int(u) for u in saved})
        touched["members"] = sorted(set(touched["members"]) | {s["member_id"] for s in saved.values() if s.get("member_id")})
    save_state(state)
    say(f"restored {len(saved)} accounts. The election itself stays in the local database.")


def username(uid):
    with db() as conn, conn.cursor() as c:
        c.execute('select "Username" from "Users" where "Id" = %s', (uid,))
        return c.fetchone()[0]


def latest_code(uid, since=None):
    with db() as conn, conn.cursor() as c:
        c.execute('select "Subject", "Body", "Status", "SentDate" from "EmailLogs" where lower("RecipientEmail") = lower(%s) '
                  'order by "Id" desc limit 1', (plus_address(uid),))
        row = c.fetchone()
    # SentDate is timestamptz on PostgreSQL, so compare both sides as naive UTC.
    sent = row[3].astimezone(timezone.utc).replace(tzinfo=None) if row and row[3] and row[3].tzinfo else (row[3] if row else None)
    if row is None or (since and sent < since):
        return None
    match = re.search(r"\b(\d{6})\b", row[0] or "") or re.search(r"\b(\d{6})\b", row[1] or "")
    return match.group(1) if match else None


def mail_error(uid, since=None):
    """Why the newest email to the user failed, or None if none failed since `since`."""
    with db() as conn, conn.cursor() as c:
        c.execute('select "ErrorMessage", "SentDate" from "EmailLogs" where lower("RecipientEmail") = lower(%s) '
                  'and "Status" = \'Failed\' order by "Id" desc limit 1', (plus_address(uid),))
        row = c.fetchone()
    if row is None:
        return None
    sent = row[1].astimezone(timezone.utc).replace(tzinfo=None) if row[1] and row[1].tzinfo else row[1]
    if since and sent and sent < since:
        return None
    text = row[0] or "no error text was logged"
    # Brevo refuses mail from an IP that is not on the account's allow list. A home or mobile
    # connection can change IP between runs, so say exactly what to add.
    ip = re.search(r"unrecognised IP address\s+([0-9a-fA-F:.]+)", text)
    if ip:
        return (f"Brevo refused to send because this machine's IP {ip.group(1).rstrip('.')} is not authorised. "
                "Add it at https://app.brevo.com/security/authorised_ips, then run again.")
    return f"The email could not be sent: {text[:300]}"


def parse_since(value):
    return datetime.fromisoformat(value.replace("Z", "+00:00")).astimezone(timezone.utc).replace(tzinfo=None) if value else None


def code_cmd(args):
    found = latest_code(args.user_id, parse_since(args.since))
    if not found:
        fail(f"no code in EmailLogs for user {args.user_id}")
    print(found)


def mail_error_cmd(args):
    print(mail_error(args.user_id, parse_since(args.since)) or "")


def call(method, path, token=None, body=None, ok=(200, 201, 202, 204)):
    headers = {"Authorization": f"Bearer {token}"} if token else {}
    r = requests.request(method, f"{API}{path}", json=body, headers=headers, timeout=120)
    if r.status_code not in ok:
        fail(f"{method} {path} -> {r.status_code} {r.text[:400]}")
    return r


def field(obj, name):
    return obj.get(name) if name in obj else obj.get(name[0].lower() + name[1:])


def login(uid):
    r = call("POST", "/auth/login", body={"Username": username(uid), "Password": PASSWORD})
    return field(r.json(), "Token")


def stepped_up(state, uid):
    """A step-up token for the user, reused while it is under 25 minutes old."""
    cached = state.get("tokens", {}).get(str(uid))
    if cached and time.time() - cached["at"] < 25 * 60:
        return cached["token"]
    token = login(uid)
    asked_at = datetime.now(timezone.utc).replace(tzinfo=None) - timedelta(seconds=5)
    # A 503 here means the code email failed. The reason is in EmailLogs, read below.
    call("POST", "/auth/step-up/request", token, ok=(200, 201, 204, 503))
    code = None
    for _ in range(30):
        code = latest_code(uid, since=asked_at)
        if code:
            break
        time.sleep(1)
    if not code:
        fail(f"no step-up code arrived for user {uid}. {mail_error(uid, asked_at) or ''}".strip())
    r = call("POST", "/auth/step-up/verify", token, {"Code": code, "Method": "email"})
    token = field(r.json(), "Token")
    state.setdefault("tokens", {})[str(uid)] = {"token": token, "at": time.time()}
    save_state(state)
    return token


def two_person(state, method, path, body=None):
    """Runs a step that may need a second person. A asks, B approves when a 202 comes back."""
    r = call(method, path, stepped_up(state, ADMIN_A), body)
    if r.status_code != 202:
        return r
    approval = field(r.json(), "Id")
    call("POST", f"/elections/approvals/{approval}/approve", stepped_up(state, ADMIN_B))
    say(f"  {path}: approved by the second person (approval {approval})")
    return r


def phase(state, eid, value):
    two_person(state, "POST", f"/elections/{eid}/phase", value)
    say(f"  phase -> {value}")


def new_key():
    from cryptography.hazmat.primitives import serialization
    from cryptography.hazmat.primitives.asymmetric import rsa
    key = rsa.generate_private_key(public_exponent=65537, key_size=3072)
    public = key.public_key().public_bytes(serialization.Encoding.DER, serialization.PublicFormat.SubjectPublicKeyInfo)
    private = key.private_bytes(serialization.Encoding.DER, serialization.PrivateFormat.PKCS8, serialization.NoEncryption())
    return base64.b64encode(public).decode(), base64.b64encode(private).decode()


PERIOD_TITLE = "Local test committee"


def local_period_start(token, now):
    """Periods may not overlap, so each run gets a one-day period after the last one.
    Earlier runs made open-ended periods, which block every later start; those get an end date."""
    latest = (now + timedelta(days=30)).date()
    for p in call("GET", "/admin/governance/periods", token).json():
        begins = datetime.fromisoformat(field(p, "StartDate").replace("Z", "+00:00")).date()
        ends = field(p, "EndDate")
        if ends is None:
            if not field(p, "Title").startswith(PERIOD_TITLE):
                fail(f"period {field(p, 'Id')} has no end date, so no new period can be added after it")
            ends = (begins + timedelta(days=1)).isoformat()
            call("PUT", f"/admin/governance/periods/{field(p, 'Id')}", token,
                 {"Title": field(p, "Title"), "StartDate": begins.isoformat(), "EndDate": ends,
                  "IsActive": field(p, "IsActive")})
            say(f"period {field(p, 'Id')} from an earlier run now ends {ends}")
        latest = max(latest, datetime.fromisoformat(ends.replace("Z", "+00:00")[:10]).date() + timedelta(days=2))
    return latest


def prepare(args):
    state = load_state()
    if state.get("election_id"):
        fail(f"election {state['election_id']} is already set up. Run restore and delete {STATE} to start over.")
    take_over_accounts(state)

    with db() as conn, conn.cursor() as c:
        c.execute('select "Id" from "EcPositions" where "IsActive" order by "Id" limit %s', (len(SEATS),))
        positions = [r[0] for r in c.fetchall()]
    if len(positions) < len(SEATS):
        fail("not enough active EC positions")

    a = stepped_up(state, ADMIN_A)
    now = datetime.now(timezone.utc)
    start = local_period_start(a, now)
    period = call("POST", "/admin/governance/periods", a,
                  {"Title": f"{PERIOD_TITLE} {now:%Y%m%d-%H%M}", "StartDate": start.isoformat(),
                   "EndDate": (start + timedelta(days=1)).isoformat()}).json()
    period_id = field(period, "Id")
    say(f"period {period_id} runs {start} to {start + timedelta(days=1)}")

    opens = now + timedelta(minutes=args.open_in)
    closes = opens + timedelta(minutes=args.vote_minutes)
    iso = lambda d: d.strftime("%Y-%m-%dT%H:%M:%SZ")
    created = call("POST", "/admin/elections", a, {
        "Title": f"Local phone check {now:%Y-%m-%d %H:%M}",
        "ECPeriodId": period_id,
        "NominationOpensOn": iso(now - timedelta(hours=1)),
        "NominationClosesOn": iso(opens),
        "PollingOpensOn": iso(opens),
        "PollingClosesOn": iso(closes),
        "Description": "Created by scripts/local-election.py",
        "Positions": [{"Title": "", "Seats": 1, "PositionId": p} for p in positions],
        "IsTest": False,
    }).json()
    eid = field(created, "Id")
    state.update(election_id=eid, period_id=period_id, polling_opens=iso(opens), polling_closes=iso(closes))
    save_state(state)
    say(f"election {eid}: polling {iso(opens)} to {iso(closes)}")

    roll = call("POST", f"/elections/{eid}/voter-roll/freeze", a).json()
    say(f"  roll frozen: {field(roll, 'Count')} voters")
    phase(state, eid, 1)

    listed = next(e for e in call("GET", "/admin/elections", a).json() if field(e, "Id") == eid)
    seat_ids = [field(s, "Id") for s in (field(listed, "Positions") or [])]
    if len(seat_ids) < len(SEATS):
        fail(f"expected {len(SEATS)} seats, got {seat_ids}")
    nomination_ids = []
    for seat_id, (candidates, proposers, seconders) in zip(seat_ids, SEATS):
        for cand, prop, sec in zip(candidates, proposers, seconders):
            n = call("POST", f"/elections/{eid}/nominations", login(prop), {
                "ElectionSeatId": seat_id, "CandidateMemberId": cand, "ProposerMemberId": prop,
                "SeconderMemberId": sec, "Statement": "Scripted local candidate.", "PhotoPath": None}).json()
            nomination_ids.append(field(n, "Id"))
    say(f"  nominations: {nomination_ids}")

    phase(state, eid, 2)
    for nid in nomination_ids:
        call("POST", f"/elections/nominations/{nid}/scrutiny", stepped_up(state, ADMIN_A), {"Accepted": True, "Reason": "Local check"})
    for p in (3, 4, 5):
        phase(state, eid, p)
    for condition in ("mock-election", "technical-audit", "appeals-decided", "paper-stock"):
        call("POST", f"/elections/{eid}/readiness/attest", stepped_up(state, ADMIN_A), {"Condition": condition}, ok=(204, 409))

    if args.no_key:
        say("no ballot key set. Set it from the phone, then run `open`.")
    else:
        public, private = new_key()
        with open(KEY_FILE, "w", encoding="utf-8") as f:
            f.write(private)
        two_person(state, "POST", f"/admin/elections/{eid}/ballot-key", {"PublicKey": public})
        say(f"  ballot key set. Private key saved to {KEY_FILE}")
    readiness(state)


def readiness(state):
    r = call("GET", f"/elections/{state['election_id']}/readiness", stepped_up(state, ADMIN_A)).json()
    for item in field(r, "Items") or []:
        say(f"  {'ok  ' if field(item, 'Met') else 'MISS'} {field(item, 'Key')}")
    say(f"ready: {field(r, 'Ready')}")


def wait_until(iso_time, what):
    target = datetime.strptime(iso_time, "%Y-%m-%dT%H:%M:%SZ").replace(tzinfo=timezone.utc)
    left = (target - datetime.now(timezone.utc)).total_seconds()
    if left > 0:
        say(f"waiting {int(left)}s for {what}")
        time.sleep(left + 2)


def open_cmd(_args):
    state = need_election()
    phase(state, state["election_id"], 6)
    wait_until(state["polling_opens"], "polling to open")
    say(f"polling open. User {APP_VOTER} ({username(APP_VOTER)}) is left for the phone, password {PASSWORD}")


def ballot_for(eid, uid, token):
    ballot = call("GET", f"/elections/{eid}/ballot", token).json()
    return [{"ElectionSeatId": field(s, "SeatId"),
             "NominationIds": [field(field(s, "Candidates")[uid % len(field(s, "Candidates"))], "NominationId")]}
            for s in field(ballot, "Seats")]


def vote(args):
    state = need_election()
    eid = state["election_id"]
    cast = state.setdefault("tracking_codes", {})
    if args.together:
        return vote_together(state, eid, cast, [u for u in VOTERS[:args.n] if str(u) not in cast])
    for uid in VOTERS[:args.n]:
        if str(uid) in cast:
            continue
        token = stepped_up(state, uid)
        r = call("POST", f"/elections/{eid}/vote", token, {"Seats": ballot_for(eid, uid, token)}).json()
        cast[str(uid)] = field(r, "TrackingCode")
        save_state(state)
        say(f"  user {uid} voted, tracking code {cast[str(uid)]}, receipt pdf {'yes' if field(r, 'ReceiptPdfBase64') else 'NO'}")
    say(f"{len(cast)} scripted ballots cast")


def vote_together(state, eid, cast, uids):
    """84.53. Sends every ballot at the same moment, then checks the database kept each one once.

    Step-up and the ballot fetch go one at a time first, since they send email and are not
    what is being tested. A barrier then releases all the POSTs together.
    """
    if len(uids) < 2:
        fail("need at least two voters who have not voted. Run prepare again for a new election.")
    ready = {uid: (stepped_up(state, uid),) for uid in uids}
    ready = {uid: (tok, ballot_for(eid, uid, tok)) for uid, (tok,) in ready.items()}
    gate = threading.Barrier(len(uids))
    outcome = {}

    def send(uid):
        token, choices = ready[uid]
        gate.wait()
        r = requests.post(f"{API}/elections/{eid}/vote", json={"Seats": choices},
                          headers={"Authorization": f"Bearer {token}"}, timeout=120)
        outcome[uid] = r

    say(f"sending {len(uids)} ballots at once")
    threads = [threading.Thread(target=send, args=(uid,)) for uid in uids]
    for t in threads:
        t.start()
    for t in threads:
        t.join()

    refused = {uid: r for uid, r in outcome.items() if r.status_code != 200}
    for uid, r in sorted(outcome.items()):
        if r.status_code == 200:
            cast[str(uid)] = field(r.json(), "TrackingCode")
        say(f"  user {uid}: {r.status_code} {'' if r.status_code == 200 else r.text[:200]}")
    save_state(state)

    with db() as conn, conn.cursor() as c:
        c.execute('select count(*) from "VoterRolls" where "ElectionId" = %s and "VotedAt" is not null', (eid,))
        voted = c.fetchone()[0]
        c.execute('select count(*) from "PendingBallots" where "ElectionId" = %s', (eid,))
        sealed = c.fetchone()[0]
        c.execute('select count(*) from (select 1 from "SeatVotes" where "ElectionId" = %s '
                  'group by "ElectionSeatId", "MemberId" having count(*) > 1) d', (eid,))
        doubled = c.fetchone()[0]
    say(f"database: {voted} marked voted, {sealed} sealed ballots, {doubled} doubled seat votes")
    if refused or voted != sealed or sealed != len(cast) or doubled:
        fail(f"{len(refused)} of {len(uids)} ballots refused, or the database does not add up")
    say(f"all {len(uids)} ballots sent together were recorded once")


def close(_args):
    state = need_election()
    wait_until(state["polling_closes"], "polling to close")
    phase(state, state["election_id"], 7)


def count(_args):
    state = need_election()
    eid = state["election_id"]
    if not os.path.exists(KEY_FILE):
        fail("no saved private key. The phone set the key, so count from the phone.")
    with open(KEY_FILE, encoding="utf-8") as f:
        private = f.read().strip()
    first = two_person(state, "POST", f"/elections/{eid}/count", {"PrivateKey": private})
    results = first.json() if first.status_code == 200 else call("POST", f"/elections/{eid}/count", stepped_up(state, ADMIN_A), {"PrivateKey": private}).json()
    say(f"count: {json.dumps(results)[:600]}")
    two_person(state, "POST", f"/elections/{eid}/declare")
    say("declared")


def status(_args):
    state = need_election()
    readiness(state)
    say(f"tracking codes: {state.get('tracking_codes', {})}")


def who(args):
    ids = [int(x) for x in args.users.split(",") if x.strip()]
    with db() as conn, conn.cursor() as c:
        c.execute('select u."Id", u."Username", u."MemberId", m."FullName" from "Users" u '
                  'left join "Members" m on m."Id" = u."MemberId" where u."Id" = any(%s)', (ids,))
        rows = {str(r[0]): dict(username=r[1], member_id=r[2], name=r[3] or r[1]) for r in c.fetchall()}
    missing = [i for i in ids if str(i) not in rows]
    if missing:
        fail(f"users not found: {missing}")
    print(json.dumps(rows, ensure_ascii=False))


def record(args):
    state = load_state()
    ids = state.setdefault(f"{args.kind}s", [])
    if args.id not in ids:
        ids.append(args.id)
    save_state(state)


WINDOW_COLUMNS = {"opens": "PollingOpensOn", "closes": "PollingClosesOn",
                  "recount_ends": "RecountWindowEndsOn", "silence": "SilenceStartsAt"}


def window(args):
    """Moves the polling dates of a walkthrough election, so a demo does not sit through them.

    The API cannot change these dates once the election exists, so this writes them straight
    to the row. The audit chain does not record the move. Only an election this state file
    recorded and carrying the walkthrough title is touched."""
    if args.id not in load_state().get("elections", []):
        fail(f"election {args.id} was not recorded by this run")
    # Kept time-zone aware: a naive value would be read in the session's zone, not UTC.
    sets = {WINDOW_COLUMNS[k]: datetime.fromisoformat(v.replace("Z", "+00:00"))
            for k, v in vars(args).items() if k in WINDOW_COLUMNS and v}
    if not sets:
        fail("give at least one of --opens, --closes, --recount-ends, --silence")
    cols = ", ".join(f'"{c}" = %s' for c in sets)
    with db() as conn, conn.cursor() as c:
        c.execute(f'update "Elections" set {cols} where "Id" = %s and "Title" like %s',
                  (*sets.values(), args.id, "%" + WALKTHROUGH_SUFFIX))
        if c.rowcount != 1:
            fail(f"election {args.id} is not a walkthrough election")


# Titles only the scripts use. Rows with them are test leftovers whatever the state file says.
# The browser walkthrough puts WALKTHROUGH_SUFFIX on both its titles.
WALKTHROUGH_SUFFIX = " (local walkthrough)"
LEFTOVER_ELECTIONS = ["Local phone check %", "%" + WALKTHROUGH_SUFFIX]
LEFTOVER_PERIODS = [PERIOD_TITLE + " %", "%" + WALKTHROUGH_SUFFIX]


def purge(args):
    """Deletes the elections and periods the runs made, by recorded id, plus the side rows.

    A period is only removed when it is newer than the marks taken at the first takeover,
    or carries a script-only title, and never while it is active. Real periods stay put."""
    state = load_state()
    marks = state.get("marks", {})
    elections = set(state.get("elections", [])) | ({state["election_id"]} if state.get("election_id") else set())
    periods = set(state.get("periods", [])) | ({state["period_id"]} if state.get("period_id") else set())
    touched = state.get("touched", {})
    users = sorted({int(u) for u in state.get("accounts", {})} | set(touched.get("users", [])))
    members = sorted({s["member_id"] for s in state.get("accounts", {}).values() if s.get("member_id")}
                     | set(touched.get("members", [])))
    pattern = plus_address(0).replace("+v0@", "+v%@")
    removed = {}
    conn = psycopg2.connect(**DB)
    try:
        with conn, conn.cursor() as c:
            c.execute('select "Id" from "ECPeriods" where "Title" like any(%s)', (LEFTOVER_PERIODS,))
            legacy = {r[0] for r in c.fetchall()}
            if args.leftovers:
                periods |= legacy
                c.execute('select "Id" from "Elections" where "Title" like any(%s)', (LEFTOVER_ELECTIONS,))
                elections |= {r[0] for r in c.fetchall()}
            safe = set()
            for pid in periods:
                c.execute('select "IsActive" from "ECPeriods" where "Id" = %s', (pid,))
                row = c.fetchone()
                if row is None:
                    continue
                if row[0]:
                    say(f"period {pid} is active, so it stays")
                elif pid in legacy or ("ECPeriods" in marks and pid > marks["ECPeriods"]):
                    safe.add(pid)
                else:
                    say(f"period {pid} was not made by a run, so it stays")
            if safe:
                c.execute('select "Id" from "Elections" where "ECPeriodId" = any(%s)', (list(safe),))
                elections |= {r[0] for r in c.fetchall()}
            # A stale state file could name an id that now belongs to a real election.
            c.execute('select "Id" from "Elections" where "Id" = any(%s) and ("ECPeriodId" = any(%s) or "Title" like any(%s) or "Id" > %s)',
                      (list(elections), list(safe), LEFTOVER_ELECTIONS, marks.get("Elections", 2**31 - 1)))
            e = [r[0] for r in c.fetchall()]
            for kept in elections - set(e):
                say(f"election {kept} was not made by a run, so it stays")
            steps = [
                ("BallotVotes", 'delete from "BallotVotes" where "BallotId" in (select "Id" from "Ballots" where "ElectionId" = any(%s))'),
                ("ElectionRecountFigures", 'delete from "ElectionRecountFigures" where "NominationId" in (select "Id" from "Nominations" where "ElectionId" = any(%s))'),
                ("ElectionRecountRequests", 'delete from "ElectionRecountRequests" where "ElectionId" = any(%s)'),
                ("SeatVotes", 'delete from "SeatVotes" where "ElectionId" = any(%s)'),
                ("Ballots", 'delete from "Ballots" where "ElectionId" = any(%s)'),
                ("Nominations", 'delete from "Nominations" where "ElectionId" = any(%s)'),
                # The rest go by cascade, the audit chain with them, whole.
                ("Elections", 'delete from "Elections" where "Id" = any(%s)'),
            ]
            if e:
                for name, sql in steps:
                    c.execute(sql, (e,))
                    removed[name] = c.rowcount
            if safe:
                # A declared run puts its winners on the period's committee.
                c.execute('delete from "ECMembers" where "ECPeriodId" = any(%s)', (list(safe),))
                removed["ECMembers"] = c.rowcount
                c.execute('delete from "ECPeriods" where "Id" = any(%s)', (list(safe),))
                removed["ECPeriods"] = c.rowcount
            # Plus addresses only ever come from these scripts, so their mail and codes go
            # whatever run made them. The other side rows need a mark and a touched user.
            c.execute('delete from "EmailLogs" where lower("RecipientEmail") like lower(%s)', (pattern,))
            removed["EmailLogs"] = c.rowcount
            c.execute('delete from "Otps" where lower("Email") like lower(%s)', (pattern,))
            removed["Otps"] = c.rowcount
            if marks and users:
                for table, sql, ids in [
                    ("EmailLogs", '"RecipientMemberId" = any(%s)', members),
                    ("Notifications", '"MemberId" = any(%s)', members),
                    ("RefreshTokens", '"UserId" = any(%s)', users),
                    ("ActivityLogs", '"MemberId" = any(%s)', members),
                ]:
                    c.execute(f'delete from "{table}" where "Id" > %s and {sql}', (marks[table], ids))
                    removed[table] = removed.get(table, 0) + c.rowcount
    finally:
        conn.close()
    for key in ("elections", "periods", "election_id", "period_id", "polling_opens", "polling_closes", "tracking_codes", "touched"):
        state.pop(key, None)
    if not state.get("accounts"):
        state.pop("marks", None)
    save_state(state)
    say("purged: " + (", ".join(f"{k} {v}" for k, v in removed.items() if v) or "nothing"))


def need_election():
    state = load_state()
    if not state.get("election_id"):
        fail("no election yet. Run prepare first.")
    return state


def main():
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = p.add_subparsers(dest="cmd", required=True)
    s = sub.add_parser("prepare")
    s.add_argument("--no-key", action="store_true")
    s.add_argument("--open-in", type=int, default=5, help="minutes until polling opens")
    s.add_argument("--vote-minutes", type=int, default=30, help="minutes polling stays open")
    s.set_defaults(fn=prepare)
    sub.add_parser("open").set_defaults(fn=open_cmd)
    s = sub.add_parser("vote")
    s.add_argument("--n", type=int, default=len(VOTERS))
    s.add_argument("--together", action="store_true", help="84.53: send every ballot at the same moment")
    s.set_defaults(fn=vote)
    sub.add_parser("close").set_defaults(fn=close)
    sub.add_parser("count").set_defaults(fn=count)
    sub.add_parser("status").set_defaults(fn=status)
    s = sub.add_parser("code")
    s.add_argument("user_id", type=int)
    s.add_argument("--since", help="only a code sent after this ISO time (UTC)")
    s.set_defaults(fn=code_cmd)
    s = sub.add_parser("mail-error", help="why the newest email to the user failed, blank if none did")
    s.add_argument("user_id", type=int)
    s.add_argument("--since", help="only a failure after this ISO time (UTC)")
    s.set_defaults(fn=mail_error_cmd)
    sub.add_parser("restore").set_defaults(fn=restore)
    s = sub.add_parser("accounts")
    s.add_argument("users", help="comma-separated user ids")
    s.set_defaults(fn=accounts_cmd)
    s = sub.add_parser("who")
    s.add_argument("users", help="comma-separated user ids")
    s.set_defaults(fn=who)
    s = sub.add_parser("record")
    s.add_argument("kind", choices=["election", "period"])
    s.add_argument("id", type=int)
    s.set_defaults(fn=record)
    s = sub.add_parser("window", help="move a walkthrough election's polling dates (ISO times)")
    s.add_argument("id", type=int)
    for name in WINDOW_COLUMNS:
        s.add_argument("--" + name.replace("_", "-"), dest=name)
    s.set_defaults(fn=window)
    s = sub.add_parser("purge")
    s.add_argument("--leftovers", action="store_true", help="also remove rows with the script-only titles")
    s.set_defaults(fn=purge)
    args = p.parse_args()
    args.fn(args)


if __name__ == "__main__":
    main()
