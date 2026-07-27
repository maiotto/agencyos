import { apiClient } from './client'
import type {
  RecommendationHistory,
  RecommendationHistoryQuery,
  RecommendationHistoryTimelineEntry,
} from '../types/recommendationHistory'

const BASE = '/recommendations/history'

export async function listRecommendationHistory(
  query: RecommendationHistoryQuery = {},
): Promise<RecommendationHistory[]> {
  const { data } = await apiClient.get<RecommendationHistory[]>(BASE, { params: query })
  return data
}

export async function getRecommendationHistoryEntry(id: string): Promise<RecommendationHistory> {
  const { data } = await apiClient.get<RecommendationHistory>(`${BASE}/${id}`)
  return data
}

export async function getRecommendationHistoryVersions(
  recommendationId: string,
): Promise<RecommendationHistory[]> {
  const { data } = await apiClient.get<RecommendationHistory[]>(
    `${BASE}/${recommendationId}/versions`,
  )
  return data
}

export async function getRecommendationHistoryTimeline(
  recommendationId: string,
): Promise<RecommendationHistoryTimelineEntry[]> {
  const { data } = await apiClient.get<RecommendationHistoryTimelineEntry[]>(
    `${BASE}/${recommendationId}/timeline`,
  )
  return data
}
