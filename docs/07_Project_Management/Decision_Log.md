# AgencyOS Decision Log

Version: 1.0

Status: Active

---

# Purpose

This document registers architectural and engineering decisions taken during AgencyOS development.

Each entry captures the decision context, the decision itself and its consequences.

Decisions recorded here reflect approved implementation choices. They must not be changed through documentation updates alone.

---

# Sprint 7 Decisions

## DEC-007-001

**Title**

Analytical Engine Separation

**Date**

2026-07-21

**Status**

Accepted

**Context**

Operational intelligence requires progressive calculation of capacity, workload, availability and allocation conflicts.

**Decision**

Implement four independent analytical engines as dedicated Application services:

- CapacityCalculatorService
- WorkloadCalculatorService
- AvailabilityEngineService
- AllocationConflictDetectionService

Each engine exposes its own REST controller and reuses upstream calculation results.

**Consequences**

Calculation responsibilities are isolated, testable and reusable by the Decision Engine.

---

## DEC-007-002

**Title**

Static Calculation Classes for Analytical Logic

**Date**

2026-07-21

**Status**

Accepted

**Context**

Analytical engines require deterministic, side-effect-free calculations.

**Decision**

Extract pure calculation logic into static classes in the Application layer:

- CapacityCalculation
- WorkloadCalculation
- AvailabilityCalculation
- AllocationConflictCalculation

Services orchestrate data loading and delegate calculations to these classes.

**Consequences**

Business calculations remain testable without mocking infrastructure dependencies.

---

## DEC-007-003

**Title**

Progressive Engine Orchestration

**Date**

2026-07-21

**Status**

Accepted

**Context**

Each analytical metric depends on previously calculated operational data.

**Decision**

Engines must compose upstream services rather than duplicate logic:

Availability consumes Capacity and Workload.

Allocation Conflict Detection consumes Availability.

**Consequences**

A consistent calculation chain is established for reuse by the Delivery Strategy Engine.

---

## DEC-007-004

**Title**

Reuse of Calculation Services

**Date**

2026-07-21

**Status**

Accepted

**Context**

Analytical engines operate on shared operational data. Reimplementing formulas or reloading raw execution data in each engine would produce inconsistent metrics and duplicate maintenance effort.

**Decision**

Each analytical engine must invoke upstream calculation services rather than duplicating logic or independently recomputing shared metrics.

Capacity, Workload, Availability and Allocation Conflict Detection share results through service composition, not repeated calculation code.

**Consequences**

Operational metrics have a single source of truth across all analytical engines and downstream Decision Engine stages.

Regression risk is reduced because calculation changes propagate through shared service contracts.

---

# Sprint 8 Decisions

## DEC-008-001

**Title**

Decision Engine Pipeline Architecture

**Date**

2026-07-21

**Status**

Accepted

**Context**

The Delivery Strategy Engine must generate, evaluate, rank and explain execution alternatives while preserving human decision authority.

**Decision**

Implement the Decision Engine as four isolated Application services orchestrated through DeliveryStrategyController:

- DeliveryStrategyBuilderService
- DeliveryStrategyEvaluatorService
- DeliveryStrategyRankingService
- DeliveryStrategyExplanationService

Each service has a single responsibility and must not perform responsibilities assigned to downstream stages.

**Consequences**

The Decision Engine is modular, testable and aligned with the operational decision model.

Each stage can evolve independently without breaking upstream contracts.

---

## DEC-008-002

**Title**

Configuration-Based Company Decision Profiles

**Date**

2026-07-21

**Status**

Accepted

**Context**

Strategy ranking requires configurable business priorities such as profit maximization, delivery speed and AI adoption.

The MVP must avoid unnecessary database schema changes.

**Decision**

Store Company Decision Profiles in application configuration (`appsettings.json`) using the `CompanyDecisionProfiles` section.

Load profiles through `IOptions<CompanyDecisionProfilesOptions>` and expose them via `ICompanyDecisionProfileRepository`.

Six default profiles are provided: Profit Maximization, Delivery Speed, Operational Stability, AI Adoption, Human Resource Optimization and Balanced Strategy.

**Consequences**

Ranking priorities are configurable without database migrations during the MVP.

Database-backed profile management remains a post-MVP evolution path.

See ADR-007.

---

## DEC-008-003

**Title**

Dynamic Strategy Generation

**Date**

2026-07-21

**Status**

Accepted

**Context**

Execution strategies must reflect available resource types without hardcoded strategy templates.

**Decision**

Generate strategy candidates dynamically using resource mix subset generation in `DeliveryStrategyGeneration`.

Strategy names are composed from the selected resource types.

Strategy identifiers are generated deterministically from contract, mission and resource mix inputs.

**Consequences**

New resource types automatically produce new strategy combinations.

Strategy results are reproducible for the same operational inputs.

---

## DEC-008-004

**Title**

Min-Max Normalization for Strategy Ranking

**Date**

2026-07-21

**Status**

Accepted

**Context**

Evaluation metrics use different scales and directions. Ranking must compare strategies fairly within a single evaluation set.

**Decision**

Normalize each active Decision Profile dimension using min-max scaling across all evaluated strategies before applying configured weights.

Respect the `PreferHigherValues` flag per dimension when computing normalized scores.

**Consequences**

Ranking scores are deterministic and comparable within a strategy set.

Profile weight changes immediately affect ranking without code changes.

---

## DEC-008-005

**Title**

Structured Explanation Without LLM

**Date**

2026-07-21

**Status**

Accepted

**Context**

Managers must understand why a strategy received its ranking. Explainability is a core product principle.

The MVP must not depend on external LLM services for operational explanations.

**Decision**

Implement `DeliveryStrategyExplanationCalculation` as a deterministic structured explanation engine.

Explanations include decision factors, strengths, weaknesses, risks, resource composition and capacity impact using predefined reason codes and evaluation metric thresholds.

No natural language generation is performed.

**Consequences**

Explanations are auditable, deterministic and testable.

LLM-based conversational explanations remain out of MVP scope.

---

## DEC-008-006

**Title**

Engine Reuse Across Decision Stages

**Date**

2026-07-21

**Status**

Accepted

**Context**

The Evaluator, Ranking and Explanation stages require data already produced by upstream engines.

**Decision**

Each Decision Engine stage must reuse prior implementations:

- Evaluator reuses Builder and Sprint 7 analytical engines
- Ranking reuses Evaluator
- Explanation reuses Ranking

No stage may duplicate calculation logic owned by an upstream service.

**Consequences**

The Decision Engine maintains a single source of truth for strategy data and metrics.

Regression risk is reduced through shared orchestration paths.

---

## DEC-008-007

**Title**

Sprint-End Documentation Consolidation Workflow

**Date**

2026-07-21

**Status**

Accepted

**Context**

Multiple User Stories completed across a sprint require synchronized updates to project documentation, ADRs and agent execution guides.

**Decision**

Introduce a Documentation Update workflow executed after sprint completion.

A Documentation Agent consolidates implementation outcomes into Sprint Register, Decision Log, Baseline, Architecture documents and ADRs without modifying source code or changing approved decisions.

**Consequences**

Project documentation remains aligned with implemented architecture.

Backend Agents focus on implementation. Documentation consolidation is a separate governed step.

See ADR-008 and prompts/system/06_Documentation_Update_Guide.md.

---

## DEC-008-008

**Title**

AgencyOS AI Factory as a Parallel Program

**Date**

2026-07-21

**Status**

Accepted

**Context**

AI-assisted engineering workflows demonstrated significant delivery acceleration during Sprint 7 and Sprint 8 without altering the AgencyOS product scope.

A dedicated engineering platform is required to evolve agent orchestration independently from the operational decision product.

**Decision**

Establish AgencyOS AI Factory as Program B, running in parallel with Program A (AgencyOS Product).

The AI Factory is not part of the AgencyOS MVP.

Both programs share vision but maintain independent delivery dependencies, repositories and governance boundaries.

Until the AgencyOS MVP is delivered, the AI Factory shall not require changes to Product Architecture, Domain Model, Product Roadmap, Product Backlog or Product Schedule.

**Consequences**

AgencyOS maintains implementation focus on the operational decision platform.

The AI Factory evolves agent workflows, prompts and orchestration in parallel.

See ADR-006 and docs/02_Architecture/Program_Architecture.md.

---

## DEC-008-009

**Title**

Agent Runtime Profiles

**Date**

2026-07-21

**Status**

Planned

**Context**

AI Factory agents require standardized configuration for model selection, context scope, execution constraints and role-specific behavior.

Ad-hoc agent configuration per task does not scale as the AI Factory matures.

**Decision**

Introduce Agent Runtime Profiles as a planned AI Factory capability.

Each profile will define execution parameters for a specific agent role or task type, enabling consistent and governable agent behavior across sprints.

Implementation is deferred to a future AI Factory evolution phase.

**Consequences**

Agent configuration will become explicit, versioned and reusable.

No Agent Runtime Profile infrastructure is implemented during Sprint 8 or the AgencyOS MVP.

---

## DEC-008-010

**Title**

Agent Analytics and Engineering Intelligence

**Date**

2026-07-21

**Status**

Roadmap

**Context**

The AI Factory program requires visibility into agent effectiveness, sprint delivery metrics and engineering quality trends to support continuous improvement of AI-assisted development.

**Decision**

Define Agent Analytics and Engineering Intelligence as a roadmap capability for the AI Factory program.

The capability will track agent execution outcomes, story completion patterns, review findings and engineering velocity to inform process optimization.

Implementation is deferred beyond the current MVP and AI Factory initial phases.

**Consequences**

Engineering intelligence becomes a governed evolution path rather than an ad-hoc reporting effort.

No analytics infrastructure is implemented during Sprint 8.

---

# Sprint 9 Decisions

## DEC-009-001

**Title**

Deterministic Reference Data via seed.sql

**Date**

2026-07-22

**Status**

Accepted

**Context**

Sprint 9 MVP validation identified that `supabase db reset` recreates schema through migrations but leaves lookup tables empty. Mission and Task creation require reference data that was previously inserted manually.

**Decision**

Initialize all reference data through `supabase/seed.sql` executed automatically after migrations during `supabase db reset`.

All seed records use deterministic UUIDs and fixed timestamps so every reset produces identical reference data.

**Consequences**

Local development environments are fully usable immediately after reset.

Reference data changes are version-controlled alongside migrations.

Manual database inserts are no longer required for MVP operational journeys.

See `supabase/seed.sql` and Program Architecture – Seed Data layer.

---

## DEC-009-002

**Title**

Delivery Strategy Builder Diagnostic Instrumentation

**Date**

2026-07-22

**Status**

Accepted

**Context**

Sprint 9 MVP validation observed cases where Delivery Strategy Builder produced zero strategies. Without structured diagnostics inside generation, it was not possible to identify whether failure occurred at policy filtering, task assignment, or operational validation.

**Decision**

Instrument `DeliveryStrategyBuilderService.GenerateStrategiesAsync()` with structured diagnostic logging only.

Logging covers generation inputs, resource-mix validity (including rejection reasons), assignment completeness, operational validation inputs and failure reasons, successful strategy creation, and end-of-run counters.

No algorithms, domain rules, repositories, DTOs, or controllers are modified.

**Consequences**

Zero-strategy outcomes can be diagnosed from application logs without changing Decision Engine behavior.

Diagnostic reason helpers mirror existing validation checks for logging clarity and must not become alternate execution paths.

---

# Program Decisions

The following decisions represent strategic program decisions consolidated after the AgencyOS Program Audit.

Unlike Sprint Engineering Decisions, these decisions define the long-term direction of the product, architecture and engineering governance.

---

## DEC-010-001

### Title

Capacity First Principle

### Date

2026-07

### Status

Accepted

### Introduced

Phase 1

### Context

AgencyOS required a single operational concept capable of connecting commercial activities, operational planning and execution.

### Decision

Productive Capacity becomes the primary planning variable of the platform.

Commercial acceptance, operational planning and execution strategies must always consider available capacity before execution begins.

### Consequences

Capacity becomes the foundation of:

- Commercial Planning
- Operational Planning
- Planning Engines
- Decision Engine
- Future Decision Intelligence

### References

Audit-01

AgencyOS Baseline v2.0

---

## DEC-010-002

### Title

Hours as the Operational Unit

### Date

2026-07

### Status

Accepted

### Introduced

Phase 1

### Context

Projects, videos and campaigns differ significantly across service companies.

### Decision

AgencyOS adopts productive hours as the universal operational measurement unit.

Services consume hours.

Resources provide hours.

Planning balances hours.

### Consequences

The platform remains independent from specific business verticals.

### References

Audit-01

AgencyOS Baseline v2.0

---

## DEC-010-003

### Title

Goal-Oriented Planning

### Date

2026-07

### Status

Accepted

### Introduced

Sprint 10

### Context

Traditional planning begins with available resources.

AgencyOS should instead begin with business objectives.

### Decision

Planning starts from business goals.

Operational strategies are generated to satisfy objectives while respecting operational constraints.

Resources become part of the solution rather than the starting point.

### Consequences

Decision Intelligence adopts Goal-Oriented Planning as its primary architectural principle.

### References

Audit-12

AgencyOS Baseline v2.0

---

## DEC-010-004

### Title

Decision Intelligence Architecture

### Date

2026-07

### Status

Approved Architecture

### Introduced

Sprint 10

### Context

The MVP Decision Engine evaluates predefined execution alternatives.

Future versions require intelligent operational planning.

### Decision

Introduce the Decision Intelligence Layer as the strategic evolution of the Decision Engine.

Main components:

- Constraint Engine
- Strategy Generation Engine
- Optimization Engine
- Ranking Engine
- Explainability Engine
- Simulation Engine

### Consequences

The MVP architecture remains valid.

Decision Intelligence extends rather than replaces the existing architecture.

### References

Audit-12

Program Architecture v2.0

---

## DEC-010-005

### Title

Hybrid AI Architecture

### Date

2026-07

### Status

Accepted

### Introduced

Sprint 10

### Context

Artificial Intelligence should complement deterministic business processing without replacing it.

### Decision

AgencyOS adopts a Hybrid AI Architecture.

Deterministic Components

- Capacity
- Workload
- Availability
- Conflict Detection
- Constraints
- Ranking

AI Components

- Strategy Generation
- Scenario Exploration
- Recommendation Refinement
- Natural Language Interaction

### Consequences

Business consistency is preserved while enabling future AI capabilities.

### References

Audit-12

AgencyOS Baseline v2.0

---

## DEC-010-006

### Title

Explainable Decision Principle

### Date

2026-07

### Status

Accepted

### Introduced

Sprint 8

### Context

Operational recommendations affect business outcomes and must be transparent.

### Decision

Every recommendation generated by AgencyOS shall include an explanation describing:

- Selected strategy
- Evaluation criteria
- Applied constraints
- Trade-offs
- Rejected alternatives

### Consequences

Decision transparency becomes a permanent architectural requirement.

### References

Audit-10

AgencyOS Baseline v2.0

---

## DEC-010-007

### Title

Human Governance Principle

### Date

2026-07

### Status

Accepted

### Introduced

Sprint 10

### Context

Artificial Intelligence assists operational planning but organizational responsibility remains human.

### Decision

AgencyOS is a Decision Support Platform.

Artificial Intelligence proposes.

Authorized users approve.

### Consequences

Human accountability remains mandatory for operational execution.

### References

Audit-12

AgencyOS Baseline v2.0

---

## DEC-010-008

### Title

Documentation as Product

### Date

2026-07

### Status

Accepted

### Introduced

Sprint 4

### Context

Documentation drift creates architectural inconsistency and reduces AI effectiveness.

### Decision

Documentation is considered part of the product.

Every implementation shall update the corresponding documentation.

### Consequences

Documentation becomes part of the Definition of Done.

### References

Audit-06

Audit-11

AgencyOS Baseline v2.0

---

## DEC-010-009

### Title

Baseline Governance

### Date

2026-07

### Status

Accepted

### Introduced

Post Audit

### Context

Multiple historical documents existed with overlapping information.

### Decision

The AgencyOS Baseline becomes the official reference for:

- Product
- Architecture
- Engineering
- Governance

Historical documentation remains available for traceability only.

### Consequences

The Baseline becomes the single source of truth for future development.

### References

Audit-14

AgencyOS Baseline v2.0

---

## DEC-010-010

### Title

Program Audit as Historical Evidence

### Date

2026-07

### Status

Accepted

### Introduced

Post Audit

### Context

The project evolved through extensive design discussions distributed across multiple conversations.

### Decision

Program Audits preserve the historical evolution of AgencyOS.

Audits document:

- Architectural evolution
- Product evolution
- Engineering evolution
- Roadmap evolution

They support governance but do not replace the Baseline.

### Consequences

Future architectural reviews may reference the Audit to understand why decisions were made without modifying the official documentation.

### References

Audit-01 through Audit-14

AgencyOS Baseline v2.0

---

# Release 1.1 Decisions

## DEC-101-001

**Title**

Company Working Calendar as Configurable Planning Input

**Date**

2026-07-26

**Status**

Accepted

**Context**

Capacity Planning requires a company working calendar. During MVP, Availability and Capacity engines used a hardcoded Monday–Friday week. Release 1.1 introduces US-101 to make the calendar administrable.

**Decision**

Introduce a `WorkingCalendar` aggregate persisted in `working_calendar` with:

- Mandatory `CompanyId` (opaque GUID until a Company aggregate exists)
- Mandatory name and `EffectiveFrom`
- Optional `EffectiveTo`
- At least one unique weekday name in `working_days`
- Lifecycle: create as Inactive → Activate / Deactivate → Delete inactive non-historical calendars
- BR-107 overlap enforcement for active calendars of the same company
- BR-109 structural immutability after `EffectiveFrom` has started

Application layer follows the existing Service + FluentValidation pattern (no MediatR).

**Consequences**

Administrators can configure operational calendars through REST and the Release 1.1 admin UI. Planning engines may continue using the default Monday–Friday fallback until they are wired to `IWorkingCalendarService.GetActiveForCompanyAsync`.

---

## DEC-101-002

**Title**

CompanyId Without Company Aggregate

**Date**

2026-07-26

**Status**

Accepted

**Context**

BR-102 requires `CompanyId`, but AgencyOS has no Company entity or multi-tenant table yet.

**Decision**

Persist `company_id` as a required UUID without a foreign key. The frontend uses a documented default company GUID until Company management is introduced.

**Consequences**

Working Calendar is company-scoped without blocking Release 1.1. A future Company aggregate can add FK and tenant resolution without redesigning calendar APIs.

---

## DEC-102-001

**Title**

Holiday Aggregate as Official Non-Working Days Source

**Date**

2026-07-26

**Status**

Accepted

**Context**

Capacity Planning needs configurable holidays beyond weekday patterns. US-102 introduces Holiday Management without changing Capacity Engine calculation code yet.

**Decision**

Introduce a `Holiday` aggregate with types National, State, Municipal, and Company; Active/Inactive lifecycle; recurring and one-time dates; scope uniqueness (BR-211); and historical immutability (BR-213).

Working Calendar exposes holidays through:

- `WorkingCalendar.IsWorkingDay(date, activeHolidays)`
- `GET /working-calendars/operational-day`
- `GET /working-calendars/{id}/holidays`

**Consequences**

Administrators can manage holidays via REST and UI. Planning engines can adopt `GetOperationalWorkingDayAsync` later without redesigning holiday storage.

---

## DEC-102-002

**Title**

Holiday Scope Rules by Type

**Date**

2026-07-26

**Status**

Accepted

**Context**

Holiday geography and company association differ by type.

**Decision**

- Company holidays require `CompanyId`
- State holidays require `StateCode`
- Municipal holidays require `StateCode` and `City`
- National holidays clear State and City
- Optional `CompanyId` may associate National/State/Municipal holidays to a company configuration

**Consequences**

Validation and persistence enforce scope consistently across API, domain, and database CHECK constraints.

---

## DEC-103-001

**Title**

Working Hours Aggregate Linked to Working Calendar

**Date**

2026-07-26

**Status**

Accepted

**Context**

Planning needs standard daily schedules beyond weekday flags. US-103 introduces Working Hours without Capacity Engine refactoring.

**Decision**

Persist `working_hours` with FK to `working_calendar` and child `working_hours_day` rows for each weekday. Enforce one active overlapping configuration per calendar (BR-308). Enrich `GET /working-calendars/operational-day` with active schedule and planned net hours.

**Consequences**

Administrators configure schedules in admin UI/API. Capacity Engine can later consume operational-day planned hours without redesigning storage.

---

## DEC-104-001

**Title**

Resource Availability Aggregate Linked to Resource, Calendar, and Working Hours

**Date**

2026-07-26

**Status**

Accepted

**Context**

Planning needs planned individual availability beyond company calendars and standard hours. US-104 introduces Resource Availability without requiring vacations/leaves/shifts.

**Decision**

Persist `resource_availability` with FKs to execution resource, working calendar, and working hours; child weekly flags and daily overrides. Enforce one active overlapping configuration per resource (BR-407/409). Expose `GET /resource-availabilities/operational` combining calendar, holidays, hours, weekly flags, and overrides.

**Consequences**

Administrators configure individual planned availability in admin UI/API. Capacity Engine consumes operational planned hours instead of Monday–Friday defaults.

---

## DEC-105-001

**Title**

Capacity Engine uses operational configuration exclusively

**Date**

2026-07-26

**Status**

Accepted

**Context**

Capacity previously prorated `CapacityHoursPerWeek` by calendar days and related engines assumed Monday–Friday working days. US-101–US-104 now provide complete operational configuration.

**Decision**

Refactor Capacity to sum planned net hours per day from Active Resource Availability + Active Working Calendar + Holidays + Working Hours (BR-501..BR-509). If configuration is missing for any day in the planning period, return a business-rule error. Do not fall back to Monday–Friday defaults. Extend Capacity responses with operational-day breakdown and wire Availability/Allocation Conflict to those days.

**Consequences**

Capacity, Availability time slots, and calendar conflict detection are deterministic against stored configuration. Planners must maintain Active Resource Availability covering planning periods.

---

## DEC-106-001

**Title**

Immutable Capacity History auto-persisted after successful calculation

**Date**

2026-07-26

**Status**

Accepted

**Context**

US-105 delivers configuration-driven Capacity calculations on demand. US-106 requires analyzing capacity evolution over time without recalculating historical periods, while preserving reproducibility and calculation version.

**Decision**

Introduce a Create-only `CapacityHistory` aggregate persisted to `capacity_history`. After every successful `GetAll` / `GetByResourceId` Capacity Engine calculation, persist one immutable snapshot per resource including operational day inputs (JSON). Recalculation always inserts a new row (never overwrite/update/delete). `GetSummary` recalculates without writing history. Query APIs support resource, company, period, version, compare, and aggregate under `/capacity/history`.

**Consequences**

Historical capacity is available for analysis and comparison without mutating live Capacity Engine behavior. Storage grows with each successful calculation. CompanyId is resolved from the Working Calendar linked through Resource Availability.

---

## DEC-107-001

**Title**

Immutable Workload History auto-persisted after successful calculation

**Date**

2026-07-26

**Status**

Accepted

**Context**

Workload calculations are on-demand and previously left no durable trail. US-107 requires analyzing workload evolution over time without recalculating historical periods, mirroring Capacity History (US-106).

**Decision**

Introduce a Create-only `WorkloadHistory` aggregate persisted to `workload_history`. After every successful `GetAll` / `GetByResourceId` Workload Engine calculation, persist one immutable snapshot per resource including assignment distribution and operational day enrichment (JSON). Recalculation always inserts a new row. `GetSummary` does not write history. Query APIs support resource, company, period, version, compare, aggregate, and trends under `/workload/history`. CompanyId and day counts are enriched from Resource Availability / Working Calendar / Holidays when available; otherwise weekday fallback and the Release 1.1 default CompanyId are used without changing live Workload formulas.

**Consequences**

Historical workload can be queried, compared, aggregated, and trended without changing live Workload Engine calculation semantics. Storage grows with each successful calculation.

---

## DEC-108-001

**Title**

Planning Templates store configuration references only

**Date**

2026-07-26

**Status**

Accepted

**Context**

Administrators repeatedly configure the same Working Calendar, Working Hours, availability strategy, and planning window for common planning scenarios. Templates must not duplicate operational data or mutate historical Capacity/Workload.

**Decision**

Introduce a Create/Reconfigure/Activate/Deactivate/Delete `PlanningTemplate` aggregate that references Working Calendar and Working Hours IDs, stores Resource Availability strategy identity, default planning window, and capacity-rule metadata. Name is unique per company. Activate/Apply require Active referenced calendar/hours. Clone copies configuration into a new Inactive template. Apply produces an `AppliedPlanningConfiguration` response (optionally running Capacity/Workload) without updating the template. Historical Capacity/Workload remain append-only.

**Consequences**

Reusable planning setup is available without inventing a separate operational configuration store. Capacity Engine and Workload Engine remain the sources of truth for calculations and history.

---

## DEC-201-001

**Title**

Recommendation Approval Workflow as governance over ephemeral Decision Engine strategies

**Date**

2026-07-26

**Status**

Accepted

**Context**

Delivery Strategy recommendations are computed in-memory and are not persisted as operational entities. Human approval is mandatory before execution, but no durable workflow existed.

**Decision**

Introduce `RecommendationWorkflow` referencing `DeliveryStrategyId` plus contract/mission snapshot metadata (title/summary/rank/score). Lifecycle: Draft → PendingApproval → Approved|Rejected; Cancel before approval; Reopen only from Rejected; Approved immutable. Every transition is append-only in `recommendation_workflow_transition`. Workflow APIs do not generate or mutate Decision Engine strategies.

**Consequences**

Approval governance is durable and auditable without changing recommendation generation. UI/operators must create a workflow from an existing strategy id produced by build/rank.

---

## DEC-202-001

**Title**

Recommendation Persistence as canonical Decision Engine output store

**Date**

2026-07-26

**Status**

Accepted

**Context**

US-201 workflows referenced ephemeral `DeliveryStrategyId` values. Decision Intelligence (history, comparison, audit) requires durable recommendation snapshots. Ranking must not publish transient recommendations.

**Decision**

Introduce immutable `Recommendation` aggregate persisted on Decision Engine ranking (before publication). Store payload + capacity/workload snapshots, version lineage via `recommendation_number` + `version`, and archive/restore without delete. Workflow create requires `RecommendationId` and hydrates contract/mission/title/score from the persisted recommendation. `CompanyId` is supplied on rank requests (aligned with Capacity/Workload History).

**Consequences**

Recommendations become first-class domain objects. Ranking API requires `CompanyId` and returns `RecommendationId` per ranked strategy. Persistence failure aborts publication. US-201 workflows consume durable recommendations without changing approval lifecycle behavior.

---

## DEC-203-001

**Title**

Recommendation History as immutable append-only evolution store

**Date**

2026-07-26

**Status**

Accepted

**Context**

US-202 persists versioned Recommendations and US-201 records workflow transitions separately. Operators need a single read-only view of recommendation evolution (versions, snapshots, workflow) without mutating live Recommendation or Workflow aggregates.

**Decision**

Introduce create-only `RecommendationHistory` rows appended on Recommendation create/version/archive/restore and on Workflow create/transitions. Each entry snapshots recommendation payload, capacity/workload, planning template, decision engine version, workflow/approval fields, and event type (`VersionCreated`, `Archived`, `Restored`, `WorkflowTransition`). No update or delete APIs. Version and timeline queries resolve by recommendation-number lineage from any recommendation id in the series. Filters support company, mission, contract, recommendation, version, workflow status, and date (BR-1206).

**Consequences**

History is durable and queryable independently of live aggregates. Recommendation generation and approval lifecycle behavior remain unchanged aside from append-only side effects. Comparison and Decision Audit Trail remain separate stories.

---

## DEC-204-001

**Title**

Recommendation Comparison as read-only projection over History snapshots

**Date**

2026-07-26

**Status**

Accepted

**Context**

US-203 stores immutable recommendation evolution snapshots. Operators need side-by-side comparison of versions and recommendations (capacity, workload, payload, scores, rankings, strategy, workflow) without mutating live aggregates or duplicating recommendation data.

**Decision**

Implement a read-only `RecommendationComparison` domain/service that loads two `RecommendationHistory` snapshots and computes highlighted field/JSON diffs. Ids resolve as history ids first, otherwise as recommendation ids to their VersionCreated snapshot. Version comparison uses recommendation number lineage (`/recommendations/compare/version/{recommendationNumber}`). No comparison persistence table. Export is a client-side JSON download of the comparison response.

**Consequences**

Comparison never modifies Recommendations (BR-1301/BR-1302). Historical snapshots remain the source of truth (BR-1306). Decision Audit Trail remains a separate story.

---

## DEC-205-001

**Title**

Decision Tracking as post-approval lifecycle aggregate

**Date**

2026-07-26

**Status**

Accepted

**Context**

US-201..US-204 cover recommendation approval, persistence, history, and comparison. Operators need to track business decisions after approval through implementation and outcome without mutating Recommendations.

**Decision**

Introduce `Decision` aggregate linked 1:1 to an Approved Recommendation (`recommendation_id` unique). Lifecycle methods: Create, StartImplementation, Complete, Cancel, RecordOutcome. DecisionStatus (Created/InProgress/Completed/Cancelled) and ImplementationStatus (NotStarted/InProgress/Completed/Cancelled) are independent fields. Timeline is append-only in `decision_timeline`. Outcome may be recorded only after completion. Delete is prohibited. Create requires a non-archived Recommendation with an Approved workflow.

**Consequences**

Decision Tracking is operational without changing Recommendation generation or approval behavior. Decision Audit Trail remains a separate story.

---

## DEC-206-001

**Title**

Centralized Decision Audit Trail with non-blocking automatic hooks

**Date**

2026-07-26

**Status**

Accepted

**Context**

EPIC-02 requires an official governance/compliance layer covering Recommendation and Decision lifecycle actions. Domain-specific history (Recommendation History, Decision timeline) is insufficient as a platform-wide audit log. Audit failures must not interrupt business transactions (BR-1510).

**Decision**

Introduce create-only `AuditEvent` persisted in `audit_event`. Automatic `IAuditService.RecordSafeAsync` hooks fire after successful Recommendation, Workflow, Decision, Planning Template, Portfolio, Capacity History, and Workload History mutations. Failures are logged and swallowed. HTTP middleware assigns/propagates `X-Correlation-Id` and request ids (BR-1508). Query APIs are read-only under `/audit`. No manual audit creation endpoints.

**Consequences**

EPIC-02 Decision Evolution is complete (US-201–US-206). External SIEM, notifications, AI anomaly detection, compliance reporting, and digital signatures remain out of scope.

---

## DEC-301-001

**Title**

Deterministic advisory AI Recommendations without replacing Decision Engine output

**Date**

2026-07-26

**Status**

Accepted

**Context**

EPIC-03 requires AI decision support that analyzes persisted Recommendations and proposes alternatives with structured rationale. External LLM providers, autonomous approval/execution, and multi-agent orchestration are out of scope. Human approval must remain mandatory (BR-1603). AI failures must not alter Recommendation lifecycle (BR-1608).

**Decision**

Introduce immutable `AIRecommendation` aggregate persisted in `ai_recommendation`, always referencing one Recommendation (BR-1604). Generation uses a deterministic in-process advisor (`AIRecommendationGenerationService`) with versioned model/prompt identifiers (BR-1610). Confidence, assumptions, risks, and alternatives are mandatory fields. Compare is read-only against the live Recommendation. Archive is the only post-create mutation. Generate/Archive emit non-blocking Audit Events. No Update/Delete APIs. Recommendation workflow and Decision Tracking remain unchanged.

**Consequences**

US-301 delivers advisory AI support without autonomous decisions. External LLMs, fine-tuning, and automatic approval/execution remain deferred.

---

## DEC-302-001

**Title**

Deterministic LLM Explainability as informational overlay over Recommendations

**Date**

2026-07-26

**Status**

Accepted

**Context**

EPIC-03 requires natural-language explanations for Recommendations and AI-assisted Recommendations. Explainability must remain informational: it must never change Recommendation logic or Decision Engine calculations (BR-1701). External prompt management, fine-tuning, and autonomous execution are out of scope. Generation failures must not affect Recommendations (BR-1705).

**Decision**

Introduce immutable `Explainability` persisted in `recommendation_explainability`, always referencing one Recommendation (BR-1702) with optional AIRecommendationId. Generation uses a deterministic in-process explainer (`ExplainabilityGenerationService`) with versioned model/prompt identifiers (BR-1707 / BR-1708). Executive summary and confidence explanation are mandatory (BR-1710 / BR-1709). Archive is the only post-create mutation. Generate/Archive emit non-blocking Audit Events. No Update/Delete APIs.

**Consequences**

US-302 delivers transparent, auditable explanations without altering Decision Engine behavior. External LLM providers and prompt management platforms remain deferred.

---

## DEC-303-001

**Title**

Deterministic Executive Recommendation Summaries as informational briefings completing EPIC-03

**Date**

2026-07-26

**Status**

Accepted

**Context**

EPIC-03 requires a concise executive briefing that consolidates Recommendation, AI Recommendation, Explainability, and Decision information. The briefing must remain informational and must never change source aggregates (BR-1801). Interactive chat, copilots, voice, presentation generation, and external LLMs are out of scope. Generation failures must not affect Recommendations (BR-1805).

**Decision**

Introduce immutable `ExecutiveRecommendationSummary` persisted in `executive_recommendation_summary`, always referencing one Recommendation (BR-1802) with optional AIRecommendationId and ExplainabilityId. Generation uses a deterministic in-process briefing service. Every generation/regeneration creates a new SummaryVersion (BR-1804) via Generate or CreateNewVersion. Archive is the only in-place lifecycle mutation; archived summaries remain queryable (BR-1810). Compare is read-only against the live Recommendation. Generate/CreateNewVersion/Archive emit non-blocking Audit Events. No Update/Delete APIs.

**Consequences**

US-303 completes EPIC-03 AI Decision Support (US-301–US-303). External LLM providers, interactive copilots, and presentation generation remain deferred.

---

## DEC-109-001

**Title**

Portfolio Planning as aggregation over Missions and planning engines

**Date**

2026-07-26

**Status**

Accepted

**Context**

EPIC-01 needs a consolidation layer for multiple Missions without inventing a new capacity/workload engine or mutating Mission planning.

**Decision**

Introduce `Portfolio` with `portfolio_mission` associations (Active Missions = PLANNED/IN_PROGRESS). Portfolio Capacity/Workload call existing Capacity and Workload engines for the portfolio planning period; historical analysis uses Capacity/Workload History aggregates by company and period. Portfolio Health is deterministic from utilization/workload vs optional Planning Template warning threshold. Inactive portfolios cannot be modified; Active portfolios cannot be deleted. Calculations never modify Missions.

**Consequences**

EPIC-01 Advanced Planning is complete (US-101–US-109). Portfolio remains an aggregation/analysis layer; engines and history stay sources of truth.

---

# Release 1.1 Decisions

## DEC-401-001

**Title**

Company Decision Profiles migrate from configuration to a versioned, database-backed aggregate

**Date**

2026-07-26

**Status**

Accepted

**Context**

ADR-007 introduced Company Decision Profiles as a read-only, configuration-driven mechanism (`appsettings.json` + `IOptions<CompanyDecisionProfilesOptions>`) to keep the MVP simple. US-401 requires companies to create, edit, clone, and manage their own ranking profiles at runtime, without code deployment, while preserving exactly one default Active profile per company (BR-1901) and a full audit trail of changes.

**Decision**

Replace the configuration-only model with a database-backed `CompanyDecisionProfile` aggregate persisted in `company_decision_profile`, managed through `ICompanyDecisionProfileRepository` (EF Core against `ApplicationDbContext`) and `ICompanyDecisionProfileService`, and exposed via `DecisionProfilesController` (`/decision-profiles`). Each profile is versioned by an immutable `ProfileFamilyId`: editing creates a new immutable version and deactivates the previous one (BR-1905) rather than mutating history. Only one profile per company may be the default Active profile (BR-1901), enforced by the service and a partial unique database index. Only Active profiles may be used for ranking (BR-1904); Delivery Strategy Ranking and every downstream AI-assisted artifact (`AIRecommendation`, `Explainability`, `ExecutiveRecommendationSummary`, `RecommendationHistory`) now records the `CompanyDecisionProfileId` and `Version` used at generation time for traceability. The original six default profiles are seeded for the default company with the same Ids used previously in configuration. All create/update/clone/activate/deactivate/archive/set-default operations emit non-blocking Audit Events via `IAuditService.RecordSafeAsync`.

**Consequences**

US-401 delivers self-service, auditable Company Decision Profile management without redeployment. This supersedes ADR-007's "Database-Backed Profiles" alternative (previously deferred) and its MVP configuration-binding approach; the ranking algorithm contract (`DeliveryStrategyRankingCalculation`) is unchanged and remains independent of business priorities.

---

## DEC-402-001

**Title**

Company becomes a first-class, database-backed multi-company tenant aggregate; AI-generated artifacts carry CompanyId, existing company-scoped modules remain unchanged

**Date**

2026-07-26

**Status**

Accepted

**Context**

Prior to US-402, "Company" existed only implicitly as a `companyId` scalar referenced by company-scoped aggregates (Mission, Client, Working Calendar, Company Decision Profile, etc.), with a single seeded default company id (`AgencyOSCompanies.DefaultCompanyId`) hardcoded across services and the frontend (`DEFAULT_COMPANY_ID`). There was no way to create, configure, activate/deactivate, or archive additional companies, nor a mechanism for a request to declare which company it is operating against. US-402 requires a first-class `Company` configuration aggregate, a per-request active-company mechanism, and traceability of which company an AI-generated artifact belongs to.

**Decision**

Introduce a `Company` aggregate persisted in `company` (BR-2001 unique CompanyName, BR-2002 unique CompanyCode, Active/Inactive/Archived lifecycle, optional `DecisionProfileId`/`DefaultPlanningTemplateId`/`DefaultCalendarId`/`PlanningConfiguration`), managed through `ICompanyRepository` (EF Core) and `ICompanyService`, and exposed via `CompaniesController` (`/companies`). Archived Companies are excluded from listings by default and remain queryable via `IncludeArchived` (BR-2009). `Company.Update` validates that an assigned `DecisionProfileId` belongs to the company and is its Active default Company Decision Profile (BR-2007), and that an assigned `DefaultPlanningTemplateId` belongs to the company (BR-2008); `CompanyDecisionProfileService.SetDefaultAsync` keeps `Company.DecisionProfileId` in sync whenever a new default profile is designated, closing the BR-2007 loop bidirectionally.

A scoped, mutable `ICompanyContext` (populated per request by `ICompanyContextService`) tracks the active company for the current request. `CompanyContextMiddleware` reads the `X-Company-Id` header, calls `ICompanyContextService.SelectAsync` (which validates the company exists and is Active per BR-2003), and rejects the request with 400 Bad Request when the header is present but invalid, unknown, or not selectable; when the header is absent, the context is left empty and existing endpoints that already accept an explicit `companyId` (query parameter or route segment) are unaffected, preserving backward compatibility. `GET /companies/active` returns the selected company or falls back to the seeded default company when none is selected.

`company_id` (nullable, with backfill to the default company) was added to `ai_recommendation`, `recommendation_explainability`, `executive_recommendation_summary`, and `recommendation_workflow`, and the corresponding domain entities now carry an optional `CompanyId` set from the originating `Recommendation.CompanyId` at generation/creation time (BR-2004). Mission, Client, and other CRM/commercial aggregates are intentionally **not** modified in this story: they remain company-scoped indirectly through their existing relationships (e.g., via Portfolio, Working Calendar, Company Decision Profile), and adding a first-class `CompanyId` column to them is deferred to a future story to avoid a broad, high-risk schema/behavior change unrelated to US-402's scope. The frontend persists the active company id in `localStorage` (`agencyos.activeCompanyId`, defaulting to `DEFAULT_COMPANY_ID`) and the API client (`apiClient`) attaches it as `X-Company-Id` on every request; a company selector in the app header lets users switch the active company, which also best-effort calls `POST /companies/select`.

**Consequences**

US-402 delivers self-service multi-company configuration with company selection, lifecycle management, and BR-2007/BR-2008 cross-aggregate consistency checks, while every existing company-scoped module and its automated tests continue to work unchanged because the `X-Company-Id` header is optional and additive. AI-generated artifacts (AI Recommendations, Explainability, Executive Recommendation Summaries, Recommendation Workflow) are now traceable to the company that produced them. Extending first-class `CompanyId` to Mission/CRM aggregates remains explicit future work, tracked separately from this story.

---

## DEC-403-001

**Title**

Enterprise Dashboard is a read-only aggregation layer over existing repositories, with no duplicated analytical storage and no recalculation of historical Capacity/Workload data; EPIC-04 is renamed Enterprise Capabilities and completes with US-403

**Date**

2026-07-26

**Status**

Accepted

**Context**

US-401 (Company Decision Profiles) and US-402 (Multi-Company Configuration) delivered self-service ranking configuration and first-class multi-company tenancy, but there was no single place for a planner or executive to see a cross-domain rollup of Planning, Portfolio, Capacity, Workload, Recommendation, Decision, AI Decision Support, and Audit activity for a Company and reporting period. Building such a view naively risks two anti-patterns the program explicitly wants to avoid: (1) introducing a duplicated analytical/reporting store that could drift from the operational data it summarizes, and (2) recalculating Capacity/Workload figures on the fly, which would bypass the immutable `CapacityHistory`/`WorkloadHistory` aggregates and their calculation versioning guarantees (BR-601..BR-609, BR-701..BR-709). US-403 requires a dashboard that is strictly read-only and additive, sourcing every figure from data that already exists.

**Decision**

Introduce an Enterprise Dashboard vertical slice composed entirely of read-only aggregation over existing repositories — `IRecommendationRepository`, `IDecisionRepository`, `IPortfolioRepository`, `ICapacityHistoryRepository`, `IWorkloadHistoryRepository`, `IPlanningTemplateRepository`, `IAIRecommendationRepository`, `IExplainabilityRepository`, `IExecutiveRecommendationSummaryRepository`, `IAuditEventRepository`, `ICompanyRepository`, and `ICompanyDecisionProfileRepository` — with no new database tables and no write operations (BR-2101, BR-2102). `IDashboardAggregationService`/`DashboardAggregationService` builds each section (Summary, Planning, Portfolio, Capacity, Workload, Recommendations, Decisions, AI, Audit) directly from repository queries for a given Company and period, computing counts, status breakdowns, totals, and averages without ever re-deriving Capacity/Workload from operational-day inputs — Capacity/Workload figures always come from the already-persisted, immutable `CapacityHistory`/`WorkloadHistory` records. `IDashboardHealthCalculationService`/`DashboardHealthCalculationService` delegates every health determination to the existing `PortfolioHealth.Calculate`/`Canonicalize` static logic (BR-2107), so the Dashboard introduces no independent health thresholds. `IDashboardTrendService`/`DashboardTrendService` computes a period-over-period trend (Up/Down/Flat + deltaPercent) by comparing the current window against an immediately preceding window of equal length (BR-2106). `IEnterpriseDashboardService`/`EnterpriseDashboardService` resolves the effective CompanyId as `parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId` (BR-2104), validates the Company exists, defaults an unset reporting window to the trailing 30 days (BR-2105), and fetches all nine sections concurrently via `Task.WhenAll` for acceptable latency under BR-2102's "performant parallel queries where reasonable" requirement. `EnterpriseDashboardController` exposes the full dashboard and each individual section under `/enterprise-dashboard`, validated by `EnterpriseDashboardQueryParametersValidator` (From ≤ To, PeriodStart ≤ PeriodEnd when both set). The frontend `EnterpriseDashboardPage` becomes the application's default landing route (`/` redirects to `/enterprise-dashboard`) with "Dashboard" as the first navigation item, presenting KPI cards, health chips, trend indicators, utilization/workload progress bars, status breakdowns, and drill-down links back into the underlying list pages (`/portfolios`, `/planning-templates`, `/capacity/history`, `/workload/history`, `/recommendations`, `/decisions`, `/ai-recommendations`, `/audit`) so every summarized figure remains one click from its source of truth.

As part of this story, EPIC-04 is renamed from "Multi-Company Configuration" to **Enterprise Capabilities**, retroactively encompassing US-401 (Company Decision Profiles), US-402 (Multi-Company Configuration), and now US-403 (Enterprise Dashboard); the label change reflects that this epic's scope is company- and organization-level enterprise capabilities rather than multi-company configuration alone.

**Consequences**

Executives and planners get a single, trustworthy, read-only rollup of program state without any risk of the dashboard's figures diverging from the operational systems of record, because every number is a live projection rather than a stored copy. Because Capacity/Workload sections read exclusively from `CapacityHistory`/`WorkloadHistory`, the Dashboard has zero coupling to and zero risk of invalidating the calculation-version guarantees those aggregates already provide. The parallelized, per-section aggregation design also means additional dashboard sections (e.g., a future Commercial Domain rollup) can be added as new `Build*Async`/`Get*Async` pairs without altering the orchestration contract. EPIC-04's renaming to Enterprise Capabilities is a documentation-only change with no code or schema impact; all EPIC-04 program-status references (Sprint Register, README) are updated to reflect US-401–US-403 completeness.

---

## DEC-404-001

**Title**

Portfolio Analytics is a read-only per-Portfolio aggregation layer over Portfolio snapshot fields and existing Recommendation/Decision/Capacity/Workload history, with no duplicated analytical storage and no recalculation of the Capacity/Workload engines; EPIC-04 Enterprise Capabilities completes with US-404

**Date**

2026-07-26

**Status**

Accepted

**Context**

US-403 (Enterprise Dashboard) delivered a Company-wide, cross-domain rollup, but planners and executives still had no dedicated view for comparing, trending, ranking, and risk-assessing individual Portfolios — the aggregate-level Dashboard cards do not surface per-Portfolio trends over time, side-by-side comparisons between two Portfolios, or an explicit ranking/risk rollup. Building this naively risks the same two anti-patterns US-403 avoided: (1) a duplicated analytical store that could drift from `Portfolio`, `Recommendation`, `Decision`, `CapacityHistory`, and `WorkloadHistory`, and (2) recalculating Capacity/Workload on the fly instead of relying on the already-persisted, immutable history and the Portfolio's own stored `CapacitySummary`/`WorkloadSummary`/`PortfolioHealth`/`HealthDetails` snapshot fields (BR-908..BR-912). US-404 requires Portfolio Analytics to be strictly read-only, additive, and sourced entirely from data that already exists.

**Decision**

Introduce a Portfolio Analytics vertical slice (BR-2201..BR-2210) composed of `IPortfolioAnalyticsService`/`PortfolioAnalyticsService` (orchestration), `IPortfolioTrendAnalysisService`/`PortfolioTrendAnalysisService` (month-bucketed Capacity/Workload/Health/Recommendation/Decision trends, optionally scoped to one Portfolio's Missions), `IPortfolioComparisonService`/`PortfolioComparisonService` (side-by-side two-Portfolio comparison with field-level diffs), `IPortfolioHealthAnalyticsService`/`PortfolioHealthAnalyticsService` (health distribution and risk rollup), and `IPortfolioRiskAnalyticsService`/`PortfolioRiskAnalyticsService` (low Recommendation score / high Decision cancellation ratio flags) — with no new database tables and no write operations. A shared, static `PortfolioAnalyticsSnapshotBuilder` centralizes the read-only projection logic: it parses `Portfolio.CapacitySummary`/`WorkloadSummary` JSON tolerantly (never re-deriving utilization/workload, BR-2204), reads `Portfolio.PortfolioHealth` as-is (never recalculating health, BR-2203), and filters a company-wide Recommendation/Decision set down to a Portfolio's own Missions — mirroring how `DashboardAggregationService` reads `CapacityHistory`/`WorkloadHistory` without re-deriving them. `PortfolioHealthAnalyticsService`/`PortfolioComparisonService` delegate every health determination to the existing `IDashboardHealthCalculationService` (BR-2203), so no independent health thresholds are introduced, and DTOs reuse `HealthIndicator`/`TrendIndicator`/`StatusCountItem` from `EnterpriseDashboardDtos.cs` rather than duplicating them. `PortfolioAnalyticsService` resolves the effective CompanyId identically to US-403 (`parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId`, BR-2207), defaults an unset window to the trailing 180 days, and — for Company-wide endpoints (overview, ranking, health, performance) — fetches Recommendations/Decisions once for the whole Company and filters them in memory per Portfolio to avoid N+1 queries (BR-2208), while Portfolio-scoped endpoints (detail, compare) use per-Mission repository queries since the Mission set is small and bounded. Portfolio ranking combines stored health severity, snapshot utilization (penalizing both under- and over-utilization), and Decision completion effectiveness into a single weighted score (BR-2210). `PortfolioAnalyticsController` exposes `/portfolio-analytics` (overview), `/portfolio-analytics/trends`, `/portfolio-analytics/compare`, `/portfolio-analytics/ranking`, `/portfolio-analytics/health`, `/portfolio-analytics/performance`, and `/portfolio-analytics/{portfolioId}`, with the named routes registered ahead of the `{portfolioId:guid}` route to prevent route shadowing, validated by `PortfolioAnalyticsQueryParametersValidator`/`PortfolioCompareQueryParametersValidator` (From ≤ To, PeriodStart ≤ PeriodEnd, Compare requires two distinct Portfolio ids). The frontend `PortfolioAnalyticsPage` sits alongside the existing Portfolios list ("Portfolio Analytics" navigation entry) with overview/ranking tables, health distribution and risk indicators, a month-bucketed trend table, and a two-Portfolio comparison panel, all linking back into `/portfolios/:id`.

EPIC-04 Enterprise Capabilities — previously completed at US-401–US-403 — now completes with US-404 added; no further renaming is required since "Enterprise Capabilities" already covers per-Portfolio analytics.

**Consequences**

Executives and planners get per-Portfolio trends, comparisons, ranking, and risk/health rollups without any risk of divergence from the Portfolio Planning, Recommendation, Decision, and Capacity/Workload systems of record, because every figure is either a live projection or the Portfolio's own already-persisted snapshot. Because health is always read from `Portfolio.PortfolioHealth` (never recalculated) and Capacity/Workload trends read exclusively from `CapacityHistory`/`WorkloadHistory`, Portfolio Analytics has zero coupling to and zero risk of invalidating the calculation-version guarantees those aggregates already provide. Sharing `PortfolioAnalyticsSnapshotBuilder` between the Overview, Ranking, Health, Performance, Detail, and Compare code paths means the same read-only projection rules apply everywhere, so a single bug fix or business-rule change (e.g., a new risk factor) needs to happen in only one place. All EPIC-04 program-status references (Sprint Register, Change Log, README) are updated to reflect US-401–US-404 completeness.

---

## DEC-405-001

**Title**

Cross-Portfolio Planning has no persistent aggregate: a singleton in-memory `ICrossPortfolioScenarioStore` holds TEMPORARY simulated scenarios for the process lifetime, the Audit trail is the durable record of every simulation, and every response is advisory-only, requiring human approval; EPIC-04 Enterprise Capabilities completes with US-405

**Date**

2026-07-26

**Status**

Accepted

**Context**

US-404 (Portfolio Analytics) delivered read-only, per-Portfolio analytics, but planners still had no way to explore "what if we planned these Portfolios together" — comparing enterprise-wide Capacity/Workload balance across a selected set of Portfolios, detecting Mission and Resource conflicts between them, and getting advisory rebalancing suggestions, without touching any Portfolio's own data. US-405 explicitly rules out automatic reallocation, AI/financial optimization, hiring recommendations, multi-company planning, and automatic execution (BR-2301..BR-2310) — any persistence design had to make it structurally impossible to "accidentally" turn an advisory simulation into a real, executed change, and had to avoid introducing a new mutable aggregate that could drift from the Portfolio, Capacity, and Workload systems of record.

**Decision**

Introduce a Cross-Portfolio Planning vertical slice (BR-2301..BR-2310) composed of `ICrossPortfolioPlanningService`/`CrossPortfolioPlanningService` (orchestration: overview, scenarios, conflicts, balance, simulate, compare), `IEnterpriseCapacityService`/`EnterpriseCapacityService` and `IEnterpriseWorkloadService`/`EnterpriseWorkloadService` (aggregate the selected Portfolios' stored `CapacitySummary`/`WorkloadSummary` snapshots, optionally complemented by a live Capacity/Workload Engine aggregate for the planning period), `ICrossPortfolioConflictDetectionService`/`CrossPortfolioConflictDetectionService` (Portfolio Mission overlap plus delegated `IAllocationConflictDetectionService` resource conflicts, BR-2305), `ICrossPortfolioBalancingService`/`CrossPortfolioBalancingService` (advisory-only rebalancing suggestions, Mission priorities read and never reordered, BR-2306), and `ICrossPortfolioScenarioComparisonService`/`CrossPortfolioScenarioComparisonService` (field-level diff of two scenarios) — none of which ever call `UpdateAsync`/`AddAsync`/`DeleteAsync` on a `Portfolio`.

Critically, Cross-Portfolio Plans introduce **no persistent aggregate and no new database tables**. A singleton, thread-safe `ICrossPortfolioScenarioStore`/`CrossPortfolioScenarioStore`, backed by `ConcurrentDictionary<Guid, CrossPortfolioScenarioRecord>` with optional TTL and max-size eviction, holds simulated scenarios for side-by-side comparison purely within the process lifetime — scenarios are explicitly TEMPORARY and may be evicted at any time; nothing about a scenario's existence or absence is ever load-bearing for a business decision. The durable, queryable record of a simulation is instead the Audit trail: every `SimulateAsync` call invokes `IAuditService.RecordSafeAsync` with `AuditEntityTypes.CrossPortfolioPlan` and `AuditEventTypes.Simulated` (BR-2309), capturing the scenario id, selected Portfolio ids, and conflict/participation counts as immutable audit state. Every response across all six endpoints — overview, scenarios, conflicts, balance, simulate, and compare — carries `RequiresHumanApproval = true` and a shared `CrossPortfolioPlanningConstants.AdvisoryDisclaimer` string (BR-2308), making it structurally explicit at the API contract level that Cross-Portfolio Planning never executes a change automatically.

EPIC-04 Enterprise Capabilities — previously completed at US-401–US-404 — now completes with US-405 added; no further renaming is required since "Enterprise Capabilities" already covers advisory cross-portfolio simulation.

**Consequences**

Planners get an enterprise-wide, advisory simulation across multiple Portfolios — Capacity/Workload balance, conflict detection, and rebalancing suggestions — while Portfolios, Missions, and historical Capacity/Workload/Allocation data remain fully immutable and untouched, because the only services that read Portfolio data are Application-layer read paths and the only services that write anything write exclusively to the in-memory scenario store or the Audit log. Because the scenario store is a singleton `ConcurrentDictionary` scoped to process memory, restarting the API clears all scenarios with zero data-loss risk to any durable system of record — the Audit trail alone is authoritative for "what was simulated and when" (BR-2309). Reusing `ICapacityCalculatorService`, `IWorkloadCalculatorService`, and `IAllocationConflictDetectionService` rather than re-implementing balance/conflict logic means Cross-Portfolio Planning has zero risk of diverging from the existing Capacity, Workload, and Allocation Conflict engines' calculation-version guarantees. All EPIC-04 program-status references (Sprint Register, Change Log, README, Program Architecture) are updated to reflect US-401–US-405 completeness.

---

## DEC-501-001

**Title**

My Work Dashboard resolves caller identity via an explicit UserId/CompanyId/ExecutionResourceId fallback chain (no full authentication provider yet), is strictly read-only over existing Assignment/Task/Mission/Recommendation/Decision/Capacity/Workload/Audit data, and starts a new EPIC-05 Operational Workspace

**Date**

2026-07-27

**Status**

Accepted

**Context**

EPIC-04 Enterprise Capabilities (US-401–US-405) delivered Company-wide and Portfolio-wide read-only rollups for executives and planners, but there was still no personalized, task-level view for an individual contributor to see their own assigned Missions/Tasks, pending Recommendations/Decisions awaiting their attention, their Capacity/Workload standing, recent activity, and personal KPIs in one place. Building this naively raises two identity problems specific to this story, on top of the two anti-patterns already ruled out by US-403/404/405 (duplicated analytical storage; recalculating Capacity/Workload instead of reading the existing engines): (1) AgencyOS has no full authentication/identity provider yet, so "the current user" cannot be resolved from a session or JWT claim, and (2) Assignments/Tasks/Missions are scoped by `ExecutionResourceId`, not by a user identity string, so a caller-supplied `UserId` must be bridged to an `ExecutionResourceId` before any operational section can be scoped at all. US-501 (BR-2401..BR-2410) requires the dashboard to be strictly read-only, user-specific, company-isolated, and built entirely from existing modules, with no new business calculations and no new persistence.

**Decision**

Resolve the caller's effective identity once per request, in `IMyWorkDashboardService`/`MyWorkDashboardService`, using an explicit and fully documented fallback chain rather than any implicit session state: `UserId = parameters.UserId ?? IAuditContext.UserId ?? "system"`; `CompanyId = parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId` (validated to reference an existing Company); `ExecutionResourceId = parameters.ExecutionResourceId ?? (first Active ExecutionResource whose Code equals UserId, case-insensitive, scoped to CompanyId)` — this heuristic lookup is intentionally skipped when `UserId` is the literal `"system"`, since no Execution Resource should ever be matched to the anonymous fallback identity. The resolved `ExecutionResourceId` (nullable) scopes Assignments (via `IAssignmentRepository`, excluding `Completed`/`Cancelled` per BR-2406), Tasks resolved from those Assignments' TaskIds, Missions resolved from those Tasks' MissionIds, and the existing `ICapacityCalculatorService`/`IWorkloadCalculatorService` for the requested period; when it cannot be resolved, `IPersonalDashboardAggregationService`/`PersonalDashboardAggregationService` returns empty collections for Missions/Tasks and `HasData = false` Capacity/Workload summaries instead of failing the request (BR-2401), and the same graceful-empty behavior applies when the Capacity/Workload engines throw `NotFoundException`/`BusinessRuleException` for a misconfigured or inactive resource.

Pending Recommendations union company-scoped `RecommendationWorkflow` records with `Status = PendingApproval` (the approval queue has no assignee field, so it is inherently a company-wide attention queue) with any workflow whose `Recommendation.GeneratedBy` equals the resolved `UserId`, de-duplicated by workflow id. Pending Decisions select company-scoped `Decision` records with `DecisionStatus` `Created` or `InProgress`; when `UserId != "system"`, Decisions whose `CreatedBy` matches `UserId` are preferred/highlighted, but company-isolated pending Decisions are still shown so the queue is never empty solely due to attribution gaps — Company isolation is never relaxed. Since `DecisionQueryParameters` only accepts a single status value, both statuses are fetched company-wide and filtered in-memory. The Activity Timeline (`IPersonalTimelineService`/`PersonalTimelineService`) calls the existing `IAuditQueryService.GetByUserIdAsync(userId)` — the only Audit query keyed by user — and filters the result in-memory by `CompanyId` and the optional `From`/`To` window, since `GetByUserIdAsync` does not natively accept those filters; no new Audit query method is introduced. `IPersonalKpiService`/`PersonalKpiService` is pure arithmetic over the already-aggregated counts (assigned Task/Mission counts, pending Recommendation/Decision counts, overdue Task count where `PlannedEnd` is in the past and the Task is still active, upcoming-deadline count within a 14-day default window, and nullable Utilization/Workload percentages sourced from the Capacity/Workload summaries) — it never touches a repository.

`MyWorkDashboardController` exposes `GET /my-work` (full `MyWorkDashboardResponse`) plus granular `GET /my-work/summary`, `/tasks`, `/missions`, `/recommendations`, `/decisions`, `/activity`, and `/kpis` endpoints, validated by `MyWorkDashboardQueryParametersValidator` (From ≤ To, PeriodStart ≤ PeriodEnd). Every card and summary DTO carries a `DrillDownPath` into an existing, already-shipped frontend route — Mission/Task cards route to `/recommendations?missionId={id}` (no dedicated Mission/Task detail pages exist yet), Recommendation cards to `/recommendations/workflow/{id}`, Decision cards to `/decisions/{id}`, Capacity/Workload to `/capacity`/`/workload/history`, and Activity items to the existing `/audit` routes — so drill-down can never modify data, satisfying BR-2401..BR-2410 without any new write-capable endpoint. The frontend `MyWorkDashboardPage` ("My Work" navigation entry) adds explicit User Id / Execution Resource Id override fields (the User Id is persisted to `localStorage` and also sent as an `X-User-Id` request header on every API call, which the existing `HttpAuditContext` already reads as `IAuditContext.UserId` — no new identity infrastructure was added), From/To/PeriodStart/PeriodEnd period filters, KPI tiles, Capacity/Workload widgets, overdue/upcoming-deadline lists, Mission/Task/Recommendation/Decision card sections, an Activity Timeline, and a persistent "Operational workspace — read-only. Drill-down does not modify data." banner.

This story opens a new **EPIC-05 — Operational Workspace**, distinct from EPIC-04 Enterprise Capabilities: EPIC-04 is Company-wide/Portfolio-wide and executive-facing, while EPIC-05 is single-user and task-level. US-501 starts EPIC-05; the epic is explicitly **not** complete after this single story.

**Consequences**

Individual contributors get a single, trustworthy, read-only view of their own operational work, pending approvals/decisions, standing capacity/workload, and recent activity, without AgencyOS needing a full authentication/identity provider — the DEC-501-001 fallback chain is explicit, testable, and safe by construction (the `"system"` default never matches a real Execution Resource, and Company isolation is never bypassed even when a `UserId` cannot be attributed to any record). Because every section is a direct projection over `IAssignmentRepository`/`ITaskRepository`/`IMissionRepository`/`IRecommendationWorkflowService`/`IRecommendationService`/`IDecisionService`/`ICapacityCalculatorService`/`IWorkloadCalculatorService`/`IAuditQueryService` — the same services and calculation engines already used elsewhere — the My Work Dashboard has zero risk of diverging from or invalidating any existing business rule or calculation-version guarantee, and introduces zero new database tables. Routing Mission/Task drill-down through the existing filterable `/recommendations` list (rather than inventing placeholder detail pages) keeps the story's scope strictly additive; dedicated Mission/Task detail pages remain a natural candidate for a future EPIC-05 story. All EPIC-05 program-status references (Sprint Register, Change Log, README, Program Architecture) reflect that US-501 starts, but does not complete, EPIC-05.

---

## DEC-502-001

**Title**

Planning Workspace is a read-only orchestration façade over existing Planning Template, Capacity/Workload History, Portfolio, and Cross-Portfolio Planning services — calculation launches are navigation deep-links only; no duplicate planning data and no engine changes; EPIC-05 continues with US-502

**Date**

2026-07-27

**Status**

Accepted

**Context**

US-501 delivered a personalized operational dashboard, but planners still lacked a single surface that unifies Planning Templates, Capacity/Workload History, Portfolio Planning, and advisory Cross-Portfolio scenarios without inventing a second planning stack. Naively wrapping engines with new write APIs would violate BR-2501..BR-2510 (orchestration only, engines unchanged, history immutable, no duplicate planning data).

**Decision**

Introduce `IPlanningWorkspaceService`/`PlanningWorkspaceService` as a read-only orchestration façade (DEC-502-001) that resolves Company identically to US-403/501, aggregates existing Template/Portfolio/Capacity History/Workload History/Cross-Portfolio scenario data via `IPlanningOverviewService`, `IPlanningHistoryService`, and existing Application services, and exposes Planning Actions exclusively as deep-links to already-audited frontend routes (`/capacity`, `/planning-templates/{id}/apply`, `/cross-portfolio-planning`, etc.). Cross-Portfolio scenarios remain temporary and advisory (`RequiresHumanApproval = true`). No new tables. `PlanningWorkspaceController` exposes `/planning-workspace` and section GETs. Frontend `PlanningWorkspacePage` presents the unified workspace.

**Consequences**

Planners navigate the full planning lifecycle from one place without risk of divergent calculations or duplicated history. EPIC-05 remains incomplete after US-502 (further workspace stories may follow).

---

## DEC-503-001

**Title**

Recommendation Workspace is a read-only orchestration façade over existing Recommendation, Recommendation Workflow, AI Recommendation, Explainability, Executive Summary, Recommendation History, and Recommendation Comparison services — governed actions are navigation deep-links only; no duplicate recommendation data and no bypass of human approval; EPIC-05 continues with US-503

**Date**

2026-07-27

**Status**

Accepted

**Context**

US-502 delivered a Planning Workspace, but planners and approvers still lacked a single surface that unifies Recommendations, Recommendation Workflow approval, AI Recommendations, Explainability, Executive Summaries, Recommendation History, and Recommendation Comparison without inventing a second recommendation-management stack. Naively wrapping these services with new write APIs would violate BR-2601..BR-2610 (orchestration only; Decision Engine unchanged; lifecycle unchanged; human approval mandatory; history immutable; AI advisory; Explainability informational; drill-down; company isolation; workspace actions auditable).

**Decision**

Introduce `IRecommendationWorkspaceService`/`RecommendationWorkspaceService` as a read-only orchestration façade (DEC-503-001) that resolves Company identically to US-403/501/502, and aggregates existing Recommendation, Recommendation Workflow, AI Recommendation, Explainability, Executive Summary, Recommendation History, and Recommendation Comparison data via `IRecommendationOverviewService`, `IRecommendationNavigationService`, `IRecommendationSummaryService`, and existing Application services. Recommendation Workspace is a read-only orchestration façade. Generate / Approve / Reject / Archive / Restore / Start Workflow / Generate AI / Generate Explainability / Generate Executive Summary are exposed as navigation actions (deep-links) to existing frontend routes — the workspace never calls Create/Archive/Approve/etc. Company resolution: `CompanyId = parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId`. No new tables. `RecommendationWorkspaceController` exposes `/recommendation-workspace` and section GETs. Frontend `RecommendationWorkspacePage` presents the unified workspace.

**Consequences**

Planners and approvers navigate the full recommendation governance lifecycle — from generation through approval, AI advisory, explainability, executive summary, history, and comparison — from one place without risk of divergent Decision Engine behavior or duplicated recommendation data. EPIC-05 remains incomplete after US-503 (further workspace stories may follow).

---

## DEC-504-001

**Title**

Decision Workspace is a read-only orchestration façade over the existing Decision lifecycle, Recommendation linkage, Decision Timeline, and Decision Audit — Create Decision/Start Implementation/Complete/Cancel/Record Outcome are navigation deep-links only; no duplicate decision data and no bypass of human approval; EPIC-05 continues with US-504

**Date**

2026-07-27

**Status**

Accepted

**Context**

US-503 delivered a Recommendation Workspace, but stakeholders still lacked a single surface tracking a Decision's lifecycle from an approved Recommendation through implementation, outcomes, and audit without inventing a second decision-tracking stack. Naively wrapping `IDecisionService` with new write APIs, or calling `CreateAsync`/`StartImplementationAsync`/`CompleteAsync`/`CancelAsync`/`RecordOutcomeAsync` from the Workspace, would violate BR-2701..BR-2710 (orchestration only; Decision lifecycle unchanged; Recommendation linkage unchanged; Timeline immutable; Decision Audit immutable; outcome recording follows existing rules; drill-down; company isolation; actions remain audited via existing endpoints; no duplicate decision data).

**Decision**

Introduce `IDecisionWorkspaceService`/`DecisionWorkspaceService` as a read-only orchestration façade (DEC-504-001) that resolves Company identically to US-403/501/502/503, and aggregates existing Decision, Decision Timeline, and Decision Audit data via `IDecisionOverviewService`, `IDecisionNavigationService`, `IDecisionSummaryService`, and the existing `IDecisionService`/`IAuditQueryService`. Decision Workspace is a read-only orchestration façade. Create Decision / Start Implementation / Complete / Cancel / Record Outcome are exposed as navigation actions (deep-links) to existing frontend routes (`/decisions/new`, `/decisions/{id}`) — the workspace never calls `CreateAsync`/`StartImplementationAsync`/`CompleteAsync`/`CancelAsync`/`RecordOutcomeAsync`. Company resolution: `CompanyId = parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId`. No new tables. `DecisionWorkspaceController` exposes `/decision-workspace` and section GETs. Frontend `DecisionWorkspacePage` presents the unified workspace.

**Consequences**

Stakeholders navigate the full Decision lifecycle — from an approved Recommendation through implementation, completion, outcome recording, and audit — from one place without risk of divergent Decision lifecycle behavior or duplicated decision data. EPIC-05 remains incomplete after US-504 (further workspace stories may follow).

---

## DEC-505-001

**Title**

Executive Workspace is a read-only orchestration façade that primarily reuses `IEnterpriseDashboardService` section methods and adds executive navigation deep-links into every existing operational workspace and dashboard; no duplicate analytical data; this story completes EPIC-05

**Date**

2026-07-27

**Status**

Accepted

**Context**

US-501..US-504 delivered personal, planning, recommendation, and decision workspaces, but executives still lacked a single, read-only surface consolidating strategic KPIs, enterprise health, portfolios, recommendations, decisions, AI insights, and organizational performance. Naively re-aggregating from every repository (Portfolio, Recommendation, Decision, Capacity History, Workload History, AI Recommendation, Explainability, Executive Summary, Audit) would violate BR-2801..BR-2810 (orchestration only; KPIs from existing data; no operational data modified; company isolation; health reuses existing calculations; historical data remains immutable; drill-down; performance standards; no duplicate analytical data; executive actions audited via existing modules) and would duplicate the Enterprise Dashboard's own aggregation (US-403).

**Decision**

Introduce `IExecutiveWorkspaceService`/`ExecutiveWorkspaceService` as a read-only orchestration façade (DEC-505-001) that resolves Company identically to US-403/501/502/503/504 (`CompanyId = parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId`), maps the query window onto the existing `EnterpriseDashboardQueryParameters` (CompanyId, From, To, PeriodStart, PeriodEnd; default trailing 30 days), and delegates every section to `IExecutiveAggregationService`/`ExecutiveAggregationService`, which wraps each existing `IEnterpriseDashboardService` section getter (`GetSummaryAsync`, `GetPortfolioAsync`, `GetRecommendationsAsync`, `GetDecisionsAsync`, `GetCapacityAsync`, `GetWorkloadAsync`, `GetAiAsync`, `GetAuditAsync`) into an Executive*SectionResponse that embeds the existing dashboard data plus a `DrillDownPath` — never re-aggregating from a repository directly. `IExecutiveOverviewService`/`ExecutiveOverviewService` combines the Summary, Portfolio, Capacity, Workload, and Audit sections into Executive KPIs (via the pure `IExecutiveKpiService`/`ExecutiveKpiService`, which performs no repository or service calls) and an overall health rollup reused from the existing `IDashboardHealthCalculationService.CalculateOverall` (BR-2805 — no new health logic). `IExecutiveNavigationService`/`ExecutiveNavigationService` is a pure function returning 13 deep-links across the Overview/Planning/Recommendations/Decisions/Capacity/Workload/AI/Audit/Portfolios categories into the Enterprise Dashboard, My Work, Planning Workspace, Recommendation Workspace, Decision Workspace, Portfolio Analytics, Cross-Portfolio Planning (advisory), Capacity History, Workload History, AI Recommendations, Explainability, Executive Summaries, and Audit — none of these services ever write operational data. No new tables. `ExecutiveWorkspaceController` exposes `/executive-workspace` and section GETs (`/overview`, `/enterprise`, `/portfolios`, `/recommendations`, `/decisions`, `/capacity`, `/workload`, `/ai`, `/audit`). Frontend `ExecutiveWorkspacePage` presents the unified, read-only executive view.

**Consequences**

Executives get a single view of strategic KPIs, enterprise health, and organizational performance across every operational workspace and dashboard, with drill-down to the underlying detail — without duplicating analytical data, changing any existing calculation, or introducing new persistence. **EPIC-05 — Operational Workspace continues with US-506.**

---

## DEC-506-001

**Date:** 2026-07-27

**Status:** Accepted

**Context**

US-506 requires a centralized in-application Notification Center that consolidates events from AgencyOS modules. Notifications must be user-specific and company-isolated, informational only, automatically generated, and must never affect business transactions when generation fails (BR-2909). Notification actions (read/unread/archive) must be audited (BR-2910) without creating generation loops.

**Decision**

Persist a first-class `Notification` aggregate (`notification` table) with Create / MarkRead / MarkUnread / Archive. Expose three application services: `NotificationQueryService` (list/filter/detail/unread-count), `NotificationService` (read/unread/archive + audit), and `NotificationGenerationService.GenerateSafeAsync` (never throws). Generate notifications primarily from successful Audit Trail writes by mapping entity types to categories/source entities inside `AuditService` (skipping `AuditEntityTypes.Notification` to prevent recursion), with additional direct hooks for Portfolio create and Company Context select where no audit event is emitted. Resolve identity as `UserId = parameters.UserId ?? IAuditContext.UserId ?? "system"` and `CompanyId = parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId` (consistent with DEC-501-001). Map `SourceEntity`/`SourceEntityId` to frontend `NavigationPath` values for originating-entity drill-down (BR-2907). Out of scope: email, SMS, push, Teams, Slack, webhooks, mobile.

**Consequences**

Users get a single in-app notification hub with filters, unread badge, and source navigation. Business write paths remain unchanged when notification persistence fails. **EPIC-05 — Operational Workspace continues with US-507.**

---

## DEC-507-001

**Date:** 2026-07-27

**Status:** Accepted

**Context**

US-507 requires a Personal Productivity Dashboard that consolidates execution, capacity utilization, workload trends, completed/pending work, and personal operational metrics. The dashboard must remain user-specific, company-isolated, read-only, and must not introduce employee evaluation, ranking, AI coaching, or gamification. Metrics must originate from existing operational data (BR-3003), and dashboard usage must be auditable (BR-3010).

**Decision**

Implement a read-only orchestration façade (DEC-507-001) with no new persistence. Reuse existing My Work aggregation (`IPersonalDashboardAggregationService`), timeline (`IPersonalTimelineService`), and KPI calculation (`IPersonalKpiService`) under DEC-501-001 identity resolution. Add pure projection services — `PersonalMetricsService`, `PersonalTrendService`, and `ActivitySummaryService` — plus `PersonalProductivityDashboardService` to assemble summary/KPIs/trends/capacity/workload/activity/statistics. Period-over-period trends compare the selected Capacity/Workload window to the immediately preceding equal-length window. Completed work is projected from completed Decisions in the activity window. Record each section access via `IAuditService.RecordSafeAsync` with `AuditEntityTypes.PersonalProductivityDashboard` (BR-3010). Focus hints and completion rates are informational only and never modify operational behavior (BR-3007). `PersonalProductivityDashboardController` exposes `/personal-dashboard` and section GETs. Frontend `PersonalProductivityDashboardPage` presents the consolidated productivity view with drill-down into My Work, workspaces, Notification Center, Capacity, Workload, and Audit.

**Consequences**

Users get an analytical personal productivity surface without new storage or business-rule changes. **This story completes EPIC-05 — Operational Workspace (US-501–US-507) and the AgencyOS Release 1.1 MVP.**

---

