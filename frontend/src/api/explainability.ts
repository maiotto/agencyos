import { apiClient } from './client'
import type {
  Explainability,
  ExplainabilityQuery,
  GenerateExplainabilityRequest,
} from '../types/explainability'

const BASE = '/explainability'

export async function listExplainability(
  query: ExplainabilityQuery = {},
): Promise<Explainability[]> {
  const { data } = await apiClient.get<Explainability[]>(BASE, { params: query })
  return data
}

export async function getExplainability(id: string): Promise<Explainability> {
  const { data } = await apiClient.get<Explainability>(`${BASE}/${id}`)
  return data
}

export async function getExplainabilityForRecommendation(
  recommendationId: string,
): Promise<Explainability[]> {
  const { data } = await apiClient.get<Explainability[]>(
    `${BASE}/recommendation/${recommendationId}`,
  )
  return data
}

export async function generateExplainability(
  request: GenerateExplainabilityRequest,
): Promise<Explainability> {
  const { data } = await apiClient.post<Explainability>(`${BASE}/generate`, request)
  return data
}

export async function archiveExplainability(id: string): Promise<Explainability> {
  const { data } = await apiClient.post<Explainability>(`${BASE}/${id}/archive`)
  return data
}
