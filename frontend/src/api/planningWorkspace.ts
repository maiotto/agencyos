import { apiClient } from './client'
import type { PlanningWorkspace, PlanningWorkspaceQuery } from '../types/planningWorkspace'

export async function getPlanningWorkspace(query: PlanningWorkspaceQuery): Promise<PlanningWorkspace> {
  const { data } = await apiClient.get<PlanningWorkspace>('/planning-workspace', { params: query })
  return data
}
