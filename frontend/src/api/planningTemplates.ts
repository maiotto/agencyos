import { apiClient } from './client'
import type {
  AppliedPlanningConfiguration,
  ApplyPlanningTemplatePayload,
  PlanningTemplate,
  PlanningTemplatePayload,
  PlanningTemplateQuery,
} from '../types/planningTemplate'
import type { WorkingCalendar } from '../types/workingCalendar'
import type { WorkingHours } from '../types/workingHours'

export async function listPlanningTemplates(
  query: PlanningTemplateQuery = {},
): Promise<PlanningTemplate[]> {
  const { data } = await apiClient.get<PlanningTemplate[]>('/planning-templates', { params: query })
  return data
}

export async function filterPlanningTemplates(
  query: PlanningTemplateQuery = {},
): Promise<PlanningTemplate[]> {
  const { data } = await apiClient.get<PlanningTemplate[]>('/planning-templates/filter', {
    params: query,
  })
  return data
}

export async function getPlanningTemplate(id: string): Promise<PlanningTemplate> {
  const { data } = await apiClient.get<PlanningTemplate>(`/planning-templates/${id}`)
  return data
}

export async function createPlanningTemplate(
  payload: PlanningTemplatePayload & { companyId: string },
): Promise<PlanningTemplate> {
  const { data } = await apiClient.post<PlanningTemplate>('/planning-templates', payload)
  return data
}

export async function updatePlanningTemplate(
  id: string,
  payload: PlanningTemplatePayload,
): Promise<PlanningTemplate> {
  const { data } = await apiClient.put<PlanningTemplate>(`/planning-templates/${id}`, payload)
  return data
}

export async function activatePlanningTemplate(id: string): Promise<void> {
  await apiClient.post(`/planning-templates/${id}/activate`)
}

export async function deactivatePlanningTemplate(id: string): Promise<void> {
  await apiClient.post(`/planning-templates/${id}/deactivate`)
}

export async function deletePlanningTemplate(id: string): Promise<void> {
  await apiClient.delete(`/planning-templates/${id}`)
}

export async function clonePlanningTemplate(id: string, name: string): Promise<PlanningTemplate> {
  const { data } = await apiClient.post<PlanningTemplate>(`/planning-templates/${id}/clone`, {
    name,
  })
  return data
}

export async function applyPlanningTemplate(
  id: string,
  payload: ApplyPlanningTemplatePayload,
): Promise<AppliedPlanningConfiguration> {
  const { data } = await apiClient.post<AppliedPlanningConfiguration>(
    `/planning-templates/${id}/apply`,
    payload,
  )
  return data
}

export async function listWorkingCalendarsForTemplate(): Promise<WorkingCalendar[]> {
  const { data } = await apiClient.get<WorkingCalendar[]>('/working-calendars')
  return data
}

export async function listWorkingHoursForTemplate(): Promise<WorkingHours[]> {
  const { data } = await apiClient.get<WorkingHours[]>('/working-hours')
  return data
}
