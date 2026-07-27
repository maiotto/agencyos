export interface DecisionWorkspaceQuery {
  companyId?: string
  from?: string
  to?: string
  decisionId?: string
}

export interface DecisionAction {
  key: string
  label: string
  category: string
  drillDownPath: string
  description: string
  requiresHumanApproval: boolean
}

export interface DecisionNavigation {
  companyId: string
  actions: DecisionAction[]
}

export interface DecisionKpiSummary {
  totalCount: number
  pendingCount: number
  inProgressCount: number
  completedCount: number
  cancelledCount: number
  withOutcomeCount: number
  implementationNotStartedCount: number
}

export interface DecisionOverview {
  companyId: string
  generatedAt: string
  from?: string | null
  to?: string | null
  kpis: DecisionKpiSummary
  requiresHumanApproval: boolean
  humanApprovalDisclaimer: string
  drillDownPath: string
}

export interface DecisionCard {
  id: string
  recommendationId: string
  companyId: string
  missionId: string
  contractId: string
  decisionStatus: string
  implementationStatus: string
  decisionDate: string
  implementationDate?: string | null
  completedDate?: string | null
  outcome?: string | null
  businessValue?: string | null
  createdBy: string
  drillDownPath: string
  recommendationDrillDownPath: string
}

export interface DecisionsSection {
  companyId: string
  totalCount: number
  pending: DecisionCard[]
  inProgress: DecisionCard[]
  completed: DecisionCard[]
  cancelled: DecisionCard[]
  listAction: DecisionAction
  createAction: DecisionAction
}

export interface DecisionTimelineItem {
  decisionId: string
  recommendationId: string
  eventType: string
  fromDecisionStatus: string
  toDecisionStatus: string
  fromImplementationStatus: string
  toImplementationStatus: string
  actor: string
  comment?: string | null
  occurredAt: string
  drillDownPath: string
}

export interface DecisionTimelineSection {
  companyId: string
  decisionId?: string | null
  items: DecisionTimelineItem[]
  action: DecisionAction
}

export interface DecisionOutcomeCard {
  id: string
  recommendationId: string
  decisionStatus: string
  implementationStatus: string
  outcome: string
  businessValue?: string | null
  completedDate?: string | null
  drillDownPath: string
  recommendationDrillDownPath: string
}

export interface DecisionOutcomesSection {
  companyId: string
  outcomes: DecisionOutcomeCard[]
  recordOutcomeAction: DecisionAction
}

export interface DecisionAuditItem {
  id: string
  entityId: string
  eventType: string
  action: string
  occurredAt: string
  userId: string
  drillDownPath: string
}

export interface DecisionAuditSection {
  companyId: string
  from?: string | null
  to?: string | null
  items: DecisionAuditItem[]
  action: DecisionAction
}

export interface DecisionWorkspace {
  companyId: string
  generatedAt: string
  from?: string | null
  to?: string | null
  decisionId?: string | null
  kpis: DecisionKpiSummary
  overview: DecisionOverview
  decisions: DecisionsSection
  timeline: DecisionTimelineSection
  outcomes: DecisionOutcomesSection
  audit: DecisionAuditSection
  navigation: DecisionNavigation
}
