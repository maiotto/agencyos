# AgencyOS Functional Specification

# Business_Rules.md

Version: 1.0

Status: Approved

Baseline: 1.0

Owner: Product Management

Related Documents

- Product Vision
- Product Principles
- Product Roadmap
- AgencyOS Baseline
- Program_Architecture.md
- Business_Domains.md
- Commercial_Domain.md
- Operations_Domain.md
- Planning_Engines.md
- Decision_Engine.md
- ADR-006 AgencyOS AI Factory
- ADR-007 Company Decision Profiles
- ADR-008 Documentation Update Workflow
- ADR-009 Audit and History Strategy
- ADR-010 Aggregate Boundary Strategy
- ADR-011 Archive and Soft Delete Policy
- ADR-012 Enumeration Persistence Strategy

---

# 1. Purpose

This document defines the official Business Rules of AgencyOS.

Business Rules describe mandatory behaviors that govern the operation of the platform.

These rules are independent of implementation technology, APIs, user interfaces or database design.

Every implementation shall comply with these rules.

This document is the single authoritative source for Commercial business behavior and the governing business behavior for Operations, Planning and Decision within AgencyOS Baseline 1.0.

---

# 2. Scope

This document consolidates all business rules governing the AgencyOS MVP.

Business Rules apply to:

- Commercial Domain
- Operations Domain
- Planning Domain
- Decision Domain

Future product releases may introduce new rules but shall not violate the principles established in this document.

MVP scope is unchanged.

No new business capabilities are introduced by this document.

---

# 3. Business Rule Principles

Business Rules follow these principles.

- Business First
- Capacity First
- Goal-Oriented Planning
- Deterministic Core
- Explainable Decisions
- Human Governance
- Single Responsibility
- Traceability
- Auditability

Business Rules define business behavior.

They never define implementation.

Approved architecture decisions remain binding.

Business Rules shall not contradict ADR-006 through ADR-012.

---

# 4. Commercial Business Rules

## BR-COM-001 — Lead Lifecycle Status

Every Lead shall have exactly one valid lifecycle status at all times.

The official Lead lifecycle statuses are:

- New
- Qualified
- Proposal
- Negotiation
- Won
- Lost
- Converted
- Archived

No other Lead lifecycle status is valid in MVP 1.0.

Lead status values are closed enumerations governed by ADR-012.

---

## BR-COM-002 — Lead Conversion

A Lead may become the origin of a Client only through an explicit conversion action.

A Lead is never automatically converted.

Human approval is required for conversion.

Only a Lead in status Won may be converted.

Conversion creates exactly one Client.

Conversion does not transform the Lead entity into a Client entity.

The Lead remains a Lead Aggregate Root.

Upon successful conversion:

- Lead status becomes Converted
- Lead history is preserved
- the created Client becomes an independent Client Aggregate Root
- the commercial relationship between the originating Lead and the created Client remains historically traceable

Converted Leads are read-only for business workflows.

Conversion shall be auditable according to BR-COM-010.

---

## BR-COM-003 — Client Creation and Mandatory Information

Clients originate from Lead conversion.

Direct Client creation without Lead conversion is not allowed in MVP 1.0.

Every Client shall contain mandatory identification information before it may be used in commercial workflows.

Mandatory Client identification includes at least:

- Legal name
- Tax identifier

A Client without mandatory identification information is invalid.

---

## BR-COM-004 — Multiple Contacts

A Client may have multiple Contacts.

Contacts represent individuals associated with the Client for commercial communication.

Contacts do not own Contracts.

Contacts do not own Missions.

---

## BR-COM-005 — Contact Ownership

Every Contact belongs to exactly one Client.

Contact is a child entity of the Client Aggregate.

Contact is not an Aggregate Root.

A Contact shall never exist without a Client.

Contact creation, update, activation, deactivation, archival and primary assignment occur through the Client Aggregate ownership boundary.

Only one Primary Contact is allowed per Client.

When a Contact is designated as Primary, any previous Primary Contact for the same Client ceases to be Primary.

---

## BR-COM-006 — Contract Client Reference

Every Contract belongs to exactly one Client.

Contract references Client by identity.

Contract is an independent Aggregate Root.

Contract does not belong inside the Client Aggregate consistency boundary.

A Contract cannot exist without a Client.

---

## BR-COM-007 — Multiple Contracts per Client

A Client may be associated with multiple Contracts.

Business association between Client and Contract does not imply Aggregate ownership.

Contracts are persisted and validated through the Contract Aggregate.

Only Active Clients may create new Contracts.

Suspended Clients cannot create Contracts.

Archived Clients cannot create Contracts.

---

## BR-COM-008 — Operational Authorization for Missions

This is the single business rule that authorizes Mission creation.

Missions may be created only when the related Contract status is Active.

Clarification of Contract states used by this rule:

- Approved means the Contract has received commercial approval and has generated operational demand.
- Active means the Contract is in force for operational structuring.
- A Contract may become Active only after it has been Approved.
- Approved alone does not authorize Mission creation.
- Active is the only Contract status that authorizes Mission creation.

No other Contract status authorizes Mission creation.

---

## BR-COM-009 — Contracts That Never Authorize Operational Work

Cancelled Contracts shall never authorize Mission creation or operational work.

Archived Contracts shall never authorize Mission creation or operational work.

Draft, Under Review, Approved and Completed Contracts shall never authorize Mission creation.

Completed Contracts are read-only for business mutation.

---

## BR-COM-010 — Commercial Audit History

Commercial history shall remain auditable.

Commercial Aggregate Roots shall generate audit records for successful state-changing operations.

Business history is obtained from centralized Audit Events.

GetHistory behavior relies on the centralized AuditEvents model defined by ADR-009.

Domain Events are not the system of record for long-term commercial history.

Event Sourcing is not used to rebuild commercial Aggregate state.

Audit records are created only after successful transaction commit.

Failed transactions do not produce commercial audit history.

Approved commercial records shall never be physically deleted through business workflows.

---

## BR-COM-011 — Official Contract Lifecycle

Every Contract shall have exactly one valid lifecycle status at all times.

The official Contract lifecycle statuses are:

- Draft
- Under Review
- Approved
- Active
- Completed
- Cancelled
- Archived

Meaning of critical states:

| Status | Business meaning |
| --- | --- |
| Approved | Commercial approval completed; operational demand generated; may transition to Active |
| Active | Contract in force; sole status that authorizes Mission creation |
| Cancelled | Terminal commercial cancellation; never authorizes operational work |
| Archived | Business inactivation; historical retention; never authorizes new operational work |

Contract status values are closed enumerations governed by ADR-012.

---

## BR-COM-012 — Commercial Aggregate Ownership

Commercial Aggregate ownership follows ADR-010.

| Concept | Ownership |
| --- | --- |
| Lead | Commercial Aggregate Root |
| Client | Commercial Aggregate Root |
| Contact | Child entity of Client |
| Contract | Commercial Aggregate Root |
| Mission | Operations Aggregate concern; not part of Commercial |

Repositories exist only for Aggregate Roots.

Commercial repositories exist only for Lead, Client and Contract.

Contacts are persisted through the Client Aggregate.

One business use-case transaction modifies one Aggregate Root consistency boundary.

---

## BR-COM-013 — Archive Policy

Business Archive is the official business inactivation behavior for Commercial entities.

Archive follows ADR-011.

An archived commercial entity:

- has Status = Archived
- has ArchivedAt populated
- remains available for history, audit and authorized read access
- is read-only for business workflows

Business inactivation of Lead, Client, Contact and Contract uses Archive.

SoftDelete is not part of business workflows.

SoftDelete is a technical maintenance concept and shall not be used as a substitute for Archive.

Physical deletion of commercial business information through business workflows is prohibited.

---

## BR-COM-014 — Closed Enumerations

Commercial closed value sets, including lifecycle statuses and other fixed commercial classifications, follow ADR-012.

Business Rules do not require lookup tables for these closed enumerations.

Business Rules do not require seed data for these closed enumerations.

Enumeration values are governed closed product values.

---

# 5. Operations Business Rules

## BR-OPS-001

Every Mission belongs to one Contract.

The Contract must satisfy BR-COM-008 at the moment of Mission creation.

---

## BR-OPS-002

Every Mission contains one or more Tasks.

---

## BR-OPS-003

Every Task belongs to exactly one Mission.

---

## BR-OPS-004

Tasks consume productive capacity.

---

## BR-OPS-005

Assignments associate Tasks with Execution Resources.

---

## BR-OPS-006

Execution Resources may participate in multiple Assignments.

---

## BR-OPS-007

Assignments do not guarantee operational feasibility.

---

## BR-OPS-008

Operational structures shall exist independently from planning calculations.

---

## BR-OPS-009

Execution Resources may represent:

- Human Resources
- AI Resources
- Hybrid Resources
- External Resources

---

## BR-OPS-010

Operational structures remain valid until explicitly modified.

---

## BR-OPS-011

Mission belongs to the Operations Domain.

Mission is not a Commercial Aggregate Root and is not a child of Lead, Client, Contact or Contract Aggregates.

Mission references Contract by identity according to ADR-010.

---

# 6. Planning Business Rules

## BR-PLAN-001

Capacity shall always be measured in productive hours.

---

## BR-PLAN-002

Capacity calculations shall be deterministic.

---

## BR-501

Capacity shall only consider Active Working Calendars.

---

## BR-502

Capacity shall ignore non-working weekdays.

---

## BR-503

Capacity shall ignore Holidays.

---

## BR-504

Capacity shall respect configured Working Hours.

---

## BR-505

Capacity shall respect Resource Availability.

---

## BR-506

Capacity shall calculate planned working hours.

---

## BR-507

Historical calculations must remain reproducible from stored operational configuration.

---

## BR-508

If no operational configuration exists, Capacity shall return a validation/business-rule error and shall not fall back to hardcoded Monday–Friday logic.

---

## BR-509

Capacity calculation must be deterministic for the same inputs and configuration snapshot.

---

## BR-PLAN-003

Capacity never modifies operational information.

---

## BR-PLAN-004

Workload is calculated from planned operational effort.

---

## BR-PLAN-005

Availability depends on Capacity and Workload.

---

## BR-PLAN-006

Availability calculations never modify Assignments.

---

## BR-PLAN-007

Allocation Conflict Detection reports inconsistencies only.

---

## BR-PLAN-008

Planning calculations shall never recommend operational actions.

---

## BR-PLAN-009

Planning engines execute sequentially.

Capacity

↓

Workload

↓

Availability

↓

Conflict Detection

---

## BR-PLAN-010

Planning results remain reproducible.

---

# 7. Decision Business Rules

## BR-DEC-001

Only feasible execution strategies shall be generated.

---

## BR-DEC-002

Generated strategies shall always be evaluated.

---

## BR-DEC-003

Every evaluated strategy shall receive a ranking.

---

## BR-DEC-004

Every recommendation shall include an explanation.

---

## BR-DEC-005

Company Decision Profiles affect ranking only.

Company Decision Profiles follow ADR-007.

---

## BR-DEC-006

Ranking never changes evaluation metrics.

---

## BR-DEC-007

Decision recommendations remain deterministic.

---

## BR-DEC-008

Artificial Intelligence is excluded from MVP decision generation.

---

## BR-DEC-009

Human approval is mandatory before execution.

---

## BR-DEC-010

Decision Engine never executes operational work.

---

# 8. Cross-Domain Business Rules

## BR-SYS-001

Business Domains shall remain independent.

---

## BR-SYS-002

Business responsibilities shall never overlap.

---

## BR-SYS-003

Commercial generates demand.

Operations structures work.

Planning measures feasibility.

Decision recommends execution.

---

## BR-SYS-004

Business flow shall always follow:

Commercial

↓

Operations

↓

Planning

↓

Decision

↓

Execution

---

## BR-SYS-005

No downstream domain modifies upstream business information.

---

## BR-SYS-006

Business calculations remain deterministic.

---

## BR-SYS-007

Artificial Intelligence complements deterministic processing.

It never replaces it.

AI Factory concerns remain outside MVP product scope according to ADR-006.

---

## BR-SYS-008

Operational execution always requires human approval.

---

## BR-SYS-009

Every recommendation shall be reproducible.

---

## BR-SYS-010

Business history shall remain auditable.

Business history is obtained from centralized Audit Events according to ADR-009.

---

## BR-SYS-011

Documentation updates follow ADR-008 whenever governed delivery changes business behavior documentation.

---

# 9. Business Constraints

The following constraints are mandatory.

Commercial

- Lead conversion creates Client and preserves Lead history.
- Client creation in MVP occurs through Lead conversion only.
- Contact belongs to Client.
- Contract references Client by identity.
- Contract must be Approved before it may become Active.
- Mission creation requires Contract status Active.

Operations

- Mission required before Task.
- Mission references Contract by identity.
- Mission belongs to Operations, not Commercial.

Planning

- Operational structure required before calculations.

Decision

- Planning results required before recommendation.
- Company Decision Profiles affect ranking only.

Execution

- Human approval required before execution.

Persistence Behavior

- Archive is the business inactivation model.
- SoftDelete is outside business workflows.
- Closed enumerations do not require lookup tables.

---

# 10. Business Rule Classification

Business Rules are classified as follows.

Commercial

BR-COM-xxx

Operations

BR-OPS-xxx

Planning

BR-PLAN-xxx

Decision

BR-DEC-xxx

System

BR-SYS-xxx

Future releases shall follow this numbering convention.

Existing rule identifiers shall never be renumbered.

New rules may receive the next available identifier in the same series.

---

# 11. Rule Governance

Business Rules belong to Product Management.

Changes require:

- Product Review
- Functional Documentation Update
- Architecture Review (when applicable)
- Baseline Update (when applicable)

Business Rules shall never change through implementation alone.

Approved ADRs remain binding architecture decisions.

When Business Rules and Engineering Specifications diverge, Business Rules and approved ADRs take precedence until Engineering Specifications are reconciled.

---

# 12. Rule Traceability

Every Business Rule shall be traceable to:

- Functional Requirements
- Use Cases
- User Stories
- Acceptance Criteria
- Automated Tests

No Business Rule shall exist without downstream traceability.

Commercial rules BR-COM-001 through BR-COM-014 are authoritative for Commercial Engineering Specifications.

---

# 13. Traceability Matrix

| Rule Group | Related Documents |
|------------|-------------------|
| BR-COM | Commercial_Domain.md, ADR-009, ADR-010, ADR-011, ADR-012 |
| BR-OPS | Operations_Domain.md, ADR-010 |
| BR-PLAN | Planning_Engines.md |
| BR-DEC | Decision_Engine.md, ADR-007 |
| BR-SYS | Business_Domains.md, Program_Architecture.md, ADR-006, ADR-008, ADR-009 |

---

# 14. Architecture Alignment

## ADR-009

Commercial and system auditability obtain history from centralized Audit Events.

Domain Events are not long-term history storage.

## ADR-010

Lead, Client and Contract are Commercial Aggregate Roots.

Contact is a child of Client.

Mission belongs to Operations.

## ADR-011

Archive is official business inactivation behavior.

SoftDelete is not part of business workflows.

## ADR-012

Business Rules do not require lookup tables for closed enumerations.

## ADR-006 / ADR-007 / ADR-008

AI Factory remains outside MVP product scope.

Company Decision Profiles affect ranking only.

Documentation updates remain governed.

---

# 15. Functional Compliance Statement

The Business Rules defined in this document establish the official business behavior of AgencyOS Baseline 1.0.

All Functional Requirements, Use Cases, User Stories, Acceptance Criteria and implementations shall comply with these rules.

This document is the single authoritative source for Commercial business behavior.

Business Rules take precedence over implementation decisions whenever inconsistencies are identified.

No Business Rule in this document redesigns the product, expands MVP scope or modifies approved architecture.
