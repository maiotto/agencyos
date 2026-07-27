import type { HealthIndicator, StatusCountItem } from './enterpriseDashboard'

export interface PortfolioAnalyticsQuery {
  companyId?: string
  from?: string
  to?: string
  periodStart?: string
  periodEnd?: string
  portfolioId?: string
}

export interface PortfolioCompareQuery {
  companyId?: string
  leftPortfolioId: string
  rightPortfolioId: string
  from?: string
  to?: string
  periodStart?: string
  periodEnd?: string
}

export interface PortfolioAnalyticsCard {
  portfolioId: string
  name: string
  status: string
  health: HealthIndicator
  missionCount: number
  utilizationPercentage?: number | null
  workloadPercentage?: number | null
  recommendationCount: number
  decisionCount: number
  drillDownPath: string
}

export interface PortfolioAnalyticsOverview {
  companyId: string
  generatedAt: string
  from?: string | null
  to?: string | null
  periodStart?: string | null
  periodEnd?: string | null
  portfolioCount: number
  overallHealth: HealthIndicator
  portfolios: PortfolioAnalyticsCard[]
  drillDownPath: string
}

export interface PortfolioAnalyticsDetail {
  portfolioId: string
  companyId: string
  name: string
  description?: string | null
  status: string
  planningPeriodStart: string
  planningPeriodEnd: string
  health: HealthIndicator
  missionCount: number
  missionIds: string[]
  utilizationPercentage?: number | null
  workloadPercentage?: number | null
  historicalAverageUtilizationPercentage?: number | null
  historicalAverageWorkloadPercentage?: number | null
  historicalCapacityRecordCount: number
  historicalWorkloadRecordCount: number
  recommendationCount: number
  recommendationStatusBreakdown: StatusCountItem[]
  averageRecommendationScore?: number | null
  recommendationHistoryCount: number
  decisionCount: number
  decisionStatusBreakdown: StatusCountItem[]
  completedDecisionCount: number
  cancelledDecisionCount: number
  drillDownPath: string
}

export interface PortfolioTrendPoint {
  periodLabel: string
  periodStart: string
  periodEnd: string
  capacityUtilization?: number | null
  workloadUtilization?: number | null
  healthStatus?: string | null
  recommendationCount: number
  decisionCount: number
}

export interface PortfolioTrends {
  companyId: string
  portfolioId?: string | null
  generatedAt: string
  from?: string | null
  to?: string | null
  periodStart?: string | null
  periodEnd?: string | null
  points: PortfolioTrendPoint[]
  drillDownPath: string
}

export interface PortfolioComparisonSide {
  portfolioId: string
  name: string
  status: string
  health: HealthIndicator
  missionCount: number
  utilizationPercentage?: number | null
  workloadPercentage?: number | null
  recommendationCount: number
  averageRecommendationScore?: number | null
  decisionCount: number
  completedDecisionCount: number
  cancelledDecisionCount: number
  drillDownPath: string
}

export interface PortfolioComparisonFieldDiff {
  field: string
  leftValue?: string | null
  rightValue?: string | null
  delta?: number | null
}

export interface PortfolioComparison {
  companyId: string
  generatedAt: string
  left: PortfolioComparisonSide
  right: PortfolioComparisonSide
  fieldDiffs: PortfolioComparisonFieldDiff[]
}

export interface PortfolioRankingItem {
  rank: number
  portfolioId: string
  name: string
  score: number
  health: HealthIndicator
  utilizationPercentage?: number | null
  workloadPercentage?: number | null
  recommendationEffectivenessPercentage?: number | null
  drillDownPath: string
}

export interface PortfolioRanking {
  companyId: string
  generatedAt: string
  items: PortfolioRankingItem[]
}

export interface PortfolioRiskIndicator {
  portfolioId: string
  name: string
  riskLevel: string
  reason: string
  health: HealthIndicator
  drillDownPath: string
}

export interface PortfolioHealthAnalytics {
  companyId: string
  generatedAt: string
  portfolioCount: number
  distribution: StatusCountItem[]
  overallHealth: HealthIndicator
  healthyCount: number
  atRiskCount: number
  overloadedCount: number
  underutilizedCount: number
  unknownCount: number
  riskIndicators: PortfolioRiskIndicator[]
  drillDownPath: string
}

export interface PortfolioPerformance {
  companyId: string
  generatedAt: string
  portfolioCount: number
  averageUtilizationPercentage: number
  averageWorkloadPercentage: number
  averageRecommendationScore?: number | null
  recommendationCount: number
  decisionCount: number
  decisionCompletionRatePercentage: number
  decisionCancellationRatePercentage: number
  recommendationEffectivenessPercentage: number
  drillDownPath: string
}
