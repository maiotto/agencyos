import { apiClient } from './client'
import type {
  ApproveRecommendationWorkflowPayload,
  CreateRecommendationWorkflowPayload,
  RecommendationWorkflow,
  RecommendationWorkflowActionPayload,
  RecommendationWorkflowQuery,
  RecommendationWorkflowTransition,
} from '../types/recommendationWorkflow'

const BASE = '/recommendations/workflow'

export async function listRecommendationWorkflows(
  query: RecommendationWorkflowQuery = {},
): Promise<RecommendationWorkflow[]> {
  const { data } = await apiClient.get<RecommendationWorkflow[]>(BASE, { params: query })
  return data
}

export async function getRecommendationWorkflow(id: string): Promise<RecommendationWorkflow> {
  const { data } = await apiClient.get<RecommendationWorkflow>(`${BASE}/${id}`)
  return data
}

export async function getRecommendationWorkflowTimeline(
  id: string,
): Promise<RecommendationWorkflowTransition[]> {
  const { data } = await apiClient.get<RecommendationWorkflowTransition[]>(`${BASE}/${id}/timeline`)
  return data
}

export async function createRecommendationWorkflow(
  payload: CreateRecommendationWorkflowPayload,
): Promise<RecommendationWorkflow> {
  const { data } = await apiClient.post<RecommendationWorkflow>(BASE, payload)
  return data
}

export async function submitRecommendationWorkflow(
  id: string,
  payload: RecommendationWorkflowActionPayload,
): Promise<RecommendationWorkflow> {
  const { data } = await apiClient.post<RecommendationWorkflow>(`${BASE}/${id}/submit`, payload)
  return data
}

export async function approveRecommendationWorkflow(
  id: string,
  payload: ApproveRecommendationWorkflowPayload,
): Promise<RecommendationWorkflow> {
  const { data } = await apiClient.post<RecommendationWorkflow>(`${BASE}/${id}/approve`, payload)
  return data
}

export async function rejectRecommendationWorkflow(
  id: string,
  payload: RecommendationWorkflowActionPayload,
): Promise<RecommendationWorkflow> {
  const { data } = await apiClient.post<RecommendationWorkflow>(`${BASE}/${id}/reject`, payload)
  return data
}

export async function cancelRecommendationWorkflow(
  id: string,
  payload: RecommendationWorkflowActionPayload,
): Promise<RecommendationWorkflow> {
  const { data } = await apiClient.post<RecommendationWorkflow>(`${BASE}/${id}/cancel`, payload)
  return data
}

export async function reopenRecommendationWorkflow(
  id: string,
  payload: RecommendationWorkflowActionPayload,
): Promise<RecommendationWorkflow> {
  const { data } = await apiClient.post<RecommendationWorkflow>(`${BASE}/${id}/reopen`, payload)
  return data
}
