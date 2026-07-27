import { apiClient } from './client'
import type { ExecutiveWorkspace, ExecutiveWorkspaceQuery } from '../types/executiveWorkspace'

export async function getExecutiveWorkspace(query: ExecutiveWorkspaceQuery): Promise<ExecutiveWorkspace> {
  const { data } = await apiClient.get<ExecutiveWorkspace>('/executive-workspace', { params: query })
  return data
}
