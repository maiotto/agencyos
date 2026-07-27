import { apiClient } from './client'
import type { Capacity, CapacityQuery, CapacitySummary } from '../types/capacity'

export async function listCapacity(query: CapacityQuery): Promise<Capacity[]> {
  const { data } = await apiClient.get<Capacity[]>('/capacity', { params: query })
  return data
}

export async function getCapacitySummary(query: CapacityQuery): Promise<CapacitySummary> {
  const { data } = await apiClient.get<CapacitySummary>('/capacity/summary', { params: query })
  return data
}

export async function getCapacityForResource(
  resourceId: string,
  query: CapacityQuery,
): Promise<Capacity> {
  const { data } = await apiClient.get<Capacity>(`/capacity/${resourceId}`, { params: query })
  return data
}
