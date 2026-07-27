# Low Coverage Report (Generated on 2026-05-26)

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
1. **Add unit tests** for core services (`FinancialService`, `MemberService`, `IDCardService`) covering happy‑path and error cases.
2. **Add integration tests** for API controllers to hit the endpoints and increase line‑rate for the `GHCAA.API` package.
3. **Enable code coverage for generated code** (e.g., EF Core migrations) by adding tests that exercise the data layer.
4. **Run coverage after each test run** and verify the `line-rate` moves towards the 80% target.

Once additional tests are written, re‑run the coverage generation and update this report.
