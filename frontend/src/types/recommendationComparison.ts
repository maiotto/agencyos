import type { RecommendationHistory } from './recommendationHistory'

export interface RecommendationComparisonFieldDiff {
  section: string
  path: string
  leftValue?: string | null
  rightValue?: string | null
  changed: boolean
}

export interface RecommendationComparisonSection {
  section: string
  hasDifferences: boolean
  fields: RecommendationComparisonFieldDiff[]
}

export interface RecommendationComparison {
  left: RecommendationHistory
  right: RecommendationHistory
  hasDifferences: boolean
  scoreDelta?: number | null
  rankDelta?: number | null
  versionDelta: number
  differences: RecommendationComparisonFieldDiff[]
  sections: RecommendationComparisonSection[]
}
