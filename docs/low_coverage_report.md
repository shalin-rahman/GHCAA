# Low Coverage Report (Generated on 2026-05-26)

> **Stale — do not quote these numbers, and do not set a gate against them.** Checked
> 2026-08-11: there is no `coverage.cobertura.xml` in the repo and nothing regenerates this
> report, so it reflects the code as it stood in May. The headline 0.43 % is also misleading
> in its own right — "1,002,454 lines valid" counts generated and vendor code, so the figure
> says more about what was measured than about what is tested. Since this ran, 26 service
> test files, 17 controller test files and a SQLite in-memory `TestBase` have landed.
> Rebuilding it from a real run scoped to first-party assemblies is tracked as **27.1**, and
> the 80 % gate that depends on it as **27.8**.

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
1. ~~**Add unit tests** for core services~~ — done: 26 service test files under `GHCAA.Tests/Services/`.
2. ~~**Add integration tests** for API controllers~~ — 17 controller test files exist, though they
   use Moq and direct instantiation rather than `WebApplicationFactory`. Whether to add real
   in-process HTTP tests on top is the open question in **27.3**.
3. **Exclude generated code from the measurement** rather than trying to cover it — counting EF
   migrations and vendor code is what produced the misleading 0.43 % above (**27.1**).
4. **Run coverage after each test run** and verify the `line-rate` moves towards the 80% target (**27.8**).

Once regenerated, replace this file wholesale — do not append to it.
