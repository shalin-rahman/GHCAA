# Chapter 11 — Project Management

*[Chapter not written. The headings below are generated from `docs/DOCUMENTATION_BOOK_OUTLINE.md` and are kept in step with it: `build.py --strict` fails if a section exists in one and not the other.]*

## 11.0 The Four P's Applied

*[Not written. Brief: People, Product, Process, Project, with the single-maintainer case stated plainly against each. None of Pressman's four organisational paradigms (closed, random, open, synchronous) describes one unpaid maintainer; saying so is the finding, not forcing a label]*

## 11.1 Process Model in Practice and its Deviations from Plan

*[Not written. Brief: led by the arrival profile: **67% of delivered tasks were not planned** (409 of 615, figures of 2 September 2026), arriving as stakeholder feedback (35%), review findings (18%) or defects (14%), and 26 of the dated areas landed in August 2026 alone. A critical path over an up-front work breakdown would be fiction, because two thirds of the work did not exist when that breakdown would have been drawn]*

## 11.2 Work Breakdown Structure

*[Not written. Brief: the four streams above; the 17 code components each tied to the tracker areas that produced them, so the activity list and the tracker are one list read two ways]*

## 11.3 Scheduling, Task Network and Critical Path

*[Not written. Brief: activity-on-node by the precedence diagram method, with duration, float, early and late start and finish. Reported as a **retrospective** network: critical path 48 working days against 63 worked and 206 elapsed. The gap is availability, not dependency, and that is the section's point. Persistence and security behave as **hammock activities**, touched on 42 and 33 separate days across the whole span, and are drawn as such rather than as boxes at day zero]*

### 11.3.1 CPM summary ordered by float

*[Not written. Brief: so the schedule can be read by slack rather than by sequence]*

### 11.3.2 Crashing analysis

*[Not written. Brief: and the honest result: the single resource on the critical path cannot be crashed, so every classical crashing lever is unavailable. What shortened the schedule instead was scope deferral, recorded in the Won't set of §3.8]*

## 11.4 Effort Estimation

*[Not written. Brief: the function-point chain, computed from the delivered system: EI, EO and EQ from the 276 endpoint attributes, ILF from the 49 `DbSet` properties, EIF from the four payment gateways plus email, SMS and social identity. UFP, then TDI over the fourteen general system characteristics, **VAF = 0.65 + 0.01 × TDI**, AFP, effort at a stated productivity factor, LOC via the language factor, and cost in BDT. **COCOMO II is dropped**: its five scale factors and seventeen effort multipliers cannot be justified here, and a model nobody can defend adds no evidence]*

### 11.4.1 Two estimates compared

*[Not written. Brief: apportioned commit-days against completed-task counts, on a common base. Eight of the seventeen components disagree by more than twofold: authentication, governance, gallery and security look heavy by task count because the tracker holds many small items there; persistence, events and the job board look heavy by commit-day because a handful of items each took days. **Task granularity varies by an order of magnitude between components, so any estimate built on task counts inherits that noise** — a stronger result than either estimate alone]*

### 11.4.2 The estimate against the actual

*[Not written. Brief: 97 days, about 4.4 person-months across all four streams, against the function-point model's prediction at the standard productivity factor. The model over-predicts by roughly two orders of magnitude, and the reasons are stated: framework scaffolding, no coordination overhead, no separate quality-assurance or project-management roles, and generated code counted as delivered function. The productivity factor assumes a team; there is no team]*

## 11.5 Progress Tracking and Earned Value

*[Not written. Brief: BCWS, BCWP, BAC and ACWP, with **SPI = BCWP/BCWS** and **CPI = BCWP/ACWP**, reconstructed from the dated tracker items and the commit record, which is the only effort evidence this project has. The reconstruction and its limits are stated as such; no weekly earned-value record was kept, and the chapter does not pretend one was]*

## 11.6 Team Structure and Responsibilities

*[Not written. Brief: one maintainer holding every role the four P's assign to different people, and what that costs: no independent review, and no separation between the person who declares a payment rule and the person who tests it. The mitigation was mechanical, being the tracker and the test suite, and §12.11 treats it as a validity threat]*

## 11.7 Configuration and Change Management in Practice

*[Not written. Brief: there was no change-control board. There was one file: `docs/TODO.md`, 3,839 lines, simultaneously the project plan, the change log, the defect log and the decision record. The 409 reactive tasks *are* the change log, and Table 11.5 is generated from them rather than reconstructed]*

## 11.8 Risk Monitoring Record

*[Not written. Brief: the RMMM plan of §4.8 as it was executed, with **risk exposure RE = P × C** computed per risk and impact on the 1–5 scale, plus one worked Risk Information Sheet. §4.8 needs revising to carry RE; the impact costs are author-stated and have to be supplied]*

## 11.9 Quality Assurance Activities Performed

*[Not written. Brief: against Pressman's 40‑20‑40 allocation, this project spent roughly a third of its evidenced effort on feature code and about 15% on testing, well under the 40% prescribed. Data-backed self-criticism, with **DRE = E / (E + D)** computed from the findings log as E and live defects as D, and **MTTC** named as what QAS-06 already measures]*

## 11.10 Lessons in Project Management

*[Not written.]*

## 11.11 Summary

*[Not written.]*

## Figures and Tables

*[Not drawn. The outline specifies 12 artefacts for this chapter. Each is added here with its caption as it is made, then `renumber.py --apply` is run.]*

