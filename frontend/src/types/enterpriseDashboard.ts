export interface EnterpriseDashboardQuery {
  companyId?: string
  from?: string
  to?: string
  periodStart?: string
  periodEnd?: string
}

export interface StatusCountItem {
  status: string
  count: number
}

export interface HealthIndicator {
  status: string
  label: string
  detail?: string | null
}

export interface TrendIndicator {
  direction: 'Up' | 'Down' | 'Flat' | string
  deltaPercent: number
  currentValue: number
  previousValue: number
}

export interface EnterpriseDashboardSummary {
  companyId: string
  generatedAt: string
  portfolioCount: number
  activePortfolioCount: number
  planningTemplateCount: number
  recommendationCount: number
  decisionCount: number
  pendingDecisionCount: number
  completedDecisionCount: number
  defaultDecisionProfileName?: string | null
  overallHealth: HealthIndicator
  recommendationTrend: TrendIndicator
  decisionTrend: TrendIndicator
  drillDownPath: string
}

export interface EnterpriseDashboardPlanning {
  templateCount: number
  activeTemplateCount: number
  inactiveTemplateCount: number
  statusBreakdown: StatusCountItem[]
  drillDownPath: string
}

export interface EnterpriseDashboardPortfolio {
  portfolioCount: number
  statusBreakdown: StatusCountItem[]
  healthBreakdown: StatusCountItem[]
  averageUtilizationPercentage: number
  averageWorkloadPercentage: number
  overallHealth: HealthIndicator
  drillDownPath: string
}

export interface EnterpriseDashboardCapacity {
  recordCount: number
  totalCapacityHours: number
  totalAllocatedHours: number
  averageUtilizationPercentage: number
  utilizationHealth: HealthIndicator
  utilizationTrend: TrendIndicator
  drillDownPath: string
}

export interface EnterpriseDashboardWorkload {
  recordCount: number
  totalAllocatedHours: number
  totalCapacityHours: number
  averageWorkloadPercentage: number
  workloadHealth: HealthIndicator
  workloadTrend: TrendIndicator
  drillDownPath: string
}

export interface EnterpriseDashboardRecommendations {
  totalCount: number
  activeCount: number
  archivedCount: number
  statusBreakdown: StatusCountItem[]
  averageScore?: number | null
  trend: TrendIndicator
  drillDownPath: string
}

export interface EnterpriseDashboardDecisions {
  totalCount: number
  decisionStatusBreakdown: StatusCountItem[]
  implementationStatusBreakdown: StatusCountItem[]
  completedCount: number
  cancelledCount: number
  trend: TrendIndicator
  drillDownPath: string
}

export interface EnterpriseDashboardAi {
  aiRecommendationCount: number
  averageConfidenceScore: number
  explainabilityCount: number
  executiveSummaryCount: number
  averageExecutiveSummaryConfidenceLevel: number
  trend: TrendIndicator
  drillDownPath: string
}

export interface EnterpriseDashboardAudit {
  eventCount: number
  eventTypeBreakdown: StatusCountItem[]
  entityTypeBreakdown: StatusCountItem[]
  lastEventAt?: string | null
  trend: TrendIndicator
  drillDownPath: string
}

export interface EnterpriseDashboard {
  companyId: string
  generatedAt: string
  from?: string | null
  to?: string | null
  periodStart?: string | null
  periodEnd?: string | null
  summary: EnterpriseDashboardSummary
  planning: EnterpriseDashboardPlanning
  portfolio: EnterpriseDashboardPortfolio
  capacity: EnterpriseDashboardCapacity
  workload: EnterpriseDashboardWorkload
  recommendations: EnterpriseDashboardRecommendations
  decisions: EnterpriseDashboardDecisions
  ai: EnterpriseDashboardAi
  audit: EnterpriseDashboardAudit
}
