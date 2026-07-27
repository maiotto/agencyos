# AgencyOS Functional Specification

# Decision_Engine.md

Version: 1.0

Status: Draft

Baseline: 1.0

Owner: Product Management

Related Documents

- Business_Domains.md
- Planning_Engines.md
- Product Vision
- Program Architecture
- AgencyOS Baseline

---

# 1. Purpose

This document defines the functional specification of the AgencyOS Decision Engine.

The Decision Engine is responsible for transforming deterministic operational intelligence into explainable business recommendations.

It evaluates operational alternatives before execution begins.

It never executes work.

It never changes operational data.

It supports human decision making.

---

# 2. Scope

The Decision Engine consists of four independent business services.

- Delivery Strategy Builder
- Delivery Strategy Evaluator
- Delivery Strategy Ranking
- Delivery Strategy Explanation

These services execute sequentially.

Each service has a single responsibility.

---

# 3. Business Objective

Recommend the most appropriate operational execution strategy while respecting business objectives and operational constraints.

Typical business questions include:

- Which execution strategy is best?
- Which strategy minimizes risk?
- Which strategy maximizes profit?
- Which strategy best utilizes available capacity?
- Which strategy aligns with company priorities?

The Decision Engine provides recommendations only.

Execution remains a human responsibility.

---

# 4. Functional Architecture

Planning Results

↓

Delivery Strategy Builder

↓

Delivery Strategy Evaluator

↓

Delivery Strategy Ranking

↓

Delivery Strategy Explanation

↓

Human Approval

↓

Execution

Each service consumes the outputs produced by previous stages.

---

# 5. Delivery Strategy Builder

## Purpose

Generate feasible operational execution strategies.

---

## Responsibilities

Generate strategy alternatives based on:

- Operational structure
- Available resources
- Business policies
- Operational constraints

---

## Inputs

- Contracts
- Missions
- Tasks
- Resources
- Assignments
- Planning Results

---

## Outputs

Candidate execution strategies.

---

## Business Rules

### BR-DEC-001

Only feasible strategies shall be generated.

---

### BR-DEC-002

Strategy generation remains deterministic.

---

### BR-DEC-003

The Builder never evaluates generated strategies.

---

# 6. Delivery Strategy Evaluator

## Purpose

Measure the quality of every generated strategy.

---

## Responsibilities

Calculate:

- Operational Cost
- Delivery Time
- Capacity Consumption
- Operational Risk
- Resource Utilization
- Constraint Violations

---

## Inputs

Generated strategies.

---

## Outputs

Evaluated strategies.

---

## Business Rules

### BR-DEC-004

Every strategy shall be evaluated using identical criteria.

---

### BR-DEC-005

Evaluation remains deterministic.

---

### BR-DEC-006

The Evaluator never ranks strategies.

---

# 7. Delivery Strategy Ranking

## Purpose

Order evaluated strategies according to business priorities.

---

## Responsibilities

Rank execution alternatives.

Apply Company Decision Profiles.

Normalize evaluation metrics.

Produce ordered recommendations.

---

## Inputs

Evaluated strategies.

Selected Company Decision Profile.

---

## Outputs

Ordered strategy list.

---

## Business Rules

### BR-DEC-007

Ranking uses Company Decision Profiles.

---

### BR-DEC-008

Ranking uses weighted normalized scores.

---

### BR-DEC-009

Ranking never recalculates evaluation metrics.

---

# 8. Delivery Strategy Explanation

## Purpose

Explain every recommendation.

---

## Responsibilities

Produce deterministic explanations.

Describe:

- Selection reasons
- Business trade-offs
- Capacity impact
- Operational risks
- Rejected alternatives

---

## Inputs

Ranked strategies.

---

## Outputs

Explainable recommendations.

---

## Business Rules

### BR-DEC-010

Every recommendation shall include an explanation.

---

### BR-DEC-011

Explanations remain deterministic.

---

### BR-DEC-012

Large Language Models are excluded from the MVP.

---

# 9. Company Decision Profiles

The Decision Engine supports configurable business priorities.

Default profiles:

- Balanced Strategy
- Profit Maximization
- Delivery Speed
- Operational Stability
- AI Adoption
- Human Resource Optimization

Profiles influence ranking only.

They never modify calculations.

As of Release 1.1 (US-401), Company Decision Profiles are managed as a versioned, database-backed aggregate (`company_decision_profile`) instead of static application configuration. Each company owns its own profiles, versioned by an immutable lineage: editing a profile creates a new version and deactivates the previous one, exactly one profile per company may be the default Active profile, and only Active profiles are eligible for ranking. Profiles are managed through `/decision-profiles` (Create, Update, Clone, Activate, Deactivate, Archive, SetDefault, ClearDefault). See ADR-007 (Implementation Note) and DEC-401-001 for details. The six default profiles above remain seeded for the default company; ranking and scoring behavior are unchanged.

As of Release 1.1 (US-402), "each company" above is backed by a first-class `Company` aggregate (`/companies`) rather than an implicit id. A Company's default Decision Profile (`Company.DecisionProfileId`) must be one of its own Active default Company Decision Profiles (BR-2007), and designating a new default profile via `/decision-profiles/{id}/set-default` keeps the Company record in sync. AI-generated artifacts produced from ranking (AI Recommendation, Explainability, Executive Recommendation Summary, Recommendation Workflow) now also record the `CompanyId` of the Recommendation they originate from (BR-2004), in addition to the `CompanyDecisionProfileId`/`Version` already recorded. See DEC-402-001.

---

# 10. Functional Inputs

The Decision Engine receives:

- Capacity Analysis
- Workload Analysis
- Availability Analysis
- Conflict Reports
- Operational Structure
- Company Decision Profile

---

# 11. Functional Outputs

The Decision Engine produces:

- Candidate Strategies
- Evaluation Results
- Ranked Recommendations
- Strategy Explanations

---

# 12. Business Events

Events Produced

- Strategy Generated
- Strategy Evaluated
- Strategy Ranked
- Recommendation Generated

Events Consumed

- Planning Completed
- Company Profile Selected

---

# 13. Functional Boundaries

The Decision Engine SHALL NOT

- create contracts
- modify missions
- modify tasks
- allocate resources
- calculate capacity
- calculate workload
- execute operational work

Its responsibility is recommendation only.

---

# 14. Functional Workflow

Planning Results

↓

Generate Strategies

↓

Evaluate Strategies

↓

Rank Strategies

↓

Explain Recommendation

↓

Human Approval

↓

Operational Execution

---

## Recommendation Persistence (US-202)

Every ranked Decision Engine recommendation is automatically persisted as an immutable `Recommendation` aggregate before publication:

- Canonical storage for Recommendation Workflow, History, Comparison, and Decision Tracking
- Versioning: regenerating a strategy creates a new version (never overwrites)
- Archive / Restore supported; delete is prohibited
- Payload + capacity/workload snapshots preserve operational inputs (BR-1105)
- Ranking responses include `RecommendationId` for each ranked strategy
- Approval Workflow consumes persisted `RecommendationId` (BR-1102)

Persistence failures abort recommendation publication (BR-1110).

---

## Recommendation History (US-203)

Every persisted Recommendation maintains an immutable, append-only history:

- Captures version creation, archive/restore, and workflow transitions (BR-1201..BR-1203)
- Historical payload and snapshots are never modified (BR-1204); history cannot be deleted (BR-1205)
- Filters by company, mission, contract, recommendation, version, workflow status, and date (BR-1206)
- Timeline APIs expose recommendation + workflow evolution for a lineage
- Read-only; Decision Engine generation and approval lifecycle behavior unchanged aside from history appends

---

## Recommendation Comparison (US-204)

Operators can compare recommendation versions and snapshots side-by-side:

- Source of truth is Recommendation History snapshots (BR-1306)
- Read-only; never modifies Recommendations (BR-1301, BR-1302)
- Diffs cover capacity, workload, planning template, score, rank, workflow status, delivery strategy, and payload (BR-1304)
- Differences are highlighted (BR-1305); client can export the comparison JSON
- APIs under `/recommendations/compare`

---

## Decision Tracking (US-205)

Approved Recommendations can be tracked as business Decisions:

- One Decision per Approved Recommendation (BR-1401); RecommendationId immutable (BR-1402)
- Independent DecisionStatus and ImplementationStatus (BR-1403)
- Append-only timeline (BR-1404); Completed cannot return to In Progress (BR-1405)
- Outcome recorded only after completion (BR-1406); delete prohibited (BR-1407)
- APIs under `/decisions` — create, start, complete, cancel, outcome, timeline
- Does not modify Recommendation generation or approval behavior

---

## Decision Audit Trail (US-206)

Platform-wide immutable audit log for Decision Evolution governance:

- Append-only `AuditEvent` records (BR-1501..BR-1504); read-only queries (BR-1509)
- Automatic coverage of Recommendation, Workflow, Decision, Planning Template, Portfolio, Capacity, and Workload executions (BR-1505 / BR-1506)
- Previous/current state preserved (BR-1507); correlation via request headers (BR-1508)
- Audit generation failures never prevent business transactions (BR-1510)
- APIs under `/audit`
- Completes EPIC-02 — Decision Evolution

---

## AI-assisted Recommendation (US-301)

Advisory decision support over persisted Recommendations:

- Immutable `AIRecommendation` aggregate (BR-1601..BR-1610); Generate + Archive only
- Never replaces Recommendations (BR-1601); output is advisory only (BR-1602)
- Human approval remains mandatory (BR-1603); always references one Recommendation (BR-1604)
- Generation is versioned and auditable (BR-1606 / BR-1607); failures do not affect Recommendation lifecycle (BR-1608)
- Confidence Score, Model Version, and Prompt Version always present (BR-1609 / BR-1610)
- Deterministic in-process advisor (no external LLM providers in this release)
- APIs under `/ai-recommendations` — list, get, by recommendation, generate, archive, compare
- Starts EPIC-03 — AI Decision Support

---

## LLM Explainability (US-302)

Informational natural-language explanations for Recommendations and AI Recommendations:

- Immutable `Explainability` aggregate (BR-1701..BR-1710); Generate + Archive only
- Never changes Recommendations or Decision Engine calculations (BR-1701)
- Always references one Recommendation (BR-1702); optional AIRecommendationId for AI explanations
- Versioned and auditable (BR-1704 / BR-1706); generation failures do not affect Recommendations (BR-1705)
- Executive Summary and Confidence Explanation mandatory (BR-1710 / BR-1709)
- Model Version and Prompt Version always stored (BR-1707 / BR-1708)
- Deterministic in-process explainer (no external prompt management in this release)
- APIs under `/explainability` — list, get, by recommendation, generate, archive

---

## Executive Recommendation Summary (US-303)

Informational executive briefings consolidating Recommendation, AI, Explainability, and Decision context:

- Immutable `ExecutiveRecommendationSummary` aggregate (BR-1801..BR-1810); Generate + CreateNewVersion + Archive
- Never changes Recommendations, AI Recommendations, or Decisions (BR-1801)
- Always references one Recommendation (BR-1802); optional AIRecommendationId / ExplainabilityId
- Every generation creates a new version (BR-1804); archived summaries remain queryable (BR-1810)
- Confidence Level, Executive Summary, and Recommended Actions mandatory (BR-1807..BR-1809)
- Generation failures do not affect Recommendations (BR-1805); auditable (BR-1806)
- Deterministic in-process briefing generator (no external LLM providers)
- APIs under `/executive-summaries` — list, get, by recommendation, generate, archive, versions, compare
- Completes EPIC-03 — AI Decision Support

---

## Recommendation Approval Workflow (US-201)

After a recommendation is **persisted** by the Decision Engine (US-202), governance is handled by `RecommendationWorkflow`:

- Starts in **Draft**
- **Submit** → Pending Approval
- **Approve** or **Reject**
- **Cancel** before approval
- **Reopen** rejected recommendations only
- **Approved** recommendations are immutable
- Every transition is timestamped in an append-only transition history

Workflow APIs do not generate recommendations and do not change Decision Engine calculation behavior. Workflow create requires a persisted Recommendation.

---

# 15. Validation Rules

Every strategy shall:

- satisfy operational constraints
- be evaluated
- be ranked
- receive an explanation

Strategies failing validation shall not be recommended.

---

# 16. Security Responsibilities

Recommendations shall remain auditable.

Every recommendation shall be reproducible.

Decision Profiles shall be controlled.

Operational approval shall require authorized users.

---

# 17. Functional Constraints

The Decision Engine is deterministic.

Artificial Intelligence does not participate in MVP Decision Engine generation or ranking. Release 1.1 US-301 adds a separate advisory AI Recommendation capability that never replaces engine Recommendations.

Business calculations remain reproducible.

Human approval is mandatory before execution.

---

# 18. MVP Coverage

Included

✔ Delivery Strategy Builder

✔ Delivery Strategy Evaluator

✔ Delivery Strategy Ranking

✔ Delivery Strategy Explanation

✔ Company Decision Profiles

Excluded

Decision Intelligence

Optimization Engine

Simulation Engine

Natural Language Explanations

LLM-based Recommendations

Autonomous Decisions

Predictive Recommendations

These capabilities belong to future releases.

---

# 19. Traceability

Related Functional Documents

Business_Domains.md

Planning_Engines.md

Business_Rules.md

Use_Cases.md

Functional_Requirements.md

User_Stories.md

Acceptance_Criteria.md

Related Product Documents

Product Vision

Program Architecture

AgencyOS Baseline

ADR-007 Company Decision Profiles

---

# 20. Functional Compliance Statement

The Decision Engine is responsible for producing deterministic and explainable operational recommendations.

It consumes operational intelligence.

It produces decision support.

It never executes operational work.

Final responsibility always belongs to authorized human users.

This document becomes the official functional specification for the Decision Engine of AgencyOS Baseline 1.0.

---

# ============================================================
# DOCUMENT REVIEW & MVP VALIDATION
# ============================================================

## Validation Objective

Validate this specification against the current MVP implementation.

The objective is to verify implementation adherence.

Architecture redesign is out of scope.

---

## Validation Checklist

Review:

✔ Delivery Strategy Builder

✔ Delivery Strategy Evaluator

✔ Delivery Strategy Ranking

✔ Delivery Strategy Explanation

✔ Company Decision Profiles

✔ Services

✔ Repositories

✔ Controllers

✔ DTOs

✔ Validators

✔ REST APIs

✔ Strategy Generation

✔ Evaluation Metrics

✔ Ranking Logic

✔ Explanation Logic

✔ Business Rules

✔ Domain Boundaries

✔ Dependencies

✔ Human Approval Flow

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