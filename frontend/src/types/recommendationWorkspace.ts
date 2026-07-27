import type { RecommendationComparison } from './recommendationComparison'

export interface RecommendationWorkspaceQuery {
  companyId?: string
  from?: string
  to?: string
  leftRecommendationId?: string
  rightRecommendationId?: string
}

export interface RecommendationAction {
  key: string
  label: string
  category: string
  drillDownPath: string
  description: string
  requiresHumanApproval: boolean
  isAdvisory: boolean
  isInformational: boolean
}

export interface RecommendationNavigation {
  companyId: string
  actions: RecommendationAction[]
}

export interface RecommendationKpiSummary {
  activeRecommendationCount: number
  archivedRecommendationCount: number
  pendingApprovalCount: number
  aiRecommendationCount: number
  explainabilityCount: number
  executiveSummaryCount: number
  historyEventCount: number
}

export interface RecommendationOverview {
  companyId: string
  generatedAt: string
  from?: string | null
  to?: string | null
  kpis: RecommendationKpiSummary
  requiresHumanApproval: boolean
  humanApprovalDisclaimer: string
  drillDownPath: string
}

export interface RecommendationCard {
  id: string
  recommendationNumber: string
  title: string
  status: string
  score?: number | null
  rank?: number | null
  version: number
  archived: boolean
  generatedAt: string
  generatedBy: string
  drillDownPath: string
}

export interface RecommendationsSection {
  companyId: string
  recommendations: RecommendationCard[]
  listAction: RecommendationAction
  generateAction: RecommendationAction
  archiveRestoreAction: RecommendationAction
}

export interface RecommendationApprovalCard {
  workflowId: string
  recommendationId: string
  title: string
  status: string
  createdBy: string
  createdAt: string
  drillDownPath: string
  approvePath: string
  rejectPath: string
  requiresHumanApproval: boolean
}

export interface ApprovalSection {
  companyId: string
  pendingApprovals: RecommendationApprovalCard[]
  requiresHumanApproval: boolean
  humanApprovalDisclaimer: string
  approvalQueueAction: RecommendationAction
  startWorkflowAction: RecommendationAction
}

export interface RecommendationHistoryCard {
  id: string
  recommendationId: string
  recommendationNumber: string
  eventType: string
  recommendationStatus: string
  workflowStatus?: string | null
  createdBy: string
  createdAt: string
  drillDownPath: string
}

export interface HistorySection {
  companyId: string
  from?: string | null
  to?: string | null
  items: RecommendationHistoryCard[]
  action: RecommendationAction
}

export interface CompareSection {
  companyId: string
  leftRecommendationId?: string | null
  rightRecommendationId?: string | null
  hasComparison: boolean
  comparison?: RecommendationComparison | null
  requiresHumanApproval: boolean
  action: RecommendationAction
}

export interface AIRecommendationCard {
  id: string
  recommendationId: string
  confidenceScore: number
  status: string
  generatedAt: string
  generatedBy: string
  drillDownPath: string
  isAdvisory: boolean
}

export interface ExplainabilityCard {
  id: string
  recommendationId: string
  explanationType: string
  status: string
  generatedAt: string
  drillDownPath: string
  isInformational: boolean
}

export interface AiSection {
  companyId: string
  aiRecommendations: AIRecommendationCard[]
  explainability: ExplainabilityCard[]
  isAdvisory: boolean
  isInformational: boolean
  aiAdvisoryDisclaimer: string
  explainabilityInformationalDisclaimer: string
  aiListAction: RecommendationAction
  generateAiAction: RecommendationAction
  explainabilityListAction: RecommendationAction
  generateExplainabilityAction: RecommendationAction
}

export interface ExecutiveSummaryCard {
  id: string
  recommendationId: string
  summaryVersion: number
  confidenceLevel: number
  status: string
  generatedAt: string
  drillDownPath: string
}

export interface ExecutiveSummarySection {
  companyId: string
  summaries: ExecutiveSummaryCard[]
  listAction: RecommendationAction
  generateAction: RecommendationAction
}

export interface RecommendationWorkspace {
  companyId: string
  generatedAt: string
  from?: string | null
  to?: string | null
  kpis: RecommendationKpiSummary
  overview: RecommendationOverview
  recommendations: RecommendationsSection
  approval: ApprovalSection
  history: HistorySection
  compare: CompareSection
  ai: AiSection
  executiveSummary: ExecutiveSummarySection
  navigation: RecommendationNavigation
}
