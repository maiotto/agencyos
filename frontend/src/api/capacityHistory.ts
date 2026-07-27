import { apiClient } from './client'
import type {
  CapacityHistory,
  CapacityHistoryAggregate,
  CapacityHistoryCompare,
  CapacityHistoryQuery,
} from '../types/capacityHistory'

export async function listCapacityHistory(query: CapacityHistoryQuery = {}): Promise<CapacityHistory[]> {
  const { data } = await apiClient.get<CapacityHistory[]>('/capacity/history', { params: query })
  return data
}

export async function getCapacityHistory(id: string): Promise<CapacityHistory> {
  const { data } = await apiClient.get<CapacityHistory>(`/capacity/history/${id}`)
  return data
}

export async function getCapacityHistoryByResource(
  executionResourceId: string,
  query: CapacityHistoryQuery = {},
): Promise<CapacityHistory[]> {
  const { data } = await apiClient.get<CapacityHistory[]>(
    `/capacity/history/resource/${executionResourceId}`,
    { params: query },
  )
  return data
}

export async function getCapacityHistoryByCompany(
  companyId: string,
  query: CapacityHistoryQuery = {},
): Promise<CapacityHistory[]> {
  const { data } = await apiClient.get<CapacityHistory[]>(
    `/capacity/history/company/${companyId}`,
    { params: query },
  )
  return data
}

export async function compareCapacityHistory(
  leftHistoryId: string,
  rightHistoryId: string,
): Promise<CapacityHistoryCompare> {
  const { data } = await apiClient.get<CapacityHistoryCompare>('/capacity/history/compare', {
    params: { leftHistoryId, rightHistoryId },
  })
  return data
}

export async function aggregateCapacityHistory(
  query: CapacityHistoryQuery = {},
): Promise<CapacityHistoryAggregate> {
  const { data } = await apiClient.get<CapacityHistoryAggregate>('/capacity/history/aggregate', {
    params: query,
  })
  return data
}
