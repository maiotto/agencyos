import { apiClient } from './client'
import type { RecommendationComparison } from '../types/recommendationComparison'

const BASE = '/recommendations/compare'

export async function compareRecommendations(
  leftId: string,
  rightId: string,
): Promise<RecommendationComparison> {
  const { data } = await apiClient.get<RecommendationComparison>(BASE, {
    params: { leftId, rightId },
  })
  return data
}

export async function compareRecommendationsByIds(
  leftId: string,
  rightId: string,
): Promise<RecommendationComparison> {
  const { data } = await apiClient.get<RecommendationComparison>(`${BASE}/${leftId}/${rightId}`)
  return data
}

export async function compareRecommendationVersions(
  recommendationNumber: string,
  leftVersion?: number,
  rightVersion?: number,
): Promise<RecommendationComparison> {
  const { data } = await apiClient.get<RecommendationComparison>(
    `${BASE}/version/${encodeURIComponent(recommendationNumber)}`,
    {
      params: {
        leftVersion,
        rightVersion,
      },
    },
  )
  return data
}
