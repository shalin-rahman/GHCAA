# Implemented activities missing from the tracker

This is a retrospective release list. It records implemented work found in the
repository that has no clear matching activity in `docs/TODO.md`. Work already
covered by an existing work package is excluded. The rows are ordered by
dependency. Dates come from the implementation history. `Not stated` means the
repository does not give that detail.

| Activity | Parent work package | Status | Component | Dependencies | Relevancy | Estimate | Dates | Suggested title |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| R1.1 | Release 1 - Retrospective | DONE | Infrastructure | None stated | Low | Not stated | 2026-08-30 | Remove the redundant Docker build step. |
| R1.2 | Release 1 - Retrospective | DONE | Build assets | R1.1 | Low | Not stated | 2026-08-30 | Remove obsolete platform resource files from the build output. |
| R1.3 | Release 1 - Retrospective | DONE | Documentation tooling | None stated | Medium | Not stated | 2026-09-03 | Generate the tracker page and update WBS calculations automatically. |
| R1.4 | Release 1 - Retrospective | DONE | CI/CD | None stated | Medium | Not stated | 2026-08-23 | Compress deployment packages and use the correct artifact paths. |
| R1.5 | Release 1 - Retrospective | DONE | Web dependencies | None stated | Medium | Not stated | 2026-08-29 | Update the Angular dependencies to the verified release versions. |
| R1.6 | Release 1 - Retrospective | DONE | Web editor | None stated | Medium | Not stated | 2026-08-31 | Clean up the rich-text editor when its component is destroyed. |
| R1.7 | Release 1 - Retrospective | DONE | Web authentication | None stated | High | Not stated | 2026-08-28 | Restore the user session without triggering an Angular change-detection error. |
| R1.8 | Release 1 - Retrospective | DONE | Backend, Web hosting | R1.1 | Medium | Not stated | 2026-08-28 | Improve static-file fallback and asset retrieval after deployment. |
| R1.9 | Release 1 - Retrospective | DONE | Test data | None stated | Medium | Not stated | 2026-08-30 | Add the May 2026 member records and photo migration data. |
| R1.10 | Release 1 - Retrospective | DONE | Backend tests | None stated | Medium | Not stated | 2026-09-04 | Test guest payments that do not have a member ID. |
| R1.11 | Release 1 - Retrospective | DONE | Backend tests | None stated | Medium | Not stated | 2026-09-04 | Test organization configuration and financial audit history. |
