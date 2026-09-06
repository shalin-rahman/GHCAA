# ADR-0001: Two-format date contract (read dd-MM-yyyy, write ISO-8601)

Status: Accepted

## Context

`GHCAA.API/Utils/DateFormatConverter.cs` defines `DateFormatConverter` and
`NullableDateFormatConverter`, registered globally on the API's JSON options
(`Program.cs:189-190`). They don't read and write the same format.

`Read()` tries `dd-MM-yyyy` first (`DateTime.TryParseExact`) and falls back to `DateTime.Parse` if
that fails. `Write()` always emits `yyyy-MM-ddTHH:mm:ss.fff` — ISO-8601.

The write side changed for a concrete reason, recorded in the file's own comment: `dd-MM-yyyy` on the
wire is invalid input to JavaScript's `new Date(...)`, so a client-side `new Date(apiValue)` either
threw or silently produced `Invalid Date` depending on the browser. Angular's `DatePipe` has the same
problem. ISO-8601 is unambiguous to both.

The read side stayed permissive. Admin-entered dates and some older client payloads still send
`dd-MM-yyyy`, and rejecting them outright would break those call sites. `DateTime.Parse` as a fallback
also accepts ISO-8601, so the same converter reads what it writes.

## Decision

The API reads two date formats and writes one:

- **Read**: `dd-MM-yyyy` first, then `DateTime.Parse` (which also accepts ISO-8601) as a fallback.
- **Write**: always `yyyy-MM-ddTHH:mm:ss.fff` (ISO-8601), never `dd-MM-yyyy`.

This is deliberate, not an inconsistency to "fix" by making both sides use one format. Locking write
to ISO-8601 is what stopped the client-side parse failures; loosening read to accept both is what let
existing `dd-MM-yyyy` payloads keep working while that happened.

## Consequences

- A new client integration only needs to send ISO-8601 or `dd-MM-yyyy` — both work — but must not
  assume the API returns `dd-MM-yyyy`. It always returns ISO-8601.
- Removing the `dd-MM-yyyy` read path would be a breaking change for any caller still sending it, and
  needs a survey of actual traffic first, not just a grep for the format string.
- Any future date-handling converter or DTO added in `GHCAA.API` should match this asymmetry rather
  than inventing a third format.
