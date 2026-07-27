export interface AIRecommendation {
  id: string
  recommendationId: string
  recommendationVersion: number
  generationVersion: number
  generatedAt: string
  generatedBy: string
  confidenceScore: number
  executiveSummary: string
  reasoning: string
  assumptions: string
  risks: string
  alternatives: string
  suggestedDeliveryStrategy: string
  suggestedCapacityImpact: string
  suggestedWorkloadImpact: string
  modelVersion: string
  promptVersion: string
  status: string
  archivedAt?: string | null
  archived: boolean
}

export interface AIRecommendationQuery {
  recommendationId?: string
  status?: string
  search?: string
  minConfidenceScore?: number
  orderBy?: string
  orderDirection?: string
}

export interface GenerateAIRecommendationRequest {
  recommendationId: string
  generatedBy: string
}

export interface AIRecommendationComparisonField {
  path: string
  leftValue?: string | null
  rightValue?: string | null
  changed: boolean
}

export interface AIRecommendationComparison {
  aiRecommendation: AIRecommendation
  recommendationId: string
  recommendationTitle: string
  recommendationScore?: number | null
  recommendationRank?: number | null
  recommendationVersion: number
  hasDifferences: boolean
  differences: AIRecommendationComparisonField[]
}
