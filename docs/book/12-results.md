# PART IV — EVALUATION AND CLOSURE

# Chapter 12 — Results, Evaluation and Discussion

**What this chapter owns.** What the evidence showed, and what it means. Every result in the
dissertation is reported here and only here.

**What it must not repeat.** How anything was tested or measured, which is Chapter 9, and what was
promised, which is §4.5. This chapter is read against Table 4.3 row by row, including the three
commitments §4.5 records as reduced or not run. §12.6 answers the four research questions directly;
RQ3's answer applies the five-class scheme of §3.7 to the delivered system.

## 12.1 Overview of the Delivered Artefact

The delivered system has three parts: an API that holds the rules and the data, a web client for
members and office holders, and a mobile client for members. It covers membership, dues, events,
committees, elections and the Association's published constitution. It runs today on the shared
test environment described in §10.1, deployment architecture, environments and containers.

*[Numbers needed: endpoints, controllers, backend tests, web tests and mobile tests, each from a
command run against the tree with the date. The counts in the README are dated 14 October 2026 and
must be taken again.]*

## 12.2 Functional Evaluation

The first commitment of Table 4.3 is that every requirement of priority Must is traced to code and
to a test. The traceability matrix is Table 3.4 in §3.6, prioritisation and traceability.

*[Result needed: the share of Must requirements traced to code and test, the list of any that are
not, and the number of review defects resolved against the number recorded.]*

## 12.3 Quality Evaluation against ISO/IEC 25010

Each quality characteristic is reported with the metric of Table 4.1 and its threshold. Where a
metric was not taken, the row says so. An empty row is a finding, not something to hide.

*[Results needed: one short paragraph per characteristic, covering functional suitability,
performance efficiency, compatibility, usability, reliability, security, maintainability and
portability, each with its metric value from §9.12, product metrics, static analysis and defect
analysis.]*

## 12.4 Performance, Security and Usability Results

Performance was measured for a single client only, as §9.9, performance and load testing, explains.
The result can show whether one user waits too long. It cannot show how the system behaves under
load.

*[Results needed: 95th-percentile response time per measured endpoint, with the date and the
hosting tier.]*

Security is reported as my own assessment against OWASP ASVS level 2. A requirement marked as met
has not been checked by anyone independent.

*[Results needed: ASVS requirements met, not met and not applicable, by chapter, from §8.11, ASVS
conformance and residual risks.]*

Usability is reported as task success per office and a System Usability Scale score set against the
benchmark of 68.

*[Results needed: task success and time per task by office, the SUS score with the number of
participants, and the heuristic findings by severity. Sessions are not yet recorded in
`docs/book/instruments/responses`.]*

## 12.5 Stakeholder and Expert Evaluation

Two sources are used. Office holders judged the system against their own work in the acceptance
sessions of §9.11, user acceptance testing. The formal reviews of §9.2, the software quality
assurance plan, judged the specification and the architecture, with the findings logged in
`docs/BUSINESS_FINDINGS.md`.

*[Results needed: acceptance outcome by office, and review findings by status: resolved, open and
accepted as a limitation.]*

## 12.6 Answering the Research Questions

Each question is answered below with the evidence behind the answer and how far that evidence
reaches. Where the evidence is not yet in, the answer is not given.

**RQ1** asks what requirements characterise a platform of this kind. The answer is the requirement
set of Chapter 3, and its fitness is judged by the trace of §12.2, functional evaluation, and the
defects the specification review found.

*[Answer pending the result of §12.2, functional evaluation.]*

**RQ2** asks which architecture meets those requirements for a single maintainer on a small budget.
The answer offered is the layered single-deployment design of Chapter 6, judged by the
maintainability metrics and by the cost of §10.6, operating cost, maintenance and handover.

*[Answer pending the metric values and the cost figures.]*

**RQ3** asks how far the Association's governance can be put into software without losing its
legitimacy. §3.7, domain constraints, sorts each constraint into five classes: Deterministic,
State, Evidence, Procedural and Authority. The method of answering is fixed even though the count
is not. The first two classes can be enforced by code alone. The Evidence class can be supported by
code that keeps a record, but a person still judges the record. The Procedural and Authority
classes stay with people, and the software can only refuse to proceed until the person has acted.

*[Answer pending: the number of constraints in each class, how many of priority M are traced and
enforced, and the cases where the software had to leave the decision to an office holder.]*

**RQ4** asks what measured quality the system reaches. The answer is §12.3, quality
evaluation against ISO/IEC 25010, and §12.4, performance, security and usability results, taken
together and read with the three reduced commitments in mind.

*[Answer pending the results of those two sections.]*

## 12.7 Discussion

The literature of §2.7, digital governance and e-voting, warns against moving a vote online without
keeping the checks that made the paper vote trusted. The design here follows that warning. It keeps
the offices, the forms and the seals of the paper process and puts the record-keeping into
software. Whether that was enough is a question for the results of §12.6.

The survey of §2.9, existing systems, found that general alumni products do not carry an
institution's own constitution. The gap stated in §2.11, the research gap, was that no reported system
encodes one. I can say the system was built. I cannot yet say how well it holds.

*[Discussion pending the results: where the findings agree with the literature of Chapter 2 and
where they differ.]*

## 12.8 Comparison against the Existing Manual System

Before this project the Association kept its records in paper files and spreadsheets and ran its
elections on paper. The comparison is made on what an office holder has to do for the same task,
before and after.

*[Comparison needed: for registration, dues collection and one election step, the manual steps set
beside the system's steps, from the task scripts and the office holders' accounts. No timing claim
is made unless it was measured.]*

## 12.9 Threats to Validity

The threats are grouped in the usual four kinds [32].

Construct validity is about whether the measures mean what I say they mean. Coverage is not test
quality, and a usability score is not usefulness. I report each measure as what it is and do not
stretch it. The performance figure is the clearest case: it is a single-client response time and is
named as that everywhere.

Internal validity is about whether something else explains the results. I built the system, wrote
the tests and assessed the security. One person in all three roles will miss the same things three
times. The two formal reviews are the only check from outside my own reading, and the second
records that two of its streams did not complete. The backend tests run on SQLite and an in-memory
store and not on PostgreSQL, so a fault that only PostgreSQL shows could pass them.

External validity is about whether the conclusion holds elsewhere, and here the threat is heaviest.
The platform was affordable because I brought my own project library from 2024 and did not charge
for my time. The conclusion that a small institution can build and run such a system transfers only
to an institution that has such a person. There is also one case, one Association and one
constitution.

Conclusion validity is about whether the numbers support the claims. The usability sample is small
and has no control condition, so I describe it and do not test hypotheses on it.

## 12.10 Limitations of the Artefact

The system runs as a single instance and cannot be scaled out without first moving its caches out
of process. No database backup is documented. No automated security scanner runs, and there has
been no penetration test. No alert is raised when the system fails. The mobile client's layout
tests run on one machine only. Mutation testing was not run. One person can maintain the system
today.

## 12.11 Reflection on the Design Science Contribution

The artefact is the system. The contribution that may travel is the way of sorting an institution's
rules before building anything: which rules a program can enforce, which it can only record, and
which must stay with a named office. Another association with a different constitution could use
the same five classes and would get a different split.

What does not travel is the cost. §12.9, threats to validity, gives the reason.

## 12.12 Summary

This chapter sets out where each result belongs and how each research question will be answered.
The measured values are not yet entered. The threats and limitations do not depend on those values
and are stated in full.

## Figures and Tables

*[Not drawn. The outline specifies 9 artefacts for this chapter. Each is added here with its caption as it is made, then `renumber.py --apply` is run.]*
