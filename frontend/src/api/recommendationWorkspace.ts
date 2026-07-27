import { apiClient } from './client'
import type { RecommendationWorkspace, RecommendationWorkspaceQuery } from '../types/recommendationWorkspace'

export async function getRecommendationWorkspace(
  query: RecommendationWorkspaceQuery,
): Promise<RecommendationWorkspace> {
  const { data } = await apiClient.get<RecommendationWorkspace>('/recommendation-workspace', { params: query })
  return data
}
