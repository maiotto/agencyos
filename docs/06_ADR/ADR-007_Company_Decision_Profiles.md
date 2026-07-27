# ADR-007

Title

Company Decision Profiles

Status

Accepted

Date

2026-07-21

---

## Context

The Delivery Strategy Ranking Engine compares evaluated execution strategies and produces an ordered list from best to worst.

Different organizations prioritize cost, speed, operational stability, AI adoption or human resource utilization differently.

Hard-coded ranking weights would violate the Decision Support First principle and prevent the platform from adapting to different operational contexts.

The MVP must remain simple and avoid unnecessary database schema changes.

---

## Decision

The Delivery Strategy Ranking Engine shall be driven by configurable Company Decision Profiles instead of hard-coded business rules.

Each Company Decision Profile defines:

- Id
- Code
- Name
- Dimensions (dimension name, weight, preferHigherValues flag)

During the MVP, profiles are configuration-driven and stored in application configuration under the `CompanyDecisionProfiles` section in `appsettings.json`.

Profiles are loaded through `IOptions<CompanyDecisionProfilesOptions>` and exposed via `ICompanyDecisionProfileRepository` implemented in Infrastructure.

The ranking algorithm (`DeliveryStrategyRankingCalculation`) applies min-max normalization and weighted scoring using only the active dimensions supplied by the selected profile. It does not embed business priorities.

Six default profiles are provided:

- Balanced Strategy
- Profit Maximization
- Delivery Speed
- Operational Stability
- AI Adoption
- Human Resource Optimization

Ranking uses only dimensions with positive weights.

Each dimension value is normalized using min-max scaling across all evaluated strategies before applying the configured weight.

---

## Rationale

Separating business priorities from the ranking algorithm preserves a single, deterministic scoring mechanism while allowing operational priorities to change without code deployment.

Configuration-driven profiles satisfy the MVP constraint of avoiding database migrations while still enabling organizations to express distinct decision strategies.

A profile-based model aligns with the Decision Engine pipeline: strategy generation and evaluation remain objective; ranking applies configurable priorities as a downstream concern.

Keeping the ranking calculation independent of business priorities ensures the algorithm is testable, auditable and reusable regardless of which profile is selected.

---

## Alternatives Considered

### Hard-Coded Business Profiles

Rejected because it prevents configuration changes without code deployment and violates the requirement that profiles must not be hardcoded.

### Database-Backed Profiles

Rejected for MVP because it requires schema changes and CRUD APIs not required to validate the Decision Engine.

Deferred to post-MVP when profile management becomes a product feature.

### LLM-Based Ranking

Rejected because ranking must be deterministic, auditable and independent of external AI services during the MVP.

### Business Priorities Embedded in Ranking Algorithm

Rejected because it couples scoring logic to specific operational strategies and prevents profile changes without modifying application code.

---

## Consequences

Positive

- Ranking priorities are configurable without code changes.
- New profiles can be added through configuration.
- Ranking behavior is deterministic and testable.
- The ranking algorithm remains independent of business priorities.
- No database migration required during the MVP.

Negative

- Profile changes require application configuration updates and redeployment.
- No runtime profile management UI during the MVP.
- Profile validation is limited to application startup configuration binding.

---

## Future Evolution

Post-MVP, Company Decision Profiles may migrate from application configuration to database administration with management APIs.

Such a migration must preserve the same domain model, repository interface and ranking algorithm contract.

Profile persistence may evolve; the ranking algorithm must remain independent of business priorities.

---

## Related Decisions

- DEC-008-002 in Decision Log
- DEC-008-004 in Decision Log
- DEC-401-001 in Decision Log
- ADR-008 Documentation Update Workflow

---

## Implementation Note (Release 1.1 / US-401)

The "Future Evolution" migration described above was implemented in Release 1.1 (US-401, Company Decision Profiles).

Company Decision Profiles are now database-backed:

- Persisted in the `company_decision_profile` table (see migration `20260726340000_create_company_decision_profile_tables.sql`), replacing the configuration-only `CompanyDecisionProfiles` section in `appsettings.json` and the `CompanyDecisionProfilesOptions` binding.
- `ICompanyDecisionProfileRepository` is implemented directly against `ApplicationDbContext` (EF Core) instead of `IOptions<CompanyDecisionProfilesOptions>`.
- Each Company owns its own set of profiles, versioned by an immutable `ProfileFamilyId` lineage: editing a profile creates a new `Version` (BR-1905) and deactivates the previous version rather than mutating it in place.
- Exactly one profile per company may be the default Active profile (BR-1901), enforced both at the service layer and via a partial unique index (`uq_company_decision_profile_one_default`).
- A `CompanyDecisionProfileService` and `DecisionProfilesController` (`/decision-profiles`) expose full CRUD plus Activate/Deactivate/Archive/Clone/SetDefault/ClearDefault lifecycle operations, replacing the MVP's read-only configuration binding.
- The six original default profiles are preserved as seed data for the default company (`aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa`) with the same Ids used previously in configuration, with "Balanced Strategy" seeded as the default Active profile.
- The ranking algorithm contract is unchanged: `DeliveryStrategyRankingCalculation` still applies min-max normalization and weighted scoring using only the active dimensions supplied by the selected profile, and remains independent of business priorities. Ranking now additionally rejects Inactive/Archived profiles (BR-1904) and records the `CompanyDecisionProfileId`/`Version` used on every generated `Recommendation`, `AIRecommendation`, `Explainability`, `ExecutiveRecommendationSummary`, and `RecommendationHistory` record for traceability.

This supersedes the "Database-Backed Profiles" alternative previously rejected for MVP scope; it is no longer rejected, it is the current implementation for Release 1.1 and beyond.
