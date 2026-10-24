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
    python scripts/local-election.py close              # waits for the closing time, then Counting
    python scripts/local-election.py count              # count with the saved key, then declare
    python scripts/local-election.py status
    python scripts/local-election.py code 200           # newest emailed code for a user id
    python scripts/local-election.py restore            # put emails and passwords back

All accounts it touches use the password in PASSWORD below.
"""
import argparse
import base64
import json
import os
import re
import sys
import tempfile
import time
from datetime import datetime, timedelta, timezone

import bcrypt
import psycopg2
import requests

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
API = os.environ.get("GHCAA_LOCAL_API", "http://localhost:5087/api")
DB = dict(host="localhost", dbname="GHCAADB_v2", user="postgres", password="postgres")
STATE_DIR = os.path.join(tempfile.gettempdir(), "ghcaa-local-election")
STATE = os.path.join(STATE_DIR, "state.json")
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


def take_over_accounts(state):
    """Saves each touched account's email and login fields once, then sets the test values."""
    saved = state.setdefault("accounts", {})
    hashed = bcrypt.hashpw(PASSWORD.encode(), bcrypt.gensalt(11)).decode()
    with db() as conn, conn.cursor() as c:
        for uid in TOUCHED:
            c.execute('select u."MemberId", u."PasswordHash", u."MustChangePassword", u."FailedLoginAttempts", '
                      'u."LockoutUntil", m."Email" from "Users" u left join "Members" m on m."Id" = u."MemberId" '
                      'where u."Id" = %s', (uid,))
            row = c.fetchone()
            if row is None:
                fail(f"user {uid} not found")
            if str(uid) not in saved:
                saved[str(uid)] = dict(member_id=row[0], password_hash=row[1], must_change=row[2],
                                       failed=row[3], lockout=row[4].isoformat() if row[4] else None, email=row[5])
                save_state(state)
            c.execute('update "Users" set "PasswordHash" = %s, "MustChangePassword" = false, '
                      '"FailedLoginAttempts" = 0, "LockoutUntil" = null where "Id" = %s', (hashed, uid))
            if row[0] is not None:
                c.execute('update "Members" set "Email" = %s where "Id" = %s', (plus_address(uid), row[0]))
    say(f"accounts ready: {len(TOUCHED)} users, password {PASSWORD}, emails on plus addresses")


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
    state.pop("accounts")
    state.pop("tokens", None)
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
    if row is None or (since and row[3] < since):
        return None
    match = re.search(r"\b(\d{6})\b", row[0] or "") or re.search(r"\b(\d{6})\b", row[1] or "")
    return match.group(1) if match else None


def code_cmd(args):
    found = latest_code(args.user_id)
    if not found:
        fail(f"no code in EmailLogs for user {args.user_id}")
    print(found)


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
    call("POST", "/auth/step-up/request", token)
    code = None
    for _ in range(30):
        code = latest_code(uid, since=asked_at)
        if code:
            break
        time.sleep(1)
    if not code:
        fail(f"no step-up code arrived for user {uid}")
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
    start = (now + timedelta(days=30)).date().isoformat()
    period = call("POST", "/admin/governance/periods", a,
                  {"Title": f"Local test committee {now:%Y%m%d-%H%M}", "StartDate": start, "EndDate": None}).json()
    period_id = field(period, "Id")
    say(f"period {period_id} starts {start}")

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


def vote(args):
    state = need_election()
    eid = state["election_id"]
    cast = state.setdefault("tracking_codes", {})
    for uid in VOTERS[:args.n]:
        if str(uid) in cast:
            continue
        token = stepped_up(state, uid)
        ballot = call("GET", f"/elections/{eid}/ballot", token).json()
        choices = [{"ElectionSeatId": field(s, "SeatId"),
                    "NominationIds": [field(field(s, "Candidates")[uid % len(field(s, "Candidates"))], "NominationId")]}
                   for s in field(ballot, "Seats")]
        r = call("POST", f"/elections/{eid}/vote", token, {"Seats": choices}).json()
        cast[str(uid)] = field(r, "TrackingCode")
        save_state(state)
        say(f"  user {uid} voted, tracking code {cast[str(uid)]}, receipt pdf {'yes' if field(r, 'ReceiptPdfBase64') else 'NO'}")
    say(f"{len(cast)} scripted ballots cast")


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
    s.set_defaults(fn=vote)
    sub.add_parser("close").set_defaults(fn=close)
    sub.add_parser("count").set_defaults(fn=count)
    sub.add_parser("status").set_defaults(fn=status)
    s = sub.add_parser("code")
    s.add_argument("user_id", type=int)
    s.set_defaults(fn=code_cmd)
    sub.add_parser("restore").set_defaults(fn=restore)
    args = p.parse_args()
    args.fn(args)


if __name__ == "__main__":
    main()
