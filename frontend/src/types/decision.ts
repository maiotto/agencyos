export type DecisionStatus = 'Created' | 'InProgress' | 'Completed' | 'Cancelled'
export type DecisionImplementationStatus =
  | 'NotStarted'
  | 'InProgress'
  | 'Completed'
  | 'Cancelled'

export interface DecisionTimelineEntry {
  id: string
  eventType: string
  fromDecisionStatus: string
  toDecisionStatus: string
  fromImplementationStatus: string
  toImplementationStatus: string
  actor: string
  comment?: string | null
  occurredAt: string
}

export interface Decision {
  id: string
  recommendationId: string
  companyId: string
  missionId: string
  contractId: string
  decisionStatus: DecisionStatus | string
  implementationStatus: DecisionImplementationStatus | string
  decisionDate: string
  implementationDate?: string | null
  completedDate?: string | null
  outcome?: string | null
  businessValue?: string | null
  createdBy: string
  createdAt: string
  updatedAt: string
  timeline: DecisionTimelineEntry[]
}

export interface DecisionQuery {
  companyId?: string
  missionId?: string
  contractId?: string
  recommendationId?: string
  decisionStatus?: string
  implementationStatus?: string
  search?: string
  decisionFrom?: string
  decisionTo?: string
  orderBy?: string
  orderDirection?: string
}

export interface CreateDecisionRequest {
  recommendationId: string
  createdBy: string
  decisionDate?: string
}

export interface DecisionActionRequest {
  actor: string
  comment?: string
}

export interface RecordDecisionOutcomeRequest {
  outcome: string
  businessValue?: string
  actor: string
  comment?: string
}
