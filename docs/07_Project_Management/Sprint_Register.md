# AgencyOS Sprint Register

Version: 1.0

Status: Official

---

# Purpose

This document records the official execution history of the AgencyOS Program.

Unlike the Product Roadmap, which describes future evolution, the Sprint Register documents only completed or active work.

It serves as the official historical record of program execution.

---

# Program Timeline

| Phase | Status |
|---------|--------|
| Phase 1 – Product Definition | Completed |
| Sprint 0 – Engineering Foundation | Completed |
| Sprint 1 – Commercial Foundation | Completed |
| Sprint 2 – Mission Domain | Completed |
| Sprint 3 – Task Domain | Completed |
| Sprint 4 – Application Foundation | Completed |
| Sprint 5 – Commercial Module | Completed |
| Sprint 6 – Operational Module | Completed |
| Sprint 7 – Planning Engines | Completed |
| Sprint 8 – Decision Engine | Completed |
| Sprint 9 – MVP Validation & Hardening | Completed |

---

# Phase 1 — Product Definition

Status

Completed

## Objective

Define the AgencyOS product vision, business model, operational concepts and initial architecture.

## Deliverables

- Product Vision
- Initial Roadmap
- Conceptual Domain Model
- Product Scope
- Spreadsheet MVP
- Initial Backlog

## Outcome

AgencyOS concept validated.

---

# Sprint 0 — Engineering Foundation

Status

Completed

## Objective

Establish the engineering foundation for the project.

## Deliverables

- Git Repository
- Git Flow
- Supabase Project
- Cursor Configuration
- Project Structure
- Documentation Structure
- Development Standards

## Outcome

Development environment fully operational.

---

# Sprint 1 — Commercial Foundation

Status

Completed

## Objective

Implement the Commercial Domain.

## Deliverables

- Leads
- Clients
- Contacts
- Contracts
- Commercial Database Schema

## Outcome

Commercial lifecycle established.

---

# Sprint 2 — Mission Domain

Status

Completed

## Objective

Introduce Missions as the first operational entity.

## Deliverables

- Mission
- Mission Types
- Mission Status

## Outcome

Operational planning foundation established.

---

# Sprint 3 — Task Domain

Status

Completed

## Objective

Break Missions into executable Tasks.

## Deliverables

- Task
- Task Types
- Task Status

## Outcome

Operational execution model completed.

---

# Sprint 4 — Application Foundation

Status

Completed

## Objective

Implement the application architecture.

## Deliverables

- Layered Architecture
- Services
- Repositories
- Controllers
- Validation
- Dependency Injection

## Outcome

Backend architecture consolidated.

---

# Sprint 5 — Commercial Module

Status

Completed

## Objective

Complete the Commercial APIs.

## Deliverables

- CRUD APIs
- Validation
- Swagger
- HTTP Tests

## Outcome

Commercial module completed.

---

# Sprint 6 — Operational Module

Status

Completed

## Objective

Complete the Operational Domain.

## Deliverables

- Missions
- Tasks
- Execution Resources
- Assignments
- Operational APIs

## Outcome

Operational module completed.

---

# Sprint 7 — Planning Engines

Status

Completed

## Objective

Implement deterministic operational planning.

## Deliverables

Capacity Engine

Workload Engine

Availability Engine

Allocation Conflict Detection

## Outcome

Operational Intelligence completed.

---

# Sprint 8 — Decision Engine

Status

Completed

## Objective

Implement the AgencyOS Decision Engine.

## Deliverables

Delivery Strategy Builder

Delivery Strategy Evaluator

Delivery Strategy Ranking

Delivery Strategy Explanation

Company Decision Profiles

69 Automated Tests

Build

0 Warnings

0 Errors

## Outcome

AgencyOS MVP Backend completed.

---

# Sprint 9 — MVP Validation & Hardening

Status

Completed

## Objective

Validate, stabilize and prepare the MVP for production-quality integration and future frontend development.

## Work Packages

### WP-001 — Database Validation

Objectives

- Validate migrations
- Validate schema
- Validate clean environment setup
- Validate seed strategy

Status

Completed

---

### WP-002 — API Validation

Objectives

Validate all REST APIs independently.

Status

Completed

Modules

- Commercial
- Operations
- Planning
- Decision Engine

---

### WP-003 — End-to-End Validation

Objectives

Validate the complete business flow.

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

Capacity

↓

Workload

↓

Availability

↓

Allocation Conflict Detection

↓

Delivery Strategy Builder

↓

Delivery Strategy Evaluator

↓

Delivery Strategy Ranking

↓

Delivery Strategy Explanation

Status

Completed

---

### WP-004 — Backend Hardening

Objectives

- Exception Handling
- Logging
- Validation
- Configuration Review
- OpenAPI
- Health Checks
- Performance Review

Status

Completed

---

### WP-005 — Frontend Readiness

Objectives

Prepare backend consumption by the future frontend.

Status

Completed

---

## Additional Deliverables

During Sprint 9 the program also completed:

- Documentation Reconciliation
- Program Audit (Audit-01 through Audit-14)
- Architecture Reconciliation
- Baseline Reconciliation
- Decision Log Reconciliation
- AI Factory separation
- Governance consolidation

---

## Outcome

AgencyOS MVP successfully validated.

Achievements

- Backend validated
- Documentation reconciled
- Architecture consolidated
- Governance established
- Baseline finalized
- AI Factory established as an independent engineering program

The AgencyOS MVP is considered technically ready for frontend development.

---

# Program Summary

Completed Phases

1 Phase

Completed Sprints

10 execution stages
(Phase 1 + Sprint 0–9)

Business Domains

Commercial

Operations

Planning

Decision

Engineering Status

Backend

Completed

Database

Completed

Planning Engines

Completed

Decision Engine

Completed

Documentation

Reconciled

Governance

Established

Frontend

Pending

Decision Intelligence

Planned

---

# Next Program Milestone

Release 1.1 — Frontend MVP / Administrative Configuration

## Release 1.1 Progress

### US-101 — Configure Company Working Calendar

Status

Completed

Deliverables

- Working Calendar domain aggregate and business rules (BR-101..BR-109)
- Supabase migration `working_calendar`
- Application service, validators, DTOs, repository
- REST API `/working-calendars` with Swagger documentation
- Frontend administrative UI (list, create, edit, activate, deactivate, delete)
- Unit, validation, repository, and API tests

Outcome

Company Working Calendar configuration is available as the operational calendar foundation for Capacity Planning.

### US-102 — Manage Holidays

Status

Completed

Deliverables

- Holiday domain aggregate and business rules (BR-201..BR-213)
- Supabase migration `holiday`
- Application service, validators, DTOs, repository
- REST API `/holidays` with Swagger documentation
- Working Calendar integration (`operational-day`, calendar holidays)
- Frontend administrative UI (list, detail, create, edit, activate, deactivate, delete, search/filter)
- Domain, application, validator, repository, and API tests

Outcome

Holidays are the official configurable source of non-working days for Planning, integrated with Working Calendar evaluation.

### US-103 — Configure Standard Working Hours

Status

Completed

Deliverables

- Working Hours aggregate with weekday schedules (BR-301..BR-309)
- Supabase migration `working_hours` / `working_hours_day`
- Application service, validators, DTOs, repository
- REST API `/working-hours` with Swagger documentation
- Working Calendar operational-day enrichment with schedule and planned net hours
- Frontend administrative UI (list, detail, create, edit, weekday editor, activate, deactivate, delete)
- Domain, validator, service, repository, and API tests

Outcome

Standard Working Hours can be configured per Working Calendar and prepared for Capacity Engine consumption.

### US-104 — Configure Resource Availability

Status

Completed

Deliverables

- Resource Availability aggregate linking Execution Resource, Working Calendar, and Working Hours (BR-401..BR-410)
- Supabase migration `resource_availability` / `resource_availability_week_day` / `resource_availability_day_override`
- Application service, validators, DTOs, repository
- REST API `/resource-availabilities` with Swagger documentation
- Operational endpoint combining calendar, holidays, hours, weekly flags, and daily overrides
- Frontend administrative UI (list, detail, create, edit, weekly editor, daily overrides, activate, deactivate, delete)
- Domain, validator, service, repository, and API tests

Outcome

Resource Availability configurations are ready for Capacity Engine consumption without hardcoded Monday–Friday assumptions.

### US-105 — Calculate Capacity Using Calendar

Status

Completed

Deliverables

- Capacity Engine refactor to consume Active Working Calendar, Holidays, Working Hours, and Resource Availability (BR-501..BR-509)
- Planned capacity as sum of operational-day planned net hours (no Monday–Friday / weekly-proration fallback)
- Extended `CapacityResponse` with operational-day breakdown, holiday impact, RA exclusions, and configured working hours
- Availability Engine and Allocation Conflict Detection updated to use capacity operational days
- Existing REST `/capacity` endpoints and Swagger updated (no duplicate APIs)
- Frontend Capacity Planning view (summary, planned capacity, operational days, holiday/RA impact)
- Unit, service, and API tests replacing hardcoded calendar assumptions

Outcome

Capacity Planning is driven exclusively by operational configuration and fails with a business-rule error when configuration is missing.

### US-106 — Historical Capacity Analysis

Status

Completed

Deliverables

- Immutable `CapacityHistory` aggregate (BR-601..BR-609); Create-only (no update/delete)
- Database table `capacity_history` with indexes for resource, company, period, and version queries
- Automatic history persistence after every successful Capacity Engine calculation (`GetAll` / `GetByResourceId`)
- Failed calculations never generate history; summary calculation does not persist history
- History query, detail, resource, company, compare, and aggregate APIs under `/capacity/history`
- Frontend Capacity History list, detail, and comparison views with filters
- Domain, repository, service, validator, API, and Capacity Engine integration tests

Outcome

Completed Capacity calculations are retained as immutable historical snapshots that can be queried, aggregated, and compared without recalculating past periods.

### US-107 — Historical Workload Analysis

Status

Completed

Deliverables

- Immutable `WorkloadHistory` aggregate (BR-701..BR-709); Create-only (no update/delete)
- Database table `workload_history` with indexes for resource, company, period, and version queries
- Automatic history persistence after every successful Workload Engine calculation (`GetAll` / `GetByResourceId`)
- Failed calculations never generate history; summary calculation does not persist history
- History query, detail, resource, company, compare, aggregate, and trends APIs under `/workload/history`
- Frontend Workload History list, detail, comparison, and aggregation views with filters
- Domain, repository, service, validator, API, and Workload Engine integration tests

Outcome

Completed Workload calculations are retained as immutable historical snapshots that can be queried, aggregated, compared, and trended without recalculating past periods.

### US-108 — Planning Templates

Status

Completed

Deliverables

- `PlanningTemplate` aggregate (BR-801..BR-810) with Active/Inactive lifecycle
- Database table `planning_template` referencing Working Calendar and Working Hours (no operational data duplication)
- Unique template name per company; activate requires Active calendar/hours
- Clone (configuration only) and Apply (new planning configuration without mutating template)
- REST API under `/planning-templates` including filter/activate/deactivate/clone/apply
- Frontend list, detail, create/edit, clone, and apply views
- Domain, repository, service, validator, API, clone, and apply tests

Outcome

Organizations can define reusable planning configurations that reference existing calendars, hours, and availability strategies, then apply them to prepare planning windows without altering templates or historical Capacity/Workload records.

### US-201 — Recommendation Approval Workflow

Status

Completed

Deliverables

- `RecommendationWorkflow` aggregate with Draft → PendingApproval → Approved/Rejected lifecycle (BR-1001..BR-1010)
- Append-only `recommendation_workflow_transition` history (immutable timeline)
- Cancel before approval; Reopen from Rejected only; Approved is immutable
- REST API under `/recommendations/workflow` (create, submit, approve, reject, cancel, reopen, timeline)
- Frontend list, detail, create, approval, rejection, timeline, and status badges
- Domain transition, repository, service, validator, and API tests
- Decision Engine generation unchanged — workflow is governance after a recommendation exists

Outcome

Generated Decision Engine recommendations can be governed through a controlled approval workflow with a complete, immutable transition history.

### US-202 — Recommendation Persistence

Status

Completed

Deliverables

- `Recommendation` aggregate with Create / Archive / Restore / CreateNewVersion (BR-1101..BR-1110)
- Immutable persisted snapshots with versioning; delete prohibited
- Automatic persistence on Decision Engine ranking; publication aborted on persistence failure
- Workflow consumes persisted `RecommendationId`
- REST API under `/recommendations` (list, filter, create, archive, restore, company/mission/contract, versions)
- Frontend list, detail, search/filters, archive/restore, and version viewer
- Domain, repository, service, validator, persistence, version, archive, and API tests

Outcome

Every ranked Decision Engine recommendation is stored as a first-class, immutable, versioned domain object that Workflow and future Decision Intelligence stories consume.

### US-203 — Recommendation History

Status

Completed

Deliverables

- `RecommendationHistory` aggregate (create-only; no update/delete) — BR-1201..BR-1206
- Append-only history for recommendation versions, archive/restore, and workflow transitions
- Snapshots preserve payload, capacity/workload, planning template, decision engine version, approval info
- REST API under `/recommendations/history` (list, filter, by id, versions, timeline)
- Frontend history list, detail/snapshot viewer, timeline (recommendation + workflow), search/filters
- Domain, repository, service, validator, and API tests

Outcome

Every persisted Recommendation has an immutable, queryable evolution history with complete version and workflow timelines.

### US-204 — Recommendation Comparison

Status

Completed

Deliverables

- Read-only `RecommendationComparison` domain model over Recommendation History snapshots (BR-1301..BR-1306)
- No duplicated recommendation storage — comparisons are computed projections
- Diffs for metadata, capacity/workload snapshots, payload, delivery strategy, workflow/approval, score/rank
- REST API under `/recommendations/compare` (query, by ids, by recommendation number versions)
- Frontend comparison screen with highlighted diffs, section viewers, and JSON export
- Domain, repository, service, validator, and API tests

Outcome

Operators can compare recommendation versions and snapshots side-by-side with highlighted differences without mutating Recommendations.

### US-205 — Decision Tracking

Status

Completed

Deliverables

- `Decision` aggregate with Create / StartImplementation / Complete / Cancel / RecordOutcome / Validate (BR-1401..BR-1407)
- Append-only `decision_timeline`; delete prohibited; one Decision per Approved Recommendation
- DecisionStatus and ImplementationStatus independent fields
- REST API under `/decisions` (list, filter, detail, timeline, lifecycle actions, outcome)
- Frontend list, create, detail (progress, timeline, outcome), search/filters
- Domain, repository, service, validator, timeline, and API tests

Outcome

Approved Recommendations can be tracked through implementation to completion and outcome without changing Recommendation behavior.

### US-206 — Decision Audit Trail

Status

Completed

Deliverables

- Immutable create-only `AuditEvent` aggregate and `audit_event` table (BR-1501..BR-1510)
- Automatic non-blocking audit hooks across Recommendation, Workflow, Decision, Planning Template, Portfolio, Capacity, Workload
- Correlation middleware (`X-Correlation-Id` / request id) for cross-request tracing (BR-1508)
- Read-only REST API under `/audit` (list, filter, by id/entity/correlation/user/company)
- Frontend audit list, detail (JSON state viewer), entity history, correlation view, filters/search
- Domain, repository, service, hook, concurrency, and API tests

Outcome

Every governed business decision and recommendation lifecycle action is automatically audited. **EPIC-02 — Decision Evolution is complete (US-201–US-206).**

### US-301 — AI-assisted Recommendation

Status

Completed

Deliverables

- Immutable advisory `AIRecommendation` aggregate and `ai_recommendation` table (BR-1601..BR-1610)
- Deterministic generation service (no external LLM); confidence, assumptions, risks, alternatives persisted
- Comparison of AI suggestion vs originating Recommendation; Archive only (no Update/Delete)
- Automatic non-blocking Audit Events on Generate/Archive
- REST API under `/ai-recommendations` (list, by id, by recommendation, generate, archive, compare)
- Frontend list, generate, detail (reasoning/confidence/alternatives/comparison)
- Domain, repository, service, validator, generation, comparison, audit, and API tests

Outcome

Planners can request AI advisory alternatives without replacing Recommendations or bypassing human approval. **EPIC-03 — AI Decision Support begins with US-301.**

### US-302 — LLM Explainability

Status

Completed

Deliverables

- Immutable informational `Explainability` aggregate and `recommendation_explainability` table (BR-1701..BR-1710)
- Deterministic generation for Recommendation and AI Recommendation explanations
- Executive summary, detailed explanation, decision factors, assumptions, risks, confidence/capacity/workload explanations
- Automatic non-blocking Audit Events on Generate/Archive
- REST API under `/explainability` (list, by id, by recommendation, generate, archive)
- Frontend list, generate, detail (summary, factors, confidence visualization)
- Domain, repository, service, validator, generation, audit, and API tests

Outcome

Users can request transparent natural-language explanations without changing Recommendation or Decision Engine behavior.

### US-303 — Executive Recommendation Summary

Status

Completed

Deliverables

- Immutable informational `ExecutiveRecommendationSummary` aggregate and `executive_recommendation_summary` table (BR-1801..BR-1810)
- Deterministic generation consolidating Recommendation, AI Recommendation, Explainability, and Decision context
- Generate, CreateNewVersion (regenerate), Archive, Compare; archived summaries remain queryable
- Automatic non-blocking Audit Events on Generate / CreateNewVersion / Archive
- REST API under `/executive-summaries` (list, by id, by recommendation, generate, archive, versions, compare)
- Frontend list, generate, detail (briefing, impacts, actions, confidence, comparison)
- Domain, repository, service, validator, generation, version, audit, and API tests

Outcome

Executives can obtain concise, versioned briefings without changing Recommendations, AI Recommendations, or Decisions. **EPIC-03 — AI Decision Support is complete (US-301–US-303).**

### US-109 — Portfolio Planning

Status

Completed

Deliverables

- `Portfolio` aggregate with missions, optional Planning Template, Activate/Deactivate (BR-901..BR-912)
- Portfolio Capacity/Workload via existing engines; historical aggregates for analysis
- Deterministic Portfolio Health (Underutilized / Healthy / AtRisk / Overloaded)
- REST API under `/portfolios` (CRUD, missions, template, summary, health, calculate)
- Frontend list, create, detail with summary/health/mission association
- Domain, repository, service, validator, calculation, and API tests

Outcome

Planners can consolidate multiple Active Missions into a Portfolio and analyze capacity/workload without modifying Mission planning. **EPIC-01 — Advanced Planning is complete (US-101–US-109).**

### US-401 — Company Decision Profiles

Status

Completed

Deliverables

- Versioned `CompanyDecisionProfile` aggregate and `company_decision_profile` table (BR-1901..BR-1910), superseding the ADR-007 configuration-only MVP model
- `ICompanyDecisionProfileRepository` (EF Core) and `ICompanyDecisionProfileService` with Create, Update (new version + deactivate previous), Clone, Activate, Deactivate, Archive, SetDefault, ClearDefault
- One default Active profile per company enforced by service rules and a partial unique database index (BR-1901); unique Active Name per company (BR-1902); only Active profiles usable for ranking (BR-1904)
- Seed data for the default company preserving the original six profiles (Balanced Strategy, Profit Maximization, Delivery Speed, Operational Stability, AI Adoption, Human Resource Optimization), with Balanced Strategy as the seeded default
- Delivery Strategy Ranking, `AIRecommendation`, `Explainability`, `ExecutiveRecommendationSummary`, and `RecommendationHistory` now record the `CompanyDecisionProfileId`/`Version` used at generation time
- Automatic non-blocking Audit Events on Create / Update / Clone / Activate / Deactivate / Archive / SetDefault / ClearDefault
- REST API under `/decision-profiles` (list, filter, by id, by company, default, create, update, clone, activate, deactivate, archive, set-default, clear-default) with Swagger documentation
- Frontend list, create/edit with dimension weight editor, detail with clone/set-default/activate/deactivate/archive
- Domain, service, repository, validator, and API tests

Outcome

Companies can self-manage ranking preferences at runtime, with full versioning and audit traceability, without code deployment. **ADR-007 evolves from configuration-only to database-backed per DEC-401-001.**

### US-402 — Multi-Company Configuration

Status

Completed

Deliverables

- `Company` aggregate and `company` table (BR-2001..BR-2010): CompanyCode/CompanyName uniqueness, Active/Inactive/Archived lifecycle, DecisionProfileId/DefaultPlanningTemplateId/DefaultCalendarId assignment, planning configuration blob
- `ICompanyRepository` (EF Core) and `ICompanyService` with Create, Update, Activate, Deactivate, Archive; Archived Companies excluded by default and queryable via IncludeArchived (BR-2009)
- Update validates DecisionProfileId references the company's Active default Company Decision Profile (BR-2007) and DefaultPlanningTemplateId references a Planning Template owned by the company (BR-2008)
- `CompanyDecisionProfileService.SetDefaultAsync` keeps `Company.DecisionProfileId` in sync with the newly designated default profile (BR-2007)
- Scoped `ICompanyContext`/`CompanyContext` and `ICompanyContextService`/`CompanyContextService`: SelectAsync validates the Company may be selected (BR-2003, Active only), GetActiveAsync falls back to the seeded default Company when none is selected
- `CompanyContextMiddleware` resolves the active Company per request from the `X-Company-Id` header; requests without the header fall back to the default Company, invalid/inactive/unknown values return 400
- `company_id` added to `ai_recommendation`, `recommendation_explainability`, `executive_recommendation_summary`, and `recommendation_workflow` (with backfill) so AI-generated artifacts carry the owning Company (BR-2004); Mission/CRM aggregates are unchanged for this story and continue to be company-scoped indirectly through their existing relationships
- Automatic non-blocking Audit Events on Create / Update / Activate / Deactivate / Archive
- REST API under `/companies` (list, by id, active, create, update, activate, deactivate, archive, select) with Swagger documentation
- Frontend Company selector in the app header (`X-Company-Id` sent on every request), list/create/edit/detail pages with activate/deactivate/archive/select actions
- Domain, repository, service, context, validator, and API tests

Outcome

AgencyOS supports multiple companies as first-class tenants with independent Decision Profile and Planning Template defaults, while existing company-scoped modules keep working unchanged. **EPIC-04 — Enterprise Capabilities continues with US-403.**

### US-403 — Enterprise Dashboard

Status

Completed

Deliverables

- Read-only aggregation services — `IDashboardAggregationService`/`DashboardAggregationService`, `IDashboardTrendService`/`DashboardTrendService`, `IDashboardHealthCalculationService`/`DashboardHealthCalculationService` — that project Summary, Planning, Portfolio, Capacity, Workload, Recommendations, Decisions, AI, and Audit sections directly from existing repositories (BR-2101, BR-2102); no new analytical storage and no recalculation of Capacity/Workload history
- `IEnterpriseDashboardService`/`EnterpriseDashboardService` orchestrator: resolves CompanyId (`parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId`, BR-2104), defaults the reporting window to the trailing 30 days when unset (BR-2105), fetches all sections in parallel via `Task.WhenAll` (BR-2102), and computes period-over-period trend indicators against an immediately preceding equal-length window (BR-2106)
- Deterministic `HealthIndicator` rollups reusing `PortfolioHealth.Calculate`/`Canonicalize` for Portfolio and Capacity/Workload utilization bands (Unknown/Underutilized/Healthy/AtRisk/Overloaded) (BR-2107)
- `EnterpriseDashboardQueryParametersValidator` (From ≤ To, PeriodStart ≤ PeriodEnd when both set)
- REST API under `/enterprise-dashboard` (full dashboard plus `/summary`, `/planning`, `/portfolio`, `/capacity`, `/workload`, `/recommendations`, `/decisions`, `/ai`, `/audit` sections) with Swagger documentation
- Frontend `EnterpriseDashboardPage` as the new default landing route: KPI cards, health chips, trend indicators, utilization/workload progress bars, status breakdowns, and drill-down links into Portfolios, Planning Templates, Capacity/Workload History, Recommendations, Decisions, AI Recommendations, and Audit; "Dashboard" is the first navigation item
- Aggregation, trend, health, service, HTTP, and aggregation-performance smoke tests

Outcome

Executives and planners get a single read-only rollup of planning, capacity, workload, recommendation, decision, AI, and audit activity across a Company and reporting period, with no duplicated analytics and no impact on existing calculation history. **EPIC-04 — Enterprise Capabilities continues with US-404.**

### US-404 — Portfolio Analytics

Status

Completed

Deliverables

- Read-only Portfolio Analytics services (US-404 / BR-2201..BR-2210) — `IPortfolioAnalyticsService`/`PortfolioAnalyticsService` (orchestration), `IPortfolioTrendAnalysisService`/`PortfolioTrendAnalysisService` (month-bucketed Capacity/Workload/Health/Recommendation/Decision trends), `IPortfolioComparisonService`/`PortfolioComparisonService` (side-by-side two-Portfolio comparison), `IPortfolioHealthAnalyticsService`/`PortfolioHealthAnalyticsService` (health distribution and risk rollup), `IPortfolioRiskAnalyticsService`/`PortfolioRiskAnalyticsService` (score/cancellation risk flags) — none of which introduce new persistence, mutate a Portfolio, or recalculate the Capacity/Workload engines
- `PortfolioAnalyticsSnapshotBuilder`: shared read-only projection that parses `Portfolio.CapacitySummary`/`WorkloadSummary` JSON (tolerant parsing, BR-2204), never re-derives utilization/workload, and filters company-wide Recommendation/Decision sets down to a Portfolio's own Missions
- Company resolution consistent with US-403 (`parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId`, BR-2207) and reuse of `IDashboardHealthCalculationService`, `HealthIndicator`, `TrendIndicator`, and `StatusCountItem` from the Enterprise Dashboard slice — no duplicated health thresholds or DTOs
- Portfolio ranking that scores Portfolios on stored health severity, snapshot utilization (penalizing both under- and over-utilization), and Decision completion effectiveness (BR-2210)
- `PortfolioAnalyticsQueryParametersValidator`/`PortfolioCompareQueryParametersValidator` (From ≤ To, PeriodStart ≤ PeriodEnd, Compare requires two distinct Portfolio ids)
- REST API under `/portfolio-analytics` (overview, `/trends`, `/compare`, `/ranking`, `/health`, `/performance`, `/{portfolioId}` detail) with Swagger documentation; named routes registered ahead of the `{portfolioId:guid}` route to avoid route shadowing
- Frontend `PortfolioAnalyticsPage`: overview table, ranking table, health distribution and risk indicators, month-bucketed trend table, and a two-Portfolio comparison panel with field-level diffs; period/company filters via `useCompany()`; drill-down links into `/portfolios/:id`; "Portfolio Analytics" navigation entry next to "Portfolios"
- Snapshot-builder, trend, comparison, health, risk, orchestration-service, validator, and HTTP tests

Outcome

Executives and planners get read-only, per-Portfolio analytics — trends, side-by-side comparison, ranking, health/risk rollups, and effectiveness metrics — built entirely from existing Portfolio snapshot fields and Recommendation/Decision/Capacity/Workload history, with zero impact on Portfolio Planning calculations. **EPIC-04 — Enterprise Capabilities continues with US-405.**

### US-405 — Cross-Portfolio Planning

Status

Completed

Deliverables

- Read-only, advisory Cross-Portfolio Planning services (US-405 / BR-2301..BR-2310) — `ICrossPortfolioPlanningService`/`CrossPortfolioPlanningService` (orchestration: overview, scenarios, conflicts, balance, simulate, compare), `IEnterpriseCapacityService`/`EnterpriseCapacityService` and `IEnterpriseWorkloadService`/`EnterpriseWorkloadService` (aggregate Portfolio `CapacitySummary`/`WorkloadSummary` snapshots, optionally complemented by a live `ICapacityCalculatorService`/`IWorkloadCalculatorService` aggregate for the planning period), `ICrossPortfolioConflictDetectionService`/`CrossPortfolioConflictDetectionService` (Portfolio Mission overlap plus delegated `IAllocationConflictDetectionService` resource conflicts), `ICrossPortfolioBalancingService`/`CrossPortfolioBalancingService` (advisory-only rebalancing suggestions, Mission priorities preserved and never reordered), `ICrossPortfolioScenarioComparisonService`/`CrossPortfolioScenarioComparisonService` — none of which ever call `UpdateAsync`/`AddAsync`/`DeleteAsync` on a Portfolio
- **DEC-405-001**: no persistent aggregate — a singleton in-memory `ICrossPortfolioScenarioStore`/`CrossPortfolioScenarioStore` backed by `ConcurrentDictionary<Guid, CrossPortfolioScenarioRecord>` (TTL and max-size eviction) holds simulated scenarios for comparison within the process lifetime only; no new database tables. The Audit trail (`AuditEntityTypes.CrossPortfolioPlan`, `AuditEventTypes.Simulated`) is the durable record of every simulation (BR-2309)
- Company resolution consistent with US-403/US-404 (`parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId`); every response carries `RequiresHumanApproval = true` and an advisory disclaimer — nothing is executed automatically (BR-2308)
- `SimulateCrossPortfolioPlanRequestValidator`/`CompareCrossPortfolioScenariosRequestValidator`/query validators (at least two distinct Portfolio ids to simulate, PeriodStart ≤ PeriodEnd, Left ≠ Right scenario to compare)
- REST API under `/cross-portfolio-planning` (overview, `/scenarios`, `/conflicts`, `/balance`, `POST /simulate`, `POST /compare`) with Swagger documentation and a custom `GuidListModelBinder` accepting `PortfolioIds` as either repeated query keys or a comma-separated value
- Frontend `CrossPortfolioPlanningPage`: Portfolio multi-select with participation table, period filters, Analyze Conflicts / View Balance / Simulate / Compare Scenarios actions, enterprise Capacity/Workload balance tables, conflict and balancing-recommendation tables, a temporary scenario list, and a persistent "Advisory only — human approval required. Simulations never modify Portfolios." banner; drill-down links into `/portfolios`, `/portfolio-analytics`, `/enterprise-dashboard`, and `/audit`; "Cross-Portfolio Planning" navigation entry next to "Portfolio Analytics"
- Enterprise-capacity, enterprise-workload, conflict-detection, balancing, scenario-comparison, scenario-store, orchestration-service, validator, and HTTP tests, including assertions that Portfolios are never mutated and that every simulation is audited

Outcome

Executives and portfolio leads get a read-only, advisory simulation across multiple Portfolios — enterprise Capacity/Workload balance, Resource/Mission conflict detection, and rebalancing suggestions — built entirely on the existing Capacity, Workload, and Allocation Conflict engines, with temporary in-memory scenarios, mandatory human approval, and a full audit trail for every simulation. Portfolios, Missions, and historical operational data are never modified. **EPIC-04 — Enterprise Capabilities is complete (US-401–US-405).**

### US-501 — My Work Dashboard

Status

Completed

Deliverables

- Read-only Personal Operational Dashboard (US-501 / BR-2401..BR-2410) — `IPersonalDashboardAggregationService`/`PersonalDashboardAggregationService` (active Assignments/Tasks/Missions, pending Recommendations/Decisions, Capacity/Workload snapshots), `IPersonalTimelineService`/`PersonalTimelineService` (Activity Timeline from Audit Events), `IPersonalKpiService`/`PersonalKpiService` (pure KPI arithmetic over aggregated counts), `IMyWorkDashboardService`/`MyWorkDashboardService` (orchestration) — none of which write to any repository
- **DEC-501-001**: caller identity resolved via an explicit fallback chain (`UserId = parameters.UserId ?? IAuditContext.UserId ?? "system"`; `CompanyId = parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId`; `ExecutionResourceId = parameters.ExecutionResourceId ?? first Active ExecutionResource with matching Code`), since AgencyOS has no full authentication/identity provider yet; the heuristic Execution Resource lookup is skipped for the `"system"` fallback user
- Assignments/Tasks/Missions/Capacity/Workload scoped by the resolved Execution Resource when present; empty collections/`HasData = false` summaries returned gracefully when unresolved or when the Capacity/Workload engines report a `NotFoundException`/`BusinessRuleException` (BR-2401)
- Pending Recommendations union company-scoped `RecommendationWorkflow` (`Status = PendingApproval`) with workflows for Recommendations the caller generated; pending Decisions select company-scoped `DecisionStatus` `Created`/`InProgress`, preferring `CreatedBy` matches without ever relaxing company isolation
- Activity Timeline built from `IAuditQueryService.GetByUserIdAsync`, filtered in-memory by CompanyId and From/To
- `MyWorkDashboardQueryParametersValidator` (From ≤ To, PeriodStart ≤ PeriodEnd)
- REST API under `/my-work` (full dashboard plus `/summary`, `/tasks`, `/missions`, `/recommendations`, `/decisions`, `/activity`, `/kpis`) with Swagger documentation; every card/summary DTO carries a `DrillDownPath` into an existing frontend route
- Frontend `MyWorkDashboardPage` ("My Work" navigation entry): User Id/Execution Resource Id override fields (persisted to `localStorage`, also sent as an `X-User-Id` header), From/To/PeriodStart/PeriodEnd filters, KPI tiles, Capacity/Workload widgets, overdue/upcoming-deadline lists, Mission/Task/Recommendation/Decision cards, Activity Timeline, and a persistent "Operational workspace — read-only. Drill-down does not modify data." banner
- Aggregation, timeline, KPI, orchestration-service, validator, and HTTP tests, including assertions that no write method is ever invoked

Outcome

Individual contributors get a single, read-only, personalized view of their assigned work, pending approvals/decisions, Capacity/Workload standing, recent activity, and personal KPIs — built entirely from existing Assignment/Task/Mission/Recommendation/Decision/Capacity/Workload/Audit data, with an explicit and documented identity-resolution fallback (DEC-501-001) in place of a full authentication provider. **EPIC-05 — Operational Workspace continues with US-502.**

### US-502 — Planning Workspace

Status

Completed

Deliverables

- Read-only Planning Workspace orchestration (US-502 / BR-2501..BR-2510) — `IPlanningOverviewService`/`PlanningOverviewService`, `IPlanningNavigationService`/`PlanningNavigationService`, `IPlanningHistoryService`/`PlanningHistoryService`, `IPlanningWorkspaceService`/`PlanningWorkspaceService` — none of which write to Portfolio/Template repositories or recalculate engines for storage
- **DEC-502-001**: Planning Workspace is a read-only orchestration façade; "Execute Capacity/Workload Planning" and calculation launches are navigation deep-links into existing audited routes/APIs; Company resolution matches US-403/501 (`parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId`)
- Sections: Overview KPIs, Templates, Capacity History, Workload History, Portfolios, Planning History (Audit projection for PlanningTemplate/Portfolio/CapacityHistory/WorkloadHistory/CrossPortfolioPlan), Cross-Portfolio scenarios (advisory, RequiresHumanApproval)
- `PlanningWorkspaceQueryParametersValidator` (From ≤ To, PeriodStart ≤ PeriodEnd)
- REST API under `/planning-workspace` (full workspace plus `/overview`, `/templates`, `/capacity`, `/workload`, `/portfolios`, `/history`, `/scenarios`) with Swagger documentation
- Frontend `PlanningWorkspacePage` ("Planning Workspace" navigation entry): period filters, KPI tiles, navigation actions, Capacity/Workload widgets, Templates/Portfolios/Scenarios/History sections, advisory banner
- Navigation, history, orchestration, validator, and HTTP tests

Outcome

Planners get a single orchestration surface over existing Planning Templates, Capacity/Workload History, Portfolios, and advisory Cross-Portfolio scenarios — without duplicating planning data or changing planning engines. **EPIC-05 — Operational Workspace continues (US-501–US-502; not yet complete).**

### US-503 — Recommendation Workspace

Status

Completed

Deliverables

- Read-only Recommendation Workspace orchestration (US-503 / BR-2601..BR-2610) — `IRecommendationOverviewService`/`RecommendationOverviewService`, `IRecommendationNavigationService`/`RecommendationNavigationService`, `IRecommendationSummaryService`/`RecommendationSummaryService`, `IRecommendationWorkspaceService`/`RecommendationWorkspaceService` — none of which write to Recommendation/Workflow/AI Recommendation/Explainability/Executive Summary repositories or bypass mandatory human approval
- **DEC-503-001**: Recommendation Workspace is a read-only orchestration façade; Generate/Approve/Reject/Archive/Restore/Start Workflow/Generate AI/Generate Explainability/Generate Executive Summary are exposed as navigation actions (deep-links) to existing frontend routes — the workspace never calls Create/Archive/Approve/etc.; Company resolution matches US-403/501/502 (`parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId`)
- Sections: Overview KPIs, Recommendations, Approval (PendingApproval Workflow cards), History (immutable Recommendation History projection), Compare (side-by-side Recommendation comparison), AI (advisory AI Recommendation and informational Explainability cards), Executive Summary
- `RecommendationWorkspaceQueryParametersValidator` (From ≤ To, PeriodStart ≤ PeriodEnd)
- REST API under `/recommendation-workspace` (full workspace plus `/overview`, `/recommendations`, `/approval`, `/history`, `/compare`, `/ai`, `/executive-summary`) with Swagger documentation
- Frontend `RecommendationWorkspacePage` ("Recommendation Workspace" navigation entry): period filters, KPI tiles, navigation actions, Recommendations/Approval/History/Compare/AI/Executive Summary sections, advisory/informational banners
- Navigation, summary, orchestration, validator, and HTTP tests; full backend suite of 1241 automated tests passing with 0 failures

Outcome

Planners and approvers get a single orchestration surface over existing Recommendation, Recommendation Workflow, AI Recommendation, Explainability, Executive Summary, Recommendation History, and Recommendation Comparison capabilities — without duplicating recommendation data, changing the Decision Engine, or bypassing human approval. **EPIC-05 — Operational Workspace continues (US-501–US-503; not yet complete).**

---

### US-504 — Decision Workspace

Status

Completed

Deliverables

- Read-only Decision Workspace orchestration (US-504 / BR-2701..BR-2710) — `IDecisionOverviewService`/`DecisionOverviewService`, `IDecisionNavigationService`/`DecisionNavigationService`, `IDecisionSummaryService`/`DecisionSummaryService`, `IDecisionWorkspaceService`/`DecisionWorkspaceService` — none of which call `CreateAsync`/`StartImplementationAsync`/`CompleteAsync`/`CancelAsync`/`RecordOutcomeAsync` on `IDecisionService` or bypass mandatory human approval
- **DEC-504-001**: Decision Workspace is a read-only orchestration façade; Create Decision/Start Implementation/Complete/Cancel/Record Outcome are exposed as navigation actions (deep-links) to existing frontend routes (`/decisions/new`, `/decisions/{id}`) — the workspace never calls a Decision lifecycle write API; Company resolution matches US-403/501/502/503 (`parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId`)
- Sections: Overview KPIs (Total/Pending/InProgress/Completed/Cancelled/WithOutcome/ImplementationNotStarted), Decisions (Pending/InProgress/Completed/Cancelled cards with Decision and Recommendation drill-downs), Timeline (full immutable Timeline for a focused DecisionId, or a bounded aggregate of recent Decisions' Timeline entries), Outcomes (Decisions with a recorded Outcome), Audit (immutable Decision Audit trail via `IAuditQueryService` filtered to `AuditEntityTypes.Decision`)
- `DecisionWorkspaceQueryParametersValidator` (From ≤ To)
- REST API under `/decision-workspace` (full workspace plus `/overview`, `/decisions`, `/timeline`, `/outcomes`, `/audit`, `/kpis`) with Swagger documentation
- Frontend `DecisionWorkspacePage` ("Decision Workspace" navigation entry near Decisions/Recommendation Workspace): period filters, optional Decision Id Timeline focus, KPI tiles, navigation actions, Pending/In Progress/Completed queues, Timeline, Outcomes, and Audit sections, human-approval banner
- Navigation, overview, summary (Timeline with/without DecisionId, Outcomes filter, Audit filter), workspace orchestration (company resolution, never calls Decision write methods), validator, and HTTP tests; full backend suite of 1281 automated tests passing with 0 failures

Outcome

Stakeholders get a single orchestration surface tracking the Decision lifecycle from an approved Recommendation through implementation, outcomes, and audit — without duplicating Decision data, changing Decision lifecycle rules, or bypassing human approval for Decision actions. **EPIC-05 — Operational Workspace continues (US-501–US-504; not yet complete).**

---

### US-505 — Executive Workspace

Status

Completed

Deliverables

- Read-only Executive Workspace orchestration (US-505 / BR-2801..BR-2810) — `IExecutiveNavigationService`/`ExecutiveNavigationService` (pure navigation), `IExecutiveKpiService`/`ExecutiveKpiService` (pure mapping), `IExecutiveAggregationService`/`ExecutiveAggregationService` (wraps `IEnterpriseDashboardService` section getters), `IExecutiveOverviewService`/`ExecutiveOverviewService`, `IExecutiveWorkspaceService`/`ExecutiveWorkspaceService` — none of which write operational data or re-aggregate directly from repositories
- **DEC-505-001**: Executive Workspace is a read-only orchestration façade that primarily reuses `IEnterpriseDashboardService` section methods; Company resolution matches US-403/501/502/503/504 (`parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId`); the query window maps onto `EnterpriseDashboardQueryParameters` (CompanyId, From, To, PeriodStart, PeriodEnd) with a default trailing 30-day window
- Sections: Overview (Executive KPIs, overall health reused from `IDashboardHealthCalculationService`, short narrative, navigation tip), Enterprise (embeds the Enterprise Dashboard Summary), Portfolios (embeds the Portfolio rollup, drills into Portfolio Analytics), Recommendations (embeds the Recommendations rollup, drills into the Recommendation Workspace), Decisions (embeds the Decisions rollup, drills into the Decision Workspace), Capacity/Workload (embed the immutable History rollups), AI (embeds the advisory AI rollup), Audit (embeds the immutable Audit rollup, drills into the Audit Trail), and Navigation (13 deep-links spanning Overview/Planning/Recommendations/Decisions/Capacity/Workload/AI/Audit/Portfolios categories, including an advisory Cross-Portfolio Planning link)
- `ExecutiveWorkspaceQueryParametersValidator` (From ≤ To, PeriodStart ≤ PeriodEnd)
- REST API under `/executive-workspace` (full workspace plus `/overview`, `/enterprise`, `/portfolios`, `/recommendations`, `/decisions`, `/capacity`, `/workload`, `/ai`, `/audit`) with Swagger documentation
- Frontend `ExecutiveWorkspacePage` ("Executive Workspace" navigation entry next to Enterprise Dashboard): period filters, KPI tiles, Executive Overview narrative/health, navigation actions, Portfolio/Recommendation/Decision/Capacity/Workload/AI/Audit summary panels with drill-down links, and a persistent read-only banner
- Navigation, KPI (pure), aggregation (mocked `IEnterpriseDashboardService`), overview, workspace orchestration (company resolution, never writes), validator, and HTTP tests; full backend suite of 1337 automated tests passing with 0 failures

Outcome

Executives get a single, read-only view of strategic KPIs, enterprise health, portfolios, recommendations, decisions, AI insights, and organizational performance — reusing the existing Enterprise Dashboard rather than duplicating analytical data, and linking straight into every existing operational workspace and dashboard for detail. **EPIC-05 — Operational Workspace continues with US-506.**

---


### US-506 — Notification Center

Sprint

Release 1.1 / EPIC-05

Status

Completed

Scope Delivered

- In-application Notification Center (US-506 / BR-2901..BR-2910) — informational only; never modifies business data
- Domain `Notification` with Create / MarkRead / MarkUnread / Archive / Validate; status Unread|Read; immutable except read/archive (BR-2908)
- Persistence: `notification` table + migration `20260727100000_create_notification_tables.sql`, EF mapping, indexes, company FK, category/priority/status/source constraints
- Application: `INotificationRepository`/`NotificationRepository`, `INotificationQueryService`/`NotificationQueryService`, `INotificationService`/`NotificationService`, `INotificationGenerationService`/`NotificationGenerationService` (GenerateSafeAsync — BR-2909), DTOs, `NotificationQueryParametersValidator`, navigation path mapping (BR-2907)
- Automatic generation from Audit Trail (mapped Decision/Recommendation Workflow/Capacity/Portfolio/Planning/Executive/Company/Recommendation events) plus direct hooks for Portfolio create and Company Context select; Notification actions themselves are audited (BR-2910) and never re-generate notifications
- REST API under `/notifications` (list, filter, detail, unread-count, read, unread, archive) with Swagger documentation; user scope via `X-User-Id`, company isolation via `X-Company-Id`
- Frontend `NotificationCenterPage` (list, detail, category/priority/status/archive filters, unread badge in nav, priority chips, Open source navigation); Vite proxy for `/notifications`
- Domain, repository, service, generation, validator, and HTTP tests

Outcome

Users get a single in-app hub for pending actions and operational events across AgencyOS modules, with safe automatic generation that never interferes with business transactions. **EPIC-05 — Operational Workspace continues with US-507.**



### US-507 — Personal Productivity Dashboard

Sprint

Release 1.1 / EPIC-05

Status

Completed

Scope Delivered

- Read-only Personal Productivity Dashboard (US-507 / BR-3001..BR-3010) — analytical, user-focused; no new persistence
- `IPersonalMetricsService`/`PersonalMetricsService` (pure KPIs, performance indicators, statistics)
- `IPersonalTrendService`/`PersonalTrendService` (period-over-period Capacity/Workload/Activity/Completed trends)
- `IActivitySummaryService`/`ActivitySummaryService` (pending/completed work + timeline projection)
- `IPersonalProductivityDashboardService`/`PersonalProductivityDashboardService` — reuses My Work aggregation/timeline/KPI services; identity via DEC-501-001; usage audited via `RecordSafeAsync` (BR-3010)
- REST API under `/personal-dashboard` (full dashboard plus `/summary`, `/kpis`, `/trends`, `/capacity`, `/workload`, `/activity`, `/statistics`) with Swagger
- Frontend `PersonalProductivityDashboardPage` ("Productivity" nav): KPI cards, capacity/workload widgets, trends, pending/completed work, activity timeline, statistics/navigation links
- Metrics, trend, activity, validator, service, and HTTP tests

Outcome

Users get a personal productivity view consolidating execution, capacity utilization, workload trends, completed/pending work, and operational metrics without changing business behavior. **This story completes EPIC-05 — Operational Workspace (US-501–US-507) and the AgencyOS Release 1.1 MVP.**


# Next Program Milestone

Frontend MVP

The next implementation cycle continues after EPIC-04. EPIC-01 Advanced Planning (US-101–US-109) is complete. **EPIC-02 Decision Evolution is complete (US-201–US-206).** **EPIC-03 AI Decision Support is complete (US-301–US-303).** **EPIC-04 Enterprise Capabilities is complete (US-401–US-405).** **EPIC-05 Operational Workspace is complete (US-501–US-507).**

Release 1.1 delivers Decision Evolution governance, AI Decision Support (advisory Recommendations, Explainability, and Executive Summaries), self-service multi-company configuration (Company Decision Profiles and Companies), a read-only cross-domain Enterprise Dashboard, read-only per-Portfolio Analytics (trends, comparison, ranking, health/risk), advisory, read-only Cross-Portfolio Planning (enterprise balance, conflict detection, temporary simulation scenarios), and the complete Operational Workspace — My Work Dashboard (US-501), Planning Workspace (US-502), Recommendation Workspace (US-503), Decision Workspace (US-504), Executive Workspace (US-505), Notification Center (US-506), and Personal Productivity Dashboard (US-507). **AgencyOS Release 1.1 MVP is complete.**


Release 1.1 — Stabilization Sprint

Status

Completed

Objectives

- Resolve post-MVP defects
- Validate application startup
- Execute Guided Test Drive
- Approve Release 1.1

Completed Work Packages

- BUG-001 — DbContext Concurrency
- BUG-002 — UTC Standardization
- BUG-003 — Notification Review
- BUG-004 — Startup Checklist
- BUG-005 — Guided Test Drive

Outcome

Release 1.1 successfully stabilized and fully validated.

The Backend is approved for continued product evolution.