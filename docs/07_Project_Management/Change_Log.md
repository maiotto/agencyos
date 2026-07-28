# Change Log

All notable changes to the AgencyOS Program are documented in this file.

This document follows the principles of Keep a Changelog while reflecting the evolution of the entire program, including product, architecture, engineering and documentation.

---

# v1.1.0 — Release 1.1 (in progress)

Date

2026-07-26

Status

In Progress

---

## Added

### US-101 — Configure Company Working Calendar

- Working Calendar domain model with BR-101..BR-109
- Database table `working_calendar` (Supabase migration)
- REST API under `/working-calendars`
- Administrative frontend for list/create/edit/activate/deactivate/delete
- Unit, validation, repository, and API tests

### US-102 — Manage Holidays

- Holiday domain model with BR-201..BR-213
- Database table `holiday` (Supabase migration)
- REST API under `/holidays`
- Working Calendar integration for operational working-day evaluation
- Administrative frontend for list/detail/create/edit/activate/deactivate/search/filter
- Domain, application, validator, repository, and API tests

### US-103 — Configure Standard Working Hours

- Working Hours domain model with weekday schedules (BR-301..BR-309)
- Database tables `working_hours` and `working_hours_day`
- REST API under `/working-hours`
- Operational-day enrichment with schedule and planned net hours
- Administrative frontend with weekday schedule editor
- Domain, validator, service, repository, and API tests

### US-104 — Configure Resource Availability

- Resource Availability domain model with weekly flags and daily overrides (BR-401..BR-410)
- Database tables `resource_availability`, `resource_availability_week_day`, `resource_availability_day_override`
- REST API under `/resource-availabilities`
- Operational availability evaluation with Calendar, Holidays, and Working Hours
- Administrative frontend with weekly editor and daily overrides
- Domain, validator, service, repository, and API tests

### US-105 — Calculate Capacity Using Calendar

- Capacity Engine consumes Working Calendar, Holidays, Working Hours, and Resource Availability (BR-501..BR-509)
- Removed Monday–Friday / weekly-proration fallback from Capacity calculation
- Extended Capacity API responses with operational-day breakdown
- Availability Engine and Allocation Conflict Detection consume operational days
- Capacity Planning frontend view
- Updated unit/service/API tests

### US-106 — Historical Capacity Analysis

- Immutable `CapacityHistory` aggregate and `capacity_history` table (BR-601..BR-609)
- Automatic persistence after successful Capacity Engine calculations
- History query/filter/compare/aggregate REST APIs under `/capacity/history`
- Frontend Capacity History list, detail, and comparison views
- Domain, repository, service, API, and Capacity Engine integration tests

### US-107 — Historical Workload Analysis

- Immutable `WorkloadHistory` aggregate and `workload_history` table (BR-701..BR-709)
- Automatic persistence after successful Workload Engine calculations
- History query/filter/compare/aggregate/trends REST APIs under `/workload/history`
- Frontend Workload History list, detail, comparison, and aggregation views
- Domain, repository, service, API, and Workload Engine integration tests

### US-108 — Planning Templates

- `PlanningTemplate` aggregate and `planning_template` table (BR-801..BR-810)
- Templates reference Working Calendar, Working Hours, RA strategy, and default planning window
- Clone and Apply operations; Active templates cannot be deleted; Inactive templates cannot be applied
- REST API under `/planning-templates`
- Frontend Planning Template management UI
- Domain, repository, service, API, clone, and apply tests

### US-201 — Recommendation Approval Workflow

- `RecommendationWorkflow` aggregate and transition history tables (BR-1001..BR-1010)
- Submit / Approve / Reject / Cancel / Reopen with validated transitions
- Immutable timeline of status changes
- REST API under `/recommendations/workflow`
- Frontend workflow list, detail, approval, rejection, and timeline views
- Domain, repository, service, and API tests
- Decision Engine recommendation generation unchanged

### US-202 — Recommendation Persistence

- `Recommendation` aggregate and `recommendation` table (BR-1101..BR-1110)
- Automatic persistence on Decision Engine ranking; ranking returns `RecommendationId`
- Immutable versioning; archive/restore; delete prohibited
- Workflow create consumes persisted Recommendation
- REST API under `/recommendations`
- Frontend recommendation list, detail, filters, archive/restore, version viewer
- Domain, repository, service, persistence, version, archive, and API tests

### US-203 — Recommendation History

- `RecommendationHistory` aggregate and `recommendation_history` table (BR-1201..BR-1206)
- Immutable append-only history for versions, archive/restore, and workflow transitions
- Filters by company, mission, contract, recommendation, version, workflow status, date; search supported
- REST API under `/recommendations/history`
- Frontend history list, timeline, snapshot/version viewer, filters/search
- Domain, repository, service, and API tests

### US-204 — Recommendation Comparison

- Read-only comparison over Recommendation History snapshots (BR-1301..BR-1306)
- No duplicated recommendation data; computed side-by-side diffs with highlighting
- Capacity, workload, payload, planning template, score, rank, delivery strategy, workflow status
- REST API under `/recommendations/compare`
- Frontend comparison screen, section diff viewers, JSON export
- Domain, repository, service, and API tests

### US-205 — Decision Tracking

- `Decision` / `decision_timeline` with Create, StartImplementation, Complete, Cancel, RecordOutcome (BR-1401..BR-1407)
- One Decision per Approved Recommendation; originating RecommendationId immutable; delete prohibited
- Independent DecisionStatus and ImplementationStatus; append-only timeline
- REST API under `/decisions`
- Frontend decision list, create, detail (progress/timeline/outcome), search/filters
- Domain, repository, service, and API tests

### US-206 — Decision Audit Trail

- Immutable `audit_event` store (BR-1501..BR-1510); no update/delete; read-only queries
- Automatic hooks for Recommendation, Workflow, Decision, Planning Template, Portfolio, Capacity, Workload
- Audit failures never block business transactions (BR-1510); correlation headers supported
- REST API under `/audit`
- Frontend audit list, detail/JSON viewer, entity history, correlation view
- Completes EPIC-02 — Decision Evolution (US-201–US-206)

### US-301 — AI-assisted Recommendation

- Immutable advisory `AIRecommendation` / `ai_recommendation` (BR-1601..BR-1610); Generate + Archive only
- Deterministic advisor (no external LLM); confidence, assumptions, risks, alternatives, model/prompt versions required
- Never replaces Recommendations; generation failures do not affect Recommendation lifecycle (BR-1608)
- Compare AI suggestion vs live Recommendation; Audit Events on generate/archive
- REST API under `/ai-recommendations`
- Frontend list, generate, detail (reasoning, confidence, alternatives, comparison)
- Starts EPIC-03 — AI Decision Support

### US-302 — LLM Explainability

- Immutable informational `Explainability` / `recommendation_explainability` (BR-1701..BR-1710); Generate + Archive only
- Deterministic explainer for Recommendation and AI Recommendation; never changes Decision Engine output
- Executive summary, detailed explanation, decision factors, assumptions, risks, confidence/capacity/workload explanations
- Model/prompt versions required; generation failures do not affect Recommendations (BR-1705); Audit Events on generate/archive
- REST API under `/explainability`
- Frontend list, generate, detail with confidence visualization

### US-303 — Executive Recommendation Summary

- Immutable informational `ExecutiveRecommendationSummary` / `executive_recommendation_summary` (BR-1801..BR-1810)
- Generate + CreateNewVersion + Archive; consolidates Recommendation, AI, Explainability, Decision context
- Confidence, recommended actions, business/capacity/workload impact, risks, assumptions persisted
- Compare vs Recommendation; archived summaries remain queryable; Audit Events on generate/version/archive
- REST API under `/executive-summaries`
- Frontend list, generate, detail (briefing, impacts, actions, confidence, comparison)
- Completes EPIC-03 — AI Decision Support (US-301–US-303)

### US-109 — Portfolio Planning

- `Portfolio` / `portfolio_mission` with company-unique names and Active Mission associations
- Capacity and Workload via existing engines; historical analysis via history aggregates
- Deterministic Portfolio Health; Inactive portfolios locked; Active cannot be deleted
- REST API under `/portfolios`
- Frontend portfolio list, create, detail (summary, health, missions, template)
- Completes EPIC-01 — Advanced Planning (US-101–US-109)

### US-401 — Company Decision Profiles

- Versioned `CompanyDecisionProfile` / `company_decision_profile` (BR-1901..BR-1910), replacing the ADR-007 configuration-only MVP model
- Create, Update (new immutable version + deactivate previous, BR-1905), Clone, Activate, Deactivate, Archive, SetDefault, ClearDefault
- One default Active profile per company (BR-1901, partial unique index); unique Active Name per company (BR-1902); only Active profiles usable for ranking (BR-1904)
- Seed data preserves the original six profiles for the default company, with Balanced Strategy as the seeded default
- Delivery Strategy Ranking, AI Recommendation, Explainability, Executive Recommendation Summary, and Recommendation History now record the Company Decision Profile Id/Version used at generation time
- Audit Events on Create / Update / Clone / Activate / Deactivate / Archive / SetDefault / ClearDefault
- REST API under `/decision-profiles`
- Frontend list, create/edit with dimension weight editor, detail with clone/set-default/activate/deactivate/archive
- See DEC-401-001 and ADR-007 Implementation Note

### US-402 — Multi-Company Configuration

- `Company` domain aggregate and `company` table (BR-2001..BR-2010): CompanyCode/CompanyName uniqueness, Active/Inactive/Archived lifecycle, DecisionProfileId/DefaultPlanningTemplateId/DefaultCalendarId assignment
- Create, Update, Activate, Deactivate, Archive; Archived Companies excluded by default and queryable via IncludeArchived (BR-2009)
- Update validates DecisionProfileId references the company's Active default Company Decision Profile (BR-2007) and DefaultPlanningTemplateId references a Planning Template owned by the company (BR-2008); `CompanyDecisionProfileService.SetDefaultAsync` keeps `Company.DecisionProfileId` in sync (BR-2007)
- Scoped Company context (`ICompanyContext`/`ICompanyContextService`) resolved per request by `CompanyContextMiddleware` from the `X-Company-Id` header; only Active Companies may be selected (BR-2003); missing header falls back to the seeded default Company
- `company_id` added to `ai_recommendation`, `recommendation_explainability`, `executive_recommendation_summary`, and `recommendation_workflow` (with backfill) so AI-generated artifacts carry the owning Company (BR-2004); Mission/CRM aggregates are unchanged for this story
- Audit Events on Create / Update / Activate / Deactivate / Archive
- REST API under `/companies` (list, by id, active, create, update, activate, deactivate, archive, select)
- Frontend Company selector in the app header, list/create/edit/detail pages with activate/deactivate/archive/select actions
- See DEC-402-001

### US-403 — Enterprise Dashboard

- Read-only aggregation of Summary, Planning, Portfolio, Capacity, Workload, Recommendations, Decisions, AI, and Audit sections directly from existing repositories (BR-2101, BR-2102); no new analytical storage, no recalculation of Capacity/Workload history
- `IDashboardAggregationService`/`DashboardAggregationService`, `IDashboardTrendService`/`DashboardTrendService`, `IDashboardHealthCalculationService`/`DashboardHealthCalculationService`, and orchestrating `IEnterpriseDashboardService`/`EnterpriseDashboardService`
- CompanyId resolution (`parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId`, BR-2104); default trailing-30-day reporting window (BR-2105); parallel section aggregation via `Task.WhenAll` (BR-2102)
- Period-over-period trend indicators (Up/Down/Flat + deltaPercent) comparing the current period against an immediately preceding equal-length period (BR-2106)
- Deterministic Health rollups reusing `PortfolioHealth.Calculate`/`Canonicalize` (BR-2107)
- `EnterpriseDashboardQueryParametersValidator` (From ≤ To, PeriodStart ≤ PeriodEnd when both set)
- REST API under `/enterprise-dashboard` (full dashboard plus `/summary`, `/planning`, `/portfolio`, `/capacity`, `/workload`, `/recommendations`, `/decisions`, `/ai`, `/audit`)
- Frontend `EnterpriseDashboardPage` set as the new default landing route, with KPI cards, health chips, trend indicators, utilization/workload progress bars, status breakdowns, and drill-down links; "Dashboard" is the first navigation item
- Aggregation, trend, health, service, HTTP, and aggregation-performance smoke tests
- See DEC-403-001 — EPIC-04 Enterprise Capabilities continues with US-404

### US-404 — Portfolio Analytics

- Read-only Portfolio Analytics services (BR-2201..BR-2210): `IPortfolioAnalyticsService`/`PortfolioAnalyticsService` (orchestration), `IPortfolioTrendAnalysisService`/`PortfolioTrendAnalysisService`, `IPortfolioComparisonService`/`PortfolioComparisonService`, `IPortfolioHealthAnalyticsService`/`PortfolioHealthAnalyticsService`, `IPortfolioRiskAnalyticsService`/`PortfolioRiskAnalyticsService`; no new persistence, no Portfolio mutation, no recalculation of Capacity/Workload engines
- `PortfolioAnalyticsSnapshotBuilder`: shared, read-only projection parsing `Portfolio.CapacitySummary`/`WorkloadSummary` JSON (tolerant parsing, BR-2204) and filtering company-wide Recommendation/Decision sets down to a Portfolio's own Missions
- CompanyId resolution consistent with US-403 (BR-2207); reuse of `IDashboardHealthCalculationService`, `HealthIndicator`, `TrendIndicator`, and `StatusCountItem` — no duplicated health thresholds or DTOs
- Month-bucketed Capacity/Workload/Health/Recommendation/Decision trend series, optionally scoped to a single Portfolio's Missions via `PortfolioId` (BR-2204..BR-2209)
- Side-by-side two-Portfolio comparison with mission-scoped Recommendation/Decision counts and field-level diffs (BR-2202)
- Portfolio ranking by stored health severity, snapshot utilization, and Decision completion effectiveness (BR-2210); health distribution and risk indicators (Overloaded/AtRisk status, low Recommendation score, high cancellation ratio)
- `PortfolioAnalyticsQueryParametersValidator`/`PortfolioCompareQueryParametersValidator` (From ≤ To, PeriodStart ≤ PeriodEnd, Compare requires two distinct Portfolio ids)
- REST API under `/portfolio-analytics` (overview, `/trends`, `/compare`, `/ranking`, `/health`, `/performance`, `/{portfolioId}` detail) with Swagger documentation
- Frontend `PortfolioAnalyticsPage`: overview/ranking tables, health distribution and risk indicators, month-bucketed trend table, two-Portfolio comparison panel; "Portfolio Analytics" navigation entry next to "Portfolios"
- Snapshot-builder, trend, comparison, health, risk, orchestration-service, validator, and HTTP tests
- See DEC-404-001 — EPIC-04 Enterprise Capabilities continues with US-405

### US-405 — Cross-Portfolio Planning

- Read-only, advisory Cross-Portfolio Planning services (BR-2301..BR-2310): `ICrossPortfolioPlanningService`/`CrossPortfolioPlanningService` (orchestration), `IEnterpriseCapacityService`/`EnterpriseCapacityService`, `IEnterpriseWorkloadService`/`EnterpriseWorkloadService`, `ICrossPortfolioConflictDetectionService`/`CrossPortfolioConflictDetectionService`, `ICrossPortfolioBalancingService`/`CrossPortfolioBalancingService`, `ICrossPortfolioScenarioComparisonService`/`CrossPortfolioScenarioComparisonService`; never modifies a `Portfolio` (no `UpdateAsync`/`AddAsync`/`DeleteAsync`)
- `AuditEntityTypes.CrossPortfolioPlan` and `AuditEventTypes.Simulated` added; every simulation is audited via `IAuditService.RecordSafeAsync` (BR-2309)
- **DEC-405-001**: no persistent aggregate — singleton in-memory `ICrossPortfolioScenarioStore`/`CrossPortfolioScenarioStore` backed by `ConcurrentDictionary<Guid, CrossPortfolioScenarioRecord>` (TTL/max-size eviction); scenarios are TEMPORARY for the process lifetime only, no new database tables; the Audit trail is the durable record of every simulation
- Enterprise Capacity/Workload views built from selected Portfolios' `CapacitySummary`/`WorkloadSummary` snapshots, optionally complemented by a live `ICapacityCalculatorService`/`IWorkloadCalculatorService` aggregate for the planning period (BR-2303, BR-2304)
- Portfolio Mission-overlap conflict detection plus delegated Resource conflicts via the existing `IAllocationConflictDetectionService` (BR-2305)
- Advisory-only rebalancing recommendations pairing overloaded/at-risk Portfolios with underutilized ones; Mission priorities always read and never reordered (BR-2306)
- Every response carries `RequiresHumanApproval = true` and a shared advisory disclaimer (BR-2308) — nothing is executed automatically
- `SimulateCrossPortfolioPlanRequestValidator`/`CompareCrossPortfolioScenariosRequestValidator`/query validators (minimum two distinct Portfolio ids to simulate, PeriodStart ≤ PeriodEnd, Left ≠ Right scenario to compare)
- REST API under `/cross-portfolio-planning` (overview, `/scenarios`, `/conflicts`, `/balance`, `POST /simulate`, `POST /compare`) with Swagger documentation and a custom `GuidListModelBinder` for `List<Guid> PortfolioIds` (repeated keys or comma-separated)
- Frontend `CrossPortfolioPlanningPage`: Portfolio multi-select/participation table, period filters, Analyze Conflicts / View Balance / Simulate / Compare Scenarios actions, enterprise balance and conflict/recommendation tables, temporary scenario list, persistent advisory-disclaimer banner; drill-down links into `/portfolios`, `/portfolio-analytics`, `/enterprise-dashboard`, `/audit`; "Cross-Portfolio Planning" navigation entry next to "Portfolio Analytics"
- Enterprise-capacity, enterprise-workload, conflict-detection, balancing, scenario-comparison, scenario-store, orchestration-service, validator, and HTTP tests, including assertions that Portfolios are never mutated and every simulation is audited
- See DEC-405-001 — completes EPIC-04 Enterprise Capabilities (US-401–US-405)

### US-501 — My Work Dashboard

- Read-only Personal Operational Dashboard services (BR-2401..BR-2410): `IPersonalDashboardAggregationService`/`PersonalDashboardAggregationService`, `IPersonalTimelineService`/`PersonalTimelineService`, `IPersonalKpiService`/`PersonalKpiService`, `IMyWorkDashboardService`/`MyWorkDashboardService`; no write method is ever invoked on any repository
- **DEC-501-001**: caller identity resolved via an explicit fallback chain — `UserId = parameters.UserId ?? IAuditContext.UserId ?? "system"`, `CompanyId = parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId`, `ExecutionResourceId = parameters.ExecutionResourceId ?? first Active ExecutionResource with matching Code` (skipped for `"system"`) — since AgencyOS has no full authentication/identity provider yet
- Assignments/Tasks/Missions/Capacity/Workload scoped by the resolved Execution Resource; active-only filtering excludes Completed/Cancelled Assignments and Tasks (BR-2406); empty collections/`HasData = false` summaries returned gracefully when the Execution Resource is unresolved or the Capacity/Workload engines report `NotFoundException`/`BusinessRuleException`
- Pending Recommendations union company-scoped `RecommendationWorkflow` (`Status = PendingApproval`) with workflows for Recommendations the caller generated; pending Decisions select company-scoped `DecisionStatus` `Created`/`InProgress`, preferring `CreatedBy` matches without relaxing company isolation
- Activity Timeline built from `IAuditQueryService.GetByUserIdAsync`, filtered in-memory by CompanyId and From/To
- `MyWorkDashboardQueryParametersValidator` (From ≤ To, PeriodStart ≤ PeriodEnd)
- REST API under `/my-work` (full dashboard plus `/summary`, `/tasks`, `/missions`, `/recommendations`, `/decisions`, `/activity`, `/kpis`) with Swagger documentation; every card/summary DTO carries a `DrillDownPath` into an existing frontend route
- Frontend `MyWorkDashboardPage`: User Id/Execution Resource Id override fields (persisted to `localStorage`, also sent as an `X-User-Id` header consumed by the existing `HttpAuditContext`), From/To/PeriodStart/PeriodEnd filters, KPI tiles, Capacity/Workload widgets, overdue/upcoming-deadline lists, Mission/Task/Recommendation/Decision cards, Activity Timeline, and a persistent "Operational workspace — read-only. Drill-down does not modify data." banner; "My Work" navigation entry next to "Dashboard"
- Aggregation, timeline, KPI, orchestration-service, validator, and HTTP tests, including assertions that no write method is ever invoked
- See DEC-501-001 — starts EPIC-05 Operational Workspace (not yet complete)

### US-502 — Planning Workspace

- Read-only Planning Workspace orchestration (BR-2501..BR-2510): `IPlanningOverviewService`/`PlanningOverviewService`, `IPlanningNavigationService`/`PlanningNavigationService`, `IPlanningHistoryService`/`PlanningHistoryService`, `IPlanningWorkspaceService`/`PlanningWorkspaceService`; never mutates Portfolios/Templates or recalculates engines for storage
- **DEC-502-001**: orchestration façade only — calculation/execution actions are navigation deep-links into existing audited routes; Company resolution matches US-403/501
- REST API under `/planning-workspace` (full workspace plus `/overview`, `/templates`, `/capacity`, `/workload`, `/portfolios`, `/history`, `/scenarios`) with Swagger documentation
- Frontend `PlanningWorkspacePage`: period filters, KPIs, navigation actions, Capacity/Workload widgets, Templates/Portfolios/Scenarios/History; "Planning Workspace" navigation entry
- Navigation, history, orchestration, validator, and HTTP tests
- See DEC-502-001 — continues EPIC-05 Operational Workspace (US-501–US-502; not yet complete)

### US-503 — Recommendation Workspace

- Read-only Recommendation Workspace orchestration (BR-2601..BR-2610): `IRecommendationOverviewService`/`RecommendationOverviewService`, `IRecommendationNavigationService`/`RecommendationNavigationService`, `IRecommendationSummaryService`/`RecommendationSummaryService`, `IRecommendationWorkspaceService`/`RecommendationWorkspaceService`; never mutates Recommendation/Workflow/AI Recommendation/Explainability/Executive Summary data and never bypasses mandatory human approval
- **DEC-503-001**: orchestration façade only — Generate/Approve/Reject/Archive/Restore/Start Workflow/Generate AI/Generate Explainability/Generate Executive Summary are exposed as navigation actions (deep-links) to existing frontend routes; Company resolution matches US-403/501/502
- REST API under `/recommendation-workspace` (full workspace plus `/overview`, `/recommendations`, `/approval`, `/history`, `/compare`, `/ai`, `/executive-summary`) with Swagger documentation
- Frontend `RecommendationWorkspacePage`: period filters, KPIs, navigation actions, Recommendations/Approval/History/Compare/AI/Executive Summary sections; "Recommendation Workspace" navigation entry
- Navigation, summary, orchestration, validator, and HTTP tests; full backend suite of 1241 automated tests passing with 0 failures
- See DEC-503-001 — continues EPIC-05 Operational Workspace (US-501–US-503; not yet complete)

### US-504 — Decision Workspace

- Read-only Decision Workspace orchestration (BR-2701..BR-2710): `IDecisionOverviewService`/`DecisionOverviewService`, `IDecisionNavigationService`/`DecisionNavigationService`, `IDecisionSummaryService`/`DecisionSummaryService`, `IDecisionWorkspaceService`/`DecisionWorkspaceService`; never calls `CreateAsync`/`StartImplementationAsync`/`CompleteAsync`/`CancelAsync`/`RecordOutcomeAsync` on `IDecisionService` and never bypasses mandatory human approval
- **DEC-504-001**: orchestration façade only — Create Decision/Start Implementation/Complete/Cancel/Record Outcome are exposed as navigation actions (deep-links) to existing frontend routes (`/decisions/new`, `/decisions/{id}`); Company resolution matches US-403/501/502/503
- REST API under `/decision-workspace` (full workspace plus `/overview`, `/decisions`, `/timeline`, `/outcomes`, `/audit`, `/kpis`) with Swagger documentation
- Frontend `DecisionWorkspacePage`: period filters, optional Decision Id Timeline focus, KPIs, navigation actions, Pending/In Progress/Completed queues, Timeline, Outcomes, and Audit sections; "Decision Workspace" navigation entry near Decisions/Recommendation Workspace
- Navigation, overview, summary (Timeline with/without DecisionId, Outcomes filter, Audit filter), workspace orchestration, validator, and HTTP tests; full backend suite of 1281 automated tests passing with 0 failures
- See DEC-504-001 — continues EPIC-05 Operational Workspace (US-501–US-504; not yet complete)

### US-505 — Executive Workspace

- Read-only Executive Workspace orchestration (BR-2801..BR-2810): `IExecutiveNavigationService`/`ExecutiveNavigationService`, `IExecutiveKpiService`/`ExecutiveKpiService`, `IExecutiveAggregationService`/`ExecutiveAggregationService`, `IExecutiveOverviewService`/`ExecutiveOverviewService`, `IExecutiveWorkspaceService`/`ExecutiveWorkspaceService`; primarily reuses `IEnterpriseDashboardService` section methods and never re-aggregates from repositories directly or writes operational data
- **DEC-505-001**: orchestration façade only — reuses Enterprise Dashboard section methods and adds executive navigation deep-links into every existing operational workspace and dashboard; Company resolution matches US-403/501/502/503/504
- REST API under `/executive-workspace` (full workspace plus `/overview`, `/enterprise`, `/portfolios`, `/recommendations`, `/decisions`, `/capacity`, `/workload`, `/ai`, `/audit`) with Swagger documentation
- Frontend `ExecutiveWorkspacePage`: period filters, KPI tiles, Executive Overview narrative/health, navigation actions, Portfolio/Recommendation/Decision/Capacity/Workload/AI/Audit summary panels; "Executive Workspace" navigation entry next to Enterprise Dashboard
- Navigation, KPI, aggregation, overview, workspace orchestration, validator, and HTTP tests; full backend suite of 1337 automated tests passing with 0 failures
- See DEC-505-001 — EPIC-05 Operational Workspace continues with US-506

### US-506 — Notification Center

- In-application Notification Center (BR-2901..BR-2910): domain `Notification`, SQL migration, EF mapping, repository, query/service/generation services, validators, REST `/notifications`, Swagger, frontend Notification Center with unread badge
- Automatic generation via `INotificationGenerationService.GenerateSafeAsync` (BR-2909) from Audit Trail mappings and Portfolio/Company Context hooks; notification read/archive actions audited (BR-2910) without recursive generation
- User-specific + company-isolated; informational only; immutable except read/archive status
- See DEC-506-001 — EPIC-05 Operational Workspace continues with US-507

### US-507 — Personal Productivity Dashboard

- Read-only Personal Productivity Dashboard (BR-3001..BR-3010): `IPersonalMetricsService`, `IPersonalTrendService`, `IActivitySummaryService`, `IPersonalProductivityDashboardService`; reuses My Work aggregation/timeline; no new tables
- **DEC-507-001**: orchestration façade over existing operational data with DEC-501-001 identity and BR-3010 usage audit; informational productivity metrics never modify operational behavior
- REST API under `/personal-dashboard` (summary, kpis, trends, capacity, workload, activity, statistics) with Swagger
- Frontend `PersonalProductivityDashboardPage` ("Productivity" navigation): KPI cards, trend indicators, capacity/workload widgets, pending/completed work, activity timeline, statistics panel
- Metrics, trend, activity, validator, service, and HTTP tests
- See DEC-507-001 — completes EPIC-05 Operational Workspace (US-501–US-507) and AgencyOS Release 1.1 MVP

---

# v1.0.0 — AgencyOS Baseline 1.0

Date

2026-07

Status

Official Baseline

---

## Overview

AgencyOS reached its first official program baseline after completing:

- Phase 1
- Sprint 0
- Sprint 1
- Sprint 2
- Sprint 3
- Sprint 4
- Sprint 5
- Sprint 6
- Sprint 7
- Sprint 8
- Sprint 9

The MVP Backend is considered complete and validated.

Program documentation was fully reconciled through the official audit process.

---

## Added

### Product

- Commercial Domain
- Operations Domain
- Planning Engines
- Decision Engine
- Company Decision Profiles

---

### Commercial

- Lead Management
- Client Management
- Contact Management
- Contract Management

---

### Operations

- Mission Management
- Task Management
- Execution Resources
- Assignment Management

---

### Operational Intelligence

- Capacity Engine
- Workload Engine
- Availability Engine
- Allocation Conflict Detection

---

### Decision Engine

- Delivery Strategy Builder
- Delivery Strategy Evaluation
- Delivery Strategy Ranking
- Delivery Strategy Explanation

---

### Architecture

- Layered Architecture
- Domain-Oriented Design
- Database First
- Hybrid AI Architecture
- Goal-Oriented Planning Architecture

---

### Engineering

- Git Flow
- Supabase
- Cursor
- Swagger
- Automated HTTP Tests
- Development Standards
- Documentation Governance

---

### Documentation

Added official documentation for:

- Product
- Architecture
- Functional Specifications
- Technical Specifications
- ADRs
- AI Factory
- Sprint Register
- Decision Log
- Program Baseline
- Program Audit

---

## Changed

### Product Vision

AgencyOS evolved from an operational management platform into an AI-First Goal-Oriented Operational Planning Platform.

---

### Architecture

Decision Intelligence became the strategic evolution of the MVP architecture.

The deterministic Decision Engine remains the operational foundation.

---

### Engineering Governance

Documentation is now part of the Definition of Done.

Architecture changes require ADR approval.

The Baseline becomes the official source of truth.

---

### Program Governance

AgencyOS AI Factory became an independent engineering program.

Its responsibilities are limited to engineering acceleration.

Product architecture remains governed by the AgencyOS Product program.

---

## Fixed

- Delivery Strategy allocation duplication eliminated.
- Strategy evaluation stabilized.
- Documentation inconsistencies removed.
- Historical architectural conflicts reconciled.
- Roadmap inconsistencies corrected.
- Documentation hierarchy standardized.

---

## Infrastructure

- Supabase project consolidated.
- SQL migration strategy validated.
- Repository structure standardized.
- Backend architecture stabilized.
- API documentation synchronized.

---

## Quality

Backend Build

0 Errors

0 Warnings

Automated Tests

Validated

Documentation

Reconciled

Architecture

Consolidated

Governance

Established

---

## Audit

Completed

Audit-01

Program Reconstruction

Audit-02

Sprint 0

Audit-03

Sprint 1

Audit-04

Sprint 2

Audit-05

Sprint 3

Audit-06

Sprint 4

Audit-07

Sprint 5

Audit-08

Sprint 6

Audit-09

Sprint 7

Audit-10

Sprint 8

Audit-11

Sprint 9

Audit-12

Sprint 10

Audit-13

Program Reconciliation

Audit-14

Documentation Reconciliation

---

## Current Status

AgencyOS Baseline 1.0 established.

Backend MVP completed.

Documentation reconciled.

Architecture consolidated.

Governance established.

Frontend implementation becomes the next major milestone.

---

## Next Release

v1.1.0

Planned

Frontend MVP

Future releases are expected to include:

- React Frontend
- Authentication UI
- Operational Dashboard
- Decision Visualization
- User Experience Improvements

Decision Intelligence remains part of the long-term roadmap and is not included in the v1.1 release.

# v1.1.0 — Release 1.1

Date

2026-07

Status

Released

---

## Fixed

- Resolved DbContext concurrency in dashboard and workspace aggregation.
- Standardized UTC handling across the solution.
- Removed unintended notification generation.
- Validated complete startup sequence.
- Successfully completed Guided Test Drive.

---

## Validation

- Startup Checklist approved.
- Guided Test Drive approved.
- 1383 automated tests passed.
- No blocking defects remain.

---

## Result

Release 1.1 approved.

AgencyOS Backend considered stable for future development.