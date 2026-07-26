# ADR-012

Title

Enumeration Persistence Strategy

Status

Accepted

Date

2026-07-26

---

## Context

AgencyOS persists many closed business enumerations such as statuses, sources, industries, sizes, billing models and contact preferences.

Commercial Domain validation identified conflicting approaches: C# enums, lookup tables, seed data and mixed API representations.

Engineering Specifications require a single persistence and API contract for enumerations before Commercial schema and OpenAPI contracts can freeze.

---

## Problem

Without a frozen enumeration strategy, implementations may introduce:

- lookup tables for fixed value sets
- seed migrations for static enumerations
- integer API payloads that reduce readability
- divergent Swagger documentation for the same concepts

This blocks consistent Commercial Domain persistence and API design.

---

## Decision

AgencyOS adopts C# enumerations persisted as database integers for MVP 1.0.

### Implementation Model

Enumerations are implemented as C# enums in the domain and application contracts.

Enumerations are persisted as `smallint` columns.

Lookup tables are not used for these enumerations.

Seed data is not created for these enumerations.

### Persistence Mapping

Entity Framework Core conversions map C# enum values to `smallint` database values.

The database stores integer values.

Application and persistence mapping remain responsible for translating between enum members and stored integers.

### API and Documentation Contract

APIs return enumeration values as enum names.

Swagger / OpenAPI documentation uses enum names.

Clients consume symbolic names, not raw numeric codes, through the public API contract.

The database remains the integer system of record for stored enum values.

### Summary

| Layer | Representation |
| --- | --- |
| Domain / Application | C# enum |
| Database | smallint integer value |
| API response | enum name |
| Swagger | enum name |
| Lookup tables | Not used |
| Seed data | Not used |

---

## Rationale

Closed value sets that change only through governed product decisions do not require runtime lookup administration in MVP 1.0.

C# enums provide compile-time safety and a single source of allowed values.

Persisting as `smallint` keeps storage compact and relational queries efficient.

EF Core conversions avoid spreading manual cast logic across repositories.

Returning enum names through APIs and Swagger improves readability and reduces client-side magic numbers.

Avoiding lookup tables and seed data removes unnecessary schema and migration surface for values already defined in code.

---

## Consequences

Positive

- Enumeration handling is uniform across Aggregates.
- No lookup-table maintenance for fixed MVP enumerations.
- No seed scripts for static enum values.
- API and Swagger contracts remain human-readable.
- Database storage remains compact and typed as smallint.

Negative

- Adding or renaming enum members requires a governed code change.
- Integer values stored in the database must remain stable once published.
- Configurable, tenant-administered enumerations are out of scope for this model.

---

## Impacted Documents

- docs/02_Architecture/Program_Architecture.md
- docs/03_Functional/02_Commercial_Domain.md
- docs/05_Technical/
- docs/07_Project_Management/AgencyOS_Baseline_v1.0.md

---

## Impacted Engineering Specifications

- EFS-001 Lead Management
- EFS-002 Client Management
- EFS-003 Contact Management
- EFS-004 Contract Management
- Future Engineering Specifications defining closed enumerations

---

## Future Considerations

If configurable or tenant-administered enumerations become necessary, AgencyOS shall migrate through an explicit ADR.

A future migration strategy shall:

1. Identify which enumerations must become configurable.
2. Introduce lookup persistence only for those enumerations.
3. Preserve existing smallint values as stable codes during transition.
4. Provide a compatibility layer so historical rows remain interpretable.
5. Update API contracts only after compatibility rules are defined.

Fixed platform enumerations that remain closed product decisions may continue as C# enums persisted as smallint.

No silent dual model shall be introduced. A given enumeration is either code-defined or configurable by approved decision, not both without an ADR.
