# Chapter 9 — Verification, Validation and Quality Assurance

**What this chapter owns.** How the system was verified: levels, techniques, test design, tooling,
what was measured and what the measurements cannot show.

**What it must not repeat.** Results and their interpretation, which are Chapter 12 and appear
nowhere here. This chapter says what was run and why that method was chosen; the chapter that says
what came out of it is Chapter 12. The separation is the reason both chapters exist.

## 9.1 Verification and Validation Strategy and Test Levels

Verification asks whether I built the system the specification describes. Validation asks whether
that system is the one the Association needs. I kept the two apart because they fail in different
ways. A test can pass against a rule that was written down wrongly, and only a person who holds the
office can say the rule is wrong.

The work is split into four levels. Unit tests check one service or one component on its own.
Integration tests start the real API host in memory and send it requests. End-to-end tests drive a
browser through a whole workflow, such as an election from nomination to result. Acceptance work is
done by people: office holders follow task scripts and say whether the outcome is the one they
expect. Most tests sit at the unit level because they are cheap to run and point at the fault when
they fail. The slower levels are kept for what a unit test cannot see, which is the wiring between
parts.

This chapter does not give any result. Each section says what was run and why. The numbers are in
Chapter 12, which is read against the commitments of §4.5, the evaluation strategy.

## 9.2 Software Quality Assurance Plan

Quality work in this project rests on three habits. Rules are written before code. Every change
passes the same automated gates before it reaches the shared environment. Design and specification
are reviewed by reading, not only by running tests.

Two formal reviews were held, and they differ in kind. The first was the specification review of
3 July 2026, described in §3.8, feasibility and requirements validation. It read the requirements
against the governing documents and found five defects in the specification. The second was the
architecture and engineering audit of 4 September 2026, recorded in
`docs/archive/ARCHITECTURE_AUDIT_2026-09.md`. It was run against an external brief,
`docs/materials/REVIEW.md`, and not against a checklist I wrote myself. It found faults in the
implementation and its documentation, and it produced fifteen tracker items, 82.14 to 82.28.

The audit also failed in part. Two of its research streams did not complete, and the report says
so. I treat that as the useful lesson of the exercise. A review that records where it fell short
can be trusted on the rest, and §12.9, threats to validity, relies on that record.

Defect prevention is mostly done by the pipeline of §10.2.1, pipeline stages and the quality gates
actually enforced. A change that breaks formatting, type checks or a test does not deploy.

## 9.3 Test-Case Design Techniques Applied

Test cases were not chosen by instinct alone. The techniques below are the standard ones, and each
subsection says where in this system the technique was used and where it was not.

### 9.3.1 Equivalence partitioning

Inputs are grouped into classes that the code should treat alike, and one case is taken from each
class. The API tests use this for request validation: a valid request, a request with a missing
field, and a request from a caller without the right role. Parameterised NUnit cases carry the
classes for several controllers, among them registration, elections, events and payment
configuration.

### 9.3.2 Boundary value analysis

Faults gather at the edges of a range, so the edges are tested directly. This matters most for
dates and counts: the first and last day of a period, an empty list, a seat count of one.

*[Evidence needed: name two or three boundary tests by file and test name, taken from the dues and
election test classes, before this subsection can claim more than the intent.]*

### 9.3.3 Decision-table testing for the dues and eligibility rules

The dues and eligibility rules depend on several conditions at once, such as membership type,
payment status and approval status. A decision table lists every combination and the outcome the
constitution requires, and each row becomes a test.

*[Table and evidence needed: the decision table itself, built from the rules catalogue of §5.5, and
the test names that cover each row. Not drawn in this pass.]*

### 9.3.4 State-transition testing derived from the state machines of Chapter 5

The state machines of §5.4, state modelling, say which moves are allowed. Tests are written for
each allowed move and for the moves that must be refused. The election service tests follow this
pattern for the election phases, and the membership tests follow it for registration and approval.

*[Evidence needed: a count of allowed and refused transitions tested per state machine, taken from
the test classes on the day the count is made.]*

### 9.3.5 Basis-path testing using the control-flow graph and cyclomatic complexity of §7.7

Basis-path testing takes one method, draws its control-flow graph and tests as many independent
paths as its cyclomatic number [57]. I apply it to one method only, as a worked example, because
doing it by hand for a whole codebase is not worth the time.

*[Not done yet: the control-flow graph of §7.7, the web and mobile clients, is not drawn. Once it is, list its cyclomatic
number, the independent paths and the test that covers each.]*

### 9.3.6 Use-case and scenario-based testing

The end-to-end walkthroughs are scenario tests. Each one follows a use case from start to finish
with the roles the use case names. There are three: elections, membership registration and
approval, and events with participation. Each has a README with a table of what the run covers, and
that table is kept in step with the test.

### 9.3.7 Exploratory testing and its recorded charters

Some faults are found only by using the system without a script. I did this throughout the build,
and findings went into the tracker as items.

*[Evidence needed: the sessions were not recorded as charters. Either reconstruct a short list of
sessions from tracker entries with their dates, or state plainly that no charters were kept.]*

## 9.4 Unit and Component Testing

The backend carries the business rules, so it carries most of the tests. The first seven
subsections cover the backend and the last four cover the web client.

### 9.4.1 Framework, runner and project layout

Backend tests use NUnit, with Moq for mocks and FluentAssertions for readable checks. They live in
one project, `GHCAA.Tests`, with folders that mirror the layers: controllers, services,
repositories, middleware, filters, validators and integration. One command runs them all, and a
filter runs one class.

### 9.4.2 The definition of a unit in this system

A unit here is one service method together with the domain objects it uses. I test at the service
boundary because that is where the rules live. Controllers are thin and mostly pass the call on, so
a controller test proves little beyond routing and authorisation. Repository tests prove that a
query runs, not that a rule holds. Both kinds exist, but in smaller number.

### 9.4.3 Test doubles

Three kinds of stand-in are used. Mocks replace collaborators such as the email sender, where I
want to check that a call was made. The EF Core in-memory provider replaces the database where only
simple reads and writes matter. SQLite replaces it where the test needs real relational behaviour,
such as constraints.

Each one gives up some fidelity. The in-memory provider does not enforce constraints or
transactions. SQLite does, but it is not PostgreSQL, and a query that passes on one can fail on the
other. No backend test runs against PostgreSQL itself, and §12.9, threats to validity, records that.

### 9.4.4 Test structure and naming: arrange–act–assert, given–when–then

Each test sets up its data, makes one call and checks the outcome, in that order. Names follow the
pattern method, condition, expected result, so a failing test says what broke without being opened.

### 9.4.5 Testing the business rules that carry constitutional force

Some rules come from the constitution and the election code: dues, eligibility, committee terms and
voting rights. A fault in one of these is not a bug in the ordinary sense. It is the system acting
against the Association's own law. Each such rule has an identifier in §5.5, the business rules
catalogue, and its tests are tagged with the requirement they serve so the trace can be followed
from rule to test.

*[Evidence needed: the count of constitutional rules with at least one tagged test, and the list of
any rule without one.]*

### 9.4.6 Testing of failure and exception paths

A rule is tested in both directions. For each thing a caller may do, there is a test for the caller
who may not. The integration tests also check that every error leaves the API in the same response
shape, so a client can rely on it.

### 9.4.7 Test independence

Each test builds its own data and removes it afterwards. No test relies on another having run
first. Tests that touch the file system use a fresh temporary folder each time. Time-dependent code
takes the clock as an input, so a test can fix the date.

### 9.4.8 Runner, harness and component-testing strategy

The web client is tested with Vitest. Services, guards, interceptors and pipes are tested as plain
classes. Components are tested through Angular's test harness, with inputs set the way the
framework sets them at run time.

### 9.4.9 Testing signals, computed state and change propagation

Client state is held in signals. A test sets a source signal and checks that the derived value and
the rendered view follow. The point of these tests is to catch a view that fails to update, which a
type check cannot see.

### 9.4.10 Testing guards, interceptors and the token-refresh queue

The route guard and the HTTP interceptors decide who sees what and what happens when a session
expires. There are specifications for the authentication guard and for the three interceptors:
bearer retry, step-up and anti-forgery retry. The retry tests cover the case where several calls
fail at once and must wait for a single token refresh.

### 9.4.11 HTTP mocking and contract fidelity against the live API

Web tests do not call the API. They use Angular's HTTP testing controller and return hand-written
responses. The weakness is plain: a mocked response can drift from what the API really sends. Two
things limit the drift. The pipeline regenerates the API description on every run and fails if it
differs from the committed one, and the end-to-end walkthroughs of §9.6, integration, system and
end-to-end testing, run against the real API.

## 9.5 Widget and Golden Testing

The mobile client is a Flutter application, and its tests are widget tests and service tests.

### 9.5.1 Widget-test scope

Widget tests build one screen or one control with fake services and check what it shows and how it
responds to a tap. Service tests check how the client reads API responses, including error
responses. No test runs on a physical device.

### 9.5.2 Golden (snapshot) testing: what it catches

A golden test compares a rendered screen with a stored image. It catches changes in layout that no
one intended. It cannot say whether the stored image was right in the first place, and it fails
when a font or an image asset is re-encoded even though nothing a user sees has changed.

Rendering differs between operating systems, so an image made on one machine does not match on
another. For that reason the golden tests are tagged and left out of the pipeline run. They run on
my machine only, and a layout fault on another platform would not be caught by them.

## 9.6 Integration, System and End-to-End Testing

Integration was done step by step, not all at once. Each feature was joined to the running system
as it was finished. Joining everything at the end would have left me with many faults and no way to
tell which join caused which.

Integration tests start the API in memory and call it over HTTP. They cover what unit tests cannot:
the order of the middleware, the output cache and its security, the error shape, and serving the
web client's files.

End-to-end tests use Playwright. The three walkthroughs drive real browsers against the local
stack, with a separate browser session for each role. They run on my machine through the local
check script and are not part of the hosted pipeline, because they need a full running stack and
take much longer than the other suites.

## 9.7 Regression Testing and Test Selection

Every push to the shared branch runs the whole backend, web and mobile suites. I do not select a
subset in the pipeline. The suites are small enough to run in full, and a wrong selection would
cost more than the minutes saved.

Selection happens only on my machine, where the local check script can run one layer at a time. A
fault found after release gets a test that fails before the fix and passes after it, so it cannot
return unnoticed.

## 9.8 Security Testing

Security tests follow the threat model of §8.2, STRIDE. For each threat that has a control, there
is a test that tries the forbidden action and expects a refusal. Authorisation is the largest
group. A script in the pipeline also compares every endpoint's declared access rule with a
committed catalogue and fails on any change that was not recorded.

Conformance was assessed against OWASP ASVS 4.0.3 at level 2, as §8.11, ASVS conformance and
residual risks, sets out. That assessment is my own. There was no independent audit and no
penetration test, and no automated security scanner runs against the code or the running system.
§10.2.2, security in the pipeline, gives the reasons.

## 9.9 Performance and Load Testing

§4.5, the evaluation strategy, reduced this commitment. What was planned as a load test became a
measurement of response time for a single client against the shared preprod tier, reported at the
95th percentile. It shows how long one user waits. It says nothing about many users at once.

*[Method detail needed: the list of endpoints measured, the number of requests per endpoint, the
tool used and the date. Record these when the measurement is run.]*

## 9.10 Usability and Accessibility Testing

Usability is tested with task scripts, one per office: member, Treasurer, General Secretary and
President. Each participant carries out the tasks while an observer fills in a checklist, and then
completes the System Usability Scale [33]. The score is compared with the usual benchmark of 68. A
separate walkthrough applies Nielsen's heuristics [35]. The instruments are in
`docs/book/instruments/`.

There is no control condition, so the study can describe how usable the system is but cannot say it
is more usable than something else.

*[Method detail needed: the number of participants per office and the session dates. For
accessibility, state which WCAG checks were run and with what tool, or state that no audit was
run.]*

## 9.11 User Acceptance Testing

Acceptance means the office holders agree the system does their work as the constitution requires.
The checklist is `docs/BUSINESS_TEST_CHECKLIST.md`, and the end-to-end walkthroughs of §9.6, integration, system and end-to-end testing, are the repeatable
form of its membership, events and election sections. Participants are named by office and never by
name, as §4.8, research ethics, requires.

*[Method detail needed: who took part by office, the protocol followed, and whether a sign-off was
given and on what date.]*

## 9.12 Product Metrics, Static Analysis and Defect Analysis

Product metrics describe the code itself. Their definitions and thresholds are in §4.5, the
evaluation strategy. This section says how each is taken. The values are reported in §12.3, quality
evaluation against ISO/IEC 25010.

### 9.12.1 Size

*[To be measured: lines of code per layer, and the function-point count used in §11.4, effort
estimation. Record the command and the date.]*

### 9.12.2 Complexity

*[To be measured: the distribution of cyclomatic complexity per method and the ten highest methods.
Record the tool and the date.]*

### 9.12.3 Coupling and cohesion

*[To be measured: CBO, LCOM, afferent and efferent coupling and instability per project. Record the
tool and the date.]*

### 9.12.4 Maintainability index and technical-debt estimate

*[To be measured: the maintainability index per project and a debt estimate. Record the tool and
the date.]*

### 9.12.5 Test adequacy: coverage

Backend coverage is collected by coverlet during the pipeline run and turned into a report. Web
coverage comes from Vitest. Coverage says which lines ran during the tests. It does not say the
tests checked anything useful, so a high figure is weak evidence and a low figure is strong
evidence of a gap. Mutation testing would close part of that gap. It was planned and was not run.

*[To be measured: line and branch coverage for the backend and the web client, with the date.]*

Defects are counted from the tracker, where each one has an entry.

*[To be measured: defect density, distribution by component, removal efficiency and root-cause
categories, taken from `docs/TODO.md` and `docs/BUSINESS_FINDINGS.md`.]*

## 9.13 Summary

The system was checked at four levels, with most tests at the unit level and three scripted
walkthroughs at the top. Two formal reviews were held, and the second recorded its own gaps. Three
things were promised more fully than they were delivered: performance testing became a
single-client measurement, the security assessment is my own, and mutation testing was not run.
Chapter 12 reports what the methods of this chapter found.

## Figures and Tables

*[Not drawn. The outline specifies 20 artefacts for this chapter. Each is added here with its caption as it is made, then `renumber.py --apply` is run.]*
