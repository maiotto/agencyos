export interface Explainability {
  id: string
  recommendationId: string
  aiRecommendationId?: string | null
  explanationType: string
  generationVersion: number
  executiveSummary: string
  detailedExplanation: string
  decisionFactors: string
  assumptions: string
  risks: string
  confidenceExplanation: string
  capacityExplanation: string
  workloadExplanation: string
  generatedAt: string
  generatedBy: string
  modelVersion: string
  promptVersion: string
  status: string
  archivedAt?: string | null
  archived: boolean
}

export interface ExplainabilityQuery {
  recommendationId?: string
  aiRecommendationId?: string
  explanationType?: string
  status?: string
  search?: string
  orderBy?: string
  orderDirection?: string
}

export interface GenerateExplainabilityRequest {
  recommendationId: string
  aiRecommendationId?: string | null
  explanationType?: string
  generatedBy: string
}
