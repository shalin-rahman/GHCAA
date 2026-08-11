# Memory Index

- [project_ghcaa.md](project_ghcaa.md) — Stack, layer structure, test/build commands, key conventions
- [business_flow.md](business_flow.md) — Member lifecycle, payment flow, OTP flow, EC governance, key config keys
- [architecture.md](architecture.md) — Clean Architecture layer rules, DI wiring, middleware pipeline order, EF patterns, Angular/Flutter conventions
- [project_map.md](project_map.md) — project_map.md at repo root has the full class/service/route map; read it before broad file searches
- [feedback_security_phases.md](feedback_security_phases.md) — Security hardening S1–S9 complete; all Area 24 items now done (296 tests pass)
- [outstanding_todos.md](outstanding_todos.md) — Area 24 + UI/security/usability review both done; only 2 pending DB migrations remain (PhaseB + AddRefreshTokens)
- [session_area24_completion.md](session_area24_completion.md) — All new entities/endpoints/utilities added in Area 24 sessions: RefreshToken, /auth/refresh+me+logout, LoginRateLimitMiddleware, HtmlSanitizer, file-validation.util, angular interceptor 401-queue
- [session_ui_security_usability_review.md](session_ui_security_usability_review.md) — API/Web/Mobile UI+security+usability review plan fully shipped; EmptyStateWidget, mobile branding theme, screenshot protection, XSRF, IDOR fix, etc.
- [feedback_keep_lightweight.md](feedback_keep_lightweight.md) — keep app(s) lightweight; avoid new deps/abstractions without concrete duplication to justify them
- [gotcha_date_format_converter.md](gotcha_date_format_converter.md) — API globally serializes DateTime via custom converter (now ISO-8601, was dd-MM-yyyy); don't assume ISO by default
- [feedback_sri_hash_verification.md](feedback_sri_hash_verification.md) — never fabricate SRI hashes from memory; always compute from downloaded bytes
- [session_logo_spinner.md](session_logo_spinner.md) — shared LogoSpinner components (web+mobile) replacing ad hoc spinners app-wide
- [session_portal_member_photo.md](session_portal_member_photo.md) — top-right header avatar now shows real member photo across all /portal/* pages; test with member Id 201 for a matched photo
- [gotcha_seed_json_vs_live_db.md](gotcha_seed_json_vs_live_db.md) — editing Seed/*.json alone does NOT update the already-created SQLite DB's Members table; check live DB directly when debugging
- [reference_docs_folder.md](reference_docs_folder.md) — what's in the repo /docs folder (BUSINESS_FINDINGS, config framework, payment workflow); read before re-reviewing
