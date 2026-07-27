import type { HealthIndicator } from './enterpriseDashboard'

export const ADVISORY_DISCLAIMER =
  'Advisory only — human approval required. Simulations never modify Portfolios.'

export interface CrossPortfolioPlanningQuery {
  companyId?: string
  periodStart?: string
  periodEnd?: string
  from?: string
  to?: string
}

export interface CrossPortfolioSelectionQuery extends CrossPortfolioPlanningQuery {
  portfolioIds: string[]
}

export interface SimulateCrossPortfolioPlanRequest {
  companyId?: string
  portfolioIds: string[]
  periodStart?: string
  periodEnd?: string
  scenarioName?: string
}

export interface CompareCrossPortfolioScenariosRequest {
  companyId?: string
  leftScenarioId: string
  rightScenarioId: string
}

export interface PortfolioParticipationMissionItem {
  missionId: string
  priority: number
}

export interface PortfolioParticipation {
  portfolioId: string
  name: string
  status: string
  health: HealthIndicator
  missionCount: number
  missions: PortfolioParticipationMissionItem[]
  utilizationPercentage?: number | null
  workloadPercentage?: number | null
  drillDownPath: string
}

export interface CrossPortfolioOverview {
  companyId: string
  generatedAt: string
  portfolioCount: number
  portfolios: PortfolioParticipation[]
  requiresHumanApproval: boolean
  advisoryDisclaimer: string
}

export interface EnterpriseCapacityPortfolioItem {
  portfolioId: string
  name: string
  utilizationPercentage?: number | null
}

export interface EnterpriseCapacity {
  portfolioCount: number
  averageUtilizationPercentage?: number | null
  minUtilizationPercentage?: number | null
  maxUtilizationPercentage?: number | null
  portfolios: EnterpriseCapacityPortfolioItem[]
  capacityEngineUsed: boolean
  liveSummary?: {
    periodStartDate: string
    periodEndDate: string
    activeResourceCount: number
    totalCapacityHours: number
    totalAllocatedHours: number
    totalAvailableHours: number
    overallUtilizationPercentage: number
    totalRemainingCapacityHours: number
  } | null
}

export interface EnterpriseWorkloadPortfolioItem {
  portfolioId: string
  name: string
  workloadPercentage?: number | null
}

export interface EnterpriseWorkload {
  portfolioCount: number
  averageWorkloadPercentage?: number | null
  minWorkloadPercentage?: number | null
  maxWorkloadPercentage?: number | null
  portfolios: EnterpriseWorkloadPortfolioItem[]
  workloadEngineUsed: boolean
  liveSummary?: {
    periodStartDate: string
    periodEndDate: string
    activeResourceCount: number
    totalPlannedHours: number
    totalAssignmentCount: number
    averageHoursPerAssignment: number
    overallWorkloadPercentage: number
  } | null
}

export interface PortfolioConflictItem {
  missionId: string
  portfolioIds: string[]
  portfolioNames: string[]
}

export interface ResourceConflictItem {
  conflictId: string
  conflictType: string
  executionResourceId: string
  executionResourceName: string
  severity: string
  description: string
}

export interface ConflictSummary {
  portfolioConflictCount: number
  portfolioConflicts: PortfolioConflictItem[]
  resourceConflictCount: number
  resourceConflicts: ResourceConflictItem[]
  severity: string
}

export interface BalancingRecommendation {
  sourcePortfolioId?: string | null
  sourcePortfolioName?: string | null
  targetPortfolioId?: string | null
  targetPortfolioName?: string | null
  category: string
  recommendation: string
  rationale: string
}

export interface CrossPortfolioBalance {
  companyId: string
  generatedAt: string
  periodStart: string
  periodEnd: string
  capacity: EnterpriseCapacity
  workload: EnterpriseWorkload
  portfolios: PortfolioParticipation[]
  recommendations: BalancingRecommendation[]
  requiresHumanApproval: boolean
  advisoryDisclaimer: string
}

export interface CrossPortfolioScenario {
  scenarioId: string
  companyId: string
  scenarioName?: string | null
  createdAt: string
  periodStart: string
  periodEnd: string
  portfolioIds: string[]
  portfolios: PortfolioParticipation[]
  capacity: EnterpriseCapacity
  workload: EnterpriseWorkload
  conflicts: ConflictSummary
  recommendations: BalancingRecommendation[]
  requiresHumanApproval: boolean
  advisoryOnly: boolean
  advisoryDisclaimer: string
}

export interface CrossPortfolioScenarioSummaryItem {
  scenarioId: string
  scenarioName?: string | null
  createdAt: string
  portfolioCount: number
  averageUtilizationPercentage?: number | null
  averageWorkloadPercentage?: number | null
  conflictCount: number
}

export interface CrossPortfolioScenarioList {
  companyId: string
  generatedAt: string
  scenarios: CrossPortfolioScenarioSummaryItem[]
}

export interface SimulationSummary {
  scenarioId: string
  companyId: string
  simulatedAt: string
  scenario: CrossPortfolioScenario
  requiresHumanApproval: boolean
  advisoryOnly: boolean
  advisoryDisclaimer: string
}

export interface ScenarioComparisonFieldDiff {
  field: string
  leftValue?: string | null
  rightValue?: string | null
  delta?: number | null
}

export interface ScenarioComparison {
  companyId: string
  generatedAt: string
  left: CrossPortfolioScenario
  right: CrossPortfolioScenario
  fieldDiffs: ScenarioComparisonFieldDiff[]
  requiresHumanApproval: boolean
  advisoryDisclaimer: string
}
