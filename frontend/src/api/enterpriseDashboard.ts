import { apiClient } from './client'
import type {
  EnterpriseDashboard,
  EnterpriseDashboardAi,
  EnterpriseDashboardAudit,
  EnterpriseDashboardCapacity,
  EnterpriseDashboardDecisions,
  EnterpriseDashboardPlanning,
  EnterpriseDashboardPortfolio,
  EnterpriseDashboardQuery,
  EnterpriseDashboardRecommendations,
  EnterpriseDashboardSummary,
  EnterpriseDashboardWorkload,
} from '../types/enterpriseDashboard'

export async function getEnterpriseDashboard(
  query: EnterpriseDashboardQuery,
): Promise<EnterpriseDashboard> {
  const { data } = await apiClient.get<EnterpriseDashboard>('/enterprise-dashboard', { params: query })
  return data
}

export async function getEnterpriseDashboardSummary(
  query: EnterpriseDashboardQuery,
): Promise<EnterpriseDashboardSummary> {
  const { data } = await apiClient.get<EnterpriseDashboardSummary>('/enterprise-dashboard/summary', {
    params: query,
  })
  return data
}

export async function getEnterpriseDashboardPlanning(
  query: EnterpriseDashboardQuery,
): Promise<EnterpriseDashboardPlanning> {
  const { data } = await apiClient.get<EnterpriseDashboardPlanning>('/enterprise-dashboard/planning', {
    params: query,
  })
  return data
}

export async function getEnterpriseDashboardPortfolio(
  query: EnterpriseDashboardQuery,
): Promise<EnterpriseDashboardPortfolio> {
  const { data } = await apiClient.get<EnterpriseDashboardPortfolio>('/enterprise-dashboard/portfolio', {
    params: query,
  })
  return data
}

export async function getEnterpriseDashboardCapacity(
  query: EnterpriseDashboardQuery,
): Promise<EnterpriseDashboardCapacity> {
  const { data } = await apiClient.get<EnterpriseDashboardCapacity>('/enterprise-dashboard/capacity', {
    params: query,
  })
  return data
}

export async function getEnterpriseDashboardWorkload(
  query: EnterpriseDashboardQuery,
): Promise<EnterpriseDashboardWorkload> {
  const { data } = await apiClient.get<EnterpriseDashboardWorkload>('/enterprise-dashboard/workload', {
    params: query,
  })
  return data
}

export async function getEnterpriseDashboardRecommendations(
  query: EnterpriseDashboardQuery,
): Promise<EnterpriseDashboardRecommendations> {
  const { data } = await apiClient.get<EnterpriseDashboardRecommendations>(
    '/enterprise-dashboard/recommendations',
    { params: query },
  )
  return data
}

export async function getEnterpriseDashboardDecisions(
  query: EnterpriseDashboardQuery,
): Promise<EnterpriseDashboardDecisions> {
  const { data } = await apiClient.get<EnterpriseDashboardDecisions>('/enterprise-dashboard/decisions', {
    params: query,
  })
  return data
}

export async function getEnterpriseDashboardAi(
  query: EnterpriseDashboardQuery,
): Promise<EnterpriseDashboardAi> {
  const { data } = await apiClient.get<EnterpriseDashboardAi>('/enterprise-dashboard/ai', {
    params: query,
  })
  return data
}

export async function getEnterpriseDashboardAudit(
  query: EnterpriseDashboardQuery,
): Promise<EnterpriseDashboardAudit> {
  const { data } = await apiClient.get<EnterpriseDashboardAudit>('/enterprise-dashboard/audit', {
    params: query,
  })
  return data
}
