# ADR-007

Title

Company Decision Profiles

Status

Accepted

Date

2026-07-21

---

## Context

The Delivery Strategy Ranking service must compare evaluated execution strategies using configurable business priorities.

Different organizations prioritize cost, speed, operational stability, AI adoption or human resource utilization differently.

Hardcoded ranking weights would violate the Decision Support First principle and prevent the platform from adapting to different operational contexts.

The MVP must remain simple and avoid unnecessary database schema changes.

---

## Decision

Company Decision Profiles are introduced as configurable ranking weight sets.

Each profile defines:

- Id
- Code
- Name
- Dimensions (dimension name, weight, preferHigherValues flag)

During the MVP, profiles are stored in application configuration under the `CompanyDecisionProfiles` section in `appsettings.json`.

Profiles are loaded through `IOptions<CompanyDecisionProfilesOptions>` and exposed via `ICompanyDecisionProfileRepository` implemented in Infrastructure.

Six default profiles are provided:

- ProfitMaximization
- DeliverySpeed
- OperationalStability
- AiAdoption
- HumanResourceOptimization
- BalancedStrategy

Ranking uses only dimensions with positive weights.

Each dimension value is normalized using min-max scaling across all evaluated strategies before applying the configured weight.

---

## Alternatives Considered

### Database-Backed Profiles

Rejected for MVP because it requires schema changes and CRUD APIs not required to validate the Decision Engine.

Deferred to post-MVP when profile management becomes a product feature.

### Hardcoded Business Profiles

Rejected because it prevents configuration changes without code deployment and violates the requirement that profiles must not be hardcoded.

### LLM-Based Ranking

Rejected because ranking must be deterministic, auditable and independent of external AI services during the MVP.

---

## Consequences

Positive

- Ranking priorities are configurable without code changes.
- New profiles can be added through configuration.
- Ranking behavior is deterministic and testable.
- No database migration required during the MVP.

Negative

- Profile changes require application configuration updates and redeployment.
- No runtime profile management UI during the MVP.
- Profile validation is limited to application startup configuration binding.

---

## Related Decisions

- DEC-008-002 in Decision Log
- DEC-008-004 in Decision Log
- ADR-008 Documentation Update Workflow

---

## Future Evolution

Post-MVP, Company Decision Profiles may migrate to database persistence with management APIs while preserving the same domain model and repository interface.
