import type { HealthIndicator, StatusCountItem } from './enterpriseDashboard'

export interface PlanningWorkspaceQuery {
  companyId?: string
  from?: string
  to?: string
  periodStart?: string
  periodEnd?: string
}

export interface PlanningAction {
  key: string
  label: string
  category: string
  drillDownPath: string
  description: string
  isAdvisory: boolean
}

export interface PlanningKpiSummary {
  templateCount: number
  activeTemplateCount: number
  portfolioCount: number
  activePortfolioCount: number
  scenarioCount: number
  planningHistoryEventCount: number
  averageUtilizationPercentage: number
  averageWorkloadPercentage: number
  overallHealth: HealthIndicator
}

export interface PlanningTemplateCard {
  id: string
  name: string
  status: string
  isActive: boolean
  resourceAvailabilityStrategy: string
  defaultPlanningWindowDays: number
  drillDownPath: string
}

export interface PlanningPortfolioCard {
  id: string
  name: string
  status: string
  portfolioHealth: string
  planningPeriodStart: string
  planningPeriodEnd: string
  utilizationPercentage?: number | null
  workloadPercentage?: number | null
  planningTemplateId?: string | null
  drillDownPath: string
}

export interface PlanningHistoryItem {
  id: string
  entityType: string
  entityId: string
  eventType: string
  action: string
  occurredAt: string
  userId: string
  drillDownPath: string
}

export interface PlanningScenarioSummary {
  scenarioId: string
  scenarioName?: string | null
  createdAt: string
  portfolioCount: number
  averageUtilizationPercentage?: number | null
  averageWorkloadPercentage?: number | null
  conflictCount: number
  requiresHumanApproval: boolean
  advisoryOnly: boolean
}

export interface PlanningCapacitySection {
  companyId: string
  periodStart: string
  periodEnd: string
  recordCount: number
  totalCapacityHours: number
  totalAllocatedHours: number
  averageUtilizationPercentage: number
  utilizationHealth: HealthIndicator
  action: PlanningAction
}

export interface PlanningWorkloadSection {
  companyId: string
  periodStart: string
  periodEnd: string
  recordCount: number
  totalAllocatedHours: number
  totalCapacityHours: number
  averageWorkloadPercentage: number
  workloadHealth: HealthIndicator
  action: PlanningAction
}

export interface PlanningOverview {
  companyId: string
  generatedAt: string
  from?: string | null
  to?: string | null
  periodStart?: string | null
  periodEnd?: string | null
  kpis: PlanningKpiSummary
  inactiveTemplateCount: number
  portfolioHealthBreakdown: StatusCountItem[]
  capacityHistoryRecordCount: number
  workloadHistoryRecordCount: number
  crossPortfolioAdvisoryDisclaimer: string
  drillDownPath: string
}

export interface PlanningWorkspace {
  companyId: string
  generatedAt: string
  from?: string | null
  to?: string | null
  periodStart?: string | null
  periodEnd?: string | null
  kpis: PlanningKpiSummary
  overview: PlanningOverview
  templates: {
    companyId: string
    templates: PlanningTemplateCard[]
    manageAction: PlanningAction
    applyAction: PlanningAction
  }
  capacity: PlanningCapacitySection
  workload: PlanningWorkloadSection
  portfolios: {
    companyId: string
    portfolios: PlanningPortfolioCard[]
    action: PlanningAction
  }
  history: {
    companyId: string
    from?: string | null
    to?: string | null
    items: PlanningHistoryItem[]
    action: PlanningAction
  }
  scenarios: {
    companyId: string
    generatedAt: string
    scenarios: PlanningScenarioSummary[]
    requiresHumanApproval: boolean
    advisoryOnly: boolean
    advisoryDisclaimer: string
    comparisonAction: PlanningAction
  }
  navigation: {
    companyId: string
    actions: PlanningAction[]
  }
}
