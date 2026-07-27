import { apiClient } from './client'
import type { Holiday, HolidayPayload, HolidayQuery } from '../types/holiday'

export async function listHolidays(query: HolidayQuery = {}): Promise<Holiday[]> {
  const { data } = await apiClient.get<Holiday[]>('/holidays', { params: query })
  return data
}

export async function filterHolidays(query: HolidayQuery = {}): Promise<Holiday[]> {
  const { data } = await apiClient.get<Holiday[]>('/holidays/filter', { params: query })
  return data
}

export async function getHoliday(id: string): Promise<Holiday> {
  const { data } = await apiClient.get<Holiday>(`/holidays/${id}`)
  return data
}

export async function createHoliday(payload: HolidayPayload): Promise<Holiday> {
  const { data } = await apiClient.post<Holiday>('/holidays', payload)
  return data
}

export async function updateHoliday(id: string, payload: HolidayPayload): Promise<Holiday> {
  const { data } = await apiClient.put<Holiday>(`/holidays/${id}`, payload)
  return data
}

export async function activateHoliday(id: string): Promise<void> {
  await apiClient.post(`/holidays/${id}/activate`)
}

export async function deactivateHoliday(id: string): Promise<void> {
  await apiClient.post(`/holidays/${id}/deactivate`)
}

export async function deleteHoliday(id: string): Promise<void> {
  await apiClient.delete(`/holidays/${id}`)
}
