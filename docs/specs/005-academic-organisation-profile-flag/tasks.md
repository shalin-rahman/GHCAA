# Tasks: academic organisation-profile flag

- [x] Rename the Domain, Application, Infrastructure, API, Web, and Mobile
      references to `IsOrgProfile`/`isOrgProfile`.
- [x] Add legacy `isGHC` request binding without legacy response output.
- [x] Add the reversible PostgreSQL column-rename migration.
- [x] Update the current EF snapshot and shipped academic seed files.
- [x] Add or extend focused tests for legacy JSON binding and migration
      metadata. The DTO compatibility test, validator suite, and API build
      cover the binding and current model metadata.
- [x] Run the backend, Web, Mobile, and Graphify checks. The Swagger comparison
      still reports unrelated route drift in the committed snapshot, so the
      repository-wide contract gate remains open.
