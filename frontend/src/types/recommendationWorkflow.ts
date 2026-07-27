export type RecommendationWorkflowStatus =
  | 'Draft'
  | 'PendingApproval'
  | 'Approved'
  | 'Rejected'
  | 'Cancelled'
  | 'Reopened'

export interface RecommendationWorkflowTransition {
  id: string
  fromStatus: string
  toStatus: string
  actor: string
  comment?: string | null
  occurredAt: string
}

export interface RecommendationWorkflow {
  id: string
  recommendationId: string
  deliveryStrategyId: string
  contractId: string
  missionId: string
  companyDecisionProfileId?: string | null
  title: string
  summary?: string | null
  status: RecommendationWorkflowStatus | string
  createdBy: string
  approver?: string | null
  approvalDate?: string | null
  approvalComment?: string | null
  rankPosition?: number | null
  finalScore?: number | null
  createdAt: string
  updatedAt: string
  transitions: RecommendationWorkflowTransition[]
}

export interface RecommendationWorkflowQuery {
  status?: string
  recommendationId?: string
  deliveryStrategyId?: string
  contractId?: string
  missionId?: string
  search?: string
  createdBy?: string
  orderBy?: string
  orderDirection?: string
}

export interface CreateRecommendationWorkflowPayload {
  recommendationId: string
  createdBy: string
  companyDecisionProfileId?: string
  title?: string
  summary?: string
}

export interface RecommendationWorkflowActionPayload {
  actor: string
  comment?: string
}

export interface ApproveRecommendationWorkflowPayload {
  approver: string
  approvalDate?: string
  comment?: string
}
