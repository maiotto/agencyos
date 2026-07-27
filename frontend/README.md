# AgencyOS Frontend

Release 1.1 administrative UI for Working Calendar (US-101), Holidays (US-102), Working Hours (US-103), Resource Availability (US-104), Capacity Planning (US-105), Capacity History (US-106), Workload History (US-107), Planning Templates (US-108), Portfolio Planning (US-109), Recommendation Approval Workflow (US-201), Recommendation Persistence (US-202), Recommendation History (US-203), Recommendation Comparison (US-204), Decision Tracking (US-205), Decision Audit Trail (US-206), AI-assisted Recommendation (US-301), LLM Explainability (US-302), Executive Recommendation Summary (US-303), Company Decision Profiles (US-401), and Multi-Company Configuration (US-402).

## Stack

- React 18
- TypeScript
- Vite
- MUI
- React Router
- React Hook Form
- Zod
- Axios

## Setup

```bash
cd frontend
npm install
npm run dev
```

The Vite dev server proxies `/working-calendars`, `/holidays`, `/working-hours`, `/resource-availabilities`, `/capacity`, `/workload`, `/planning-templates`, `/portfolios`, `/recommendations`, `/decisions`, `/decision-profiles`, `/audit`, `/ai-recommendations`, `/explainability`, `/executive-summaries`, `/execution-resources`, and `/companies` to `http://localhost:5115`.

Every request sent through `apiClient` (`src/api/client.ts`) carries an `X-Company-Id` header with the currently active company id (persisted in `localStorage` under `agencyos.activeCompanyId`, defaulting to `DEFAULT_COMPANY_ID`). The active company can be switched from the selector in the app header or from the Companies screens (US-402).

## Screens

### Working Calendars

- List / Create / Edit

### Holidays

- List / Detail / Create / Edit

### Working Hours

- List / Detail / Create / Edit with weekday schedule editor

### Resource Availability

- List / Detail / Create / Edit with weekly availability flags and daily overrides

### Capacity

- Live Capacity Planning calculation (US-105)

### Capacity History

- History list with filters (company, resource, period, version)
- History detail with daily operational snapshot
- History comparison view

### Workload History

- History list with filters, trend strip, and aggregation chips
- History detail with assignment distribution snapshot
- History comparison view
- Dedicated aggregation view

### Planning Templates

- List / Detail / Create / Edit
- Clone / Apply / Activate / Deactivate / Delete
- Search and status filters

### Portfolios

- List with search / status filters
- Create with mission association
- Detail with summary, health, capacity/workload calculate, template assignment, mission add/remove
- Activate / Deactivate / Delete (inactive only)

### Recommendations

- List with search / filters / include-archived
- Detail with summary, payload viewer, archive/restore
- Version viewer across recommendation number lineage
- Start Approval Workflow from a persisted recommendation
- Ask AI (generate advisory AI Recommendation)
- Explain (generate LLM Explainability)
- Exec Summary (generate Executive Recommendation Summary)

### AI Recommendations

- List with status / search filters and confidence indicator
- Generate from Recommendation Id
- Detail with executive summary, reasoning, assumptions, risks, alternatives
- Comparison vs current Recommendation
- Archive (advisory record only; does not change Recommendation)
- Explain (generate LLM Explainability for the AI Recommendation)

### Explainability

- List with status / type / search filters
- Generate for Recommendation or AI Recommendation
- Detail with executive summary, detailed explanation, decision factors
- Assumptions, risks, confidence visualization, capacity/workload explanations
- Archive (informational only; does not change Recommendations)

### Executive Summaries

- List with status / search / include-archived (BR-1810)
- Generate from Recommendation (optional AI / Explainability ids)
- Detail briefing with business/capacity/workload impact, recommended actions
- Confidence indicator and comparison vs Recommendation
- Regenerate (new immutable version) and Archive

### Recommendation Workflow

- List / Detail / Create Draft from Recommendation Id
- Approve / Reject screens
- Timeline and status badges
- Submit / Cancel / Reopen actions

### Decision Profiles

- List with name / status filters
- Create / Edit (new immutable version) with dimension weight editor
- Detail with clone, set-default, activate/deactivate/archive

### Companies

- List with search / status filters and include-archived (BR-2009)
- Create / Edit
- Detail with activate/deactivate/archive and "Select as active company"
- Company selector in the app header; selection persists in `localStorage` and is sent as `X-Company-Id` on every API request
