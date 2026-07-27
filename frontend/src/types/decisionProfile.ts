export const RANKING_DIMENSIONS = [
  'EstimatedCost',
  'EstimatedDuration',
  'CapacityUtilization',
  'OperationalRisk',
  'HumanResourceUsage',
  'AiResourceUsage',
  'ExternalResourceUsage',
  'AutomationUsage',
] as const

export type RankingDimension = (typeof RANKING_DIMENSIONS)[number]

export type CompanyDecisionProfileStatus = 'Active' | 'Inactive' | 'Archived'

export interface DecisionProfileDimensionWeight {
  dimension: RankingDimension | string
  weight: number
  preferHigherValues: boolean
}

export interface CompanyDecisionProfile {
  id: string
  companyId: string
  profileFamilyId: string
  code: string
  name: string
  description: string | null
  status: CompanyDecisionProfileStatus | string
  dimensions: DecisionProfileDimensionWeight[]
  capacityWeight: number
  workloadWeight: number
  costWeight: number
  riskWeight: number
  qualityWeight: number
  preferredStrategy: string | null
  preferredCapacityThreshold: number | null
  preferredWorkloadThreshold: number | null
  defaultProfile: boolean
  version: number
  createdAt: string
  updatedAt: string
  archivedAt: string | null
}

export interface CompanyDecisionProfileQuery {
  companyId?: string
  status?: string
  name?: string
  code?: string
  defaultProfile?: boolean
  orderBy?: string
  orderDirection?: 'asc' | 'desc'
}

export interface CreateCompanyDecisionProfilePayload {
  companyId: string
  code: string
  name: string
  description?: string | null
  priorityWeights: DecisionProfileDimensionWeight[]
  capacityWeight: number
  workloadWeight: number
  costWeight: number
  riskWeight: number
  qualityWeight: number
  preferredStrategy?: string | null
  preferredCapacityThreshold?: number | null
  preferredWorkloadThreshold?: number | null
  defaultProfile: boolean
}

export interface UpdateCompanyDecisionProfilePayload {
  name?: string
  description?: string | null
  priorityWeights: DecisionProfileDimensionWeight[]
  capacityWeight: number
  workloadWeight: number
  costWeight: number
  riskWeight: number
  qualityWeight: number
  preferredStrategy?: string | null
  preferredCapacityThreshold?: number | null
  preferredWorkloadThreshold?: number | null
}

export interface CloneCompanyDecisionProfilePayload {
  name: string
  code: string
}
