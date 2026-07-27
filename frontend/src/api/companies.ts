import { apiClient } from './client'
import type { Company, CompanyQuery, CreateCompanyPayload, UpdateCompanyPayload } from '../types/company'

export async function listCompanies(query: CompanyQuery = {}): Promise<Company[]> {
  const { data } = await apiClient.get<Company[]>('/companies', { params: query })
  return data
}

export async function getActiveCompany(): Promise<Company> {
  const { data } = await apiClient.get<Company>('/companies/active')
  return data
}

export async function getCompany(id: string): Promise<Company> {
  const { data } = await apiClient.get<Company>(`/companies/${id}`)
  return data
}

export async function createCompany(payload: CreateCompanyPayload): Promise<Company> {
  const { data } = await apiClient.post<Company>('/companies', payload)
  return data
}

export async function updateCompany(id: string, payload: UpdateCompanyPayload): Promise<Company> {
  const { data } = await apiClient.put<Company>(`/companies/${id}`, payload)
  return data
}

export async function activateCompany(id: string): Promise<void> {
  await apiClient.post(`/companies/${id}/activate`)
}

export async function deactivateCompany(id: string): Promise<void> {
  await apiClient.post(`/companies/${id}/deactivate`)
}

export async function archiveCompany(id: string): Promise<void> {
  await apiClient.post(`/companies/${id}/archive`)
}

export async function selectCompany(companyId: string): Promise<void> {
  await apiClient.post('/companies/select', { companyId })
}
