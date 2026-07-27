# AgencyOS

> AI Native Operating System for Creative Agencies

## Status

**Version:** v1.1.0-dev

**Release:** Release 1.1 In Progress

**Branch:** develop

**State:** Active development

---

## Visão

AgencyOS é uma plataforma AI Native para gestão operacional de empresas de serviços, iniciando pelo mercado de produção audiovisual.

O sistema integra processos comerciais, planejamento operacional, capacidade produtiva, recursos de execução, analytics e um Decision Engine capaz de gerar, avaliar, ranquear e explicar estratégias de entrega.

---

## Módulos do MVP

### Commercial

- Lead Management
- Client Management
- Contact Management
- Contract Management

### Operations

- Missions
- Tasks
- Execution Resources
- Assignments
- Working Calendar (US-101)
- Holidays (US-102)
- Working Hours (US-103)
- Resource Availability (US-104)
- Capacity History (US-106)
- Workload History (US-107)
- Planning Templates (US-108)
- Portfolio Planning (US-109) — completes EPIC-01 Advanced Planning
- Recommendation Approval Workflow (US-201)
- Recommendation Persistence (US-202)
- Recommendation History (US-203)
- Recommendation Comparison (US-204)
- Decision Tracking (US-205)
- Decision Audit Trail (US-206) — completes EPIC-02 Decision Evolution
- AI-assisted Recommendation (US-301) — starts EPIC-03 AI Decision Support
- LLM Explainability (US-302)
- Executive Recommendation Summary (US-303) — completes EPIC-03 AI Decision Support

### Analytics

- Capacity Planning (US-105 — calendar-driven)
- Historical Capacity Analysis (US-106)
- Historical Workload Analysis (US-107)
- Workload Analysis
- Availability Analysis
- Allocation Conflict Detection

### Decision Engine

- Delivery Strategy Builder
- Strategy Evaluation
- Strategy Ranking
- Explainability
- Company Decision Profiles (US-401 — versioned, database-backed ranking preferences under `/decision-profiles`)

### Configuration

- Multi-Company Configuration (US-402 — `Company` as a first-class tenant aggregate under `/companies`; Active/Inactive/Archived lifecycle, per-request company selection via the `X-Company-Id` header)

### Enterprise

- Enterprise Dashboard (US-403 — read-only, cross-domain rollup under `/enterprise-dashboard` covering Summary, Planning, Portfolio, Capacity, Workload, Recommendations, Decisions, AI, and Audit; company + period filters, health rollups, and period-over-period trends sourced entirely from existing repositories, with no duplicated analytical storage)
- Portfolio Analytics (US-404 — read-only, per-Portfolio analytics under `/portfolio-analytics`: overview, month-bucketed trends, side-by-side comparison, ranking, health/risk distribution, and effectiveness metrics; sourced entirely from Portfolio snapshot fields plus existing Recommendation/Decision/Capacity/Workload history, with no new persistence or recalculation)
- Cross-Portfolio Planning (US-405 — read-only, advisory simulation under `/cross-portfolio-planning`: enterprise Capacity/Workload balance, Portfolio Mission-overlap and Resource conflict detection, and advisory rebalancing recommendations built on the existing Capacity, Workload, and Allocation Conflict engines; scenarios are temporary in-memory records (DEC-405-001), every simulation is audited, and every response requires human approval — Portfolios are never modified) — completes EPIC-04 Enterprise Capabilities (US-401–US-405)

### Operational Workspace

- My Work Dashboard (US-501 — read-only, personalized operational dashboard under `/my-work`: the caller's active Assignments/Tasks/Missions, pending Recommendations/Decisions, Capacity/Workload standing, Activity Timeline, and personal KPIs; caller identity is resolved via an explicit UserId/CompanyId/ExecutionResourceId fallback chain in the absence of a full authentication provider (DEC-501-001); no new persistence and no business calculations are modified) — starts EPIC-05 Operational Workspace
- Planning Workspace (US-502 — read-only orchestration façade under `/planning-workspace` unifying Planning Templates, Capacity/Workload History, Portfolios, advisory Cross-Portfolio scenarios, and Planning History; calculation launches are navigation deep-links only (DEC-502-001); no duplicate planning data and no engine changes) — continues EPIC-05
- Recommendation Workspace (US-503 — read-only orchestration façade under `/recommendation-workspace` unifying Recommendations, Approval Queue, History, Comparison, AI Recommendations, Explainability, and Executive Summaries; Generate/Approve/Reject/Archive/Restore are navigation deep-links only (DEC-503-001); Decision Engine and Recommendation lifecycle unchanged; human approval mandatory; AI advisory; Explainability informational) — continues EPIC-05
- Decision Workspace (US-504 — read-only orchestration façade under `/decision-workspace` tracking the Decision lifecycle from an approved Recommendation through implementation, outcomes, and audit: Overview KPIs, Pending/In Progress/Completed/Cancelled queues, immutable Timeline (full for a focused Decision, or aggregated across recent Decisions), Outcomes, and Audit; Create Decision/Start Implementation/Complete/Cancel/Record Outcome are navigation deep-links only (DEC-504-001); Decision lifecycle and Recommendation linkage unchanged; no duplicate decision data) — continues EPIC-05
- Executive Workspace (US-505 — read-only orchestration façade under `/executive-workspace` giving executives a single view of strategic KPIs, enterprise health, portfolios, recommendations, decisions, AI insights, and organizational performance; primarily reuses `IEnterpriseDashboardService` section methods rather than re-aggregating from repositories (DEC-505-001), reuses existing health calculations, and adds navigation deep-links into every existing operational workspace and dashboard; no new persistence, no business logic changes) — EPIC-05 continues with US-506
- Notification Center (US-506 — centralized in-app notification hub under `/notifications`: user-specific, company-isolated notifications with category/priority/status filters, mark read/unread/archive, unread badge, and navigation to originating entities; automatically generated from Audit Trail and key module events via `GenerateSafeAsync` so generation failures never affect business transactions (DEC-506-001); informational only; no email/SMS/push/Teams/Slack/webhooks) — EPIC-05 continues with US-507
- Personal Productivity Dashboard (US-507 — read-only analytical dashboard under `/personal-dashboard`: personal KPIs, period-over-period trends, capacity/workload utilization, pending/completed work, activity timeline, and operational statistics; reuses My Work aggregation/timeline with DEC-501-001 identity; usage audited (DEC-507-001); no new persistence and no operational behavior changes) — completes EPIC-05 Operational Workspace (US-501–US-507) and AgencyOS Release 1.1 MVP

---

## Arquitetura

- Clean Architecture
- Domain Driven Design (DDD)
- REST API
- Decision Intelligence
- AI Native Ready

---

## Tecnologias

### Backend

- .NET 8
- ASP.NET Core
- Entity Framework Core

### Database

- PostgreSQL
- Supabase

### Frontend

- React 18
- TypeScript
- Vite
- MUI

### Infrastructure

- Docker

---

## Estrutura

```text
agencyos/

backend/
frontend/
database/
docs/
supabase/
scripts/
prompts/
infrastructure/
```

---

## Executando o projeto

### Backend

```bash
cd backend/src
dotnet run --project AgencyOS.Api
```

### Frontend (Release 1.1 — Working Calendar admin)

```bash
cd frontend
npm install
npm run dev
```

Vite proxies `/working-calendars`, `/my-work`, and the other backend routes to `http://localhost:5115`.

### Testes

```bash
cd backend/src
dotnet test
```

### Swagger

```
http://localhost:5115/swagger
```

---

## Resultado do MVP

- Commercial Workflow
- Operational Workflow
- Analytics Engine
- Decision Engine
- Explainability

Todos os fluxos do MVP foram homologados.

72 testes automatizados aprovados.

---

## Licença

Privado – AgencyOS