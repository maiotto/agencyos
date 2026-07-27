import { apiClient } from './client'
import type {
  CreateRecommendationPayload,
  Recommendation,
  RecommendationQuery,
} from '../types/recommendation'

const BASE = '/recommendations'

export async function listRecommendations(
  query: RecommendationQuery = {},
): Promise<Recommendation[]> {
  const { data } = await apiClient.get<Recommendation[]>(BASE, { params: query })
  return data
}

export async function filterRecommendations(
  query: RecommendationQuery = {},
): Promise<Recommendation[]> {
  const { data } = await apiClient.get<Recommendation[]>(`${BASE}/filter`, { params: query })
  return data
}

export async function getRecommendation(id: string): Promise<Recommendation> {
  const { data } = await apiClient.get<Recommendation>(`${BASE}/${id}`)
  return data
}

export async function getRecommendationVersions(id: string): Promise<Recommendation[]> {
  const { data } = await apiClient.get<Recommendation[]>(`${BASE}/${id}/versions`)
  return data
}

export async function createRecommendation(
  payload: CreateRecommendationPayload,
): Promise<Recommendation> {
  const { data } = await apiClient.post<Recommendation>(BASE, payload)
  return data
}

export async function archiveRecommendation(id: string): Promise<Recommendation> {
  const { data } = await apiClient.post<Recommendation>(`${BASE}/${id}/archive`)
  return data
}

export async function restoreRecommendation(id: string): Promise<Recommendation> {
  const { data } = await apiClient.post<Recommendation>(`${BASE}/${id}/restore`)
  return data
}
