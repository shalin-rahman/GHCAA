# Low Coverage Report (Generated on 2026-05-26)

> ## STALE — do not quote these numbers (reviewed 2026-08-22)
>
> This report is a **single historical snapshot from 2026-05-26** and has not been regenerated.
> Two things are wrong with reading it as current:
>
> 1. **The suite has grown a lot since.** `dotnet test` on 2026-08-22 = **330 passed / 0 failed**
> (web `vitest` = 60 files / 244 tests). Most of the `0.00` rows below — `MemberService`,
> `FinancialService`, the API controllers — now have real tests under `GHCAA.Tests/Services/`
> (~20 classes) and `GHCAA.Tests/Controllers/` (18 classes). See TODO 27.3–27.6, all DONE.
> 2. **The headline percentage is meaningless.** "Total lines valid: 1,002,454" for a codebase this
> size means the coverlet run swept generated/vendor output (`bin/`, `obj/`, migrations), so the
> 0.43% line-rate is an artifact of the denominator, not a measurement. No coverage threshold is
> enforced anywhere in CI and README deliberately declines to publish a figure (TODO 27.8 open).
>
> **UPDATE 2026-08-22 (same day, later): the converter gap below is CLOSED.**
> `GHCAA.Tests/Utils/DateFormatConverterTests.cs` now covers both types with 20 passing tests
> (backend suite: **350 passed / 0 failed**), pinning ISO-8601 on write, dd-MM-yyyy-then-ISO on
> read, day-first precedence for ambiguous input, the empty-string divergence between the nullable
> and non-nullable converters, and `FormatException` on malformed input. **TODO 27.7 is DONE.**
> The paragraph below is retained as the rationale for why it mattered.
>
> **What was still true (before that fix):** `DateFormatConverter` / `NullableDateFormatConverter` remained at **0.00 /
> 0.00** — genuinely untested as of 2026-08-22, not just stale data. That was tracked as **TODO 27.7**
> and is the highest-value remaining gap in Area 27, because those two converters sit in the global
> `JsonSerializerOptions` (`GHCAA.API/Program.cs`, ~lines 137–138) and therefore govern the
> serialization of **every** `DateTime` crossing the wire, in both directions, for every endpoint.
> A silent regression there re-breaks the ISO-8601 contract settled in 29F.3 across the whole API at
> once, with no failing test to catch it. Minimum coverage worth adding: round-trip ISO-8601, the
> legacy `dd-MM-yyyy` read path, `null` handling in the nullable variant, and malformed input.
>
> Before citing any number here, regenerate:
> `dotnet test --collect:"XPlat Code Coverage"` with `bin`/`obj`/`Migrations` excluded.

## Summary
- **Overall line coverage**: **0.43%** (line-rate="0.0043")
- **Overall branch coverage**: **24.82%** (branch-rate="0.2482")
- **Total lines valid**: 1,002,454
- **Lines covered**: 4,338
- **Branches valid**: 3,622
- **Branches covered**: 899

## Files / Classes with Coverage < 80%
*(All listed files are below the 80% threshold. Only a subset is shown for brevity.)*

| Package | Class / File | Line‑rate | Branch‑rate |
|---------|--------------|----------|------------|
| GHCAA.API | Program.cs (GHCAA.API\\Program.cs) | 0.00 | 0.00 |
| GHCAA.API | DateFormatConverter.cs (GHCAA.API\\Utils\\DateFormatConverter.cs) | 0.00 | 0.00 |
| GHCAA.API | NullableDateFormatConverter.cs (GHCAA.API\\Utils\\DateFormatConverter.cs) | 0.00 | 0.00 |
| GHCAA.API | RealTimeService.cs (GHCAA.API\\Services\\RealTimeService.cs) | 0.00 | 1.00 |
| GHCAA.API | AuditLogMiddleware.cs (GHCAA.API\\Middleware\\AuditLogMiddleware.cs) | 0.00 | 1.00 |
| GHCAA.Infrastructure | FinancialService.cs (GHCAA.Infrastructure\\Services\\FinancialService.cs) | 0.00 | 0.00 |
| GHCAA.Infrastructure | IDCardService.cs (GHCAA.Infrastructure\\Services\\IDCardService.cs) | 0.00 | 0.00 |
| GHCAA.Infrastructure | MemberService.cs (GHCAA.Infrastructure\\Services\\MemberService.cs) | 0.00 | 0.00 |
| ... | ... | ... | ... |

> **Note**: The XML shows most classes have a line‑rate of `0` because the unit‑test suite currently does not exercise any code paths in those classes.

## Recommended Actions

*(Original 2026-05-26 list — items 1 and 2 have since been carried out; see the staleness note above.)*

1. ~~**Add unit tests** for core services (`FinancialService`, `MemberService`, `IDCardService`) covering happy‑path and error cases.~~ — done, TODO 27.4.
2. ~~**Add integration tests** for API controllers to hit the endpoints and increase line‑rate for the `GHCAA.API` package.~~ — done, TODO 27.3.
3. **Exclude** generated code (`bin/`, `obj/`, EF migrations) from the coverage run rather than trying to cover it — the 1,002,454-line denominator above is what happens otherwise.
4. **Run coverage after each test run** and verify the `line-rate` moves towards the 80% target.

### Still outstanding (2026-08-22)

5. **`DateFormatConverter` + `NullableDateFormatConverter` have zero tests** — `GHCAA.API/Utils/DateFormatConverter.cs`, registered globally in `GHCAA.API/Program.cs` (~137–138). Highest-priority gap in Area 27: they mediate every `DateTime` on every request and response, so an untested change there breaks the whole API's date contract silently. Tracked as **TODO 27.7**.
6. **No coverage threshold is enforced** in CI (**TODO 27.8**). `coverlet.collector 6.0.2` is referenced, so coverage can be collected locally, but nothing fails a build on regression. A per-file ≥80% gate would fail today.

Once additional tests are written, re‑run the coverage generation and update this report.
