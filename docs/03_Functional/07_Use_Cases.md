# AgencyOS Functional Specification

# Use_Cases.md

Version: 1.0

Status: Approved

Baseline: 1.0

Owner: Product Management

Related Documents

- Business_Domains.md
- Commercial_Domain.md
- Operations_Domain.md
- Planning_Engines.md
- Decision_Engine.md
- Business_Rules.md
- Functional_Requirements.md
- Product Vision
- AgencyOS Baseline
- ADR-009
- ADR-010
- ADR-011
- ADR-012

---

# 1. Purpose

This document defines the official Use Cases of AgencyOS Baseline 1.0.

A Use Case describes how users interact with the system to achieve a business objective.

Each Use Case represents a complete business interaction.

Use Cases define business behavior.

They do not define implementation.

---

# 2. Scope

The MVP includes Use Cases for the following domains.

Commercial

Operations

Planning

Decision

Execution is intentionally outside the MVP scope.

---

# 3. Actors

The following actors interact with AgencyOS.

## Sales Representative

Responsible for commercial activities.

---

## Account Manager

Responsible for client management.

---

## Operations Manager

Responsible for operational organization.

---

## Resource Manager

Responsible for execution resources.

---

## Planning Manager

Responsible for operational planning.

---

## Decision Maker

Responsible for approving operational recommendations.

---

## System Administrator

Responsible for system configuration.

---

# 4. Commercial Use Cases

---

## UC-COM-001

### Register Lead

Objective

Register a new business opportunity.

Primary Actor

Sales Representative

Preconditions

None.

Main Flow

1. Create Lead.
2. Enter Lead information.
3. Save Lead.

Postconditions

Lead available for qualification.

---

## UC-COM-002

### Qualify Lead

Objective

Evaluate commercial opportunity.

Primary Actor

Sales Representative

Preconditions

Lead exists.

Lead Status is New.

Main Flow

1. Review Lead.
2. Update qualification.
3. Save result.

Postconditions

Lead Status becomes Qualified.

Invalid transitions are rejected.

Rejected is not a Lead lifecycle status.

Lost and Archived remain separate lifecycle outcomes defined by Business Rules.

---

## UC-COM-003

### Convert Lead to Client

Objective

Create a Client through Lead conversion.

Primary Actor

Account Manager

Sales Manager

Preconditions

Lead exists.

Lead Status is Won.

Mandatory Client identification is available.

Main Flow

1. Select Won Lead.
2. Enter mandatory Client identification.
3. Confirm conversion.
4. System creates exactly one Client Aggregate Root.
5. System sets Lead Status to Converted.
6. System preserves Lead history.

Postconditions

Client exists with OriginLeadId referencing the converted Lead.

Lead remains a Lead Aggregate Root with Status Converted.

Lead is read-only for business workflows.

Direct Client creation without Lead conversion is not allowed.

Reference

BR-COM-002

BR-COM-003

FR-COM-003

---

## UC-COM-004

### Register Client Contact

Objective

Register Client contact.

Primary Actor

Account Manager

Preconditions

Client exists.

Client is not Archived.

Main Flow

1. Select Client.
2. Add Contact through the Client Aggregate.
3. Optionally designate Primary Contact.
4. Save Contact.

Postconditions

Contact belongs to exactly one Client.

Contact is persisted through the Client Aggregate.

Only one Primary Contact exists per Client when Primary is assigned.

Reference

BR-COM-004

BR-COM-005

FR-COM-004

---

## UC-COM-005

### Register Contract

Objective

Create commercial contract.

Primary Actor

Sales Representative

Preconditions

Client exists.

Client Status is Active.

Main Flow

1. Select Active Client.
2. Create Contract referencing Client by identity.
3. Enter commercial information.
4. Save Contract with Status Draft.

Postconditions

Contract exists as an independent Aggregate Root.

Contract references exactly one Client.

Contract Status is Draft.

Suspended or Archived Clients cannot receive new Contracts.

Reference

BR-COM-006

BR-COM-007

FR-COM-005

---

## UC-COM-006

### Approve Contract

Objective

Approve commercial agreement.

Primary Actor

Sales Manager

Preconditions

Contract exists.

Contract Status is Under Review.

Main Flow

1. Review Contract.
2. Approve Contract.
3. System sets Status to Approved.
4. System records ApprovedByUserId and ApprovedAt.
5. System generates operational demand.

Postconditions

Contract Status is Approved.

Operational demand is generated.

Mission creation is not yet authorized.

Mission creation requires a later transition to Active according to BR-COM-008.

Reference

BR-COM-008

BR-COM-011

FR-COM-006

FR-COM-008

---

# 5. Operations Use Cases

---

## UC-OPS-001

Create Mission

Actor

Operations Manager

Objective

Create operational mission.

Preconditions

Contract exists.

Contract Status is Active.

Postconditions

Mission references Contract by identity.

Mission belongs to the Operations Domain.

Reference

BR-COM-008

BR-OPS-001

FR-OPS-001

---

## UC-OPS-002

Create Task

Actor

Operations Manager

Objective

Break Mission into Tasks.

---

## UC-OPS-003

Register Execution Resource

Actor

Resource Manager

Objective

Maintain execution resources.

---

## UC-OPS-004

Assign Resource

Actor

Operations Manager

Objective

Associate Resources with Tasks.

---

# 6. Planning Use Cases

---

## UC-PLAN-001

Calculate Capacity

Actor

Planning Manager

Objective

Calculate productive capacity.

---

## UC-PLAN-002

Calculate Workload

Objective

Measure operational effort.

---

## UC-PLAN-003

Calculate Availability

Objective

Determine resource availability.

---

## UC-PLAN-004

Detect Allocation Conflicts

Objective

Identify operational inconsistencies.

---

# 7. Decision Use Cases

---

## UC-DEC-001

Generate Strategies

Actor

Decision Manager

Objective

Generate candidate strategies.

---

## UC-DEC-002

Evaluate Strategies

Objective

Measure strategy quality.

---

## UC-DEC-003

Rank Strategies

Objective

Order strategies according to Decision Profiles.

---

## UC-DEC-004

Explain Recommendation

Objective

Produce deterministic explanation.

---

## UC-DEC-005

Approve Recommendation

Actor

Decision Maker

Objective

Approve recommended strategy.

Postcondition

Operational execution authorized.

---

# 8. Complete Business Flow

UC-COM-001 Register Lead

↓

UC-COM-002 Qualify Lead

↓

UC-COM-003 Convert Lead to Client

↓

UC-COM-004 Register Client Contact

↓

UC-COM-005 Register Contract

↓

UC-COM-006 Approve Contract

↓

Contract Activation to Active

↓

UC-OPS-001 Create Mission

↓

UC-OPS-002

↓

UC-OPS-003

↓

UC-OPS-004

↓

UC-PLAN-001

↓

UC-PLAN-002

↓

UC-PLAN-003

↓

UC-PLAN-004

↓

UC-DEC-001

↓

UC-DEC-002

↓

UC-DEC-003

↓

UC-DEC-004

↓

UC-DEC-005

Mission creation requires Contract Status Active according to BR-COM-008.

---

# 9. Use Case Relationships

Commercial

↓

Operations

↓

Planning

↓

Decision

Each Use Case consumes outputs from previous Use Cases.

---

# 10. Traceability

Each Use Case maps to:

Business Rules

↓

Functional Requirements

↓

User Stories

↓

Acceptance Criteria

↓

Automated Tests

---

# 11. Use Case Numbering

Commercial

UC-COM-xxx

Operations

UC-OPS-xxx

Planning

UC-PLAN-xxx

Decision

UC-DEC-xxx

Existing identifiers shall never change.

---

# 12. Functional Compliance Statement

These Use Cases describe the complete business behavior of AgencyOS Baseline 1.0.

Every Functional Requirement, User Story, Acceptance Criterion and implementation shall be traceable to one or more Use Cases.

No implementation shall introduce business behavior not represented by an approved Use Case.

These Use Cases are aligned with Business_Rules.md, Functional_Requirements.md and ADR-009 through ADR-012.