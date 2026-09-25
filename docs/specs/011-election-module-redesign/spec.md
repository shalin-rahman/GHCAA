# Election module — web and mobile client redesign

## Scope

This specification covers new work: bringing the Angular (`GHCAA.Web`) and Flutter
(`GHCAA.Mobile`) election clients in line with the backend engine documented in
`docs/specs/010-election-engine-fixes/spec.md`. That spec covered backend-only defects. This
one covers the client-side gap found by a follow-up review of both clients on 2026-09-24: the
Angular member voting flow calls routes that do not exist on the backend at all, and both
clients carry dead code, missing admin UI, and inconsistencies with the rest of each codebase's
established patterns. Tracked as `docs/TODO.md` item 37.1h.

## Goal

Both clients drive the real per-seat nomination/vote/count API end to end — nothing calls a
route that doesn't exist — and both are built the same way the rest of each codebase is built:
reusing the campaigns module's (Angular) and governance-registry screen's (Flutter) established
components and conventions, not a one-off implementation.

## Backend addition

`GET /api/elections/current` (new) — returns the single election currently outside
`Announced`/`Archived`/`Declared` phase for the caller's EC scope, or `204 No Content`. Backed by
a new `IElectionService.GetCurrentAsync()`. This is the one gap in the backend surface: no
existing member-facing route lets a client discover which election is active. Everything else
the clients need (`nominations`, `vote`, `count`, `declare`, admin create/publish/close/candidate
management) already exists per the DTO/endpoint tables in `docs/specs/010-.../spec.md`.

## Angular, as it is today

`core/services/elections.service.ts` and `core/models/election.models.ts` each carry two
parallel model/method families: an older whole-election `Election`/`ElectionCandidate`/
`ElectionBallot` set that every component actually calls but that doesn't match the backend's
per-seat shape (the member voting page calls `GET /elections/current`, `GET
/elections/{id}/results`, `POST /elections/{id}/ballot` — none of which exist server-side, so
voting is broken in production today) — and a newer `NominationDto`/`CastVoteDto`/
`ElectionResultDto`/`ElectionSummaryDto` set that matches the backend exactly but that no
component calls. `admin-elections.ts`'s `publish()`/`close()` have no `saving`-style in-flight
guard, unlike `create()`. Both `create()` paths still send `createdBy` in the request body,
already meaningless since the backend derives officer identity from the auth claim.

## Angular, as redesigned

- Delete the unused whole-election model/method family; build every component against the
  `NominationDto`/`CastVoteDto`/`ElectionResultDto`/`ElectionSummaryDto` set.
- Member voting (`member/election/`): `GET /elections/current` → nominations grouped by seat →
  per-seat `CastVoteDto` submission. Track `votedSeatIds: number[]`, not a single `hasVoted`
  flag, since one election can have multiple seats a member votes on.
- Results (`public/elections/election-results.ts`): sourced from `POST
  /elections/{id}/count`'s response body, the only place `ElectionResultDto[]` is returned.
- Admin (`admin/elections/`): per-action `saving`-style signals for every mutation, matching
  `admin/campaigns/admin-campaigns.ts`'s `saving`/`confirmingReceipt` pattern; a new
  nomination/scrutiny tab, hand-rolled markup matching the existing tabbed-admin-page
  convention (no shared tabs component exists anywhere in the app); `ConfirmDialogService` for
  destructive actions instead of `window.confirm`.
- Reuse `PageHeaderComponent`, `LoadingPanelComponent`, existing `getStatusLabel/Class`-style
  helpers in `core/constants/app.constants.ts` for phase/nomination-status badges.

## Flutter, as it is today

`features/elections/election_service.dart` defines 11 methods; only `getNominations` and `vote`
are called from any screen. `currentElectionProvider` reaches into `ElectionService`'s private
`_dio` field directly rather than calling a public method, and its `catch (_) { return null; }`
swallows every error, so the screen's error UI branch is unreachable. `screens/member/
election_screen.dart` has no per-seat voted tracking, no phase check before showing the vote
button, and force-unwraps `photoPath!` without checking for an empty (non-null) string. There is
no Flutter admin election screen at all, despite `create`, `setPhase`, `freezeVoterRoll`, `count`,
`declare`, `downloadOfficialDocument` already existing on the service, unused.

## Flutter, as redesigned

- `ElectionService` gets a public `getCurrent()` method backed by `GET /elections/current`;
  `currentElectionProvider` calls it instead of reaching into a private field, and only catches
  `DioException`/expected failures.
- `election_screen.dart`: nominations loaded via a `FutureProvider` (not a raw `Future` field in
  `initState`), wrapped in the shared `AsyncValueWidget`; nominations grouped by
  `electionSeatId`; per-seat voted state tracked locally; vote affordance shown only when
  `election.phase == ElectionPhase.polling`; `photoPath` guarded against empty string before
  unwrapping.
- New `screens/admin/election_management_screen.dart`, modeled on
  `governance_registry_screen.dart`'s list + create/edit dialog + single in-flight flag per
  action pattern, wired to the currently-unused lifecycle methods; registered in
  `core/router/app_router.dart` alongside the other flat admin routes.
- Reuse `EmptyStateWidget`, `confirm_dialog.dart`'s `showConfirmDialog`, `LoadingPanel`/
  `LogoSpinner`. No new shared status-badge or busy-button widget — neither exists anywhere in
  the app; match the established per-screen convention instead.

## EC seat/position configurability (org-scoped)

Problem statement already recorded as `docs/specs/018-governance-elections-polls/spec.md`
ENH-009 — not repeated in full here. Summary: `ECPosition` is a fixed C# enum
(`GHCAA.Domain/Enums.cs`), so the 15-seat GHC committee shape is compiled into the binary.
`Lookups`/`LOOKUP_GROUPS.ECPosition` already moved the *display labels* out of code (see
`SeedEcPositionLookup` migration), but the *set* of seats an institution runs is still fixed —
another institution with a different EC structure needs a code change and redeploy to represent
it, which is the same class of problem WP62 already solved for `MembershipType` via
`membership-tiers.json` (label/policy layer over a fixed enum, not a string-key rewrite — see
`session_area62_genericization_plan.md`, "enum int values fixed" listed there as an accepted
non-breaking trap, not something every enum needs to escape).

This spec applies the same pattern to `ECPosition`, not a bigger one:

- **Backend.** A new org-scoped election-position config (reusing the `Lookups`/`OrgConfig`
  mechanism, not a new table type): per organization, which `ECPosition` enum values are active,
  their display order, and default `SeatCount`. `AdminElectionsController.Create`'s
  `ParsePosition` free-text matching (ENH-009's other half) validates against this org's active
  list instead of matching against every enum name regardless of org. A new
  `GET /api/elections/positions` (or an addition to the existing `/api/config` payload) returns
  the caller's org's active positions with labels and order, so clients build seat pickers from
  config instead of assuming "always these 15."
- **Not in scope.** Renaming the underlying enum values, or letting an org invent a position name
  that isn't one of the enum's defined values — that still needs a code change, same as
  `MembershipType`. Scoping it further than "which of the existing positions apply, in what
  order, how many seats" would be a bigger structural change than this budget-capped pass covers,
  and would break with the codebase's own established precedent for this exact kind of enum.
- **Angular.** `admin/elections/admin-elections.ts`'s seat-creation UI and
  `common/governance/governance.ts` (which per ENH-003 already duplicates the GHC position order
  in two places) both build their position list from the new config endpoint instead of a
  hardcoded array — fixing ENH-003 and ENH-009 with the same source-of-truth change.
- **Flutter.** `election_management_screen.dart`'s seat-creation form and
  `governance_registry_screen.dart` read the same config endpoint rather than a hardcoded list,
  if either currently hardcodes one — verify against current code before assuming, since
  `election_management_screen.dart` may already exist per `docs/specs/018-.../spec.md`'s FR-027
  evidence row.
- **Tests.** `GHCAA.Tests`: `ParsePosition` rejects a position outside the caller's org's active
  list; the new positions endpoint returns the seeded GHC 15 for the GHC org config. Vitest: the
  Angular seat-creation component builds its picker from the config response, not a literal
  array. `flutter test`: the same for the Flutter seat-creation form, if it doesn't already read
  from config.

## Non-functional requirements

- No client calls a backend route that doesn't exist.
- No client sends an identity field (`createdBy` or equivalent) that the server ignores in favor
  of the auth claim.
- Every state-mutating admin action has an in-flight guard preventing a double-submit.
- New UI reuses the shared component/pattern already established for its client and layer
  (campaigns module for Angular, governance-registry screen for Flutter admin), rather than
  introducing a new one-off pattern.

## Acceptance criteria

1. The Angular member voting flow completes end to end against the real API (vote for a seat,
   see it reflected as voted, blocked from re-voting the same seat, not blocked from a second
   seat).
2. The Angular results page renders `ElectionResultDto[]` from `/count`, not a nonexistent
   `/results` route.
3. Angular admin `publish`/`close`/candidate actions are each guarded against double-submit.
4. Flutter's `currentElectionProvider` surfaces real errors to the UI instead of swallowing them.
5. Flutter's member election screen shows per-seat voted state and only offers voting during
   `Polling`.
6. A Flutter admin can create an election, advance its phase, and declare results through the
   new admin screen.
7. `dotnet test`, `npm run test:unit`, and `flutter test` all pass with no regressions.
8. An org's active EC positions, their order, and default seat counts come from config for both
   clients' seat-creation UI — no client hardcodes "the 15 GHC positions," and
   `AdminElectionsController.ParsePosition` rejects a position not in the caller's org's active
   list rather than matching against the full enum.

## Evidence

To be filled in as each stage lands — see `docs/TODO.md` item 37.1h for stage-by-stage status.
