import { apiClient } from './client'
import type {
  WorkloadHistory,
  WorkloadHistoryAggregate,
  WorkloadHistoryCompare,
  WorkloadHistoryQuery,
  WorkloadHistoryTrend,
} from '../types/workloadHistory'

export async function listWorkloadHistory(query: WorkloadHistoryQuery = {}): Promise<WorkloadHistory[]> {
  const { data } = await apiClient.get<WorkloadHistory[]>('/workload/history', { params: query })
  return data
}

export async function getWorkloadHistory(id: string): Promise<WorkloadHistory> {
  const { data } = await apiClient.get<WorkloadHistory>(`/workload/history/${id}`)
  return data
}

export async function getWorkloadHistoryByResource(
  executionResourceId: string,
  query: WorkloadHistoryQuery = {},
): Promise<WorkloadHistory[]> {
  const { data } = await apiClient.get<WorkloadHistory[]>(
    `/workload/history/resource/${executionResourceId}`,
    { params: query },
  )
  return data
}

export async function getWorkloadHistoryByCompany(
  companyId: string,
  query: WorkloadHistoryQuery = {},
): Promise<WorkloadHistory[]> {
  const { data } = await apiClient.get<WorkloadHistory[]>(
    `/workload/history/company/${companyId}`,
    { params: query },
  )
  return data
}

export async function compareWorkloadHistory(
  leftHistoryId: string,
  rightHistoryId: string,
): Promise<WorkloadHistoryCompare> {
  const { data } = await apiClient.get<WorkloadHistoryCompare>('/workload/history/compare', {
    params: { leftHistoryId, rightHistoryId },
  })
  return data
}

export async function aggregateWorkloadHistory(
  query: WorkloadHistoryQuery = {},
): Promise<WorkloadHistoryAggregate> {
  const { data } = await apiClient.get<WorkloadHistoryAggregate>('/workload/history/aggregate', {
    params: query,
  })
  return data
}

export async function getWorkloadHistoryTrends(
  query: WorkloadHistoryQuery = {},
): Promise<WorkloadHistoryTrend> {
  const { data } = await apiClient.get<WorkloadHistoryTrend>('/workload/history/trends', {
    params: query,
  })
  return data
}
