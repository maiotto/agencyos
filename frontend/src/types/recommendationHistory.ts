export type RecommendationHistoryEventType =
  | 'VersionCreated'
  | 'Archived'
  | 'Restored'
  | 'WorkflowTransition'

export interface RecommendationHistory {
  id: string
  recommendationId: string
  recommendationNumber: string
  recommendationVersion: number
  companyId: string
  missionId: string
  contractId: string
  deliveryStrategyId: string
  title: string
  summary?: string | null
  eventType: RecommendationHistoryEventType | string
  recommendationStatus: string
  workflowStatus?: string | null
  workflowId?: string | null
  approver?: string | null
  approvalDate?: string | null
  approvalComment?: string | null
  score?: number | null
  rank?: number | null
  planningTemplateId?: string | null
  decisionEngineVersion: string
  capacitySnapshot: string
  workloadSnapshot: string
  recommendationPayload: string
  createdBy: string
  createdAt: string
}

export interface RecommendationHistoryTimelineEntry {
  id: string
  eventType: string
  recommendationVersion: number
  recommendationStatus: string
  workflowStatus?: string | null
  workflowId?: string | null
  approver?: string | null
  approvalComment?: string | null
  createdBy: string
  createdAt: string
  title: string
}

export interface RecommendationHistoryQuery {
  companyId?: string
  missionId?: string
  contractId?: string
  recommendationId?: string
  recommendationNumber?: string
  version?: number
  workflowStatus?: string
  eventType?: string
  search?: string
  createdFrom?: string
  createdTo?: string
  orderBy?: string
  orderDirection?: string
}
