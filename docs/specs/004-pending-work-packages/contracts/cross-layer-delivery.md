# Cross-Layer Delivery Contract

Every pending product feature follows this contract.

| Layer | Required artifact |
|---|---|
| Domain | Entity, enum, archive semantics, relationships |
| Application | DTOs, interface, validator, privacy projection, error contract |
| Infrastructure | Service, EF configuration, indexes, provider migrations, storage/ledger integration |
| API | Thin controller, authorization, rate limiting where required, ProblemDetails, API registry entry |
| Web | Typed service/model, route/guard, loading/error/empty states, responsive UI, unit/E2E tests |
| Mobile | Typed Dio service/model, Riverpod state, route/screen if surfaced, offline/error behavior, tests |
| Documentation | SRS/feature catalog/project map/API registry/TODO evidence |
| Verification | Backend, Web, Mobile, contract, migration, security, and Graphify checks |

A client may be marked “not applicable” only when the specification records
that decision and a contract test confirms the API remains compatible.
