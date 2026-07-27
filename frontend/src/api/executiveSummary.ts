import { apiClient } from './client'
import type {
  CreateExecutiveRecommendationSummaryVersionRequest,
  ExecutiveRecommendationSummary,
  ExecutiveRecommendationSummaryComparison,
  ExecutiveRecommendationSummaryQuery,
  GenerateExecutiveRecommendationSummaryRequest,
} from '../types/executiveSummary'

const BASE = '/executive-summaries'

export async function listExecutiveSummaries(
  query: ExecutiveRecommendationSummaryQuery = {},
): Promise<ExecutiveRecommendationSummary[]> {
  const { data } = await apiClient.get<ExecutiveRecommendationSummary[]>(BASE, { params: query })
  return data
}

export async function getExecutiveSummary(id: string): Promise<ExecutiveRecommendationSummary> {
  const { data } = await apiClient.get<ExecutiveRecommendationSummary>(`${BASE}/${id}`)
  return data
}

export async function getExecutiveSummariesForRecommendation(
  recommendationId: string,
): Promise<ExecutiveRecommendationSummary[]> {
  const { data } = await apiClient.get<ExecutiveRecommendationSummary[]>(
    `${BASE}/recommendation/${recommendationId}`,
  )
  return data
}

export async function generateExecutiveSummary(
  request: GenerateExecutiveRecommendationSummaryRequest,
): Promise<ExecutiveRecommendationSummary> {
  const { data } = await apiClient.post<ExecutiveRecommendationSummary>(`${BASE}/generate`, request)
  return data
}

export async function archiveExecutiveSummary(
  id: string,
): Promise<ExecutiveRecommendationSummary> {
  const { data } = await apiClient.post<ExecutiveRecommendationSummary>(`${BASE}/${id}/archive`)
  return data
}

export async function createExecutiveSummaryVersion(
  id: string,
  request: CreateExecutiveRecommendationSummaryVersionRequest,
): Promise<ExecutiveRecommendationSummary> {
  const { data } = await apiClient.post<ExecutiveRecommendationSummary>(
    `${BASE}/${id}/versions`,
    request,
  )
  return data
}

export async function compareExecutiveSummary(
  id: string,
): Promise<ExecutiveRecommendationSummaryComparison> {
  const { data } = await apiClient.get<ExecutiveRecommendationSummaryComparison>(
    `${BASE}/${id}/compare`,
  )
  return data
}
