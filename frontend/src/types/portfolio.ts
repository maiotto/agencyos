export type PortfolioStatus = 'Active' | 'Inactive'
export type PortfolioHealth =
  | 'Unknown'
  | 'Underutilized'
  | 'Healthy'
  | 'AtRisk'
  | 'Overloaded'

export interface PortfolioMission {
  id: string
  missionId: string
  priority: number
  includedAt: string
}

export interface Portfolio {
  id: string
  companyId: string
  name: string
  description?: string | null
  status: PortfolioStatus | string
  planningTemplateId?: string | null
  planningPeriodStart: string
  planningPeriodEnd: string
  capacitySummary?: string | null
  workloadSummary?: string | null
  portfolioHealth: PortfolioHealth | string
  healthDetails?: string | null
  createdAt: string
  updatedAt: string
  missions: PortfolioMission[]
}

export interface PortfolioQuery {
  companyId?: string
  status?: string
  planningTemplateId?: string
  missionId?: string
  search?: string
  periodStart?: string
  periodEnd?: string
  orderBy?: string
  orderDirection?: string
}

export interface PortfolioMissionPayload {
  missionId: string
  priority: number
}

export interface CreatePortfolioPayload {
  companyId: string
  name: string
  description?: string
  planningPeriodStart: string
  planningPeriodEnd: string
  planningTemplateId?: string
  missions: PortfolioMissionPayload[]
}

export interface UpdatePortfolioPayload {
  name: string
  description?: string
  planningPeriodStart: string
  planningPeriodEnd: string
}

export interface PortfolioSummary {
  portfolioId: string
  name: string
  status: string
  planningPeriodStart: string
  planningPeriodEnd: string
  missionCount: number
  planningTemplateId?: string | null
  portfolioHealth: string
  capacity?: {
    overallUtilizationPercentage: number
    totalCapacityHours: number
    totalAllocatedHours: number
    totalAvailableHours: number
    activeResourceCount: number
  } | null
  workload?: {
    overallWorkloadPercentage: number
    totalPlannedHours: number
    totalAssignmentCount: number
  } | null
  historicalCapacity?: {
    averageUtilizationPercentage: number
    recordCount: number
  } | null
  historicalWorkload?: {
    averageWorkloadPercentage: number
    recordCount: number
  } | null
}

export interface PortfolioHealthResponse {
  portfolioId: string
  portfolioHealth: string
  utilizationPercentage?: number | null
  workloadPercentage?: number | null
  warningPercentage?: number | null
  historicalAverageUtilizationPercentage?: number | null
  historicalAverageWorkloadPercentage?: number | null
  healthDetails?: string | null
  calculatedAt: string
}
