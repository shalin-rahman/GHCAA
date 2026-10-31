# Tasks: Election ballot secrecy and election standards

**Input**: [spec.md](./spec.md), [plan.md](./plan.md), [plan-officials.md](./plan-officials.md),
[delivery-plan.md](./delivery-plan.md)

**Purpose**: One line per requirement in spec.md, with the tracker item that carries it and its
state on 2026-10-31. The detail, tests and evidence live in the tracker entries in `docs/TODO.md`
and `docs/TODO_ARCHIVE.md`, not here. When an item changes state, change it here in the same edit.

A ticked line means the tracker item is DONE. PARTIAL and TODO items stay unticked.

## Phase 1: P0 secrecy and integrity (37.1i, 37.1j, 37.1k)

- [x] T001 FR-001 to FR-004: ballot write unlinked from the voter, GUID keys, server-made serial,
  one audit row per ballot (37.1i, sealed ballots in 37.1s).
- [x] T002 FR-005 and FR-006: whole ballot in one request, OTP step-up before voting (37.1j).
- [x] T003 FR-007 to FR-009: phase guards on count, declare and candidate changes (37.1k).
- [x] T004 FR-010: the `ElectionsController.Vote` doc comment and spec 010 claim no more secrecy
  than the code gives. Checked 2026-10-31 against the doc comment.

## Phase 2: P1 cast as intended and counted as cast (37.1l, 37.1m, 37.1n, 37.1z)

- [x] T005 FR-011 to FR-014: vote screen with names, abstain, review, and the server's voted
  state (37.1l).
- [ ] T006 FR-015 and FR-016: printable receipt and published tracking codes (37.1m, PARTIAL).
  Left: look at the printed receipt in a browser and on a phone, and the owner's choice on how
  Bengali names reach the mobile PDF. The server-made receipt is 37.14o, also PARTIAL.
- [x] T007 FR-017: public declared result (37.1n).
- [x] T008 FR-044 and FR-045: early eligibility warning and an explicit abstain on mobile (37.1z).

## Phase 3: P2 election rules (37.1o, 37.1p, 37.1q, 37.13b to 37.13e, 37.14)

- [x] T009 FR-018 and FR-019: multi-place seats and the stored tie rule (37.1o).
- [ ] T010 FR-020, FR-021, FR-023 and FR-024: recount, candidate consent, spoiled ballots and
  unopposed seats (37.1p, TODO).
- [ ] T035 FR-022: officer conflict check on nomination, candidate add, appointment and
  acceptance, with declared conflicts and stepping aside (37.16e, TODO).
- [x] T011 FR-025: ER forms filled from stored records (37.1q). The later form work in 37.14m and
  37.14n is PARTIAL.
- [x] T012 FR-046: candidate order fixed at CandidateList (37.13b).
- [x] T013 FR-047: test elections (37.13d), and the public board answers for one (37.14u).
- [x] T014 FR-048: hash-chained audit list per election (37.13a). The cascade and cut-tail gap is
  37.14d, PARTIAL.
- [x] T015 FR-049: per-seat opened ballots after declare (37.13c).
- [x] T016 FR-050: key-destroyed record after declare (37.13e).
- [x] T017 FR-051: final candidate list on the public board (37.16c, review gaps closed in 37.16d).
- [ ] T018 FR-052: polling waits for the recorded key copies (37.14a, TODO). The readiness gate
  it plugs into is 37.12w, DONE.
- [ ] T019 FR-053: public form verification proves a printed form is genuine (37.14t, TODO).

## Phase 4: the incoming committee (37.1t, 37.1u)

- [x] T020 FR-042: the election names a real, future EC period (37.1t).
- [x] T021 FR-043: winners written into that period on Declare (37.1u).

## Phase 5: officials, personas and access (37.1v, 37.1w, 37.12, 37.13)

- [x] T022 FR-026: step-up open to any signed-in user (37.1w).
- [x] T023 FR-027 to FR-030: personas, appointments, the ElectionOfficial role and the admin
  handover (37.12b to 37.12e).
- [x] T024 FR-031: two-person rule for sensitive steps (37.12f, closes 37.1v).
- [x] T025 FR-032: access ends by itself after declare or archive (37.12g).
- [x] T026 FR-033: public board of officials (37.12i).
- [x] T027 FR-034 to FR-037: count approval, executor, requester-only setting and defaults
  (37.13h, 37.13m, 37.13n, 37.13o).
- [x] T028 FR-038: election settings changes audited (37.13p).
- [x] T029 FR-039: rules frozen from polling to declare, with the SuperAdmin unlock (37.13q,
  37.13v, 37.13z).
- [x] T030 FR-040: emergency revocation during the freeze (37.13r, 37.13w, 37.13x, 37.13y).
- [x] T031 FR-041: Returning Officer flag on the appointment (37.13s).

## Not covered by a requirement

- [ ] T032 Manual accessibility check of the vote screens (37.13g, TODO).
- [ ] T033 Incident response, threat model and compliance table (37.13f, PARTIAL).
- [ ] T034 Extend polling (37.12y, TODO) and the ten-year purge by request (37.14f, TODO).
