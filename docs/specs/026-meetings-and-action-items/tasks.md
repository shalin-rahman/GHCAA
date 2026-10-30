# Meetings and action items tasks

Tracker source: 84.19 and 84.20 in [docs/TODO.md](../../TODO.md). Keep status, dependencies, and acceptance criteria in the tracker.

## Sequence

- [ ] **84.8 and 84.18 — Complete prerequisites.** Finish the platform baseline and entity gap review; record intended users, governance fit, and visibility expectations.
- [ ] **84.19 — Approve the meeting scope.** Resolve the open points in `spec.md`; review reuse of the event invite and RSVP flow; approve the spec before code.
- [ ] **84.19 — Build the meeting module.** Add the domain and persistence model, application contracts, authorized API, Web and Mobile surfaces, migration, audit behavior, and cross-layer tests.
- [ ] **84.20 — Add contextual action items.** Support an owner, due date, and status for meeting actions and any EC work-item use accepted by the 84.18 review. Keep standalone notes and generic task management out unless the review establishes need.
- [ ] **84.20 — Verify and document.** Exercise role access, RSVP, agenda ordering, minutes privacy, and action-item transitions; update API registry, project map, feature catalog, and governing spec evidence.

## Completion evidence

- Approved specification records the choices from 84.18.
- Backend, migration, Web, Mobile, and API contract checks cover the accepted scope.
- Tests demonstrate restricted minutes and unauthorized write rejection.
- Tracker items close only against their own acceptance criteria.
