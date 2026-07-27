import { apiClient } from './client'
import type {
  PersonalProductivityActivity,
  PersonalProductivityCapacity,
  PersonalProductivityDashboard,
  PersonalProductivityKpi,
  PersonalProductivityQuery,
  PersonalProductivityStatistics,
  PersonalProductivityTrends,
  PersonalProductivityWorkload,
} from '../types/personalProductivity'

export async function getPersonalProductivityDashboard(
  query: PersonalProductivityQuery,
): Promise<PersonalProductivityDashboard> {
  const { data } = await apiClient.get<PersonalProductivityDashboard>('/personal-dashboard', {
    params: query,
  })
  return data
}

export async function getPersonalProductivityKpis(
  query: PersonalProductivityQuery,
): Promise<PersonalProductivityKpi> {
  const { data } = await apiClient.get<PersonalProductivityKpi>('/personal-dashboard/kpis', {
    params: query,
  })
  return data
}

export async function getPersonalProductivityTrends(
  query: PersonalProductivityQuery,
): Promise<PersonalProductivityTrends> {
  const { data } = await apiClient.get<PersonalProductivityTrends>('/personal-dashboard/trends', {
    params: query,
  })
  return data
}

export async function getPersonalProductivityCapacity(
  query: PersonalProductivityQuery,
): Promise<PersonalProductivityCapacity> {
  const { data } = await apiClient.get<PersonalProductivityCapacity>('/personal-dashboard/capacity', {
    params: query,
  })
  return data
}

export async function getPersonalProductivityWorkload(
  query: PersonalProductivityQuery,
): Promise<PersonalProductivityWorkload> {
  const { data } = await apiClient.get<PersonalProductivityWorkload>('/personal-dashboard/workload', {
    params: query,
  })
  return data
}

export async function getPersonalProductivityActivity(
  query: PersonalProductivityQuery,
): Promise<PersonalProductivityActivity> {
  const { data } = await apiClient.get<PersonalProductivityActivity>('/personal-dashboard/activity', {
    params: query,
  })
  return data
}

export async function getPersonalProductivityStatistics(
  query: PersonalProductivityQuery,
): Promise<PersonalProductivityStatistics> {
  const { data } = await apiClient.get<PersonalProductivityStatistics>(
    '/personal-dashboard/statistics',
    { params: query },
  )
  return data
}
