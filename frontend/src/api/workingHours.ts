import { apiClient } from './client'
import type { WorkingHours, WorkingHoursPayload, WorkingHoursQuery } from '../types/workingHours'
import type { WorkingCalendar } from '../types/workingCalendar'

export async function listWorkingHours(query: WorkingHoursQuery = {}): Promise<WorkingHours[]> {
  const { data } = await apiClient.get<WorkingHours[]>('/working-hours', { params: query })
  return data
}

export async function getWorkingHours(id: string): Promise<WorkingHours> {
  const { data } = await apiClient.get<WorkingHours>(`/working-hours/${id}`)
  return data
}

export async function createWorkingHours(payload: WorkingHoursPayload): Promise<WorkingHours> {
  const { data } = await apiClient.post<WorkingHours>('/working-hours', payload)
  return data
}

export async function updateWorkingHours(
  id: string,
  payload: WorkingHoursPayload,
): Promise<WorkingHours> {
  const { data } = await apiClient.put<WorkingHours>(`/working-hours/${id}`, payload)
  return data
}

export async function activateWorkingHours(id: string): Promise<void> {
  await apiClient.post(`/working-hours/${id}/activate`)
}

export async function deactivateWorkingHours(id: string): Promise<void> {
  await apiClient.post(`/working-hours/${id}/deactivate`)
}

export async function deleteWorkingHours(id: string): Promise<void> {
  await apiClient.delete(`/working-hours/${id}`)
}

export async function listWorkingCalendarsForSelect(): Promise<WorkingCalendar[]> {
  const { data } = await apiClient.get<WorkingCalendar[]>('/working-calendars')
  return data
}
