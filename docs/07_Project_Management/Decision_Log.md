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
