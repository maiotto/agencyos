import { apiClient } from './client'
import type {
  ResourceAvailability,
  ResourceAvailabilityPayload,
  ResourceAvailabilityQuery,
  ExecutionResourceOption,
} from '../types/resourceAvailability'
import type { WorkingCalendar } from '../types/workingCalendar'
import type { WorkingHours } from '../types/workingHours'

export async function listResourceAvailabilities(
  query: ResourceAvailabilityQuery = {},
): Promise<ResourceAvailability[]> {
  const { data } = await apiClient.get<ResourceAvailability[]>('/resource-availabilities', {
    params: query,
  })
  return data
}

export async function getResourceAvailability(id: string): Promise<ResourceAvailability> {
  const { data } = await apiClient.get<ResourceAvailability>(`/resource-availabilities/${id}`)
  return data
}

export async function createResourceAvailability(
  payload: ResourceAvailabilityPayload,
): Promise<ResourceAvailability> {
  const { data } = await apiClient.post<ResourceAvailability>('/resource-availabilities', payload)
  return data
}

export async function updateResourceAvailability(
  id: string,
  payload: ResourceAvailabilityPayload,
): Promise<ResourceAvailability> {
  const { data } = await apiClient.put<ResourceAvailability>(`/resource-availabilities/${id}`, payload)
  return data
}

export async function activateResourceAvailability(id: string): Promise<void> {
  await apiClient.post(`/resource-availabilities/${id}/activate`)
}

export async function deactivateResourceAvailability(id: string): Promise<void> {
  await apiClient.post(`/resource-availabilities/${id}/deactivate`)
}

export async function deleteResourceAvailability(id: string): Promise<void> {
  await apiClient.delete(`/resource-availabilities/${id}`)
}

export async function listExecutionResourcesForSelect(): Promise<ExecutionResourceOption[]> {
  const { data } = await apiClient.get<ExecutionResourceOption[]>('/execution-resources', {
    params: { status: 'Active' },
  })
  return data
}

export async function listWorkingCalendarsForSelect(): Promise<WorkingCalendar[]> {
  const { data } = await apiClient.get<WorkingCalendar[]>('/working-calendars')
  return data
}

export async function listWorkingHoursForSelect(workingCalendarId?: string): Promise<WorkingHours[]> {
  const { data } = await apiClient.get<WorkingHours[]>('/working-hours', {
    params: workingCalendarId ? { workingCalendarId } : undefined,
  })
  return data
}
