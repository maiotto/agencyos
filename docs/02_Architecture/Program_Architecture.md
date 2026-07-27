# AgencyOS Program Architecture

Version: 2.0

Status: Official Baseline

Baseline: 1.0

Date: 2026-07

---

# Overview

AgencyOS is an AI-First Goal-Oriented Operational Planning Platform designed for service companies whose operational execution depends on limited productive capacity.

Unlike traditional ERP, CRM or Project Management systems, AgencyOS does not organize work around customers, projects or tasks.

AgencyOS organizes the company around productive capacity and operational decision making.

Its purpose is to continuously answer questions such as:

- Can we sell this contract?
- Do we have enough capacity?
- Should we hire?
- Should we outsource?
- Which execution strategy is the best?
- What happens if constraints change?
- Which operational plan maximizes business objectives?

AgencyOS combines deterministic operational engines with Artificial Intelligence to generate, evaluate, optimize and explain execution strategies.

---

# Program Structure

AgencyOS consists of two independent strategic programs.

AgencyOS Program

│

├── Program A

│     AgencyOS Product

│

└── Program B

      AgencyOS AI Factory

Both programs share the same long-term vision.

However, they evolve independently.

The AI Factory exists to accelerate software engineering.

The AgencyOS Product exists to solve customer business problems.

Neither program dictates the roadmap of the other.

---

# Program A — AgencyOS Product

## Vision

Build the world's best operational planning platform for service businesses.

AgencyOS transforms business objectives into executable operational strategies.

---

## Mission

Convert commercial demand into optimized operational execution.

The platform continuously evaluates:

- business objectives
- operational constraints
- productive capacity
- execution alternatives

before recommending the best strategy.

---

# Product Evolution

AgencyOS evolved through four major architectural stages.

Phase 1

Capacity Planning

↓

Operational Planning

↓

Decision Support

↓

Decision Intelligence

Each phase extends the previous one.

No architectural redesign invalidated previous work.

---

# Product Principles

AgencyOS is based on the following principles.

## Capacity First

Capacity is the central business asset.

Every commercial decision must consider productive capacity before acceptance.

---

## Hours as Operational Unit

AgencyOS does not measure production by:

- videos
- projects
- campaigns

The operational unit is productive hours.

Every service consumes hours.

Every resource provides hours.

Every decision balances hours.

---

## Goal-Oriented Planning

Planning starts from business objectives.

Not from available resources.

Example

Business Goal

↓

Deliver 25 videos

↓

Budget

↓

Deadline

↓

Quality

↓

AgencyOS generates the operational strategy.

Resources become part of the solution.

Not the starting point.

---

## Explainable Decisions

Every recommendation must explain:

Why this strategy was selected.

Why competing strategies were rejected.

Every recommendation must be auditable.

---

## Deterministic Core

Business calculations must always be deterministic.

Given identical inputs the system must always produce identical outputs.

Artificial Intelligence never replaces deterministic business calculations.

It complements them.

---

# Product Scope

AgencyOS supports the complete operational lifecycle.

Lead

↓

Client

↓

Contract

↓

Mission

↓

Task

↓

Execution Resource

↓

Assignment

↓

Capacity Planning

↓

Decision Engine

↓

Decision Intelligence

↓

Execution

↓

Continuous Replanning

---

# High-Level Architecture

AgencyOS is organized into six architectural layers.

Presentation Layer

↓

Application Layer

↓

Business Domain Layer

↓

Operational Intelligence Layer

↓

Decision Layer

↓

Decision Intelligence Layer

Each layer depends only on lower layers.

Dependencies always point inward.

No circular dependency is permitted.

---

# Layer 1 — Presentation

Responsibilities

- REST APIs
- Future Web Application
- Future Mobile Application
- External Integrations

Current implementation

Backend REST APIs.

Future implementation

React Frontend.

---

# Layer 2 — Application

Responsibilities

- Use Cases
- Application Services
- Validation
- Authorization
- Logging
- Transactions
- Orchestration

The Application Layer never contains business rules.

Business rules belong to the Domain Layer.

---

# Layer 3 — Business Domains

Business Domains model the operational reality of the organization.

Current domains

Commercial

Operations

Planning

Decision

Future domains

Financial

Analytics

Simulation

Optimization

Knowledge

Each domain owns:

- entities
- repositories
- services
- business rules

No domain directly modifies another domain.

Interaction occurs only through public interfaces.

---

# Commercial Domain

Purpose

Manage the commercial lifecycle.

Entities

Lead

Client

Client Contact

Client Contract

Mission

Responsibilities

- Customer acquisition
- Opportunity management
- Contract management
- Commercial history

Commercial Domain is responsible for demand generation.

Operational execution begins only after Contract approval.
---

# Operations Domain

Purpose

Transform commercial commitments into executable operational work.

The Operations Domain is responsible for organizing execution independently of resource availability.

Its responsibility ends when the operational structure is completely defined.

Entities

Mission

Task

Execution Resource

Assignment

Relationships

Contract

↓

Mission

↓

Task

↓

Assignment

↓

Execution Resource

Responsibilities

- Mission management
- Task management
- Resource allocation
- Assignment lifecycle
- Operational execution structure

The Operations Domain does not calculate capacity.

Capacity belongs to the Planning Layer.

---

# Planning Layer

Purpose

Transform operational data into measurable operational intelligence.

The Planning Layer is completely deterministic.

No Artificial Intelligence is used.

It provides reusable analytical services consumed by higher-level engines.

Current Planning Engines

Capacity Engine

Workload Engine

Availability Engine

Allocation Conflict Detection Engine

Each engine has a single responsibility.

---

## Capacity Engine

Purpose

Calculate productive capacity.

Inputs

Execution Resources

Working Calendar

Configurable company working calendar (US-101). Administrators define working days and validity periods. Capacity Planning (US-105) consumes the Active calendar linked through Resource Availability for each planning day.

Holidays (US-102) provide the official non-working-day source. Working Calendar exposes holiday-aware evaluation through `GetOperationalWorkingDayAsync` and `GET /working-calendars/{id}/holidays`.

Working Hours (US-103) define standard weekday schedules linked to a Working Calendar. Operational-day responses include active schedule and planned net hours consumed by the Capacity Engine.

Resource Availability (US-104) links an Execution Resource to Working Calendar and Working Hours with weekly availability flags and daily overrides. Operational evaluation is exposed via `GET /resource-availabilities/operational`. Capacity (US-105) requires Active Resource Availability covering each day in the planning period.

Capacity History (US-106) stores immutable snapshots of every successful Capacity calculation (`capacity_history`). Query, compare, and aggregate endpoints are available under `/capacity/history`. Records are never updated or deleted.

Planning Templates (US-108) store reusable planning configuration references (`planning_template`) to Working Calendar, Working Hours, Resource Availability strategy, and default planning windows. Apply produces a new planning configuration without modifying the template or rewriting historical Capacity/Workload.

Portfolio Planning (US-109) aggregates Missions into `portfolio` / `portfolio_mission` and analyzes Capacity/Workload via existing engines plus historical aggregates. EPIC-01 Advanced Planning is complete (US-101–US-109).

Assignments

Business Calendar

Outputs

Available Hours

Consumed Hours

Remaining Capacity

Capacity Percentage

Responsibilities

- Installed Capacity
- Planned Capacity
- Remaining Capacity
- Capacity by Profile
- Capacity by Resource

---

## Workload Engine

Purpose

Measure planned operational workload.

Responsibilities

- Planned Hours
- Assigned Hours
- Workload Distribution
- Operational Load

Outputs

Workload per:

- Resource
- Mission
- Contract
- Period

Workload History (US-107) stores immutable snapshots of every successful Workload calculation (`workload_history`). Query, compare, aggregate, and trends endpoints are available under `/workload/history`. Records are never updated or deleted.

---

## Availability Engine

Purpose

Determine operational availability.

Inputs

Capacity

Workload

Assignments

Outputs

Available Resources

Unavailable Resources

Available Time Slots

Availability Matrix

---

## Allocation Conflict Detection

Purpose

Detect operational inconsistencies.

Responsibilities

Identify

- Overallocation

- Schedule conflicts

- Resource conflicts

- Invalid assignments

Produces

Conflict Report

Operational Warnings

No corrections are performed.

Only detection.

---

# Planning Pipeline

Capacity

↓

Workload

↓

Availability

↓

Allocation Conflict Detection

Each stage consumes outputs produced by previous stages.

No stage recalculates upstream information.

---

# Decision Layer

Purpose

Recommend the best operational strategy.

Unlike Planning Engines, the Decision Layer compares multiple execution alternatives.

The Decision Layer remains deterministic.

Current implementation corresponds to the MVP Decision Engine.

---

# Decision Engine

The Decision Engine consists of four independent services.

Delivery Strategy Builder

↓

Delivery Strategy Evaluator

↓

Delivery Strategy Ranking

↓

Delivery Strategy Explanation

Each service has exactly one responsibility.

---

## Delivery Strategy Builder

Purpose

Generate valid execution strategies.

Inputs

Contracts

Missions

Tasks

Execution Resources

Policies

Business Constraints

Outputs

Candidate Strategies

The Builder does not evaluate quality.

It only generates feasible alternatives.

---

## Delivery Strategy Evaluator

Purpose

Measure each candidate strategy.

Metrics include

Operational Cost

Execution Time

Capacity Consumption

Risk

Resource Utilization

Constraint Violations

The Evaluator does not rank strategies.

---

## Delivery Strategy Ranking

Purpose

Order evaluated strategies.

Uses

Company Decision Profiles

Weighted Scoring

Normalization

Business Priorities

Produces

Ordered Strategy List

No explanations are generated here.

---

## Delivery Strategy Explanation

Purpose

Explain recommendations.

Produces

Reasons

Trade-offs

Decision Metrics

Business Justifications

Rejected Alternatives

The MVP explanation engine remains deterministic.

Large Language Models are intentionally excluded.

---

# Company Decision Profiles

Every organization may prioritize different business objectives.

Default profiles

Profit Maximization

Delivery Speed

Operational Stability

AI Adoption

Human Resource Optimization

Balanced Strategy

Future releases may allow custom profiles.

Decision Profiles never modify calculations.

They modify only ranking priorities.

As of Release 1.1 (US-401), custom profiles are supported: Company Decision Profiles are a versioned, database-backed aggregate (`company_decision_profile`) managed through `/decision-profiles`, replacing the original configuration-only model. Each company manages its own profile lineage (create, edit as a new version, clone, activate/deactivate/archive, set default), with exactly one default Active profile per company. See ADR-007 (Implementation Note) and DEC-401-001.

---

# Multi-Company Configuration

As of Release 1.1 (US-402), `Company` is a first-class, database-backed aggregate (`company` table) rather than an implicit scalar id referenced by other aggregates. A Company has a unique CompanyCode and CompanyName (BR-2001/BR-2002), an Active/Inactive/Archived lifecycle, and optional pointers to its default Company Decision Profile, default Planning Template, and default Working Calendar. Archived Companies are excluded from listings by default and remain queryable when explicitly requested (BR-2009).

Only Active Companies may become the active company for a request (BR-2003). A scoped `ICompanyContext`, populated per request by `CompanyContextMiddleware` from the `X-Company-Id` header, holds the resolved active Company; requests without the header are unaffected, since every endpoint that previously required an explicit `companyId` continues to accept it directly. `GET /companies/active` returns the selected Company or the seeded default Company when none is selected.

Assigning a Decision Profile or a Planning Template to a Company is cross-checked against ownership: `DecisionProfileId` must reference the company's own Active default Company Decision Profile (BR-2007), and `DefaultPlanningTemplateId` must reference a Planning Template owned by the company (BR-2008). Setting a new default Company Decision Profile (via `/decision-profiles/{id}/set-default`) also updates the owning Company's `DecisionProfileId`, keeping BR-2007 consistent in both directions.

AI-generated artifacts now carry the Company that produced them: `company_id` was added to `AIRecommendation`, `Explainability`, `ExecutiveRecommendationSummary`, and `RecommendationWorkflow`, set from the originating `Recommendation.CompanyId` at generation/creation time (BR-2004). Mission, Client, and other CRM/commercial aggregates are unchanged by this story; they continue to be company-scoped indirectly through their existing relationships, and adopting a first-class `CompanyId` on them is deferred to a future story. See DEC-402-001.

---

# Enterprise Dashboard

As of Release 1.1 (US-403), the Enterprise Dashboard is a strictly read-only aggregation layer over existing repositories — it introduces no new analytical tables and never recalculates Capacity/Workload history (BR-2101, BR-2102). Four services compose the slice: `IDashboardAggregationService` builds each of the nine sections (Summary, Planning, Portfolio, Capacity, Workload, Recommendations, Decisions, AI, Audit) directly from `IRecommendationRepository`, `IDecisionRepository`, `IPortfolioRepository`, `ICapacityHistoryRepository`, `IWorkloadHistoryRepository`, `IPlanningTemplateRepository`, `IAIRecommendationRepository`, `IExplainabilityRepository`, `IExecutiveRecommendationSummaryRepository`, and `IAuditEventRepository`; `IDashboardHealthCalculationService` delegates every health determination to the existing `PortfolioHealth.Calculate`/`Canonicalize` logic (BR-2107) so no independent health thresholds are introduced; `IDashboardTrendService` computes Up/Down/Flat + deltaPercent trends by comparing the current reporting window against an immediately preceding window of equal length (BR-2106); and `IEnterpriseDashboardService` orchestrates the other three, resolving the effective Company as `parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId` (BR-2104), defaulting an unset window to the trailing 30 days (BR-2105), and fetching all sections concurrently via `Task.WhenAll` (BR-2102).

`EnterpriseDashboardController` exposes the full dashboard and each individual section under `/enterprise-dashboard`, validated by `EnterpriseDashboardQueryParametersValidator`. The frontend `EnterpriseDashboardPage` is the application's default landing route, presenting KPI cards, health chips, trend indicators, and drill-down links back into the underlying list pages so every summarized figure remains traceable to its source of truth. See DEC-403-001 — EPIC-04, renamed **Enterprise Capabilities**, continues with US-404.

---

# Portfolio Analytics

As of Release 1.1 (US-404 / BR-2201..BR-2210), Portfolio Analytics extends the read-only aggregation model from US-403 down to the individual Portfolio: it introduces no new tables, never mutates a `Portfolio`, and never recalculates the Capacity/Workload engines. `IPortfolioAnalyticsService`/`PortfolioAnalyticsService` orchestrates the slice, resolving the effective Company identically to US-403 (`parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId`, BR-2207) and delegating to four specialized services: `IPortfolioTrendAnalysisService`/`PortfolioTrendAnalysisService` builds month-bucketed Capacity/Workload/Health/Recommendation/Decision trend series (optionally scoped to a single Portfolio's Missions via `PortfolioId`), deriving each bucket's health via `PortfolioHealth.Calculate` over the bucket's averaged `CapacityHistory`/`WorkloadHistory` utilization; `IPortfolioComparisonService`/`PortfolioComparisonService` builds a side-by-side comparison of two Portfolios with mission-scoped Recommendation/Decision counts and field-level diffs (BR-2202); `IPortfolioHealthAnalyticsService`/`PortfolioHealthAnalyticsService` rolls up health distribution and Overloaded/AtRisk risk indicators by delegating every determination to the existing `IDashboardHealthCalculationService` (BR-2203); and `IPortfolioRiskAnalyticsService`/`PortfolioRiskAnalyticsService` is a pure, repository-free calculator (mirroring `IDashboardHealthCalculationService`) that flags low average Recommendation scores and high Decision cancellation ratios from pre-built snapshots.

A shared static `PortfolioAnalyticsSnapshotBuilder` centralizes the read-only projection: it parses `Portfolio.CapacitySummary`/`WorkloadSummary` JSON tolerantly for `overallUtilizationPercentage`/`overallWorkloadPercentage` (BR-2204, never re-deriving these figures), reads `Portfolio.PortfolioHealth` as-is, and filters a company-wide Recommendation/Decision set down to a Portfolio's own Missions — the same `PortfolioAnalyticsSnapshot` this builder produces feeds the Overview, Ranking, Health, and Performance endpoints, all fetched from a single company-wide Recommendation/Decision query per request to avoid N+1 queries (BR-2208), while the Detail and Compare endpoints use per-Mission repository queries since those Mission sets are small and bounded. Portfolio ranking (`PortfolioAnalyticsService.GetRankingAsync`) combines stored health severity, snapshot utilization (penalizing both under- and over-utilization), and Decision completion effectiveness into a single weighted score (BR-2210).

`PortfolioAnalyticsController` exposes `/portfolio-analytics` (overview), `/portfolio-analytics/trends`, `/portfolio-analytics/compare`, `/portfolio-analytics/ranking`, `/portfolio-analytics/health`, `/portfolio-analytics/performance`, and `/portfolio-analytics/{portfolioId}` — with the named routes registered ahead of the `{portfolioId:guid}` route to avoid route shadowing — validated by `PortfolioAnalyticsQueryParametersValidator`/`PortfolioCompareQueryParametersValidator`. The frontend `PortfolioAnalyticsPage` sits next to the Portfolios list ("Portfolio Analytics" navigation entry) with overview/ranking tables, health distribution and risk indicators, a trend table, and a two-Portfolio comparison panel, all linking back into `/portfolios/:id`. See DEC-404-001 — EPIC-04 Enterprise Capabilities continues with US-405.

---

# Cross-Portfolio Planning

As of Release 1.1 (US-405 / BR-2301..BR-2310), Cross-Portfolio Planning is a strictly read-only, advisory simulation layer over the existing Portfolio, Capacity, Workload, and Allocation Conflict engines: it never modifies a `Portfolio` (no `UpdateAsync`/`AddAsync`/`DeleteAsync`), never recalculates operational history, and never executes reallocation, hiring, or financial-optimization changes automatically. `ICrossPortfolioPlanningService`/`CrossPortfolioPlanningService` orchestrates the slice, resolving the effective Company identically to US-403/US-404, loading the selected Portfolios (validated to belong to that Company, minimum two for simulation), and delegating to five specialized services: `IEnterpriseCapacityService`/`EnterpriseCapacityService` and `IEnterpriseWorkloadService`/`EnterpriseWorkloadService` aggregate the selected Portfolios' stored `CapacitySummary`/`WorkloadSummary` JSON snapshots (average/min/max utilization and workload), optionally complemented by a live `ICapacityCalculatorService`/`IWorkloadCalculatorService` aggregate when a planning period is supplied (`CapacityEngineUsed`/`WorkloadEngineUsed`); `ICrossPortfolioConflictDetectionService`/`CrossPortfolioConflictDetectionService` finds Missions assigned to more than one selected Portfolio and delegates Resource-level conflicts to the existing `IAllocationConflictDetectionService` (BR-2305); `ICrossPortfolioBalancingService`/`CrossPortfolioBalancingService` produces advisory-only rebalancing suggestions by pairing overloaded/at-risk Portfolios with underutilized ones, always listing each Portfolio's Missions ordered by ascending Priority — priorities are read, never reordered (BR-2306); `ICrossPortfolioScenarioComparisonService`/`CrossPortfolioScenarioComparisonService` diffs two stored scenarios field-by-field (capacity/workload averages, conflict and participation counts).

**DEC-405-001**: Cross-Portfolio Plans introduce no persistent aggregate and no new database tables. A singleton, thread-safe `ICrossPortfolioScenarioStore`/`CrossPortfolioScenarioStore`, backed by `ConcurrentDictionary<Guid, CrossPortfolioScenarioRecord>` with TTL and max-size eviction, holds simulated scenarios for side-by-side comparison within the process lifetime only — scenarios are explicitly temporary and may be evicted at any time. The durable record of every simulation is the Audit trail: `SimulateAsync` always calls `IAuditService.RecordSafeAsync` with `AuditEntityTypes.CrossPortfolioPlan` and `AuditEventTypes.Simulated` (BR-2309), and every response — overview, balance, simulation, and comparison alike — carries `RequiresHumanApproval = true` plus an advisory disclaimer (BR-2308), reinforcing that Cross-Portfolio Planning never executes anything automatically.

`CrossPortfolioPlanningController` exposes `/cross-portfolio-planning` (overview), `/cross-portfolio-planning/scenarios`, `/cross-portfolio-planning/conflicts`, `/cross-portfolio-planning/balance`, `POST /cross-portfolio-planning/simulate`, and `POST /cross-portfolio-planning/compare`, validated by dedicated FluentValidation validators (at least two distinct Portfolio ids to simulate, PeriodStart ≤ PeriodEnd, Left ≠ Right scenario to compare). A custom `GuidListModelBinder`/`GuidListModelBinderProvider` (API layer only, keeping the Application layer framework-free) binds `List<Guid> PortfolioIds` query parameters from either repeated keys or a single comma-separated value. The frontend `CrossPortfolioPlanningPage` sits next to `PortfolioAnalyticsPage` ("Cross-Portfolio Planning" navigation entry) with a Portfolio multi-select/participation table, period filters, Analyze Conflicts / View Balance / Simulate / Compare Scenarios actions, enterprise balance and conflict/recommendation tables, a temporary scenario list, and a persistent advisory-disclaimer banner, with drill-down links into `/portfolios`, `/portfolio-analytics`, `/enterprise-dashboard`, and `/audit`. See DEC-405-001 — this story completes EPIC-04 Enterprise Capabilities (US-401–US-405).

---

# My Work Dashboard

As of Release 1.1 (US-501 / BR-2401..BR-2410), the My Work Dashboard is a strictly read-only, personalized operational workspace — it introduces no new persistence and never modifies a business calculation; every figure is a projection over `Assignment`, `Task`, `Mission`, `RecommendationWorkflow`, `Recommendation`, `Decision`, `CapacityCalculatorService`, `WorkloadCalculatorService`, and Audit Event data that already exists. This story opens **EPIC-05 — Operational Workspace**, a new epic distinct from EPIC-04 Enterprise Capabilities: where the Enterprise Dashboard and Portfolio/Cross-Portfolio Analytics are Company-wide, executive-facing rollups, the My Work Dashboard is a single-user, task-level operational view.

**DEC-501-001**: without a full authentication/identity provider, the caller's identity is resolved with an explicit, documented fallback chain rather than assumed from a session: `UserId = parameters.UserId ?? IAuditContext.UserId ?? "system"`, `CompanyId = parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId`, and `ExecutionResourceId = parameters.ExecutionResourceId ?? (first active ExecutionResource whose Code equals UserId, case-insensitive)` — the heuristic lookup is skipped entirely for the literal `"system"` user. `IMyWorkDashboardService`/`MyWorkDashboardService` performs this resolution once per request and passes the resolved identity to every section. Assignments, Tasks, Missions, Capacity, and Workload are scoped by the resolved `ExecutionResourceId` when present; when it cannot be resolved, those sections return empty collections/summaries (`HasData = false` for Capacity/Workload) rather than failing the whole dashboard (BR-2401). Pending Recommendations union company-scoped `RecommendationWorkflow` records with `Status = PendingApproval` (the approval queue has no assignee field) with any workflow whose underlying `Recommendation.GeneratedBy` equals the resolved `UserId`. Pending Decisions select company-scoped Decisions with `DecisionStatus` `Created` or `InProgress`, preferring `CreatedBy` matches to `UserId` when the caller is not `"system"` while always remaining company-isolated. The Activity Timeline calls the existing `IAuditQueryService.GetByUserIdAsync(userId)` and filters the result in-memory by `CompanyId` and the `From`/`To` window, since that query method does not natively accept those filters.

`IPersonalDashboardAggregationService`/`PersonalDashboardAggregationService` loads and filters the Assignment/Task/Mission/Recommendation/Decision/Capacity/Workload sections (active work only — Assignment/Task status not Completed/Cancelled, BR-2406); `IPersonalTimelineService`/`PersonalTimelineService` projects Audit Events into the Activity Timeline; `IPersonalKpiService`/`PersonalKpiService` performs pure arithmetic over the already-aggregated counts (assigned Task/Mission counts, pending Recommendation/Decision counts, overdue Task count, upcoming-deadline count within a 14-day default window, and Utilization/Workload percentages when Capacity/Workload data is available) — no repository access happens in the KPI service. None of these services ever call a write method on any repository (BR-2401/BR-2402).

`MyWorkDashboardController` exposes `/my-work` (full dashboard) plus `/my-work/summary`, `/my-work/tasks`, `/my-work/missions`, `/my-work/recommendations`, `/my-work/decisions`, `/my-work/activity`, and `/my-work/kpis`, validated by `MyWorkDashboardQueryParametersValidator` (From ≤ To, PeriodStart ≤ PeriodEnd). Every card DTO (Mission, Task, Recommendation, Decision) and every Capacity/Workload/Activity summary carries a `DrillDownPath` pointing at an existing frontend route (`/recommendations?missionId=`, `/recommendations/workflow/{id}`, `/decisions/{id}`, `/capacity`, `/workload/history`, `/audit/...`) so every summarized figure remains one click from its source of truth — no new detail pages were introduced for Missions/Tasks since none exist yet; their drill-down reuses the filterable Recommendations list. The frontend `MyWorkDashboardPage` ("My Work" navigation entry next to "Dashboard") exposes User Id/Execution Resource Id override fields (persisted to `localStorage` and also sent as an `X-User-Id` request header, read by the same `IAuditContext` used for identity resolution) alongside From/To/PeriodStart/PeriodEnd filters, KPI tiles, Capacity/Workload widgets, overdue/upcoming deadline lists, Mission/Task/Recommendation/Decision cards, an Activity Timeline, and a persistent "Operational workspace — read-only. Drill-down does not modify data." banner. See DEC-501-001 — this story starts EPIC-05 Operational Workspace (not yet complete).

---

# Planning Workspace

As of Release 1.1 (US-502 / BR-2501..BR-2510), the Planning Workspace is a strictly read-only orchestration façade over existing Planning Template, Capacity/Workload History, Portfolio, and Cross-Portfolio Planning capabilities (DEC-502-001). It never mutates Portfolios/Templates, never recalculates engines for storage, and never persists scenarios. `IPlanningWorkspaceService`/`PlanningWorkspaceService` resolves Company identically to US-403/501 and delegates to `IPlanningOverviewService`, `IPlanningNavigationService`, `IPlanningHistoryService`, plus existing Template/Portfolio/History/Cross-Portfolio services. Planning Actions are deep-links into existing audited routes. Cross-Portfolio scenarios remain advisory (`RequiresHumanApproval = true`).

`PlanningWorkspaceController` exposes `/planning-workspace` plus `/overview`, `/templates`, `/capacity`, `/workload`, `/portfolios`, `/history`, and `/scenarios`. The frontend `PlanningWorkspacePage` ("Planning Workspace" navigation entry) presents KPIs, navigation actions, Capacity/Workload widgets, Templates, Portfolios, advisory Scenarios, and Planning History. See DEC-502-001 — EPIC-05 continues with US-501–US-502 (not yet complete).

---

# Recommendation Workspace

As of Release 1.1 (US-503 / BR-2601..BR-2610), the Recommendation Workspace is a strictly read-only orchestration façade over existing Recommendation, Recommendation Workflow, Recommendation History, Recommendation Comparison, AI Recommendation, Explainability, and Executive Recommendation Summary capabilities (DEC-503-001). It never mutates Recommendations, never bypasses mandatory human approval, and never changes Decision Engine behavior. Generate/Approve/Reject/Archive/Restore/Start Workflow and AI/Explainability/Executive generation are deep-links into existing audited routes. AI remains advisory; Explainability remains informational.

`RecommendationWorkspaceController` exposes `/recommendation-workspace` plus `/overview`, `/recommendations`, `/approval`, `/history`, `/compare`, `/ai`, and `/executive-summary`. The frontend `RecommendationWorkspacePage` ("Recommendation Workspace" navigation entry) presents KPIs, navigation actions, recommendation and approval queues, AI and explainability panels, history, optional comparison, and executive summaries. See DEC-503-001 — EPIC-05 continues with US-501–US-503 (not yet complete).

---

# Decision Workspace

As of Release 1.1 (US-504 / BR-2701..BR-2710), the Decision Workspace is a strictly read-only orchestration façade over the existing Decision lifecycle, Recommendation linkage, Decision Timeline, and Decision Audit capabilities (DEC-504-001). It never mutates a Decision, never bypasses mandatory human approval, and never changes Decision lifecycle rules. `IDecisionWorkspaceService`/`DecisionWorkspaceService` resolves Company identically to US-403/501/502/503 and delegates to `IDecisionOverviewService`/`DecisionOverviewService` (KPI aggregation), `IDecisionNavigationService`/`DecisionNavigationService` (pure navigation), and `IDecisionSummaryService`/`DecisionSummaryService` (Decisions/Timeline/Outcomes/Audit sections), all built on the existing `IDecisionService` and `IAuditQueryService` — none of these services ever call `CreateAsync`/`StartImplementationAsync`/`CompleteAsync`/`CancelAsync`/`RecordOutcomeAsync`. Create Decision/Start Implementation/Complete/Cancel/Record Outcome are exposed exclusively as navigation Decision Actions (deep-links to `/decisions/new` and `/decisions/{id}`) — `RequiresHumanApproval = true` on every one of them.

KPIs (`TotalCount`, `PendingCount`, `InProgressCount`, `CompletedCount`, `CancelledCount`, `WithOutcomeCount`, `ImplementationNotStartedCount`) are computed from `DecisionStatus`/`DecisionImplementationStatus` — the two independent status dimensions defined by BR-1403 — over Decisions filtered by Company and the resolved From/To window (default trailing 30 days). The Decisions section groups cards into Pending/InProgress/Completed/Cancelled buckets, each with a `DrillDownPath` to `/decisions/{id}` and a `RecommendationDrillDownPath` to `/recommendations/{recommendationId}`. The Timeline section supports two modes: when `DecisionId` is supplied (and belongs to the resolved Company — otherwise a 404, enforcing company isolation), it returns that Decision's full, immutable Timeline; otherwise it aggregates Timeline entries from the most recently updated Decisions (bounded, most-recent-first, capped at 50 entries) — the Decision Timeline itself is never edited (BR-2703). The Outcomes section lists only Decisions with a non-empty `Outcome`. The Audit section queries `IAuditQueryService.GetAllAsync` filtered to `AuditEntityTypes.Decision`, Company, and the period — the Decision Audit trail is immutable (BR-2704) and every workspace navigation action remains auditable at its origin (BR-2710).

`DecisionWorkspaceController` exposes `/decision-workspace` plus `/overview`, `/decisions`, `/timeline`, `/outcomes`, `/audit`, and `/kpis`, validated by `DecisionWorkspaceQueryParametersValidator` (From ≤ To). The frontend `DecisionWorkspacePage` ("Decision Workspace" navigation entry next to "Decisions" and "Recommendation Workspace") presents KPI tiles, navigation actions, Pending/In Progress/Completed queues, an optional Decision Id Timeline focus field, Timeline/Outcomes/Audit sections, and a persistent banner explaining that Create/Start/Complete/Cancel/Record Outcome navigate to the existing Decision pages and never execute automatically. See DEC-504-001 — EPIC-05 continues with US-501–US-504 (not yet complete).

---

# Executive Workspace

As of Release 1.1 (US-505 / BR-2801..BR-2810), the Executive Workspace is a strictly read-only orchestration façade that primarily reuses `IEnterpriseDashboardService` section methods rather than re-aggregating from every repository (DEC-505-001). It never mutates operational data, never recalculates any existing engine, and never persists new analytical storage. `IExecutiveWorkspaceService`/`ExecutiveWorkspaceService` resolves Company identically to US-403/501/502/503/504 (`CompanyId = parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId`), maps the query window onto the existing `EnterpriseDashboardQueryParameters` (CompanyId, From, To, PeriodStart, PeriodEnd; default trailing 30 days), and delegates every section to `IExecutiveAggregationService`/`ExecutiveAggregationService`.

`ExecutiveAggregationService` wraps each existing `IEnterpriseDashboardService` section getter (`GetSummaryAsync`, `GetPortfolioAsync`, `GetRecommendationsAsync`, `GetDecisionsAsync`, `GetCapacityAsync`, `GetWorkloadAsync`, `GetAiAsync`, `GetAuditAsync`) into an Executive*SectionResponse that embeds the underlying Enterprise Dashboard data plus a `DrillDownPath` into the matching operational workspace or dashboard (BR-2809 — no duplicate analytical data; every section is a thin projection, never a new repository aggregation). `IExecutiveOverviewService`/`ExecutiveOverviewService` combines the Summary, Portfolio, Capacity, Workload, and Audit sections into Executive KPIs (`PortfolioCount`, `ActivePortfolioCount`, `RecommendationCount`, `DecisionCount`, `PendingDecisionCount`, `CompletedDecisionCount`, `CapacityUtilizationPercentage`, `WorkloadPercentage`, `AuditEventCount`) via the pure `IExecutiveKpiService`/`ExecutiveKpiService` — which performs no repository or service calls of its own, only mapping the supplied Enterprise Dashboard section responses — and an overall health rollup reused from the existing `IDashboardHealthCalculationService.CalculateOverall` (BR-2805: health reuses existing calculations; no new health logic is introduced). `IExecutiveNavigationService`/`ExecutiveNavigationService` is a pure function returning 13 navigation deep-links spanning the Overview/Planning/Recommendations/Decisions/Capacity/Workload/AI/Audit/Portfolios categories into the Enterprise Dashboard, My Work, Planning Workspace, Recommendation Workspace, Decision Workspace, Portfolio Analytics, Cross-Portfolio Planning (advisory), Capacity History, Workload History, AI Recommendations, Explainability, Executive Summaries, and the Audit Trail — none of these services ever write operational data.

`ExecutiveWorkspaceController` exposes `/executive-workspace` plus `/overview`, `/enterprise`, `/portfolios`, `/recommendations`, `/decisions`, `/capacity`, `/workload`, `/ai`, and `/audit`, validated by `ExecutiveWorkspaceQueryParametersValidator` (From ≤ To, PeriodStart ≤ PeriodEnd). The frontend `ExecutiveWorkspacePage` ("Executive Workspace" navigation entry next to "Enterprise Dashboard") presents period filters, KPI tiles, an Executive Overview narrative and health rollup, navigation actions, and Portfolio/Recommendation/Decision/Capacity/Workload/AI/Audit summary panels with drill-down links into their existing detail pages, plus a persistent banner explaining that the Executive Workspace consolidates existing enterprise signals and is read-only. See DEC-505-001 — EPIC-05 continues with US-506.

# Notification Center

As of Release 1.1 (US-506 / BR-2901..BR-2910), the Notification Center is a centralized in-application notification hub. It is informational only: it never modifies business data, and notification generation failures never affect business transactions (`INotificationGenerationService.GenerateSafeAsync`, BR-2909). Notifications are user-specific and company-isolated, immutable except for read/archive status, and remain queryable when archived.

`Notification` persists Title, Message, Category, Priority, Status (Unread|Read), SourceEntity/SourceEntityId, CreatedAt, ReadAt, and Archived. `NotificationQueryService` lists/filters/details with unread counts; `NotificationService` marks read/unread/archive and records audited actions (BR-2910) without recursively generating further notifications. Automatic generation is wired from Audit Trail entity mappings (Decision, Recommendation Workflow, Capacity, Portfolio, Planning Template, Executive Summary, Company, Recommendation, plus a generic Audit fallback) and from Portfolio create / Company Context select hooks. `NotificationsController` exposes `/notifications`, `/notifications/filter`, `/notifications/unread-count`, `/notifications/{id}`, and POST `/{id}/read|unread|archive`. The frontend `NotificationCenterPage` ("Notifications" nav with unread badge) provides list, detail, filters, priority indicators, and Open source navigation via `NavigationPath`. See DEC-506-001 — EPIC-05 continues with US-507.

# Personal Productivity Dashboard

As of Release 1.1 (US-507 / BR-3001..BR-3010), the Personal Productivity Dashboard is a strictly read-only, analytical, user-focused workspace. It introduces no new persistence and never modifies operational behavior (BR-3002, BR-3007). Every figure is a projection over existing Assignment/Task/Mission/Recommendation/Decision/Capacity/Workload/Audit data, assembled by `IPersonalProductivityDashboardService`/`PersonalProductivityDashboardService` using the same DEC-501-001 identity chain as My Work.

`IPersonalMetricsService`/`PersonalMetricsService` derives personal KPIs, performance indicators, and operational statistics (including navigation deep-links into My Work, Planning/Recommendation/Decision workspaces, Notification Center, Capacity, Workload History, and Audit). `IPersonalTrendService`/`PersonalTrendService` compares the selected period to the prior equal-length window for capacity utilization, workload utilization, activity events, and completed decisions. `IActivitySummaryService`/`ActivitySummaryService` projects pending work (tasks/recommendations/decisions) and completed decisions plus the personal activity timeline. Dashboard section access is audited via `RecordSafeAsync` with `AuditEntityTypes.PersonalProductivityDashboard` (BR-3010).

`PersonalProductivityDashboardController` exposes `/personal-dashboard` plus `/summary`, `/kpis`, `/trends`, `/capacity`, `/workload`, `/activity`, and `/statistics`, validated by `PersonalProductivityDashboardQueryParametersValidator`. The frontend `PersonalProductivityDashboardPage` ("Productivity" navigation) presents KPI cards, capacity/workload widgets, trend indicators, pending/completed work, activity timeline, and a statistics panel. See DEC-507-001 — **this story completes EPIC-05 — Operational Workspace (US-501–US-507) and the AgencyOS Release 1.1 MVP.**

---

# Decision Layer Responsibilities

The Decision Layer never performs operational calculations.

Instead, it orchestrates analytical services produced by the Planning Layer.

Responsibilities

Generate

Evaluate

Rank

Explain

It never executes work.

Execution belongs to Operations.

---

# Operational Intelligence Flow

Commercial

↓

Operations

↓

Planning

↓

Decision

↓

Execution

↓

Continuous Monitoring

Every recommendation is generated before execution begins.

Recommendations may be regenerated whenever operational conditions change.

---

# Decision Intelligence Layer

Purpose

Transform AgencyOS from an operational decision platform into a Goal-Oriented Operational Planning Platform.

The Decision Intelligence Layer extends the MVP Decision Engine.

It does not replace it.

The deterministic Decision Engine remains the execution foundation.

Decision Intelligence adds strategic planning capabilities.
---

# Decision Intelligence Layer

The Decision Intelligence Layer represents the long-term strategic evolution of AgencyOS.

While the Decision Engine evaluates predefined execution alternatives, the Decision Intelligence Layer creates, optimizes and simulates operational strategies from business goals.

This layer introduces Artificial Intelligence without compromising deterministic business calculations.

The objective is to answer questions such as:

- How should this project be executed?
- Is there a better operational strategy?
- What happens if the deadline changes?
- What happens if another freelancer is hired?
- What happens if AI replaces part of the operation?
- Which strategy maximizes company objectives?

---

# Decision Intelligence Architecture

The Decision Intelligence Layer is composed of six independent engines.

Business Goal

↓

Constraint Engine

↓

Strategy Generation Engine

↓

Optimization Engine

↓

Ranking Engine

↓

Explainability Engine

↓

Simulation Engine

↓

Recommended Operational Strategy

Each engine has a single responsibility.

---

## Constraint Engine

Purpose

Represent every operational restriction as reusable business constraints.

Instead of embedding business rules throughout the application, constraints become independent components.

Typical Constraints

Budget

Deadline

Capacity

Skills

Customer Policies

Internal Policies

Required Equipment

Minimum Team Size

Maximum Outsourcing

Human Approval

Risk Threshold

Recommendation Persistence (US-202) stores every ranked Decision Engine recommendation as an immutable, versioned aggregate via `/recommendations`. Recommendation History (US-203) appends immutable evolution entries via `/recommendations/history` (versions, snapshots, workflow timeline). Recommendation Comparison (US-204) computes read-only side-by-side diffs via `/recommendations/compare` over history snapshots. Decision Tracking (US-205) monitors approved-recommendation lifecycle via `/decisions` (implementation progress, completion, outcome, append-only timeline). Decision Audit Trail (US-206) records immutable platform audit events via `/audit` with automatic non-blocking hooks and correlation identifiers. Recommendation Approval Workflow (US-201) provides durable governance via `/recommendations/workflow` and consumes persisted `RecommendationId`. Approved workflows become immutable; generation calculation logic remains unchanged aside from automatic persistence on rank, append-only history side effects, and best-effort audit recording. **EPIC-02 Decision Evolution is complete (US-201–US-206).** AI-assisted Recommendation (US-301) adds an immutable advisory aggregate via `/ai-recommendations` that analyzes existing Recommendation data with a deterministic advisor (confidence, assumptions, risks, alternatives) without replacing Recommendations or bypassing human approval; generate/archive emit Audit Events. LLM Explainability (US-302) adds an immutable informational aggregate via `/explainability` that produces natural-language explanations for Recommendations and AI Recommendations without changing Decision Engine calculations; generate/archive emit Audit Events. Executive Recommendation Summary (US-303) adds an immutable executive briefing aggregate via `/executive-summaries` that consolidates Recommendation, AI, Explainability, and Decision context into versioned briefings with compare support; generate/version/archive emit Audit Events. **EPIC-03 AI Decision Support is complete (US-301–US-303).**

Every strategy generated by the platform must satisfy all mandatory constraints.

Future releases may allow organizations to create custom constraints.

---

## Strategy Generation Engine

Purpose

Generate feasible operational strategies.

Unlike the MVP Decision Engine, which evaluates predefined alternatives, the Strategy Generation Engine creates alternatives automatically.

Inputs

Business Goals

Operational Constraints

Commercial Data

Capacity

Execution Resources

Planning Results

Outputs

Candidate Operational Strategies

A strategy may differ by:

- resource allocation
- execution sequence
- outsourcing level
- AI utilization
- schedule
- operational approach

There is no predefined limit on the number of generated strategies.

---

## Optimization Engine

Purpose

Identify the optimal strategy according to configurable business objectives.

Optimization Objectives

Lowest Cost

Shortest Delivery Time

Lowest Operational Risk

Highest Profit

Highest Capacity Utilization

Maximum AI Utilization

Minimum Human Effort

Balanced Strategy

Future versions may support multi-objective optimization.

Optimization never changes business constraints.

It searches only within feasible strategies.

---

## Ranking Engine

Purpose

Order optimized strategies.

The Ranking Engine extends the MVP implementation.

Future versions may consider:

Business Priorities

Historical Performance

Machine Learning Scores

Company Decision Profiles

Operational Preferences

Ranking remains deterministic.

Artificial Intelligence may assist ranking but never replace deterministic scoring.

---

## Explainability Engine

Purpose

Produce transparent operational recommendations.

AgencyOS shall never recommend a strategy without explaining:

Why it was selected.

Why competing strategies were rejected.

Which constraints influenced the decision.

Which trade-offs were accepted.

Which optimization objective was prioritized.

Every recommendation must be reproducible.

---

## Simulation Engine

Purpose

Support operational simulations.

The Simulation Engine evaluates hypothetical scenarios without modifying operational data.

Typical simulations

Increase budget

Reduce deadline

Hire another freelancer

Replace human resources with AI

Increase installed capacity

Reduce available staff

Introduce new operational policies

Modify customer priorities

Outputs

Operational Impact

Cost Impact

Capacity Impact

Delivery Impact

Recommended Actions

Simulation becomes one of the principal competitive differentiators of AgencyOS.

---

# Artificial Intelligence Strategy

AgencyOS adopts a Hybrid AI Architecture.

Artificial Intelligence complements deterministic business logic.

It never replaces it.

---

## Deterministic Components

Remain fully deterministic.

Includes

Commercial Rules

Capacity

Workload

Availability

Conflict Detection

Ranking

Business Constraints

Financial Calculations

Business Validation

These components always produce identical outputs for identical inputs.

---

## AI Components

Artificial Intelligence is responsible for exploratory reasoning.

Includes

Strategy Generation

Scenario Exploration

Optimization Assistance

Recommendation Refinement

Natural Language Interaction

Future Copilot Features

Artificial Intelligence operates within deterministic boundaries defined by business rules.

---

# Architecture Principles

AgencyOS follows these architectural principles.

Single Responsibility

Every service performs one business responsibility.

Layer Isolation

Dependencies always point toward lower layers.

Deterministic Core

Business calculations remain deterministic.

Explainability

Every recommendation must be explainable.

Auditability

Every recommendation must be reproducible.

Extensibility

New engines must be added without modifying existing engines.

Business First

Technology serves business objectives.

Goal-Oriented Planning

Planning starts from desired outcomes rather than available resources.

---

# Technical Architecture

AgencyOS follows a layered backend architecture.

Presentation

↓

Application

↓

Domain

↓

Infrastructure

↓

Database

Cross-cutting concerns are isolated in shared components.

The architecture supports independent evolution of business domains.

---

## Presentation Layer

Responsibilities

REST APIs

Authentication

Authorization

Request Validation

Swagger

Future Frontend Integration

---

## Application Layer

Responsibilities

Application Services

Use Cases

DTO Mapping

Transactions

Logging

Orchestration

Controllers invoke Application Services directly.

No MediatR.

No CQRS framework.

Business calculations are delegated to domain services.

---

## Domain Layer

Responsibilities

Business Entities

Business Rules

Value Objects

Domain Services

Repositories Interfaces

This layer contains no infrastructure dependencies.

---

## Infrastructure Layer

Responsibilities

Persistence

Repositories

External Services

Supabase Integration

Logging

Configuration

Messaging

Infrastructure never contains business rules.

---

## Shared Components

AgencyOS.Shared contains reusable components.

Examples

Common Exceptions

Base Classes

Utilities

Constants

Shared DTOs

Shared Interfaces

These components may be referenced by every layer without violating dependency rules.
---

# Database Architecture

AgencyOS uses PostgreSQL hosted on Supabase as the system of record.

The database is designed following a Database First approach.

Business entities are modeled before application services and user interfaces.

---

## Database Governance

Database evolution is managed exclusively through version-controlled SQL migrations.

Rules

- Every schema change must be implemented through a migration.
- Migrations are immutable after execution.
- Each migration represents a single logical change.
- Rollback procedures must be documented when applicable.

Entity Framework Core is used exclusively as the data access layer.

Entity Framework migrations are not permitted.

---

## Naming Standards

The following conventions apply to all database objects.

Tables

snake_case

Columns

snake_case

Primary Keys

UUID

Foreign Keys

*_id

Audit Fields

created_at

updated_at

archived_at (when the entity supports Archive)

soft_delete (technical maintenance flag only; never a business lifecycle field)

Status Fields

status

Business inactivation uses Archive according to ADR-011.

Physical deletion and deleted_at lifecycle fields are not used for Commercial business entities.

These conventions are mandatory across every business domain.

---

## Seed Data

Reference data initialization uses:

supabase/seed.sql

Seed data is deterministic.

Random identifiers are not permitted.

Closed enumerations defined by ADR-012 are not seeded.

Closed enumerations are implemented as C# enums and persisted as smallint columns.

No lookup tables are created for closed enumerations.

Transactional data is never seeded.

Business data is created exclusively through application APIs.

Optional non-enumeration reference data may be seeded only when explicitly approved by architecture and never for MVP Commercial closed enumerations.

---

## Audit Persistence

Audit history is centralized in AuditEvents according to ADR-009.

Audit records are written only after successful transaction commit.

GetHistory queries read exclusively from AuditEvents.

Event Sourcing is not adopted.

Aggregate state is never reconstructed from audit history.

---

## Aggregate Persistence Boundaries

According to ADR-010:

Commercial Aggregate Roots

- Lead
- Client
- Contract

Contact is persisted as a child of Client.

Mission belongs to Operations and is not owned by Commercial Aggregates.

Repositories exist only for Aggregate Roots.

---

# API Architecture

AgencyOS exposes all business capabilities through REST APIs.

Each business domain owns its own API surface.

The API layer is responsible for:

- Request validation
- Authentication
- Authorization
- DTO mapping
- Error handling
- API documentation

Business rules remain in the Domain Layer.

---

## Current API Domains

Commercial

- Leads
- Clients
- Contacts
- Contracts

Operations

- Missions
- Tasks
- Execution Resources
- Assignments

Planning

- Capacity
- Workload
- Availability
- Allocation Conflicts

Decision

- Delivery Strategy Builder
- Delivery Strategy Evaluation
- Delivery Strategy Ranking
- Delivery Strategy Explanation

Future

- Decision Intelligence
- Simulation
- Optimization
- Analytics

---

# Security Principles

AgencyOS follows Security by Design.

Core principles

- Authentication before authorization
- Least privilege
- Secure defaults
- Input validation
- Structured exception handling
- Audit logging
- API versioning
- Deterministic business processing

Future releases may include Row Level Security policies where appropriate.

---

# Observability

Every business service should support:

- Structured logging
- Correlation identifiers
- Performance metrics
- Error tracking
- Operational diagnostics

Business calculations should remain observable without exposing sensitive information.

---

# Program B — AgencyOS AI Factory

## Vision

AI executes.

Humans govern.

---

## Purpose

AgencyOS AI Factory is an independent engineering program responsible for accelerating software delivery through AI-assisted development.

It is not part of the AgencyOS product scope.

Its responsibility is to improve how AgencyOS is built.

---

## Engineering Principles

The AI Factory follows these principles.

Documentation First

Every implementation begins from approved documentation.

Small Iterations

Development is organized into small, independently verifiable work packages.

Human Governance

AI proposes.

Humans approve.

Single Responsibility

Each AI Agent performs one clearly defined responsibility.

Traceability

Every implementation can be traced back to its originating requirement.

---

## AI Agent Organization

Management

- Product Owner Agent
- Product Manager Agent
- Scrum Master Agent

Architecture

- Solution Architect Agent

Engineering

- Backend Agent
- Frontend Agent
- Database Agent
- DevOps Agent

Quality

- QA Agent
- Code Review Agent
- Documentation Agent

Future agent specializations may be introduced without changing the overall engineering model.

---

## Engineering Workflow

The standard implementation lifecycle is:

Business Requirement

↓

Architecture Review

↓

User Story / Work Package

↓

Implementation

↓

Code Review

↓

Testing

↓

Documentation Update

↓

Commit

↓

Sprint Validation

Documentation is updated as part of the Definition of Done.

---

## Documentation Governance

The consolidated documentation is the official source of truth.

Historical sprint documents remain available for traceability.

Audit documents preserve the evolution of the program.

Baseline documents define the current official architecture.

When conflicts exist, precedence is:

1. Approved Baseline
2. Architecture Decision Records
3. Architecture Documentation
4. Product Documentation
5. Technical Documentation
6. Sprint Documentation
7. Historical Audit Records

---

# Program Evolution Roadmap

AgencyOS evolution is organized into successive maturity stages.

Stage 1

Commercial Management

↓

Stage 2

Operational Management

↓

Stage 3

Operational Planning

↓

Stage 4

Operational Intelligence

↓

Stage 5

Decision Support

↓

Stage 6

Decision Intelligence

↓

Stage 7

Goal-Oriented Operational Planning

Future evolution may include autonomous planning assistance while preserving deterministic business governance.

---

# Cross-Program Relationship

| Aspect | AgencyOS Product | AgencyOS AI Factory |
|--------|------------------|---------------------|
| Primary Goal | Operational Planning Platform | AI-Assisted Engineering Platform |
| Scope | Product | Development Process |
| Deliverable | Customer Value | Engineering Productivity |
| Governance | Product Management | Technical Leadership |
| Repository | AgencyOS | AgencyOS AI Factory (future) |
| Dependency | Independent | Supports Product Delivery |

The two programs evolve independently while sharing architectural principles and governance standards.

---

# Architecture Summary

AgencyOS is built around three strategic concepts:

Business Domains

Represent operational reality.

Operational Intelligence

Measures operational feasibility through deterministic calculations.

Decision Intelligence

Generates, evaluates, optimizes and explains execution strategies based on business goals and operational constraints.

These concepts establish AgencyOS as a Goal-Oriented Operational Planning Platform rather than a traditional ERP, CRM or Project Management system.

---

# Official Baseline Statement

This document defines the official Program Architecture for AgencyOS Baseline 1.0.

It supersedes previous architectural summaries and consolidates the architectural decisions established throughout Phase 1, Sprints 0–10 and the subsequent program audit.

Future architectural evolution shall extend this baseline without violating its fundamental principles:

- Capacity First
- Goal-Oriented Planning
- Deterministic Core
- Explainable Decisions
- Layered Architecture
- Independent Business Domains
- AI-Assisted Engineering
- Human Governance

---
---

# Architecture Decisions

The following Architecture Decision Records (ADRs) are part of the frozen AgencyOS MVP 1.0 architecture and complement the architectural principles defined in this document.

## ADR-006 – AI Factory

Defines the AI Factory architecture responsible for orchestrating AI providers, prompts, execution flows, explainability, and future extensibility.

## ADR-007 – Company Decision Profiles

Defines Company Decision Profiles as the mechanism for representing organization-specific business preferences used by the Decision Engine.

## ADR-008 – Documentation Update Workflow

Defines the official documentation governance process, ensuring that Product, Architecture, Functional Specification, Engineering Specification, ADRs, Baseline and Decision Log remain synchronized throughout the project.

## ADR-009 – Audit and History Strategy

Defines the platform audit strategy.

Architecture decisions:

- Audit is centralized.
- Audit data is stored in the AuditEvents repository.
- Domain Events are business notifications.
- Audit Events are persistence records.
- Event Sourcing is not adopted.
- Audit Events are generated only after successful transaction commit.
- Historical queries are executed exclusively against the centralized audit repository.

This strategy guarantees complete traceability while preserving a conventional transactional architecture.

## ADR-010 – Aggregate Boundary Strategy

Defines the Aggregate Root boundaries for the AgencyOS domain model.

Commercial Domain

Aggregate Roots

- Lead
- Client
- Contract

Child Entities

- Contact (owned by Client)

Operations Domain

Aggregate Roots

- Mission
- Task

Repositories exist only for Aggregate Roots.

Transactional consistency is guaranteed inside each Aggregate boundary.

Relationships between Aggregates occur exclusively through identity references.

This decision enforces Domain-Driven Design aggregate consistency.

## ADR-011 – Archive and Soft Delete Policy

Defines the persistence lifecycle strategy.

Business Archive

- Status = Archived
- ArchivedAt populated
- Entity preserved for history
- Entity remains queryable according to authorization rules

Soft Delete

- Technical operation only
- Reserved for exceptional maintenance scenarios
- Not used by business workflows
- Not exposed through public APIs

Business operations shall archive entities instead of deleting them.

## ADR-012 – Enumeration Persistence Strategy

Defines enumeration persistence across the platform.

Enumerations are implemented as:

- C# Enums
- Entity Framework Core Enum Conversion
- Database smallint columns

The platform does not create lookup tables for enumerations.

No enumeration seed data is required.

APIs expose enumeration names while persistence stores integer values.

This strategy minimizes complexity while maintaining strong typing and forward compatibility.

---

# Architectural Principles Reinforced

The following principles are considered mandatory for all MVP implementations.

- Clean Architecture
- Domain-Driven Design
- SOLID Principles
- Aggregate consistency
- Explicit transactional boundaries
- Immutable Domain Events
- Centralized Audit
- Business Archive instead of physical deletion
- Strongly typed enumerations
- Repository per Aggregate Root
- No Event Sourcing
- Architecture-first implementation
- Documentation-driven development

These principles are mandatory for every Engineering Specification and every implementation generated during AgencyOS MVP 1.0.
Version: 2.0

Status: Official Baseline

Owner: AgencyOS Program

Last Review: 2026-07