# AgencyOS Functional Specification

# Planning_Engines.md

Version: 1.0

Status: Draft

Baseline: 1.0

Owner: Product Management

Related Documents

- Business_Domains.md
- Operations_Domain.md
- Decision_Engine.md
- Product Vision
- Program Architecture
- AgencyOS Baseline

---

# 1. Purpose

This document defines the functional specification of the Planning Domain.

The Planning Domain is responsible for transforming operational structures into deterministic operational intelligence.

Its responsibility is analytical.

It does not execute work.

It does not recommend strategies.

It does not optimize execution.

It produces objective operational information consumed by the Decision Domain.

---

# 2. Scope

The Planning Domain includes four deterministic analytical engines.

- Capacity Engine
- Workload Engine
- Availability Engine
- Allocation Conflict Detection Engine

These engines operate sequentially.

Each engine consumes validated outputs produced by previous engines.

---

# 3. Business Objective

Measure operational feasibility before execution begins.

The Planning Domain answers questions such as:

- Do we have enough capacity?
- What is the current workload?
- Which resources remain available?
- Are there allocation conflicts?

The Planning Domain never answers:

- Which strategy is best?
- Should work be outsourced?
- Should AI replace human work?

These questions belong to the Decision Domain.

---

# 4. Planning Architecture

Operational Structure

↓

Capacity Engine

↓

Workload Engine

↓

Availability Engine

↓

Allocation Conflict Detection

↓

Decision Domain

Every engine has exactly one business responsibility.

---

# 5. Capacity Engine

## Purpose

Calculate productive capacity.

Capacity represents the total productive hours available for operational execution.

---

## Responsibilities

- Installed Capacity
- Planned Capacity
- Remaining Capacity
- Capacity Utilization
- Capacity by Resource
- Capacity by Profile

---

## Inputs

- Execution Resources
- Working Calendar (US-101; Active calendars only — BR-501)
- Holidays (US-102; non-working days via holiday-aware calendar evaluation — BR-503)
- Working Hours (US-103; weekday schedules and planned net hours — BR-504)
- Resource Availability (US-104; weekly flags and daily overrides — BR-505)
- Assignments

Capacity Engine (US-105) sums planned operational hours day-by-day. Missing operational configuration yields a business-rule error (BR-508); there is no Monday–Friday fallback.

Successful calculations automatically persist immutable Capacity History (US-106 / BR-601..BR-609) so past periods can be queried, aggregated, and compared without recalculation. History is Create-only; failed calculations never write history.

Planning Templates (US-108) capture reusable configuration references (Working Calendar, Working Hours, Resource Availability strategy, default planning window, capacity rules) without duplicating operational data. Applying a template prepares a new planning configuration and may invoke Capacity/Workload engines; templates and historical records are never mutated by Apply.

Portfolio Planning (US-109) consolidates multiple Active Missions into a Portfolio for a planning period. Portfolio Capacity and Workload use the existing engines; historical Capacity/Workload aggregates support analysis. Portfolio calculations never modify Mission planning (BR-911).

---

## Outputs

- Available Hours
- Consumed Hours
- Remaining Hours
- Capacity Percentage

---

## Business Rules

### BR-PLAN-001

Capacity shall always be expressed in productive hours.

---

### BR-PLAN-002

Capacity calculations are deterministic.

---

### BR-501

Capacity shall only consider Active Working Calendars.

### BR-502

Capacity shall ignore non-working weekdays.

### BR-503

Capacity shall ignore Holidays.

### BR-504

Capacity shall respect configured Working Hours.

### BR-505

Capacity shall respect Resource Availability.

### BR-506

Capacity shall calculate planned working hours.

### BR-507

Historical calculations must remain reproducible.

### BR-508

If no operational configuration exists, return a validation/business-rule error. Do not fall back to hardcoded Monday–Friday logic.

### BR-509

Capacity calculation must be deterministic.

---

### BR-PLAN-003

Every resource contributes independently to total capacity.

---

### BR-PLAN-004

Capacity calculations never modify operational data.

---

# 6. Workload Engine

## Purpose

Measure planned operational effort.

Workload represents committed productive hours.

---

## Responsibilities

- Planned Hours
- Assigned Hours
- Resource Load
- Mission Load
- Contract Load
- Period Load

---

## Inputs

- Tasks
- Assignments
- Estimated Hours

---

## Outputs

- Workload by Resource
- Workload by Mission
- Workload by Contract
- Workload by Period

Successful calculations automatically persist immutable Workload History (US-107 / BR-701..BR-709) so past periods can be queried, aggregated, compared, and trended without recalculation. History is Create-only; failed calculations and summary calculations never write history.

---

## Business Rules

### BR-PLAN-005

Workload is calculated exclusively from planned work.

---

### BR-PLAN-006

Completed work does not modify historical workload calculations.

---

### BR-PLAN-007

Workload calculations never modify assignments.

---

# 7. Availability Engine

## Purpose

Determine operational availability.

Availability is calculated from Capacity minus Workload.

---

## Responsibilities

- Available Resources
- Unavailable Resources
- Available Hours
- Availability Matrix

---

## Inputs

- Capacity Results
- Workload Results

---

## Outputs

- Resource Availability
- Remaining Capacity
- Availability Matrix

---

## Business Rules

### BR-PLAN-008

Availability depends on Capacity and Workload.

---

### BR-PLAN-009

Availability never reallocates resources.

---

### BR-PLAN-010

Availability calculations remain deterministic.

---

# 8. Allocation Conflict Detection Engine

## Purpose

Detect operational inconsistencies.

This engine identifies problems.

It never corrects them.

---

## Responsibilities

Detect

- Overallocation
- Schedule Conflicts
- Resource Conflicts
- Invalid Assignments

---

## Inputs

- Capacity
- Workload
- Availability
- Assignments

---

## Outputs

Conflict Report

Operational Warnings

---

## Business Rules

### BR-PLAN-011

Conflicts are reported only.

---

### BR-PLAN-012

No automatic correction is performed.

---

### BR-PLAN-013

Every detected conflict remains auditable.

---

# 9. Planning Pipeline

The Planning Domain executes analytical engines sequentially.

Capacity

↓

Workload

↓

Availability

↓

Allocation Conflict Detection

No engine recalculates upstream information.

Every engine consumes validated outputs.

---

# 10. Functional Inputs

The Planning Domain receives:

- Missions
- Tasks
- Execution Resources
- Assignments
- Working Calendars
- Business Calendars

---

# 11. Functional Outputs

The Planning Domain produces:

- Capacity Metrics
- Workload Metrics
- Availability Metrics
- Conflict Reports

These outputs become inputs for the Decision Domain.

---

# 12. Business Events

Events Produced

- Capacity Calculated
- Workload Calculated
- Availability Calculated
- Conflict Detected

Events Consumed

- Mission Updated
- Task Updated
- Assignment Updated
- Resource Updated

---

# 13. Functional Boundaries

The Planning Domain SHALL NOT

- create contracts
- create missions
- create tasks
- modify assignments
- optimize resources
- recommend execution strategies
- execute operational work

Planning only measures operational reality.

---

# 14. Validation Rules

Capacity

Must use productive hours.

---

Workload

Must originate from operational assignments.

---

Availability

Must consume Capacity and Workload.

---

Conflict Detection

Must consume Availability.

---

Pipeline validation

Capacity

↓

Workload

↓

Availability

↓

Conflict Detection

This sequence is mandatory.

---

# 15. Security Responsibilities

Planning calculations are read-only.

Planning services shall never modify operational information.

All calculations shall remain reproducible.

---

# 16. Domain Interfaces

Consumes

Operations Domain

Produces

Decision Domain

The Planning Domain connects operational structure with decision support.

---

# 17. Functional Constraints

Planning engines remain deterministic.

No Artificial Intelligence participates in Planning calculations.

Planning never recommends.

Planning never optimizes.

Planning never executes.

---

# 18. MVP Coverage

Included

✔ Capacity Engine

✔ Workload Engine

✔ Availability Engine

✔ Allocation Conflict Detection

Excluded

Predictive Capacity

Machine Learning

Simulation

Optimization

AI Planning

Scenario Generation

Automatic Corrections

These capabilities belong to future releases.

Note (Release 1.1 / US-405): Cross-Portfolio Planning is not a fifth Planning Engine and does not change this MVP baseline. It is a read-only, advisory Application-layer slice that *consumes* the existing Capacity Engine, Workload Engine, and Allocation Conflict Detection Engine outputs (stored Portfolio `CapacitySummary`/`WorkloadSummary` snapshots, `CapacityHistory`/`WorkloadHistory`, and `IAllocationConflictDetectionService`) to present an enterprise-wide, multi-Portfolio balance/conflict view and advisory rebalancing suggestions. It never recalculates, mutates, or bypasses these engines' immutable history, and its "scenarios" are TEMPORARY in-memory comparison records (DEC-405-001), not the "Scenario Generation" or "Simulation" capabilities excluded above — no optimization or automatic correction is performed.

---

# 19. Traceability

Related Functional Documents

Business_Domains.md

Operations_Domain.md

Decision_Engine.md

Business_Rules.md

Functional_Requirements.md

User_Stories.md

Acceptance_Criteria.md

Related Product Documents

Product Vision

Program Architecture

AgencyOS Baseline

---

# 20. Functional Compliance Statement

The Planning Domain is responsible exclusively for deterministic operational analysis.

Its outputs are analytical.

Its outputs support business decisions.

It never recommends operational strategies.

It never executes operational work.

This document becomes the official functional specification for the Planning Domain of AgencyOS Baseline 1.0.

---

# ============================================================
# DOCUMENT REVIEW & MVP VALIDATION
# ============================================================

## Validation Objective

Validate this specification against the current MVP implementation.

The objective is to verify implementation adherence.

No architectural redesign.

---

## Validation Checklist

Review:

✔ Capacity Engine

✔ Workload Engine

✔ Availability Engine

✔ Allocation Conflict Detection

✔ Services

✔ Repositories

✔ Controllers

✔ DTOs

✔ Validators

✔ REST APIs

✔ Calculation pipeline

✔ Business rules

✔ Domain boundaries

✔ Deterministic behavior

✔ Dependencies

✔ Business workflow

---

## Divergence Classification

Type A

Documentation Issue

Implementation is correct.

Documentation requires update.

---

Type B

Implementation Issue

Documentation is correct.

Implementation requires correction.

---

Type C

Architecture Issue

Potential architectural inconsistency.

Requires Architecture Review.

---

## Expected Deliverable

Produce a Functional Adherence Report.

Each validation item shall receive:

PASS

or

FAIL

For FAIL provide:

- affected files
- justification
- recommendation

Overall Result

- Fully Adherent
- Adherent with Minor Deviations
- Requires Corrections
- Architecture Review Required

Do not modify source code.

Do not modify documentation.

Analysis only.