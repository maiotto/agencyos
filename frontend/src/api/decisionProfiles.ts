import { apiClient } from './client'
import type {
  CloneCompanyDecisionProfilePayload,
  CompanyDecisionProfile,
  CompanyDecisionProfileQuery,
  CreateCompanyDecisionProfilePayload,
  UpdateCompanyDecisionProfilePayload,
} from '../types/decisionProfile'

export async function listDecisionProfiles(
  query: CompanyDecisionProfileQuery = {},
): Promise<CompanyDecisionProfile[]> {
  const { data } = await apiClient.get<CompanyDecisionProfile[]>('/decision-profiles', {
    params: query,
  })
  return data
}

export async function filterDecisionProfiles(
  query: CompanyDecisionProfileQuery = {},
): Promise<CompanyDecisionProfile[]> {
  const { data } = await apiClient.get<CompanyDecisionProfile[]>('/decision-profiles/filter', {
    params: query,
  })
  return data
}

export async function getDecisionProfile(id: string): Promise<CompanyDecisionProfile> {
  const { data } = await apiClient.get<CompanyDecisionProfile>(`/decision-profiles/${id}`)
  return data
}

export async function getDecisionProfilesByCompany(
  companyId: string,
): Promise<CompanyDecisionProfile[]> {
  const { data } = await apiClient.get<CompanyDecisionProfile[]>(
    `/decision-profiles/company/${companyId}`,
  )
  return data
}

export async function getDefaultActiveDecisionProfile(
  companyId: string,
): Promise<CompanyDecisionProfile> {
  const { data } = await apiClient.get<CompanyDecisionProfile>(
    `/decision-profiles/company/${companyId}/default`,
  )
  return data
}

export async function createDecisionProfile(
  payload: CreateCompanyDecisionProfilePayload,
): Promise<CompanyDecisionProfile> {
  const { data } = await apiClient.post<CompanyDecisionProfile>('/decision-profiles', payload)
  return data
}

export async function updateDecisionProfile(
  id: string,
  payload: UpdateCompanyDecisionProfilePayload,
): Promise<CompanyDecisionProfile> {
  const { data } = await apiClient.put<CompanyDecisionProfile>(`/decision-profiles/${id}`, payload)
  return data
}

export async function cloneDecisionProfile(
  id: string,
  payload: CloneCompanyDecisionProfilePayload,
): Promise<CompanyDecisionProfile> {
  const { data } = await apiClient.post<CompanyDecisionProfile>(
    `/decision-profiles/${id}/clone`,
    payload,
  )
  return data
}

export async function activateDecisionProfile(id: string): Promise<void> {
  await apiClient.post(`/decision-profiles/${id}/activate`)
}

export async function deactivateDecisionProfile(id: string): Promise<void> {
  await apiClient.post(`/decision-profiles/${id}/deactivate`)
}

export async function archiveDecisionProfile(id: string): Promise<void> {
  await apiClient.post(`/decision-profiles/${id}/archive`)
}

export async function setDefaultDecisionProfile(id: string): Promise<CompanyDecisionProfile> {
  const { data } = await apiClient.post<CompanyDecisionProfile>(`/decision-profiles/${id}/set-default`)
  return data
}

export async function clearDefaultDecisionProfile(id: string): Promise<CompanyDecisionProfile> {
  const { data } = await apiClient.post<CompanyDecisionProfile>(
    `/decision-profiles/${id}/clear-default`,
  )
  return data
}
