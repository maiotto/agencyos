# ADR-009

Title

Audit and History Strategy

Status

Accepted

Date

2026-07-26

---

## Context

AgencyOS MVP 1.0 requires that business changes remain auditable across all domains.

Commercial Domain validation identified unresolved ambiguity between Domain Events, audit persistence, history queries and aggregate state reconstruction.

Non-Functional Requirements demand that business history remain traceable and that changes to business information remain auditable.

Engineering Specifications define GetHistory queries for Aggregate Roots. No approved platform decision defined where history is stored, when it is written, or how it relates to Domain Events.

---

## Problem

Without a frozen audit strategy, Engineering Specifications cannot consistently define:

- when audit records are created
- where audit history is persisted
- how GetHistory queries obtain information
- whether Domain Events reconstruct aggregate state
- whether Event Sourcing is permitted

Unresolved ambiguity blocks Commercial Domain implementation and creates divergent persistence designs across Aggregate Roots.

---

## Decision

AgencyOS adopts a centralized Audit History strategy for MVP 1.0.

### Scope

Every Aggregate Root shall generate audit records for successful state-changing operations.

Audit history is platform-wide and centralized.

Event Sourcing is not adopted.

Aggregate state is persisted in transactional tables. Audit history is a separate persistence concern and shall never be used to rebuild Aggregate state.

### Timing

Audit records shall be written only after successful transaction commit of the business change.

Failed transactions shall not produce audit history.

### AuditEvents Persistence Model

Audit history is stored in a centralized `AuditEvents` persistence model.

Each Audit Event shall capture at least:

| Field | Purpose |
| --- | --- |
| AuditEventId | Unique identifier of the audit record |
| OccurredAt | Timestamp of the audited change |
| AggregateType | Type of the Aggregate Root |
| AggregateId | Identifier of the Aggregate Root |
| EventType | Name of the audited action |
| ActorUserId | User or system actor that caused the change |
| CorrelationId | Optional correlation across related operations |
| Payload | Structured representation of relevant before/after or change data |

Audit Events are immutable after creation.

Physical deletion of Audit Events is prohibited.

### GetHistory Queries

GetHistory queries obtain information exclusively from the centralized AuditEvents store.

GetHistory is a read-side query filtered by AggregateType and AggregateId.

GetHistory shall not read Domain Event buses, message queues or Aggregate tables to reconstruct history.

### Domain Events vs Audit Events

Domain Events and Audit Events are separate concepts with separate responsibilities.

Domain Events:

- express that a business fact occurred inside the domain model
- support application reactions, integration and workflow orchestration
- are published after successful transaction commit
- are not the system of record for long-term business history

Audit Events:

- express an immutable historical record of a successful business change
- support GetHistory, traceability and compliance
- are persisted in the centralized AuditEvents store after successful transaction commit
- are not used to drive domain workflow

A single successful business operation may produce both a Domain Event and an Audit Event. One shall not substitute for the other.

---

## Rationale

Centralized audit after commit provides a single, queryable history model without introducing Event Sourcing complexity.

Separating Domain Events from Audit Events preserves DDD communication patterns while keeping compliance history independent of messaging infrastructure.

Writing audit records only after successful commit prevents false history for rolled-back transactions.

Persisting Aggregate state transactionally keeps the MVP deterministic, inspectable and aligned with the existing relational persistence model.

---

## Consequences

Positive

- Every Aggregate Root has a uniform audit obligation.
- GetHistory has a single authoritative source.
- Aggregate persistence remains transactional and non-event-sourced.
- Domain Event design remains free of history-storage concerns.
- Failed transactions do not pollute audit history.

Negative

- Dual emission of Domain Events and Audit Events adds application responsibility after commit.
- Centralized AuditEvents becomes a cross-domain dependency for history reads.
- Payload design must remain stable enough for historical interpretation.

---

## Impacted Documents

- docs/02_Architecture/Program_Architecture.md
- docs/03_Functional/02_Commercial_Domain.md
- docs/03_Functional/09_Non_Functional_Requirements.md
- docs/07_Project_Management/AgencyOS_Baseline_v1.0.md

---

## Impacted Engineering Specifications

- EFS-001 Lead Management
- EFS-002 Client Management
- EFS-003 Contact Management
- EFS-004 Contract Management
- Future Aggregate Root Engineering Specifications requiring GetHistory

---

## Future Considerations

Post-MVP, AuditEvents may gain retention policies, archival storage tiers or specialized analytical projections.

Any evolution must preserve:

- centralized audit history
- post-commit write semantics
- prohibition of Event Sourcing for Aggregate reconstruction
- separation between Domain Events and Audit Events

---

## Implementation Note (US-206)

Release 1.1 delivers the centralized Audit Trail via `audit_event`, automatic non-blocking `IAuditService.RecordSafeAsync` hooks, correlation middleware, and read-only `/audit` APIs, completing EPIC-02 Decision Evolution.

## Implementation Note (US-301)

AI-assisted Recommendation generation and archive emit Audit Events with entity type `AIRecommendation`. Advisory metadata (model/prompt versions) is stored in audit metadata. AI generation failures never mutate Recommendation lifecycle records.

## Implementation Note (US-302)

LLM Explainability generation and archive emit Audit Events with entity type `Explainability`. Informational metadata (model/prompt versions, explanation type) is stored in audit metadata. Explanation generation failures never mutate Recommendation lifecycle records.

## Implementation Note (US-303)

Executive Recommendation Summary generation, versioning, and archive emit Audit Events with entity type `ExecutiveRecommendationSummary`. Briefing metadata (model/prompt versions, linked AI/Explainability ids) is stored in audit metadata. Generation failures never mutate Recommendation, AI Recommendation, or Decision records. Completes EPIC-03.