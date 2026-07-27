import { apiClient } from './client'
import type {
  AIRecommendation,
  AIRecommendationComparison,
  AIRecommendationQuery,
  GenerateAIRecommendationRequest,
} from '../types/aiRecommendation'

const BASE = '/ai-recommendations'

export async function listAIRecommendations(
  query: AIRecommendationQuery = {},
): Promise<AIRecommendation[]> {
  const { data } = await apiClient.get<AIRecommendation[]>(BASE, { params: query })
  return data
}

export async function getAIRecommendation(id: string): Promise<AIRecommendation> {
  const { data } = await apiClient.get<AIRecommendation>(`${BASE}/${id}`)
  return data
}

export async function getAIRecommendationsForRecommendation(
  recommendationId: string,
): Promise<AIRecommendation[]> {
  const { data } = await apiClient.get<AIRecommendation[]>(
    `${BASE}/recommendation/${recommendationId}`,
  )
  return data
}

export async function generateAIRecommendation(
  request: GenerateAIRecommendationRequest,
): Promise<AIRecommendation> {
  const { data } = await apiClient.post<AIRecommendation>(`${BASE}/generate`, request)
  return data
}

export async function archiveAIRecommendation(id: string): Promise<AIRecommendation> {
  const { data } = await apiClient.post<AIRecommendation>(`${BASE}/${id}/archive`)
  return data
}

export async function compareAIRecommendation(id: string): Promise<AIRecommendationComparison> {
  const { data } = await apiClient.get<AIRecommendationComparison>(`${BASE}/${id}/compare`)
  return data
}
