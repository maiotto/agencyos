# ADR-011

Title

Archive and Soft Delete Policy

Status

Accepted

Date

2026-07-26

---

## Context

AgencyOS persistence must preserve business history while allowing entities to leave active operational use.

Commercial Domain validation identified ambiguity between Archive and SoftDelete across Aggregate Roots and child entities.

Engineering Specifications already expose Archived status, ArchivedAt fields and SoftDelete columns. No frozen policy defined which concept is business-facing and which is technical.

---

## Problem

Without a platform persistence policy, implementations may:

- use SoftDelete as a business workflow substitute for Archive
- physically delete historical records
- expose SoftDelete through business APIs
- apply inconsistent search and API filtering for inactive records

This blocks consistent Commercial Domain behavior and auditability.

---

## Decision

AgencyOS adopts Archive as the business inactivation model and SoftDelete as a restricted technical maintenance concept.

### Archive

Archive is a business concept.

An archived entity:

- receives business Status = Archived
- receives ArchivedAt populated with the archive timestamp
- remains available for history, audit and authorized read access
- becomes read-only for business workflows
- is never physically deleted as part of normal business operations

Archive is the approved mechanism for removing an entity from active commercial or operational use while preserving historical presence.

### SoftDelete

SoftDelete is a technical concept.

SoftDelete:

- is not used by business workflows
- is not a substitute for Archive
- is reserved for exceptional maintenance operations
- does not represent a business lifecycle state
- shall not be exposed as a normal business action in product workflows

SoftDelete exists only to support exceptional technical maintenance where a record must be excluded from normal persistence visibility without physical deletion.

### Difference

| Dimension | Archive | SoftDelete |
| --- | --- | --- |
| Nature | Business concept | Technical concept |
| Trigger | Business workflow | Exceptional maintenance |
| Status | Status = Archived | Not a business status |
| Timestamp | ArchivedAt populated | Technical marker only |
| History | Remains available | Record retained but excluded from normal visibility |
| API exposure | Business Archive operations | Not part of normal business APIs |

### Preference for Archive

Archive is preferred because it:

- preserves an explicit business lifecycle state
- keeps historical records available for GetHistory and authorized reads
- aligns with auditability and commercial traceability
- avoids conflating maintenance mechanics with business meaning

Business inactivation shall use Archive.

### Search Behavior

Default search and list queries return non-archived and non-soft-deleted records.

Archived records:

- are excluded from default active searches
- may be included when the query explicitly requests archived records
- remain retrievable by identifier for authorized history and detail access

Soft-deleted records:

- are excluded from all normal business searches
- are excluded from normal detail retrieval used by business workflows
- are visible only to exceptional maintenance pathways expressly authorized for that purpose

### API Behavior

Business APIs expose Archive operations where the domain lifecycle allows archival.

Business APIs do not expose SoftDelete as a normal command.

Archived entities:

- reject business update and transition commands that would mutate business state
- remain readable through authorized detail and history endpoints

SoftDelete:

- is outside normal product API contracts
- shall not appear as a standard business status transition

Physical deletion through business APIs is prohibited.

---

## Rationale

Separating Archive from SoftDelete prevents technical maintenance flags from becoming accidental business lifecycle states.

Archive keeps entities historically available, which is required for commercial auditability and operational traceability.

Restricting SoftDelete to exceptional maintenance avoids dual inactivation models in user-facing workflows.

Default search exclusion of archived and soft-deleted records keeps operational screens focused on active work while preserving explicit access to historical records.

---

## Consequences

Positive

- Business inactivation has one approved model: Archive.
- Historical records remain available after archival.
- SoftDelete cannot silently replace business lifecycle design.
- Search and API behavior are consistent across Aggregates.

Negative

- Engineering Specifications must distinguish Status/ArchivedAt from SoftDelete.
- Maintenance tooling for SoftDelete requires separate governance.
- Queries must apply explicit filters for archived inclusion.

---

## Impacted Documents

- docs/02_Architecture/Program_Architecture.md
- docs/03_Functional/02_Commercial_Domain.md
- docs/03_Functional/06_Business_Rules.md
- docs/03_Functional/09_Non_Functional_Requirements.md
- docs/07_Project_Management/AgencyOS_Baseline_v1.0.md

---

## Impacted Engineering Specifications

- EFS-001 Lead Management
- EFS-002 Client Management
- EFS-003 Contact Management
- EFS-004 Contract Management
- Future Engineering Specifications defining inactivation behavior

---

## Future Considerations

Post-MVP, retention schedules may define when archived records move to colder storage.

Any restoration-from-archive capability requires explicit business rules and a new decision if current domain rules prohibit reactivation.

SoftDelete shall remain outside business workflows unless a future ADR reclassifies platform maintenance semantics.
