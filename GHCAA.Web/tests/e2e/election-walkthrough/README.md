# Election walkthrough (local)

Work item 84.52 in `docs/TODO.md`. This test runs one full election on the local stack and
writes an HTML report with a captioned screenshot for every step. Each person (admins,
officials, candidates, voters) works in their own browser session.

It runs against the local database only. It borrows real member accounts, so their emails
are moved to plus addresses for the run and put back afterwards.

## Before the first run

1. PostgreSQL on localhost with the `GHCAADB_v2` database. The API's development settings
   already point at it.
2. `GHCAA.API/.env` exists. Missing keys can be copied from `.env.preprod`, one key at a
   time. Never copy the whole file: it points at the preprod database.
3. curl, the .NET SDK, Node with npm, and Python on PATH. The runner script installs
   psycopg2, the web packages and Playwright's Chromium when they are missing. For a run by
   hand: `python -m pip install psycopg2-binary`, then `cd GHCAA.Web; npm ci; npx playwright install chromium`.
4. Brevo allows email only from IPs on the account's list at
   <https://app.brevo.com/security/authorised_ips>. Step-up codes go out by email, so this
   machine's public IP must be on the list. A home or mobile connection changes IP often. When
   it changes, the run stops in a few seconds with a message naming the IP to add.

## Run it

The quickest way, from the repository root:

```bat
scripts\run-election-walkthrough.bat setup
scripts\run-election-walkthrough.bat resume
```

Each time, in every mode, the script:

1. stops a walkthrough run left from before, with its browser windows,
2. checks the tools above and installs any missing package or browser,
3. for `setup` and `full` only, deletes all election test data from earlier runs, as
   `clean` does. `resume` keeps it, because it carries on with the election setup made.
4. starts the API and the web server in their own windows when they are not running,
5. checks that the web proxy reaches the API, runs the test, and opens the report.

| Mode | What it does |
|---|---|
| `setup` (default) | New election up to the campaign phase, then saves a checkpoint. About 3 minutes. |
| `resume` | From the checkpoint: polling, votes, close, count, results, recounts, archive. It waits for the polling window set in `scenario.json`. |
| `full` | Setup and resume in one run. |
| `clean` | Deletes what the walkthrough made and restores the borrowed accounts. |
| `restore` | Only restores the borrowed accounts. |

Options can follow the mode: `keep` also leaves the borrowed accounts on the test password
after the run, `headed` shows the browsers, and `slow` shows them with 400 ms between actions. For example `scripts\run-election-walkthrough.bat full slow`.

`demo` shows the browsers and skips the real-time waits. Polling cannot open, close or let the
key be destroyed before the dates set when the election was made, and the API cannot change
those dates. So at each wait the run writes new dates straight to the local database, a few
seconds ahead. The audit trail does not show these moves, and the report says the run was a
demo. For a live demonstration of the whole election:

```bat
scripts\run-election-walkthrough.bat full demo
```

`slow` and `demo` can be used together, for example `full slow demo`. Every mode with
`headed`, `slow` or `demo` shows the browsers. `clean` and `restore` never open one.

### From any folder

The script finds the repository from its own path, so it runs from any folder when called by
its full path. Replace `<repo>` with the folder you cloned into.

| Option | Meaning |
|---|---|
| `headed` | Shows the browser windows. Runs at normal speed. |
| `slow` | Shows the browsers with 400 ms between actions, so people can follow each step. |
| `demo` | Shows the browsers, moves the polling open, close and recount end dates to now in the local database instead of waiting for them, and skips the network-idle wait on page loads. |
| `slow demo` | Both: slow steps, and no wait for polling. |
| `keep` | Leaves the borrowed accounts on the test password after the run. Works with any mode. |

```powershell
# PowerShell
$W = "<repo>\scripts\run-election-walkthrough.bat"

# setup: deletes old test data, makes a new election up to the campaign phase, saves a checkpoint
& $W setup headed        # visible, normal speed
& $W setup slow          # visible, 400 ms per step
& $W setup demo          # visible, no network-idle wait on page loads; the date moves happen in resume
& $W setup slow demo     # visible, 400 ms per step

# resume: carries on from the checkpoint with polling, votes, close, count, results, recounts
& $W resume headed       # visible, waits for the real polling window in scenario.json
& $W resume slow         # visible, 400 ms per step, waits for the real polling window
& $W resume demo         # visible, polling dates moved to now, about 2 minutes
& $W resume slow demo    # visible, 400 ms per step, polling dates moved to now

# full: setup and resume in one run, old test data deleted first
& $W full headed         # visible, waits for the real polling window
& $W full slow           # visible, 400 ms per step, waits for the real polling window
& $W full demo           # visible, polling dates moved to now
& $W full slow demo      # visible, 400 ms per step, polling dates moved to now

& $W full demo keep      # as full demo, and the accounts stay on the test password

# no browser
& $W clean               # deletes all walkthrough data and restores the borrowed accounts
& $W restore             # only restores the borrowed accounts
```

`resume` needs the checkpoint from an earlier `setup`; without it the script stops and says so.
For a talk, run `setup slow` beforehand and `resume slow demo` live, or `full slow demo` in one
sitting.

```bat
rem cmd.exe
"<repo>\scripts\run-election-walkthrough.bat" full slow demo
```

```bash
# Git Bash
cmd //c "<repo>\\scripts\\run-election-walkthrough.bat" full slow demo
```

Any mode and options from the table above go after the path in the same way.

### By hand

1. Start the API with the https launch profile, which sets Development. Without it the API
   starts as Production and stops on the http CORS origins.
   ```powershell
   cd GHCAA.API; dotnet run --launch-profile https
   ```
2. Start the web server in a second terminal: `cd GHCAA.Web; npm run start`.
3. Run the test from `GHCAA.Web`:
   ```powershell
   $env:WALKTHROUGH_STAGE='setup'; $env:WALKTHROUGH_FRESH='1'; npm run test:e2e:walkthrough
   Remove-Item Env:WALKTHROUGH_STAGE, Env:WALKTHROUGH_FRESH; npm run test:e2e:walkthrough
   ```

If the API is restarted while the web server keeps running, the web proxy can answer every
`/api` call with a 500. The test checks this first and stops with a message. Restart
`npm run start` to fix it.

## Settings

| Variable | Effect |
|---|---|
| `WALKTHROUGH_STAGE=setup` | Stop after the campaign phase and save a checkpoint. |
| `WALKTHROUGH_FRESH=1` | Ignore any checkpoint and start a new election. |
| `WALKTHROUGH_KEEP=1` | Leave the borrowed accounts on the test password at the end. |
| `WALKTHROUGH_DEMO=1` | Move the polling dates to now in the database instead of waiting for them. |
| `WALKTHROUGH_HEADED=1` | Show the browser windows. |
| `WALKTHROUGH_SLOWMO=400` | Milliseconds between browser actions. |
| `WALKTHROUGH_SCENARIO=path` | Use another scenario file in place of `scenario.json`. |
| `WALKTHROUGH_PYTHON=path` | The Python to run `scripts/local-election.py` with. |
| `GHCAA_LOCAL_API=url` | The API base for direct calls. Default `http://localhost:5087/api`. |

## Changing the input data

`scenario.json` holds everything the run uses:

- the election title, description, venue and notice reference,
- the EC period title and length,
- the timing: minutes until polling opens, polling length, the recount window and the
  silence period,
- which user plays the admin and each official,
- the seats, their candidates, proposers, seconders and statements, including one who
  withdraws and one who is rejected,
- the votes, and which voters use the web app,
- the recount requests and whether each is allowed.

People are given by user ID from the local database. To try other data, copy the file, edit
the copy, and set `WALKTHROUGH_SCENARIO` to it, so the default file stays as it is.

## The report

`GHCAA.Web/walkthrough-report/index.html`, with `report.json`, screenshots and downloaded
forms beside it. The folder is git-ignored. A failed run still writes the report, up to the
step that failed, with the error.

A step's screenshot is taken when the step ends, not after every click. Where one step clicks
several buttons, such as the Chief Commissioner recording every readiness item as done, the
items change all at once between two shots. The shots titled "Screen when the run stopped"
show each person's page at the moment of the failure, so one can be further on than that
person's last step shot.

## Clean-up

The election, its EC period and its votes stay in the local database after the run, so they
can be looked at in the app. The run restores the borrowed accounts, whether it passed or
failed, unless `keep` is given. The next `setup` or `full` deletes it before it starts,
because EC periods cannot overlap.

To delete all election test data, run `scripts\clean-election-test-data.bat`. It removes every
election and period the runs made, with their notifications, emails and codes, and restores
any borrowed accounts. Real elections and periods stay. `scripts\run-election-walkthrough.bat
clean` and `npm run test:e2e:walkthrough:clean` do the same. All of them read the state file at `%TEMP%\ghcaa-local-election\walkthrough-state.json`. The setup
checkpoint is `walkthrough-checkpoint.json` in the same folder.
