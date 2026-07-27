import { apiClient } from './client'
import type {
  CreateWorkingCalendarPayload,
  UpdateWorkingCalendarPayload,
  WorkingCalendar,
  WorkingCalendarQuery,
} from '../types/workingCalendar'

export async function listWorkingCalendars(
  query: WorkingCalendarQuery = {},
): Promise<WorkingCalendar[]> {
  const { data } = await apiClient.get<WorkingCalendar[]>('/working-calendars', {
    params: query,
  })
  return data
}

export async function getWorkingCalendar(id: string): Promise<WorkingCalendar> {
  const { data } = await apiClient.get<WorkingCalendar>(`/working-calendars/${id}`)
  return data
}

export async function createWorkingCalendar(
  payload: CreateWorkingCalendarPayload,
): Promise<WorkingCalendar> {
  const { data } = await apiClient.post<WorkingCalendar>('/working-calendars', payload)
  return data
}

export async function updateWorkingCalendar(
  id: string,
  payload: UpdateWorkingCalendarPayload,
): Promise<WorkingCalendar> {
  const { data } = await apiClient.put<WorkingCalendar>(`/working-calendars/${id}`, payload)
  return data
}

export async function activateWorkingCalendar(id: string): Promise<void> {
  await apiClient.post(`/working-calendars/${id}/activate`)
}

export async function deactivateWorkingCalendar(id: string): Promise<void> {
  await apiClient.post(`/working-calendars/${id}/deactivate`)
}

export async function deleteWorkingCalendar(id: string): Promise<void> {
  await apiClient.delete(`/working-calendars/${id}`)
}
