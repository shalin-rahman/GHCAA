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

## Member identity validation (TODO 84.49)

- [ ] Allow a member to edit NID at any time; validate its format and uniqueness on every submission, including new record entry, as selected by the owner.
- [ ] Keep the PassingYear list usable when lookup rows are missing, and allow centrally managed rows when present in both Web and Mobile.
- [ ] Apply the age check using the Bangladesh local date so it does not shift near UTC midnight.
- [ ] Test valid, malformed, duplicate, imported-placeholder, and changed NID cases, plus PassingYear fallback and date boundaries.

The ballot-key progress dialog and timeout portion of 84.49 is election-device behavior and stays in the election delivery plan.
