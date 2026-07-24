---
name: gotcha-date-format-converter
description: API DateTime wire format is ISO-8601 (canonical, both directions); dd-MM-yyyy is display/input only. Custom global JsonConverter writes ISO, reads ISO + legacy dd-MM-yyyy.
metadata: 
  node_type: memory
  type: project
  originSessionId: 80e0e324-13a0-4d9f-b036-6a6d47d35135
---

`GHCAA.API/Utils/DateFormatConverter.cs` defines `DateFormatConverter`/`NullableDateFormatConverter`, registered globally in `Program.cs` (`options.JsonSerializerOptions.Converters.Add(...)`). This means **every** `DateTime`/`DateTime?` property on **every** API response goes through this converter — it is not scoped per-DTO.

Historically `Write()` emitted `dd-MM-yyyy` (e.g. `"24-01-2026"`), which is invalid input to JS `new Date(...)` and threw `NG02100`/`NG02311` in Angular's `DatePipe` (crashed the Events page). Fixed by changing `Write()` on both classes to emit ISO-8601 (`yyyy-MM-ddTHH:mm:ss.fff`, no `Z`/offset — app treats dates as local Bangladesh time elsewhere, see `ThemeService.GetBangladeshTimeNow()`). `Read()` was left unchanged since it already falls back to `DateTime.Parse` for non-`dd-MM-yyyy` strings.

**Why:** Non-standard wire date formats break client-side parsing silently in most places (`Invalid Date`) but loudly in Angular's strict `DatePipe`. 32 frontend files use `date:'dd-MM-yyyy'` as an Angular pipe *display*-format argument — that's unrelated to wire format and unaffected by this fix.

**How to apply:** Before assuming an API date field parses cleanly client-side, remember it goes through this converter. If a new DatePipe/`new Date()` crash shows up, check whether the value came from a `DateOnly` field (unaffected by this converter) or a `DateTime`/`DateTime?` field.

**FINALIZED CONTRACT (2026-07-25):** ISO-8601 is the canonical WIRE format both directions; `dd-MM-yyyy` is DISPLAY/INPUT only. API is done (Write=ISO, Read accepts ISO + legacy dd-MM-yyyy exact-match). The fix was the client WRITE path — clients used to send `dd-MM-yyyy` on the wire. Now:
- Web: `GHCAA.Web/src/app/core/utils/date.util.ts` — `toWireDate()` (dd-MM-yyyy→yyyy-MM-dd, string swap, no TZ shift) MUST wrap every date field in outgoing request bodies; `toDisplayDate()` for render; `parseDisplayDate()` for validating typed dates (never `new Date('dd-MM-yyyy')`). 9 write-path components + register.ts routed through it.
- Mobile: `AppUtils.toWire()` (→yyyy-MM-dd) for all outgoing bodies; `formatDate`/`parseDate` now ISO-first. DOB fixed in `RegisterModel.toJson()`.
- Skill `ghcaa-date-standard/SKILL.md` rewritten to this two-format contract; TODO Area 29F.3 = DONE.
The recurring bug to watch: forgetting `toWireDate`/`toWire` on a NEW send site → API silently receives dd-MM-yyyy. Also never use `toISOString()` for a date-only field (UTC shift moves DOB a day).
