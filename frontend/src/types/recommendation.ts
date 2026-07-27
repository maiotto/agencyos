export type RecommendationStatus = 'Active' | 'Archived'

export interface Recommendation {
  id: string
  companyId: string
  missionId: string
  contractId: string
  deliveryStrategyId: string
  recommendationNumber: string
  title: string
  summary?: string | null
  reason?: string | null
  score?: number | null
  rank?: number | null
  status: RecommendationStatus | string
  version: number
  decisionEngineVersion: string
  planningTemplateId?: string | null
  capacitySnapshot: string
  workloadSnapshot: string
  recommendationPayload: string
  generatedAt: string
  generatedBy: string
  archived: boolean
  archivedAt?: string | null
  createdAt: string
}

export interface RecommendationQuery {
  companyId?: string
  missionId?: string
  contractId?: string
  deliveryStrategyId?: string
  status?: string
  recommendationNumber?: string
  version?: number
  archived?: boolean
  includeArchived?: boolean
  search?: string
  generatedBy?: string
  generatedFrom?: string
  generatedTo?: string
  orderBy?: string
  orderDirection?: string
}

export interface CreateRecommendationPayload {
  companyId: string
  missionId: string
  contractId: string
  deliveryStrategyId: string
  recommendationNumber?: string
  title: string
  summary?: string
  reason?: string
  score?: number
  rank?: number
  version?: number
  decisionEngineVersion?: string
  planningTemplateId?: string
  capacitySnapshot: string
  workloadSnapshot: string
  recommendationPayload: string
  generatedBy: string
  generatedAt?: string
}
