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
