export interface ExecutiveRecommendationSummary {
  id: string
  recommendationId: string
  aiRecommendationId?: string | null
  explainabilityId?: string | null
  summaryVersion: number
  executiveSummary: string
  keyDecisionFactors: string
  businessImpact: string
  capacityImpact: string
  workloadImpact: string
  risks: string
  assumptions: string
  confidenceLevel: number
  recommendedActions: string
  generatedAt: string
  generatedBy: string
  modelVersion: string
  promptVersion: string
  status: string
  archivedAt?: string | null
  archived: boolean
}

export interface ExecutiveRecommendationSummaryQuery {
  recommendationId?: string
  aiRecommendationId?: string
  explainabilityId?: string
  status?: string
  search?: string
  minConfidenceLevel?: number
  includeArchived?: boolean
  orderBy?: string
  orderDirection?: string
}

export interface GenerateExecutiveRecommendationSummaryRequest {
  recommendationId: string
  aiRecommendationId?: string | null
  explainabilityId?: string | null
  generatedBy: string
}

export interface CreateExecutiveRecommendationSummaryVersionRequest {
  aiRecommendationId?: string | null
  explainabilityId?: string | null
  generatedBy: string
}

export interface ExecutiveRecommendationSummaryComparisonField {
  path: string
  leftValue?: string | null
  rightValue?: string | null
  changed: boolean
}

export interface ExecutiveRecommendationSummaryComparison {
  summary: ExecutiveRecommendationSummary
  recommendationId: string
  recommendationTitle: string
  recommendationScore?: number | null
  recommendationRank?: number | null
  recommendationVersion: number
  hasDifferences: boolean
  differences: ExecutiveRecommendationSummaryComparisonField[]
}
