# Chapter 9 — Verification, Validation and Quality Assurance

*[Chapter not written. The headings below are generated from `docs/DOCUMENTATION_BOOK_OUTLINE.md` and are kept in step with it: `build.py --strict` fails if a section exists in one and not the other.]*

**What this chapter owns.** How the system was verified: levels, techniques, test design, tooling,
what was measured and what the measurements cannot show.

**What it must not repeat.** Results and their interpretation, which are Chapter 12 and appear
nowhere here. This chapter says what was run and why that method was chosen; the chapter that says
what came out of it is Chapter 12. The separation is the reason both chapters exist.

## 9.1 Verification and Validation Strategy and Test Levels

*[Not written.]*

## 9.2 Software Quality Assurance Plan

*[Not written. Brief: reviews, standards conformance, defect prevention, and the role of formal technical reviews in this project. Two review sessions to draw on, and they differ in kind: the specification review of 3 July 2026 (§3.12, output = the 5 specification defects of Table 3.8), and the architecture and engineering audit of 4 September 2026 (`docs/ARCHITECTURE_AUDIT_2026-09.md`, output = 15 tracker items, 82.14-82.28). The second is worth treating separately: it was run against an external brief (`docs/materials/REVIEW.md`) rather than an internal checklist, it found defects in the *implementation and its documentation* rather than in the specification, and two of its own research streams failed, which the report states rather than conceals. That last point is the more honest material for this section than the findings themselves: a review that records its own incompleteness is the behaviour §12.11's validity-threat discussion should be able to point at]*

## 9.3 Test-Case Design Techniques Applied

*[Not written.]*

### 9.3.1 Equivalence partitioning

*[Not written.]*

### 9.3.2 Boundary value analysis

*[Not written.]*

### 9.3.3 Decision-table testing for the dues and eligibility rules

*[Not written.]*

### 9.3.4 State-transition testing derived from the state machines of Chapter 5

*[Not written.]*

### 9.3.5 Basis-path testing using the control-flow graph and cyclomatic complexity of §7.8

*[Not written.]*

### 9.3.6 Use-case and scenario-based testing

*[Not written.]*

### 9.3.7 Exploratory testing and its recorded charters

*[Not written.]*

## 9.4 Unit Testing

*[Not written. Brief: Backend]*

### 9.4.1 Framework, runner and project layout

*[Not written.]*

### 9.4.2 The definition of a unit in this system

*[Not written. Brief: and the reasoning for testing at the service boundary rather than at the controller or the repository]*

### 9.4.3 Test doubles

*[Not written. Brief: stubs, mocks, fakes and the in-memory provider; where each is appropriate and the fidelity each sacrifices]*

### 9.4.4 Test structure and naming: arrange–act–assert, given–when–then

*[Not written.]*

### 9.4.5 Testing the business rules that carry constitutional force

*[Not written. Brief: dues, eligibility, committee terms, voting rights — traced back to the rule catalogue of §5.6]*

### 9.4.6 Testing of failure and exception paths

*[Not written.]*

### 9.4.7 Test independence

*[Not written. Brief: determinism and the elimination of order dependence]*

## 9.5 Unit and Component Testing

*[Not written. Brief: Web Client]*

### 9.5.1 Runner, harness and component-testing strategy

*[Not written.]*

### 9.5.2 Testing signals, computed state and change propagation

*[Not written.]*

### 9.5.3 Testing guards, interceptors and the token-refresh queue

*[Not written.]*

### 9.5.4 HTTP mocking and contract fidelity against the live API

*[Not written.]*

## 9.6 Widget and Golden Testing

*[Not written. Brief: Mobile Client]*

### 9.6.1 Widget-test scope

*[Not written.]*

### 9.6.2 Golden (snapshot) testing: what it catches

*[Not written. Brief: what it cannot, and the platform-rendering problem that requires it to be skipped in continuous integration]*

## 9.7 Integration Testing Strategy

*[Not written. Brief: and the reasoning for rejecting big-bang integration]*

## 9.8 System and End-to-End Testing

*[Not written.]*

## 9.9 Regression Testing and Test Selection

*[Not written.]*

## 9.10 Security Testing

*[Not written. Brief: mapped to the threat model of Chapter 8 and to OWASP ASVS]*

## 9.11 Performance and Load Testing

*[Not written. Brief: workload model, environment, results]*

## 9.12 Usability and Accessibility Testing

*[Not written. Brief: task success, time on task, System Usability Scale scores, WCAG audit]*

## 9.13 User Acceptance Testing

*[Not written. Brief: participants, protocol, results, sign-off]*

## 9.14 Product Metrics and Static Analysis

*[Not written.]*

### 9.14.1 Size

*[Not written. Brief: lines of code and function points]*

### 9.14.2 Complexity

*[Not written. Brief: cyclomatic complexity distribution and the worst offenders]*

### 9.14.3 Coupling and cohesion

*[Not written. Brief: CBO, LCOM, afferent and efferent coupling, instability]*

### 9.14.4 Maintainability index and technical-debt estimate

*[Not written.]*

### 9.14.5 Test adequacy: coverage

*[Not written. Brief: and the argument coverage can and cannot support]*

## 9.15 Defect Analysis

*[Not written. Brief: density, distribution, removal efficiency, root-cause categories]*

## 9.16 Threats to the Validity of the Evaluation

*[Not written. Brief: with forward reference to §12.11]*

## 9.17 Summary

*[Not written.]*

## Figures and Tables

*[Not drawn. The outline specifies 20 artefacts for this chapter. Each is added here with its caption as it is made, then `renumber.py --apply` is run.]*

