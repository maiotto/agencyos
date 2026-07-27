import { apiClient } from './client'
import type {
  CreateDecisionRequest,
  Decision,
  DecisionActionRequest,
  DecisionQuery,
  DecisionTimelineEntry,
  RecordDecisionOutcomeRequest,
} from '../types/decision'

const BASE = '/decisions'

export async function listDecisions(query: DecisionQuery = {}): Promise<Decision[]> {
  const { data } = await apiClient.get<Decision[]>(BASE, { params: query })
  return data
}

export async function filterDecisions(query: DecisionQuery = {}): Promise<Decision[]> {
  const { data } = await apiClient.get<Decision[]>(`${BASE}/filter`, { params: query })
  return data
}

export async function getDecision(id: string): Promise<Decision> {
  const { data } = await apiClient.get<Decision>(`${BASE}/${id}`)
  return data
}

export async function getDecisionTimeline(id: string): Promise<DecisionTimelineEntry[]> {
  const { data } = await apiClient.get<DecisionTimelineEntry[]>(`${BASE}/${id}/timeline`)
  return data
}

export async function createDecision(request: CreateDecisionRequest): Promise<Decision> {
  const { data } = await apiClient.post<Decision>(BASE, request)
  return data
}

export async function startDecision(id: string, request: DecisionActionRequest): Promise<Decision> {
  const { data } = await apiClient.post<Decision>(`${BASE}/${id}/start`, request)
  return data
}

export async function completeDecision(
  id: string,
  request: DecisionActionRequest,
): Promise<Decision> {
  const { data } = await apiClient.post<Decision>(`${BASE}/${id}/complete`, request)
  return data
}

export async function cancelDecision(id: string, request: DecisionActionRequest): Promise<Decision> {
  const { data } = await apiClient.post<Decision>(`${BASE}/${id}/cancel`, request)
  return data
}

export async function recordDecisionOutcome(
  id: string,
  request: RecordDecisionOutcomeRequest,
): Promise<Decision> {
  const { data } = await apiClient.post<Decision>(`${BASE}/${id}/outcome`, request)
  return data
}
