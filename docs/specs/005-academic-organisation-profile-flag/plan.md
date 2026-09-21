# Plan: academic organisation-profile flag

1. Rename the Domain and Application property and update all service predicates.
2. Rename Web and Mobile models, controls, and request fields.
3. Add a reversible PostgreSQL column-rename migration and update the current
   EF model snapshot.
4. Keep legacy JSON input binding for the rollout window without exposing the
   old response field.
5. Update seed data, API contract evidence, and tests.
6. Run backend, Web, Mobile, contract, and Graphify checks.
