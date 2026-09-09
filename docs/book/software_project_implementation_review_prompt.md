
# Existing Software System — Architecture, Code Review & Prioritized Enhancement Plan

 I have an existing software system, including source code and possibly architecture/design documentation.

 Act as a **Senior Software Architect, Software Engineering Reviewer, and Refactoring Specialist**.

 Review the existing system against recognized software engineering standards and best practices, with particular reference to:

 - **ISO/IEC/IEEE 12207** — Software life-cycle processes
- **ISO/IEC/IEEE 42010** — Architecture descriptions
- **IEEE 1016** — Software Design Description
- **ISO/IEC/IEEE 29148** — Requirements engineering
- **ISO/IEC/IEEE 29119** — Software testing
- **IEEE 730** — Software Quality Assurance
- **IEEE 1028** — Software reviews and audits
- **OWASP guidance** — Application security, where applicable

 The project may be small or medium-sized. Therefore, recommendations must be **proportional to the actual system**.

 ## Core Objective

 Do not redesign the system simply to make it more sophisticated.

 Determine:

 1. What is already good and should remain.
2. What is incorrect or risky.
3. What is unnecessarily complex.
4. What should be refactored.
5. What should be added.
6. What should be removed.
7. What should be documented.
8. What should be tested.
9. What should be improved first.
10. What the **enhanced version should look like after the recommended changes**.

 The final recommendation should aim for:

 **Simple + Maintainable + Secure + Testable + Reliable + Standards-aligned**

---

 # 1\. Understand the Existing System

 First analyze the actual system before making recommendations.

 Identify:

 - Purpose.
- Main users.
- Major features.
- Technology stack.
- Programming languages.
- Frameworks/libraries.
- Database.
- External services/APIs.
- Deployment environment.
- Project/repository structure.
- Main modules.
- Current architecture.
- Current design approach.

 Do not invent information.

 If something cannot be determined from the provided material, explicitly state:

 > **Insufficient evidence to evaluate.**

---

 # 2\. Current Architecture Review

 Evaluate:

 - Architectural style.
- System boundaries.
- Module/component structure.
- Separation of concerns.
- Coupling.
- Cohesion.
- Dependency direction.
- Data flow.
- Database interaction.
- API interaction.
- Security boundaries.
- Configuration.
- Error handling.
- Deployment structure.

 Classify the architecture as:

 - Appropriate
- Under-structured
- Over-engineered
- Inconsistent
- Requires moderate improvement
- Requires major restructuring

 Explain **why**.

 Do not automatically recommend:

 - Microservices
- Kubernetes
- Event-driven architecture
- Multiple databases
- Message queues
- Complex design patterns
- Additional abstraction layers

 unless there is clear evidence that the existing requirements justify them.

---

 # 3\. Code Review

 Review the source code for:

 ### Structure

 - Folder/package organization.
- Module boundaries.
- Separation of responsibilities.
- Naming.
- Duplicate code.
- Dead code.
- Large files/classes/functions.
- Dependency structure.

 ### Maintainability

 - Readability.
- Complexity.
- Code duplication.
- Hard-coded values.
- Global state.
- Excessive nesting.
- Poor abstraction.
- Difficult-to-test code.

 ### Correctness

 - Logic errors.
- Edge cases.
- Error handling.
- Exception handling.
- Resource handling.
- Data integrity.
- Potential runtime failures.

 ### Design Principles

 Evaluate practical use of:

 - Separation of concerns.
- Encapsulation.
- Single Responsibility Principle.
- Dependency management.
- Appropriate abstraction.
- SOLID principles where relevant.

 Do not force design patterns where they do not solve an actual problem.

---

 # 4\. Security Review

 Check for:

 - Authentication.
- Authorization.
- Access control.
- Input validation.
- Injection vulnerabilities.
- Secrets/API keys in code.
- Password handling.
- Session management.
- Sensitive data exposure.
- Unsafe file handling.
- Database security.
- API security.
- Logging of sensitive information.
- Security configuration.

 Use practical OWASP guidance where applicable.

 Classify security findings by severity:

 **Critical / High / Medium / Low / Informational**

---

 # 5\. Database Review

 If applicable, review:

 - Schema.
- Tables/entities.
- Relationships.
- Primary/foreign keys.
- Constraints.
- Normalization.
- Indexes.
- Queries.
- Transactions.
- Connection handling.
- Data duplication.
- Migration/versioning.
- Sensitive data.

 Recommend changes only where there is a meaningful benefit.

---

 # 6\. API & External Interface Review

 Review:

 - API structure.
- Endpoint naming.
- Request/response design.
- Validation.
- Authentication.
- Authorization.
- Error responses.
- Status codes.
- Timeout handling.
- External service failures.
- Dependency handling.
- API documentation.

---

 # 7\. Testing & Testability Review

 Evaluate:

 - Existing tests.
- Unit testing.
- Integration testing.
- System testing.
- Acceptance testing.
- Critical-path coverage.
- Error/edge-case testing.
- Security testing.
- Test organization.
- Testability of modules.

 Use **ISO/IEC/IEEE 29119** as the testing reference.

 Prioritize tests for:

 1. Critical business logic.
2. Authentication/security.
3. Important database operations.
4. External integrations.
5. High-risk failure cases.

---

 # 8\. Architecture–Code Consistency

 Compare the architecture/design documentation with the actual implementation.

 Identify:

 - Documented components that do not exist.
- Undocumented components that do exist.
- Incorrect dependencies.
- Outdated diagrams.
- Incorrect database diagrams.
- Incorrect API descriptions.
- Features documented but not implemented.
- Features implemented but not documented.

 Create:

 | Documented Design | Actual Implementation | Consistent? | Required Change |
| --- | --- | --- | --- |

---

 # 9\. Requirements–Code–Test Traceability

 Where requirements are available, establish:

 **Requirement → Architecture → Module → Code → Test**

 Identify:

 - Requirements without implementation.
- Implementation without requirements.
- Requirements without tests.
- Tests without corresponding requirements.

 For a small project, use a lightweight traceability matrix rather than creating unnecessary documentation.

---

 # 10\. Standards Alignment

 Evaluate the system against the relevant standards.

 Create:

 | Area | Standard | Current Status | Evidence | Gap | Recommendation |
| --- | --- | --- | --- | --- | --- |
| Architecture | ISO/IEC/IEEE 42010 | Partial | Architecture diagram | Missing decisions | Document key decisions |
| Design | IEEE 1016 | Weak | Module description | Interfaces unclear | Add component responsibilities |
| Life cycle | ISO/IEC/IEEE 12207 | Partial | Development process | Missing maintenance process | Add lightweight maintenance plan |
| Testing | ISO/IEC/IEEE 29119 | Partial | Test cases | Weak traceability | Link tests to requirements |

 Do not claim formal "compliance" unless the evidence supports it.

 Use terms such as:

 - Aligned
- Partially aligned
- Weakly aligned
- Not addressed
- Not applicable

---

 # 11\. Identify All Improvements

 Create a comprehensive improvement list.

 For every finding, provide:

 **Issue → Evidence → Impact → Standard/Principle → Recommendation → Effort**

 Example:

 | Issue | Impact | Recommendation | Effort |
| --- | --- | --- | --- |
| Authentication logic duplicated in 3 modules | High maintenance risk | Extract shared authentication service | Low |
| API key stored in source code | Security risk | Move secret to environment/configuration management | Low |
| Simple monolithic architecture | No significant issue | Keep current architecture | None |

---

 # 12\. Prioritize the Improvements

 This is a **mandatory section**.

 Prioritize every recommendation using:

 ### P0 — Critical

 Must fix immediately.

 Examples:

 - Security vulnerability.
- Data corruption risk.
- Major functional defect.
- Credential/secret exposure.
- Serious authorization issue.

 ### P1 — High

 Should be addressed before the next major release.

 Examples:

 - Major architectural problems.
- Important code duplication.
- Poor error handling.
- Missing critical tests.
- Significant maintainability problems.

 ### P2 — Medium

 Should be improved when practical.

 Examples:

 - Refactoring.
- Documentation gaps.
- Moderate code complexity.
- Improved logging.
- Better module organization.

 ### P3 — Low

 Optional improvement.

 Examples:

 - Minor naming improvements.
- Cosmetic refactoring.
- Additional documentation.
- Small optimization without demonstrated need.

 Create:

 | Priority | Issue | Benefit | Risk if Ignored | Effort | Recommended Action |
| --- | --- | --- | --- | --- | --- |

---

 # 13\. Prepare an Enhancement Roadmap

 Create a practical implementation plan.

 ## Phase 1 — Critical Fixes

 Focus on:

 - Security.
- Data integrity.
- Critical bugs.
- Authentication/authorization.
- Serious reliability problems.

 ## Phase 2 — Structural Improvements

 Focus on:

 - Module separation.
- Code duplication.
- Complex functions/classes.
- Dependency problems.
- Architecture inconsistencies.

 ## Phase 3 — Quality & Testability

 Focus on:

 - Unit tests.
- Integration tests.
- Error handling.
- Logging.
- Validation.
- Requirement traceability.

 ## Phase 4 — Documentation

 Update:

 - Architecture documentation.
- Design documentation.
- API documentation.
- Database documentation.
- Deployment documentation.
- Maintenance documentation.

 ## Phase 5 — Optional Improvements

 Only include improvements with a clear measurable benefit.

 For each phase provide:

 **Task → Priority → Dependencies → Expected Result → Effort → Risk**

---

 # 14\. SHOW THE ENHANCED PART

 This is mandatory.

 Do not only say:

 > "Improve architecture."

 Show **what the improved version should look like**.

 For every major recommended improvement, provide a concise **Before → After** comparison.

 ### Example: Architecture

 **Before:**

```
UI
 |
 |------------------|
 |                  |
Login Logic      Database
 |
Business Logic
```

 Problems:

 - Authentication mixed with business logic.
- Direct database access from UI.
- Difficult to test.

 **After:**

```
Presentation Layer
        |
        v
Application/Service Layer
        |
        v
Business Logic
        |
        v
Data Access Layer
        |
        v
Database
```

 Then explain:

 - What changed.
- Why it changed.
- Which problem it solves.
- Why the new structure is sufficient.
- Why additional architecture is not required.

---

 # 15\. SHOW ENHANCED CODE

 Where a code improvement is recommended, show a **small representative before/after example**.

 ### Before

```
Controller
 ├── validation
 ├── business logic
 ├── SQL query
 └── response formatting
```

 ### After

```
Controller
    ↓
Service
    ↓
Repository
    ↓
Database
```

 If actual source code is provided, use the **actual code** rather than generic examples.

 For each code improvement explain:

 - Existing problem.
- Improved implementation.
- Why the change is better.
- Expected maintenance/testing benefit.
- Whether the change is P0/P1/P2/P3.

 Do not rewrite the entire codebase unnecessarily.

---

 # 16\. SHOW ENHANCED DOCUMENTATION

 Where documentation is weak, show the recommended enhanced structure.

 For example:

 ### Existing

 > Authentication is handled by the system.

 ### Enhanced

 > The authentication module validates user credentials, establishes an authenticated session, and applies role-based authorization before protected operations are executed.

 Then identify where this should appear:

 **SRS → Architecture → Implementation → Testing**

---

 # 17\. SHOW ENHANCED DIAGRAMS

 Where appropriate, provide a recommended improved version of:

 - Architecture diagram.
- Component diagram.
- Database/ER diagram.
- Sequence diagram.
- Deployment diagram.
- Data-flow diagram.

 Keep diagrams proportional to the project's size.

 Do not add diagrams that provide no useful information.

---

 # 18\. What NOT to Change

 Create a specific section called:

 ## Keep As-Is

 Identify parts of the current system that are already appropriate.

 Examples:

 - Current framework.
- Current database.
- Simple monolithic architecture.
- Existing module structure.
- Existing deployment approach.

 Explain why changing them would provide little benefit.

 Also create:

 ## Avoid

 List unnecessary complexity that should **not** be introduced.

 Examples:

 - Microservices without a requirement.
- Kubernetes for a small application.
- Unnecessary design patterns.
- Excessive abstraction.
- Premature optimization.
- Additional databases without justification.

---

 # 19\. Final Target State

 Describe the recommended final system after improvements.

 Provide:

 ### Target Architecture

 A concise architecture diagram.

 ### Target Code Structure

 Recommended folder/module structure.

 ### Target Quality

 Expected improvements in:

 - Maintainability.
- Security.
- Reliability.
- Testability.
- Performance where relevant.

 ### Target Documentation

 List the documents/diagrams that should exist after enhancement.

---

 # 20\. Final Prioritized Action Plan

 Finish with a single actionable table:

 | Order | Priority | Improvement | Area | Standard | Effort | Expected Benefit |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | P0 | Remove exposed credentials | Security | OWASP | Low | Critical security improvement |
| 2 | P1 | Separate business logic | Architecture | 42010/1016 | Medium | Maintainability |
| 3 | P1 | Add authentication tests | Testing | 29119 | Low | Reliability |
| 4 | P2 | Refactor duplicate code | Code | Engineering practice | Low | Maintainability |
| 5 | P2 | Update architecture diagram | Documentation | 42010 | Low | Better maintainability |

 Then provide:

 ### Recommended Order

 **Fix → Refactor → Test → Document → Optimize**

 Do not recommend optimization before establishing that it is actually necessary.

---

 # Final Output Structure

 Return the review in exactly this general order:

 1. **Executive Summary**
2. **Existing System Understanding**
3. **Current Architecture**
4. **Architecture Review**
5. **Code Review**
6. **Security Review**
7. **Database Review**
8. **API Review**
9. **Testing Review**
10. **Architecture-Code Consistency**
11. **Standards Alignment**
12. **Issues & Findings**
13. **Prioritized Improvements**
14. **Enhancement Roadmap**
15. **Before vs. After Architecture**
16. **Before vs. After Code**
17. **Enhanced Documentation**
18. **Enhanced Diagrams**
19. **Keep As-Is**
20. **Avoid / Do Not Introduce**
21. **Target Architecture**
22. **Final Prioritized Action Plan**

 ## Critical Rules

 - Review the **actual code and architecture**, not assumptions.
- Do not invent missing functionality.
- Use evidence from the supplied project.
- Keep recommendations proportional to project size.
- Prefer **minimal effective changes**.
- Do not rewrite working code without a clear benefit.
- Do not introduce enterprise architecture into a simple project without justification.
- Clearly distinguish **must-fix issues from optional improvements**.
- Every major recommendation must have a **priority, rationale, effort, and expected benefit**.
- For major recommendations, **SHOW the enhanced version**, not merely describe it.
- When actual code is available, use it for before/after examples.
- Maintain consistency between requirements, architecture, code, tests, and documentation.
- Do not claim formal standards compliance unless there is sufficient evidence.
- Where a standard is not relevant to the project, mark it **Not Applicable** and explain why.

 ## Final Goal

 The final result should answer four practical questions:

 **1\. What is wrong?**\
 **2\. What should be improved?**\
 **3\. What should be improved first?**\
 **4\. What will the enhanced system/code/architecture look like after the improvements?**

 The objective is not to make the project more complicated. The objective is to make the **existing system demonstrably better with the smallest justified set of changes**.