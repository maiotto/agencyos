import { apiClient } from './client'
import type { AuditEvent, AuditEventQuery } from '../types/audit'

const BASE = '/audit'

export async function listAuditEvents(query: AuditEventQuery = {}): Promise<AuditEvent[]> {
  const { data } = await apiClient.get<AuditEvent[]>(BASE, { params: query })
  return data
}

export async function filterAuditEvents(query: AuditEventQuery = {}): Promise<AuditEvent[]> {
  const { data } = await apiClient.get<AuditEvent[]>(`${BASE}/filter`, { params: query })
  return data
}

export async function getAuditEvent(id: string): Promise<AuditEvent> {
  const { data } = await apiClient.get<AuditEvent>(`${BASE}/${id}`)
  return data
}

export async function getAuditByEntity(entityId: string): Promise<AuditEvent[]> {
  const { data } = await apiClient.get<AuditEvent[]>(`${BASE}/entity/${entityId}`)
  return data
}

export async function getAuditByCorrelation(correlationId: string): Promise<AuditEvent[]> {
  const { data } = await apiClient.get<AuditEvent[]>(`${BASE}/correlation/${correlationId}`)
  return data
}

export async function getAuditByUser(userId: string): Promise<AuditEvent[]> {
  const { data } = await apiClient.get<AuditEvent[]>(`${BASE}/user/${encodeURIComponent(userId)}`)
  return data
}

export async function getAuditByCompany(companyId: string): Promise<AuditEvent[]> {
  const { data } = await apiClient.get<AuditEvent[]>(`${BASE}/company/${companyId}`)
  return data
}
