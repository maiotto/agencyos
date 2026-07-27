import { apiClient } from './client'
import type { DecisionWorkspace, DecisionWorkspaceQuery } from '../types/decisionWorkspace'

export async function getDecisionWorkspace(query: DecisionWorkspaceQuery): Promise<DecisionWorkspace> {
  const { data } = await apiClient.get<DecisionWorkspace>('/decision-workspace', { params: query })
  return data
}
