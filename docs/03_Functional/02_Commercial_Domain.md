# AgencyOS Functional Specification

# Commercial_Domain.md

Version: 1.0

Status: Approved

Baseline: 1.0

Owner: Product Management

Related Documents

- Business_Domains.md
- Product Vision
- Product Scope
- Program Architecture
- AgencyOS Baseline
- ADR-009 Audit and History Strategy
- ADR-010 Aggregate Boundary Strategy
- ADR-011 Archive and Soft Delete Policy
- ADR-012 Enumeration Persistence Strategy

---

# 1. Purpose

This document defines the functional specification of the Commercial Domain.

The Commercial Domain is responsible for managing commercial demand from the identification of a business opportunity until an approved contract becomes operational demand.

This document describes business behavior.

Implementation details are intentionally excluded.

This specification is aligned with the approved architecture decisions ADR-009, ADR-010, ADR-011 and ADR-012.

---

# 2. Scope

The Commercial Domain includes:

- Lead Management
- Client Management
- Client Contact Management
- Client Contract Management

The Commercial Domain ends when an approved contract is delivered to the Operations Domain.

Mission is not part of the Commercial Domain.

Mission belongs to the Operations Domain.

---

# 3. Business Objective

The Commercial Domain transforms market opportunities into approved operational demand.

Its responsibility is commercial management.

It is not responsible for operational planning or execution.

---

# 4. Functional Responsibilities

The Commercial Domain shall provide the following capabilities.

## Lead Management

Register and manage commercial opportunities.

Track the complete sales lifecycle.

Support opportunity qualification.

---

## Client Management

Maintain customer information.

Store organizational information.

Maintain customer lifecycle.

---

## Client Contact Management

Maintain customer contacts.

Support multiple contacts per client.

Allow different contact roles.

Contacts are managed as part of the Client Aggregate.

---

## Client Contract Management

Manage customer contracts.

Control contract lifecycle.

Generate approved operational demand.

Contracts are managed as an independent Aggregate Root that references a Client.

---

# 5. Business Components

## Lead

Represents a potential business opportunity.

Lead is an Aggregate Root.

Possible states include:

- New
- Qualified
- Proposal
- Negotiation
- Won
- Lost
- Converted
- Archived

---

## Client

Represents an organization receiving services.

Client is an Aggregate Root.

A Client may be associated with multiple Contracts.

A Client contains multiple Contacts as child entities.

Possible states include:

- Active
- Suspended
- Archived

---

## Client Contact

Represents an individual associated with a Client.

Contact is a child entity of the Client Aggregate.

Contact is not an Aggregate Root.

A Contact belongs to exactly one Client.

A Contact may participate in multiple commercial interactions.

A Contact shall never exist without a Client.

---

## Client Contract

Represents a formal commercial agreement.

Contract is an Aggregate Root.

Contract references a Client by identity.

Contract does not belong inside the Client Aggregate consistency boundary.

Association between Client and Contract is a business relationship.

Association does not imply Aggregate ownership.

Only Approved Contracts generate operational demand.

Possible states include:

- Draft
- Under Review
- Approved
- Active
- Completed
- Cancelled
- Archived

---

# 6. Aggregate Boundaries

The Commercial Domain Aggregate Roots are:

- Lead
- Client
- Contract

Contact is not an Aggregate Root.

Contact is owned by Client and persisted through the Client Aggregate.

Repositories exist only for Aggregate Roots.

Commercial Aggregate repositories are therefore limited to Lead, Client and Contract.

Each Aggregate Root protects its own invariants.

Lead invariants are enforced inside the Lead Aggregate.

Client invariants, including Contact invariants owned by Client, are enforced inside the Client Aggregate.

Contract invariants are enforced inside the Contract Aggregate.

An Aggregate may reference another Aggregate only by identity.

Cross-aggregate coordination is an application concern and does not merge Aggregate Roots into a single consistency boundary.

One business use-case transaction modifies one Aggregate Root consistency boundary.

Client and Contact operations commit within the Client transactional boundary.

Contract operations commit within the Contract transactional boundary.

Lead operations commit within the Lead transactional boundary.

Mission is outside Commercial Aggregate ownership and belongs to Operations.

This Aggregate model follows ADR-010.

---

# 7. Functional Workflow

Commercial workflow

Lead

↓

Qualification

↓

Proposal

↓

Negotiation

↓

Client

↓

Contract

↓

Approval

↓

Operations Domain

Approved Contracts generate operational demand consumed by Operations.

That handoff does not place Mission inside Commercial Aggregate boundaries.

---

# 8. Business Rules

## BR-COM-001

Every Lead must have a status.

---

## BR-COM-002

A Lead may become a Client.

Not every Lead becomes a Client.

---

## BR-COM-003

A Client may have multiple Contacts.

---

## BR-COM-004

Every Contact belongs to exactly one Client.

---

## BR-COM-005

A Client may own multiple Contracts.

---

## BR-COM-006

A Contract always belongs to one Client.

---

## BR-COM-007

Only Approved Contracts generate operational demand.

---

## BR-COM-008

Cancelled Contracts never generate operational demand.

---

## BR-COM-009

Commercial information shall remain historically traceable.

Deletion of approved commercial history is not permitted.

---

# 9. Functional Boundaries

The Commercial Domain SHALL NOT

- calculate capacity
- allocate resources
- create missions
- create tasks
- calculate workload
- recommend execution strategies

These responsibilities belong to downstream domains.

---

# 10. Functional Inputs

The Commercial Domain receives:

- Sales opportunities
- Customer information
- Commercial negotiations
- Contract amendments

---

# 11. Functional Outputs

The Commercial Domain produces:

- Approved Clients
- Commercial History
- Approved Contracts
- Operational Demand

Commercial History is obtained from the platform audit history model defined by ADR-009.

---

# 12. Domain Events and Audit History

## Domain Events

The Commercial Domain produces Domain Events that express business facts.

Events Produced

- Lead Created
- Lead Qualified
- Client Created
- Contact Created
- Contract Created
- Contract Approved
- Contract Updated
- Contract Cancelled

Events Consumed

None.

The Commercial Domain starts the business process.

Domain Events support application reactions, integration and workflow orchestration.

Domain Events are published after successful transaction commit.

Domain Events are not the system of record for long-term commercial history.

## Audit History

Every Commercial Aggregate Root generates audit records for successful state-changing operations.

Audit history is centralized in the platform AuditEvents model.

Event Sourcing is not used.

Aggregate state is persisted transactionally.

Audit history shall never be used to rebuild Aggregate state.

Audit records are written only after successful transaction commit.

Failed transactions do not produce audit history.

GetHistory queries obtain commercial history exclusively from the centralized AuditEvents store.

A successful commercial operation may produce both a Domain Event and an Audit Event.

One shall not substitute for the other.

This audit model follows ADR-009.

---

# 13. Entity Relationships

Lead

Independent Aggregate Root.

A Lead may become a Client.

Conversion creates a Client.

A Lead does not become a Client entity.

---

Client

Aggregate Root.

Client

↓

Contact

A Client contains many Contacts as child entities.

Contacts are persisted through the Client Aggregate.

---

Contract

Independent Aggregate Root.

Client ←—— references by identity —— Contract

A Client may be associated with many Contracts.

A Contract always references exactly one Client.

Contracts are not persisted through the Client Aggregate.

---

Mission

Not part of the Commercial Domain.

Mission belongs to the Operations Domain.

---

# 14. State Transitions

## Lead

New

↓

Qualified

↓

Proposal

↓

Negotiation

↓

Won

↓

Converted

or

Lost

or

Archived

Archived Leads remain available for history and authorized read access.

Archived Leads are read-only for business workflows.

---

## Client

Active

↓

Suspended

↓

Archived

or

Active

↓

Archived

Archived Clients remain available for history and authorized read access.

Archived Clients are read-only for business workflows.

---

## Contract

Draft

↓

Under Review

↓

Approved

↓

Active

↓

Completed

or

Cancelled

or

Archived

Archived Contracts remain available for history and authorized read access.

Archived Contracts are read-only for business workflows.

---

# 15. Archive and Soft Delete Policy

## Archive

Archive is the business inactivation model for Commercial entities.

An archived commercial entity:

- receives business Status = Archived
- receives ArchivedAt populated
- remains available for history, audit and authorized read access
- becomes read-only for business workflows
- is never physically deleted as part of normal business operations

Business inactivation of Lead, Client, Contact and Contract uses Archive.

## SoftDelete

SoftDelete is a technical concept.

SoftDelete is not used by Commercial business workflows.

SoftDelete is not a substitute for Archive.

SoftDelete is reserved for exceptional maintenance operations and is outside normal commercial product behavior.

## Search and Access Behavior

Default commercial searches return non-archived records.

Archived records may be included when a query explicitly requests archived records.

Archived records remain retrievable by identifier for authorized detail and history access.

Physical deletion of commercial business information through business workflows is prohibited.

This policy follows ADR-011 and supports BR-COM-009.

---

# 16. Closed Enumerations

Commercial closed value sets such as statuses and other fixed commercial classifications are platform closed enumerations.

These enumerations:

- are defined as governed closed values
- are not administered through lookup tables in MVP 1.0
- do not require seed data for static enumeration values
- are exposed to consumers as enumeration names
- are stored as integer values in persistence

This enumeration model follows ADR-012.

---

# 17. Validation Rules

Mandatory information

Lead

- Name
- Opportunity

Client

- Name

Contact

- Name
- Client

Contract

- Client
- Status

Business validations

A Contract cannot exist without a Client.

A Contact cannot exist without a Client.

Approved Contracts become operational demand.

Contact creation and change occur through the Client Aggregate.

Contract creation and change occur through the Contract Aggregate.

---

# 18. Security Responsibilities

Commercial information shall only be modified by authorized users.

Historical commercial information shall remain auditable.

Commercial history remains available through centralized audit history after successful commit.

---

# 19. Domain Interfaces

Produces information to:

Operations Domain

Consumes information from:

None

---

# 20. Functional Constraints

The Commercial Domain never performs operational calculations.

Business calculations belong to the Planning Domain.

Operational recommendations belong to the Decision Domain.

The Commercial Domain never creates Missions or Tasks.

---

# 21. MVP Coverage

Included

✔ Lead

✔ Client

✔ Contact

✔ Contract

Excluded

Sales Forecast

CRM Automation

Marketing Automation

Proposal Generator

Quotation Engine

Pipeline Analytics

These capabilities belong to future releases.

---

# 22. Traceability

Related Functional Documents

Business_Domains.md

Operations_Domain.md

Business_Rules.md

Use_Cases.md

Functional_Requirements.md

User_Stories.md

Acceptance_Criteria.md

Related Product Documents

Product Vision

Product Scope

Program Architecture

AgencyOS Baseline

Related Architecture Decision Records

ADR-009 Audit and History Strategy

ADR-010 Aggregate Boundary Strategy

ADR-011 Archive and Soft Delete Policy

ADR-012 Enumeration Persistence Strategy

---

# 23. Architecture Alignment

This Commercial Domain specification is consistent with the approved architecture as follows.

ADR-009

Commercial Aggregate Roots produce audit history after successful commit.

Domain Events and Audit Events remain separate.

Commercial history is centralized and is not event-sourced.

ADR-010

Commercial Aggregate Roots are Lead, Client and Contract.

Contact is a child entity of Client.

Contract is an independent Aggregate Root referencing Client by identity.

Mission belongs to Operations.

Repositories exist only for Aggregate Roots.

ADR-011

Archive is the business inactivation model.

SoftDelete is technical and outside commercial business workflows.

Archived commercial entities remain historically available and read-only.

ADR-012

Commercial closed enumerations follow the platform enumeration persistence strategy.

No Commercial Domain lookup tables or seed data are required for closed enumerations in MVP 1.0.

---

# 24. Functional Compliance Statement

The Commercial Domain is responsible exclusively for commercial demand management.

Operational planning begins only after contract approval.

No capability defined in this specification may violate the domain boundaries established in Business_Domains.md.

No capability defined in this specification may violate ADR-009, ADR-010, ADR-011 or ADR-012.

This document is the official functional specification for the Commercial Domain of AgencyOS Baseline 1.0.
