import { apiClient } from './client'
import type {
  CreatePortfolioPayload,
  Portfolio,
  PortfolioHealthResponse,
  PortfolioMissionPayload,
  PortfolioQuery,
  PortfolioSummary,
  UpdatePortfolioPayload,
} from '../types/portfolio'

const BASE = '/portfolios'

export async function listPortfolios(query: PortfolioQuery = {}): Promise<Portfolio[]> {
  const { data } = await apiClient.get<Portfolio[]>(BASE, { params: query })
  return data
}

export async function getPortfolio(id: string): Promise<Portfolio> {
  const { data } = await apiClient.get<Portfolio>(`${BASE}/${id}`)
  return data
}

export async function getPortfolioSummary(id: string): Promise<PortfolioSummary> {
  const { data } = await apiClient.get<PortfolioSummary>(`${BASE}/${id}/summary`)
  return data
}

export async function getPortfolioHealth(id: string): Promise<PortfolioHealthResponse> {
  const { data } = await apiClient.get<PortfolioHealthResponse>(`${BASE}/${id}/health`)
  return data
}

export async function createPortfolio(payload: CreatePortfolioPayload): Promise<Portfolio> {
  const { data } = await apiClient.post<Portfolio>(BASE, payload)
  return data
}

export async function updatePortfolio(
  id: string,
  payload: UpdatePortfolioPayload,
): Promise<Portfolio> {
  const { data } = await apiClient.put<Portfolio>(`${BASE}/${id}`, payload)
  return data
}

export async function deletePortfolio(id: string): Promise<void> {
  await apiClient.delete(`${BASE}/${id}`)
}

export async function activatePortfolio(id: string): Promise<void> {
  await apiClient.post(`${BASE}/${id}/activate`)
}

export async function deactivatePortfolio(id: string): Promise<void> {
  await apiClient.post(`${BASE}/${id}/deactivate`)
}

export async function associatePortfolioMission(
  id: string,
  payload: PortfolioMissionPayload,
): Promise<Portfolio> {
  const { data } = await apiClient.post<Portfolio>(`${BASE}/${id}/missions`, payload)
  return data
}

export async function removePortfolioMission(id: string, missionId: string): Promise<Portfolio> {
  const { data } = await apiClient.delete<Portfolio>(`${BASE}/${id}/missions/${missionId}`)
  return data
}

export async function assignPortfolioPlanningTemplate(
  id: string,
  planningTemplateId?: string | null,
): Promise<Portfolio> {
  const { data } = await apiClient.post<Portfolio>(`${BASE}/${id}/planning-template`, {
    planningTemplateId: planningTemplateId || null,
  })
  return data
}

export async function calculatePortfolioCapacity(id: string): Promise<Portfolio> {
  const { data } = await apiClient.post<Portfolio>(`${BASE}/${id}/calculate-capacity`)
  return data
}

export async function calculatePortfolioWorkload(id: string): Promise<Portfolio> {
  const { data } = await apiClient.post<Portfolio>(`${BASE}/${id}/calculate-workload`)
  return data
}

export async function calculatePortfolioHealth(id: string): Promise<Portfolio> {
  const { data } = await apiClient.post<Portfolio>(`${BASE}/${id}/calculate-health`)
  return data
}
