# AgencyOS Functional Specification

# Functional_Requirements.md

Version: 1.0

Status: Approved

Baseline: 1.0

Owner: Product Management

Related Documents

- Program_Architecture.md
- Business_Domains.md
- Commercial_Domain.md
- Operations_Domain.md
- Business_Rules.md
- Use_Cases.md
- Product Vision
- AgencyOS Baseline
- ADR-009 Audit and History Strategy
- ADR-010 Aggregate Boundary Strategy
- ADR-011 Archive and Soft Delete Policy
- ADR-012 Enumeration Persistence Strategy

---

# 1. Purpose

This document defines the Functional Requirements of AgencyOS Baseline 1.0.

Functional Requirements describe the observable behavior the system shall provide.

They define system capabilities independently from implementation.

Every implemented feature shall satisfy one or more Functional Requirements.

Every Functional Requirement in this document is traceable to an approved Business Rule and is implementable through Engineering Specifications.

---

# 2. Scope

The MVP Functional Requirements cover:

- Commercial
- Operations
- Planning
- Decision

Execution management is outside MVP scope.

MVP scope is unchanged by this document.

No new features, workflows or capabilities are introduced.

---

# 3. Requirement Classification

Requirements are organized by business domain.

Commercial

FR-COM-xxx

Operations

FR-OPS-xxx

Planning

FR-PLAN-xxx

Decision

FR-DEC-xxx

System

FR-SYS-xxx

Requirement identifiers are permanent.

Existing identifiers shall never be renumbered.

---

# 4. Commercial Functional Requirements

## FR-COM-001

The system shall allow Lead registration.

A registered Lead shall receive a valid lifecycle status.

Traceability: BR-COM-001

---

## FR-COM-002

The system shall manage the Lead lifecycle across the statuses New, Qualified, Proposal, Negotiation, Won, Lost, Converted and Archived.

The system shall reject invalid lifecycle transitions.

Traceability: BR-COM-001, BR-COM-002

---

## FR-COM-003

The system shall allow Client registration through Lead conversion.

Conversion shall be available only for a Lead in status Won.

Conversion shall require explicit human action and shall never occur automatically.

Conversion shall create exactly one Client, set the Lead status to Converted and preserve the Lead record and its history.

Direct Client creation without Lead conversion is not provided in MVP 1.0.

The system shall require mandatory Client identification information, including legal name and tax identifier.

Traceability: BR-COM-002, BR-COM-003

---

## FR-COM-004

The system shall allow Client Contact registration.

Contacts shall be registered and maintained through the owning Client.

The system shall permit multiple Contacts per Client.

The system shall enforce a single Primary Contact per Client.

Traceability: BR-COM-004, BR-COM-005, BR-COM-012

---

## FR-COM-005

The system shall allow Contract registration.

Every Contract shall reference exactly one Client.

The system shall permit Contract registration only for Active Clients.

Traceability: BR-COM-006, BR-COM-007

---

## FR-COM-006

The system shall allow Contract approval by an authorized user.

Approval shall move a Contract to the Approved status and shall record the approving user and approval timestamp.

Traceability: BR-COM-011, BR-COM-010

---

## FR-COM-007

The system shall maintain commercial history.

Commercial history shall be obtained from centralized Audit Events.

History records shall be produced only after successful transaction commit.

Traceability: BR-COM-010

---

## FR-COM-008

The system shall generate operational demand from Approved Contracts.

The system shall authorize Mission creation only when the related Contract status is Active.

Contracts in Draft, Under Review, Approved, Completed, Cancelled or Archived status shall never authorize Mission creation.

Traceability: BR-COM-008, BR-COM-009, BR-COM-011

---

## FR-COM-009

The system shall manage the Contract lifecycle across the statuses Draft, Under Review, Approved, Active, Completed, Cancelled and Archived.

A Contract shall become Active only after it has been Approved.

The system shall reject invalid lifecycle transitions.

Traceability: BR-COM-011

---

## FR-COM-010

The system shall provide Archive as the business inactivation behavior for Lead, Client, Contact and Contract.

Archived records shall retain the Archived status and the archival timestamp.

Archived records shall remain readable for history and authorized detail access.

Archived records shall be read-only for business workflows.

The system shall not expose physical deletion or technical soft deletion as business operations.

Traceability: BR-COM-013

---

## FR-COM-011

The system shall exclude archived records from default commercial search results.

The system shall return archived records when a query explicitly requests them.

The system shall allow retrieval of an archived record by its identifier for authorized detail and history access.

Traceability: BR-COM-013

---

# 5. Operations Functional Requirements

## FR-OPS-001

The system shall allow Mission creation.

Mission creation shall require a related Contract in Active status.

Every Mission shall reference exactly one Contract.

Traceability: BR-OPS-001, BR-COM-008

---

## FR-OPS-002

The system shall manage Mission lifecycle.

Traceability: BR-OPS-001, BR-OPS-010

---

## FR-OPS-003

The system shall allow Task creation.

Every Task shall belong to exactly one Mission.

Traceability: BR-OPS-002, BR-OPS-003

---

## FR-OPS-004

The system shall manage Task lifecycle.

Traceability: BR-OPS-003, BR-OPS-010

---

## FR-OPS-005

The system shall register Execution Resources.

Execution Resources may represent human, AI, hybrid or external resources.

Traceability: BR-OPS-009

---

## FR-OPS-006

The system shall allow Assignments.

Traceability: BR-OPS-005

---

## FR-OPS-007

Assignments shall associate Tasks and Execution Resources.

Assignments shall not assert operational feasibility.

Traceability: BR-OPS-005, BR-OPS-006, BR-OPS-007

---

## FR-OPS-008

Operational structures shall remain independent from Planning.

Traceability: BR-OPS-008

---

# 6. Planning Functional Requirements

## FR-PLAN-001

The system shall calculate productive Capacity.

Traceability: BR-PLAN-001, BR-PLAN-002

---

## FR-PLAN-002

Capacity shall be expressed in productive hours.

Traceability: BR-PLAN-001

---

## FR-PLAN-003

The system shall calculate operational Workload.

Traceability: BR-PLAN-004

---

## FR-PLAN-004

The system shall calculate Availability.

Traceability: BR-PLAN-005

---

## FR-PLAN-005

The system shall detect Allocation Conflicts.

Allocation Conflict Detection shall report inconsistencies only.

Traceability: BR-PLAN-007

---

## FR-PLAN-006

Planning calculations shall remain deterministic and reproducible.

Traceability: BR-PLAN-002, BR-PLAN-010

---

## FR-PLAN-007

Planning calculations shall never modify operational data.

Traceability: BR-PLAN-003, BR-PLAN-006

---

# 7. Decision Functional Requirements

## FR-DEC-001

The system shall generate execution strategies.

Only feasible strategies shall be generated.

Traceability: BR-DEC-001

---

## FR-DEC-002

The system shall evaluate generated strategies.

Traceability: BR-DEC-002

---

## FR-DEC-003

The system shall rank evaluated strategies.

Ranking shall never modify evaluation metrics.

Traceability: BR-DEC-003, BR-DEC-006

---

## FR-DEC-004

Ranking shall use Company Decision Profiles.

Company Decision Profiles shall affect ranking only.

Traceability: BR-DEC-005

---

## FR-DEC-005

The system shall explain every recommendation.

Traceability: BR-DEC-004

---

## FR-DEC-006

Recommendations shall remain deterministic.

Traceability: BR-DEC-007, BR-DEC-008

---

## FR-DEC-007

Execution recommendations shall require human approval.

The Decision Engine shall never execute operational work.

Traceability: BR-DEC-009, BR-DEC-010

---

# 8. System Functional Requirements

## FR-SYS-001

The system shall preserve domain independence.

Traceability: BR-SYS-001, BR-SYS-002

---

## FR-SYS-002

Business calculations shall remain deterministic.

Traceability: BR-SYS-006

---

## FR-SYS-003

Artificial Intelligence shall not replace deterministic calculations during the MVP.

Traceability: BR-SYS-007, BR-DEC-008

---

## FR-SYS-004

Every recommendation shall be reproducible.

Traceability: BR-SYS-009

---

## FR-SYS-005

Business history shall remain auditable.

Every Aggregate Root shall produce audit records for successful state-changing operations.

Audit records shall be written only after successful transaction commit.

History queries shall obtain information from centralized Audit Events.

Aggregate state shall not be reconstructed from history records.

Traceability: BR-SYS-010, BR-COM-010

---

## FR-SYS-006

The complete business flow shall follow:

Commercial

↓

Operations

↓

Planning

↓

Decision

↓

Execution

Traceability: BR-SYS-003, BR-SYS-004

---

## FR-SYS-007

No downstream domain shall modify upstream business information.

Traceability: BR-SYS-005

---

## FR-SYS-008

The system shall expose closed enumeration values by name through its APIs and API documentation.

Closed enumerations shall not require lookup administration or seed data in MVP 1.0.

Traceability: BR-COM-014

---

# 9. Requirement Dependencies

Commercial Requirements

↓

Operations Requirements

↓

Planning Requirements

↓

Decision Requirements

Every downstream requirement depends on outputs produced upstream.

FR-OPS-001 depends on FR-COM-008 and FR-COM-009.

---

# 10. Requirement Priorities

All Functional Requirements included in this document are mandatory for Baseline 1.0.

Priority

Mandatory

Deferred requirements belong to future product releases.

---

# 11. Requirement Traceability

Every Functional Requirement shall be traceable to:

Business Domains

↓

Business Rules

↓

Use Cases

↓

User Stories

↓

Acceptance Criteria

↓

Automated Tests

No Functional Requirement shall exist without traceability.

---

# 12. Business Rule Traceability Matrix

| Functional Requirement | Business Rules |
| --- | --- |
| FR-COM-001 | BR-COM-001 |
| FR-COM-002 | BR-COM-001, BR-COM-002 |
| FR-COM-003 | BR-COM-002, BR-COM-003 |
| FR-COM-004 | BR-COM-004, BR-COM-005, BR-COM-012 |
| FR-COM-005 | BR-COM-006, BR-COM-007 |
| FR-COM-006 | BR-COM-010, BR-COM-011 |
| FR-COM-007 | BR-COM-010 |
| FR-COM-008 | BR-COM-008, BR-COM-009, BR-COM-011 |
| FR-COM-009 | BR-COM-011 |
| FR-COM-010 | BR-COM-013 |
| FR-COM-011 | BR-COM-013 |
| FR-OPS-001 | BR-OPS-001, BR-COM-008 |
| FR-OPS-002 | BR-OPS-001, BR-OPS-010 |
| FR-OPS-003 | BR-OPS-002, BR-OPS-003 |
| FR-OPS-004 | BR-OPS-003, BR-OPS-010 |
| FR-OPS-005 | BR-OPS-009 |
| FR-OPS-006 | BR-OPS-005 |
| FR-OPS-007 | BR-OPS-005, BR-OPS-006, BR-OPS-007 |
| FR-OPS-008 | BR-OPS-008 |
| FR-PLAN-001 | BR-PLAN-001, BR-PLAN-002 |
| FR-PLAN-002 | BR-PLAN-001 |
| FR-PLAN-003 | BR-PLAN-004 |
| FR-PLAN-004 | BR-PLAN-005 |
| FR-PLAN-005 | BR-PLAN-007 |
| FR-PLAN-006 | BR-PLAN-002, BR-PLAN-010 |
| FR-PLAN-007 | BR-PLAN-003, BR-PLAN-006 |
| FR-DEC-001 | BR-DEC-001 |
| FR-DEC-002 | BR-DEC-002 |
| FR-DEC-003 | BR-DEC-003, BR-DEC-006 |
| FR-DEC-004 | BR-DEC-005 |
| FR-DEC-005 | BR-DEC-004 |
| FR-DEC-006 | BR-DEC-007, BR-DEC-008 |
| FR-DEC-007 | BR-DEC-009, BR-DEC-010 |
| FR-SYS-001 | BR-SYS-001, BR-SYS-002 |
| FR-SYS-002 | BR-SYS-006 |
| FR-SYS-003 | BR-SYS-007, BR-DEC-008 |
| FR-SYS-004 | BR-SYS-009 |
| FR-SYS-005 | BR-SYS-010, BR-COM-010 |
| FR-SYS-006 | BR-SYS-003, BR-SYS-004 |
| FR-SYS-007 | BR-SYS-005 |
| FR-SYS-008 | BR-COM-014 |

---

# 13. Architecture Alignment

## ADR-009

FR-COM-007 and FR-SYS-005 rely on centralized Audit Events written after successful commit.

History is never reconstructed from Aggregate state or event streams.

## ADR-010

FR-COM-003, FR-COM-004, FR-COM-005 and FR-OPS-001 respect Commercial Aggregate ownership.

Lead, Client and Contract are Aggregate Roots.

Contact is maintained through the Client Aggregate.

Mission belongs to the Operations Domain and references Contract by identity.

## ADR-011

FR-COM-010 and FR-COM-011 implement Archive as the business inactivation behavior.

Technical soft deletion is not exposed as a business capability.

## ADR-012

FR-SYS-008 exposes closed enumerations by name without lookup tables or seed data.

---

# 14. Functional Compliance Statement

This document defines the official Functional Requirements for AgencyOS Baseline 1.0.

Every implementation shall satisfy these requirements.

Requirements take precedence over implementation details.

No implementation shall introduce functionality outside the approved Functional Requirements.

No requirement in this document redesigns workflows, expands MVP scope or modifies approved architecture.
