# ADR-010

Title

Aggregate Boundary Strategy

Status

Accepted

Date

2026-07-26

---

## Context

AgencyOS applies Domain-Driven Design (DDD) Aggregate boundaries to protect business invariants and define transactional consistency units.

Commercial Domain validation identified conflicting Aggregate definitions among Lead, Client, Contact, Contract and Mission.

Engineering Specifications must know which entities are Aggregate Roots, which entities are children, which repositories are allowed, and where Mission belongs.

---

## Problem

Unresolved Aggregate boundaries create inconsistent persistence ownership:

- Contact treated both as Aggregate Root and as Client child
- Contract treated both as Client child and as independent Aggregate Root
- Mission incorrectly associated with Commercial ownership
- Repositories defined for non-root entities

Without frozen boundaries, transactional consistency, repository design and Engineering Specifications diverge.

---

## Decision

AgencyOS adopts the following Aggregate Boundary Strategy for MVP 1.0.

### Commercial Domain Aggregate Roots

The Commercial Domain Aggregate Roots are:

- Lead
- Client
- Contract

No other Commercial entities are Aggregate Roots.

### Client and Contact

Client owns Contact as a child entity.

Contact:

- belongs to exactly one Client
- has no independent Aggregate lifecycle
- is created, updated, activated, deactivated and archived through the Client Aggregate
- is persisted through the Client Aggregate

Contact shall never exist without a Client.

### Contract

Contract is an independent Aggregate Root.

Contract:

- references a Client by identity
- does not belong inside the Client Aggregate consistency boundary
- owns its own lifecycle, invariants and repository
- is not persisted through the Client Aggregate

A Client may be associated with many Contracts. Association does not imply Aggregate ownership.

### Mission Boundary

Mission is not part of the Commercial Domain.

Mission belongs to the Operations Domain.

Commercial Aggregates shall not own Mission as a child entity.

Approved Contracts generate operational demand consumed by Operations. That handoff does not place Mission inside Commercial Aggregate boundaries.

### Repository Rule

Repositories exist only for Aggregate Roots.

Approved Commercial repositories are:

- ILeadRepository
- IClientRepository
- IContractRepository

There is no Contact repository as an Aggregate Root repository.

Contacts are loaded and persisted exclusively through the Client Aggregate.

### Aggregate Consistency Rules

Each Aggregate Root protects its own invariants.

Lead invariants are enforced inside the Lead Aggregate.

Client invariants, including Contact invariants owned by Client, are enforced inside the Client Aggregate.

Contract invariants are enforced inside the Contract Aggregate.

An Aggregate may reference another Aggregate only by identity. It shall not hold a navigable consistency-owned graph across Aggregate Roots.

Cross-aggregate rules are application or domain-service orchestration concerns, not intra-aggregate consistency rules.

### Transactional Boundaries

One use-case transaction modifies one Aggregate Root consistency boundary.

Lead operations commit within the Lead transactional boundary.

Client and Contact operations commit within the Client transactional boundary.

Contract operations commit within the Contract transactional boundary.

Cross-aggregate workflows may coordinate multiple transactions or explicit application orchestration, but shall not merge distinct Aggregate Roots into a single consistency-owned object graph.

---

## Rationale

This model follows DDD by aligning Aggregate Roots with true consistency boundaries.

Contact has meaning only inside Client ownership and therefore remains a child entity.

Contract has an independent lifecycle, approval semantics and operational handoff responsibility, and therefore remains its own Aggregate Root.

Mission belongs to Operations because it represents executable operational structure after commercial approval, not commercial ownership itself.

Restricting repositories to Aggregate Roots prevents persistence APIs from bypassing Aggregate invariants.

---

## Consequences

Positive

- Commercial Aggregate ownership is unambiguous.
- Contact persistence cannot bypass Client invariants.
- Contract lifecycle remains independently consistent.
- Mission ownership remains correctly placed in Operations.
- Repository surface area remains aligned with DDD.

Negative

- Contact APIs must be designed around Client Aggregate ownership.
- Client/Contract workflows require explicit cross-aggregate orchestration.
- Engineering Specifications that previously implied Contact or Contract child-root ambiguity must conform to this decision.

---

## Impacted Documents

- docs/02_Architecture/Program_Architecture.md
- docs/03_Functional/01_Business_Domains.md
- docs/03_Functional/02_Commercial_Domain.md
- docs/03_Functional/03_Operations_Domain.md
- docs/07_Project_Management/AgencyOS_Baseline_v1.0.md

---

## Impacted Engineering Specifications

- EFS-001 Lead Management
- EFS-002 Client Management
- EFS-003 Contact Management
- EFS-004 Contract Management
- Future Operations Engineering Specifications defining Mission

---

## Future Considerations

Additional Commercial or Operations Aggregates may be introduced only through a new ADR.

Any future change that promotes Contact to Aggregate Root, folds Contract into Client, or relocates Mission into Commercial requires explicit architectural revision.

Cross-aggregate process managers may be introduced later for long-running commercial-to-operations workflows without changing these Aggregate boundaries.
