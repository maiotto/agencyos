import { apiClient } from './client'
import type {
  MyWorkActivitySection,
  MyWorkDashboard,
  MyWorkDashboardQuery,
  MyWorkDecisionsSection,
  MyWorkKpiSummary,
  MyWorkMissionsSection,
  MyWorkRecommendationsSection,
  MyWorkSummary,
  MyWorkTasksSection,
} from '../types/myWork'

export async function getMyWorkDashboard(query: MyWorkDashboardQuery): Promise<MyWorkDashboard> {
  const { data } = await apiClient.get<MyWorkDashboard>('/my-work', { params: query })
  return data
}

export async function getMyWorkSummary(query: MyWorkDashboardQuery): Promise<MyWorkSummary> {
  const { data } = await apiClient.get<MyWorkSummary>('/my-work/summary', { params: query })
  return data
}

export async function getMyWorkTasks(query: MyWorkDashboardQuery): Promise<MyWorkTasksSection> {
  const { data } = await apiClient.get<MyWorkTasksSection>('/my-work/tasks', { params: query })
  return data
}

export async function getMyWorkMissions(query: MyWorkDashboardQuery): Promise<MyWorkMissionsSection> {
  const { data } = await apiClient.get<MyWorkMissionsSection>('/my-work/missions', { params: query })
  return data
}

export async function getMyWorkRecommendations(
  query: MyWorkDashboardQuery,
): Promise<MyWorkRecommendationsSection> {
  const { data } = await apiClient.get<MyWorkRecommendationsSection>('/my-work/recommendations', {
    params: query,
  })
  return data
}

export async function getMyWorkDecisions(query: MyWorkDashboardQuery): Promise<MyWorkDecisionsSection> {
  const { data } = await apiClient.get<MyWorkDecisionsSection>('/my-work/decisions', { params: query })
  return data
}

export async function getMyWorkActivity(query: MyWorkDashboardQuery): Promise<MyWorkActivitySection> {
  const { data } = await apiClient.get<MyWorkActivitySection>('/my-work/activity', { params: query })
  return data
}

export async function getMyWorkKpis(query: MyWorkDashboardQuery): Promise<MyWorkKpiSummary> {
  const { data } = await apiClient.get<MyWorkKpiSummary>('/my-work/kpis', { params: query })
  return data
}
