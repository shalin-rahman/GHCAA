
 #  IEEE/ISO-Based Software Project Documentation Audit & Enhancement

 I have an existing software project document/report. I want you to act as a **Software Engineering Documentation Auditor and IEEE/ISO Standards Reviewer**.

 Your task is to carefully review the entire document and determine whether it adequately covers the major topics and guidelines expected in professional software project documentation.

 Use the following standards as the primary reference framework:

 - **ISO/IEC/IEEE 29148** — Requirements Engineering / SRS
- **ISO/IEC/IEEE 12207** — Software Life Cycle Processes
- **IEEE 16326** — Software/System Project Management Plans
- **IEEE 1016** — Software Design Description
- **ISO/IEC/IEEE 42010** — Architecture Description
- **ISO/IEC/IEEE 29119** — Software Testing
- **IEEE 730** — Software Quality Assurance
- **IEEE 1028** — Reviews and Audits
- **IEEE 830** — Older SRS reference, only where historically relevant
- **IEEE 829** — Older test-documentation reference, only where historically relevant

 Do not assume that the document follows a standard simply because it contains a similarly named chapter. Evaluate the **actual content, completeness, clarity, consistency, and evidence**.

 ## 1\. Overall Assessment

 First provide:

 - Overall quality assessment.
- Overall IEEE/ISO alignment percentage or rating.
- Major strengths.
- Major weaknesses.
- Critical missing sections.
- Sections that exist but are incomplete.
- Sections that contain unnecessary or duplicated information.
- Any major inconsistencies or contradictions.

 Use ratings such as:

 - **Fully Covered** — sufficiently addressed.
- **Partially Covered** — present but incomplete.
- **Weakly Covered** — mentioned but lacks useful detail/evidence.
- **Not Covered** — missing.
- **Not Applicable** — explain why it is not required.

 Do not invent information that is not present in the document.

---

 # 2\. Detailed Coverage Audit

 Evaluate the existing document against the following structure.

 ## A. Introduction

 Check for:

 - Background/problem context
- Purpose
- Project scope
- Objectives
- Stakeholders
- Definitions and abbreviations
- References
- System boundaries

 Identify what is covered, what is missing, and what should be improved.

 **Relevant standard:** ISO/IEC/IEEE 29148

---

 ## B. Project Planning & Management

 Check for:

 - Project organization
- Team structure
- Roles and responsibilities
- Development methodology
- Work breakdown/project activities
- Development schedule
- Milestones
- Resources
- Budget/cost considerations
- Risk management
- Communication/reporting
- Project monitoring
- Configuration management
- Change management

 **Relevant standards:** IEEE 16326, ISO/IEC/IEEE 12207

---

 ## C. Software Requirements Specification (SRS)

 Check carefully for:

 - Functional requirements
- Non-functional requirements
- User requirements
- System requirements
- External interface requirements
- Hardware/software interfaces
- API/network interfaces
- Input/output requirements
- Business rules
- Constraints
- Assumptions
- Dependencies
- Acceptance criteria
- Requirement priorities
- Requirement IDs
- Requirement traceability
- Requirement verification criteria

 Check whether requirements are:

 - Clear
- Unambiguous
- Consistent
- Complete
- Feasible
- Verifiable/testable
- Traceable

 Identify vague requirements such as "fast", "secure", "user-friendly", etc., and suggest measurable replacements.

 **Relevant standard:** ISO/IEC/IEEE 29148\
 **Historical reference:** IEEE 830

---

 ## D. System Analysis

 Check for:

 - Existing system/problem analysis
- Stakeholder analysis
- Current process/workflow
- Proposed solution
- Use-case analysis
- Activity/workflow analysis
- Business rules
- Data requirements
- Feasibility analysis
  - Technical
  - Economic
  - Operational
  - Schedule
- Alternative solutions

 Check whether the analysis logically supports the requirements.

---

 ## E. Software Architecture & Design

 Check for:

 - System architecture
- Architectural style/pattern
- Architectural components
- Component responsibilities
- Communication between components
- Module design
- Database architecture
- Database design
- Data structures
- API/interface design
- UI design
- Security architecture
- Error-handling strategy
- Performance considerations
- Scalability
- Reliability
- Design constraints
- Design decisions and justification

 Check for appropriate diagrams:

 - System architecture diagram
- Component diagram
- Class diagram
- Sequence diagram
- Activity diagram
- ER/database diagram
- Deployment diagram
- Data-flow diagram

 Check whether the diagrams are consistent with the written explanation.

 **Relevant standards:** IEEE 1016, ISO/IEC/IEEE 42010

---

 ## F. Software Development / Implementation

 Check for:

 - Development environment
- Programming languages
- Frameworks/libraries
- Development tools
- Coding standards
- Source-code organization
- Module implementation
- Database implementation
- API implementation
- Error/exception handling
- Logging
- Security practices
- Version control
- Branching strategy
- Code review
- Build process
- Dependency management
- Code documentation

 Check whether implementation can be traced back to requirements and design.

 **Relevant standard:** ISO/IEC/IEEE 12207

---

 ## G. Software Testing

 Check for:

 - Test strategy
- Test plan
- Test environment
- Test data
- Unit testing
- Integration testing
- System testing
- Acceptance testing
- Functional testing
- Non-functional testing
- Performance testing
- Security testing
- Usability testing
- Regression testing
- Test cases
- Expected results
- Actual results
- Pass/fail status
- Defect tracking
- Test summary

 For test cases, check whether the document provides:

 **Test ID → Requirement ID → Preconditions → Input → Steps → Expected Result → Actual Result → Status**

 Check whether every important requirement has at least one corresponding test.

 **Relevant standard:** ISO/IEC/IEEE 29119\
 **Historical reference:** IEEE 829

---

 ## H. Software Quality Assurance

 Check for:

 - Quality objectives
- Quality standards
- Coding/documentation standards
- Requirements reviews
- Design reviews
- Code reviews
- Inspections
- Walkthroughs
- Verification and validation
- Quality metrics
- Defect tracking
- Standards compliance
- Corrective actions
- Preventive actions
- Quality records

 Clearly distinguish **Quality Assurance** from **Testing**.

 **Relevant standards:** IEEE 730, IEEE 1028

---

 ## I. Configuration & Change Management

 Check for:

 - Version control
- Version numbering
- Configuration items
- Baselines
- Repository
- Document version history
- Change requests
- Change impact analysis
- Change approval
- Change implementation
- Change verification
- Release management
- Configuration status tracking

 Check whether there is a defined process such as:

 **Change Request → Impact Analysis → Approval → Implementation → Testing → Release**

 **Relevant standard:** ISO/IEC/IEEE 12207

---

 ## J. Security & Reliability

 Check for:

 ### Security

 - Authentication
- Authorization
- Access control
- Password/security policies
- Confidentiality
- Integrity
- Privacy
- Encryption
- Threat analysis
- Vulnerability analysis
- Secure coding
- Security testing
- Audit logging

 ### Reliability

 - Availability
- Fault handling
- Error recovery
- Backup/recovery
- Fault tolerance
- Reliability requirements
- Performance requirements

 Check whether security/reliability requirements are reflected in the architecture, implementation, and testing.

---

 ## K. Deployment & Operation

 Check for:

 - Hardware requirements
- Software/environment requirements
- Installation procedure
- Database setup
- Configuration
- Data migration
- Deployment procedure
- User/admin training
- Operational procedures
- Monitoring
- Logging
- Backup/recovery
- Rollback procedure

 **Relevant standard:** ISO/IEC/IEEE 12207

---

 ## L. Maintenance

 Check for:

 - Corrective maintenance
- Adaptive maintenance
- Perfective maintenance
- Preventive maintenance
- Bug fixes
- Updates/patches
- Change requests
- Release management
- Support responsibilities
- Maintenance procedures
- Monitoring

 **Relevant standard:** ISO/IEC/IEEE 12207

---

 ## M. Project Evaluation

 Check for:

 - Objectives achieved/not achieved
- Requirements satisfied/not satisfied
- Test results
- Performance evaluation
- Quality measurements
- User feedback
- Limitations
- Problems encountered
- Risks encountered
- Lessons learned
- Planned vs. actual results
- Planned vs. actual schedule

---

 ## N. Conclusion & Future Work

 Check for:

 - Overall project outcome
- Major achievements
- Important findings
- Remaining limitations
- Recommendations
- Future features
- Scalability/extensions

---

 ## O. References

 Check whether the document appropriately references:

 - IEEE standards
- ISO/IEC standards
- Research papers
- Books
- Official technical documentation
- Software/framework documentation
- Other reliable sources

 Check citation consistency and identify unsupported claims.

---

 ## P. Appendices

 Check whether appropriate supporting material is included:

 - Detailed test cases
- Test results
- Source-code excerpts
- Database schema
- UML diagrams
- API documentation
- User manual
- Installation guide
- Screenshots
- Sample reports
- Supporting documents

---

 # 3\. Create a Coverage Matrix

 Create a table using this structure:

 | Area | Expected Content | Existing Document Section | Coverage | Quality | Missing/Weak Items | Priority |
| --- | --- | --- | --- | --- | --- | --- |
| Requirements | Functional requirements | Section X | Partial | Medium | Add requirement IDs | High |
| Architecture | System architecture | Section X | Full | Good | Improve diagram | Medium |
| Testing | Requirement-based test cases | Section X | Weak | Low | Add traceability | Critical |

 Use:

 - **Full**
- **Partial**
- **Weak**
- **Missing**
- **N/A**

 For quality, use:

 - **Excellent**
- **Good**
- **Acceptable**
- **Weak**
- **Poor**

 Priorities:

 - **Critical** — must fix
- **High** — strongly recommended
- **Medium** — useful improvement
- **Low** — optional enhancement

---

 # 4\. Identify Cross-Document Inconsistencies

 Do not only check individual chapters. Check whether different sections agree with each other.

 Look specifically for:

 - Requirements that are not implemented.
- Implemented features that are not in the requirements.
- Requirements without test cases.
- Test cases without corresponding requirements.
- Architecture that contradicts implementation.
- UML diagrams that contradict the actual system.
- Database diagrams that differ from the implemented database.
- Different terminology for the same feature.
- Different numbers/names for modules.
- Conflicting project scope.
- Conflicting technology stacks.
- Inconsistent user roles.
- Inconsistent system behavior.
- References to sections that do not exist.

 Create a separate **Consistency & Traceability Findings** table.

---

 # 5\. Create a Requirement Traceability Matrix

 If enough information exists, create or recommend:

 | Req. ID | Requirement | Design Component | Implementation | Test Case | Test Result |
| --- | --- | --- | --- | --- | --- |
| REQ-001 | User login | Auth Module | LoginService | TC-001 | Pass |
| REQ-002 | Account lockout | Security Module | LockoutService | TC-002 | Pass |

 Identify every requirement that cannot be traced through the complete chain:

 **Requirement → Design → Implementation → Test → Result**

---

 # 6\. Identify Missing Documentation

 Create a prioritized list of everything that should be added.

 Group findings into:

 ### Critical

 Items necessary for a credible software engineering project document.

 ### High Priority

 Items that significantly improve IEEE/ISO alignment and technical quality.

 ### Medium Priority

 Useful improvements but not essential.

 ### Low Priority

 Optional professional enhancements.

 For every missing item, explain:

 1. What is missing.
2. Why it matters.
3. Which standard relates to it.
4. Where it should be added.
5. What content should be added.
6. Whether a diagram/table/template is recommended.

---

 # 7\. Prepare an Enhancement Plan

 Create a practical implementation plan rather than simply listing problems.

 Use this format:

 | Priority | Improvement | Standard | Where to Add | What to Add | Effort |
| --- | --- | --- | --- | --- | --- |
| Critical | Add requirements traceability | 29148 | SRS | Requirement IDs + RTM | Medium |
| High | Improve architecture documentation | 42010/1016 | Design | Architecture + component diagrams | Medium |
| High | Improve testing evidence | 29119 | Testing | Test cases + results | High |

 Then organize the work into phases:

 ### Phase 1 — Foundation

 - Fix document structure.
- Define scope/objectives.
- Establish terminology.
- Identify stakeholders.
- Create document version control.

 ### Phase 2 — Requirements

 - Improve SRS.
- Assign requirement IDs.
- Define functional/non-functional requirements.
- Add acceptance criteria.
- Establish traceability.

 ### Phase 3 — Analysis & Design

 - Improve system analysis.
- Add architecture.
- Add UML/design diagrams.
- Document design decisions.
- Ensure design matches requirements.

 ### Phase 4 — Implementation

 - Document technologies.
- Development environment.
- Coding standards.
- Modules/components.
- Version control.
- Implementation mapping.

 ### Phase 5 — Testing & Quality

 - Create test strategy.
- Add test cases.
- Link tests to requirements.
- Document results.
- Add defect tracking.
- Add QA/review activities.

 ### Phase 6 — Deployment & Maintenance

 - Installation.
- Deployment.
- Configuration.
- User training.
- Backup/recovery.
- Maintenance plan.

 ### Phase 7 — Final Evaluation

 - Evaluate objectives.
- Analyze test results.
- Document limitations.
- Lessons learned.
- Future work.
- Final references and appendices.

---

 # 8\. Recommend Diagrams and Tables

 Based on the actual project, recommend only the diagrams that are useful.

 Consider:

 - Context diagram
- Use-case diagram
- Activity diagram
- Data-flow diagram
- System architecture diagram
- Component diagram
- Class diagram
- Sequence diagram
- ER diagram
- Deployment diagram
- Database schema
- State diagram

 Also recommend useful tables such as:

 - Stakeholder table
- Functional requirements table
- Non-functional requirements table
- Risk register
- Project schedule
- Requirement Traceability Matrix
- Test case matrix
- Defect register
- Change request register
- Version history
- Technology stack

 Do not recommend diagrams merely for decoration. Explain the purpose of each recommended diagram.

---

 # 9\. Improve the Existing Document, Not Just the Structure

 For important weak sections, provide **specific recommendations and example replacement/additional content**.

 For example, if the existing requirement says:

 > "The system should be secure."

 Recommend a measurable version such as:

 > "The system shall lock an account after five consecutive unsuccessful authentication attempts."

 Explain why the revised requirement is more **specific, verifiable, and testable**.

 Do the same for vague or incomplete content in other sections.

---

 # 10\. Final Assessment

 At the end, provide:

 ### Overall Score

 Give separate scores for:

 - Requirements
- Project management
- Analysis
- Architecture/design
- Implementation
- Testing
- Quality assurance
- Security
- Configuration management
- Deployment
- Maintenance
- Evaluation
- Overall documentation quality

 Use a **0–100 scale**, but explain the reasoning rather than presenting arbitrary numbers.

 ### Final Deliverables

 Finish the review with these sections:

 1. **Executive Summary**
2. **Standards Alignment Summary**
3. **Coverage Matrix**
4. **Missing/Weak Areas**
5. **Consistency Problems**
6. **Requirement Traceability Findings**
7. **Recommended Diagrams**
8. **Recommended Tables**
9. **Prioritized Enhancement Plan**
10. **Suggested Revised Document Structure**
11. **Top 10 Improvements to Make First**

 ## Important Review Rules

 - Review the **actual contents**, not just section titles.
- Do not assume a section is complete because its heading exists.
- Do not invent project details.
- Clearly distinguish **missing**, **partial**, and **adequate** content.
- Do not rewrite the entire document unless specifically requested.
- Preserve useful existing content where possible.
- Identify duplicate or unnecessary sections.
- Prefer practical recommendations suitable for a university or professional software project.
- Use the **current ISO/IEC/IEEE standards where applicable**, while clearly identifying IEEE 830 and IEEE 829 as historical/superseded references.
- If a standard does not directly apply to a particular project area, say so rather than forcing compliance.
- Focus on **content quality, completeness, traceability, consistency, and evidence**, not simply formatting.
- Do not claim that a project is "IEEE compliant" unless the evidence in the document genuinely supports that conclusion.

 Finally, provide a **clear prioritized roadmap showing exactly what should be added, removed, merged, rewritten, or reorganized to turn the existing document into a stronger IEEE/ISO-aligned software project document.**