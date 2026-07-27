import type {
  EnterpriseDashboardAi,
  EnterpriseDashboardAudit,
  EnterpriseDashboardCapacity,
  EnterpriseDashboardDecisions,
  EnterpriseDashboardPortfolio,
  EnterpriseDashboardRecommendations,
  EnterpriseDashboardSummary,
  EnterpriseDashboardWorkload,
  HealthIndicator,
} from './enterpriseDashboard'

export interface ExecutiveWorkspaceQuery {
  companyId?: string
  from?: string
  to?: string
  periodStart?: string
  periodEnd?: string
}

export interface ExecutiveAction {
  key: string
  label: string
  category: string
  drillDownPath: string
  description: string
  isAdvisory: boolean
}

export interface ExecutiveNavigation {
  companyId: string
  actions: ExecutiveAction[]
}

export interface ExecutiveKpiSummary {
  portfolioCount: number
  activePortfolioCount: number
  recommendationCount: number
  decisionCount: number
  pendingDecisionCount: number
  completedDecisionCount: number
  capacityUtilizationPercentage: number
  workloadPercentage: number
  auditEventCount: number
  overallHealth: HealthIndicator
}

export interface ExecutiveOverview {
  companyId: string
  generatedAt: string
  from?: string | null
  to?: string | null
  kpis: ExecutiveKpiSummary
  overallHealth: HealthIndicator
  narrative: string
  navigationTip: string
  drillDownPath: string
}

export interface ExecutiveEnterpriseSection {
  companyId: string
  summary: EnterpriseDashboardSummary
  drillDownPath: string
}

export interface ExecutivePortfoliosSection {
  companyId: string
  portfolio: EnterpriseDashboardPortfolio
  drillDownPath: string
}

export interface ExecutiveRecommendationsSection {
  companyId: string
  recommendations: EnterpriseDashboardRecommendations
  drillDownPath: string
}

export interface ExecutiveDecisionsSection {
  companyId: string
  decisions: EnterpriseDashboardDecisions
  drillDownPath: string
}

export interface ExecutiveCapacitySection {
  companyId: string
  capacity: EnterpriseDashboardCapacity
  drillDownPath: string
}

export interface ExecutiveWorkloadSection {
  companyId: string
  workload: EnterpriseDashboardWorkload
  drillDownPath: string
}

export interface ExecutiveAiSection {
  companyId: string
  ai: EnterpriseDashboardAi
  drillDownPath: string
}

export interface ExecutiveAuditSection {
  companyId: string
  audit: EnterpriseDashboardAudit
  drillDownPath: string
}

export interface ExecutiveWorkspace {
  companyId: string
  generatedAt: string
  from?: string | null
  to?: string | null
  periodStart?: string | null
  periodEnd?: string | null
  kpis: ExecutiveKpiSummary
  overview: ExecutiveOverview
  enterprise: ExecutiveEnterpriseSection
  portfolios: ExecutivePortfoliosSection
  recommendations: ExecutiveRecommendationsSection
  decisions: ExecutiveDecisionsSection
  capacity: ExecutiveCapacitySection
  workload: ExecutiveWorkloadSection
  ai: ExecutiveAiSection
  audit: ExecutiveAuditSection
  navigation: ExecutiveNavigation
}
