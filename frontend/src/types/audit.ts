export interface AuditEvent {
  id: string
  entityType: string
  entityId: string
  entityVersion?: string | null
  eventType: string
  action: string
  companyId?: string | null
  userId: string
  userName: string
  occurredAt: string
  source: string
  correlationId?: string | null
  sessionId?: string | null
  requestId?: string | null
  previousState?: string | null
  currentState?: string | null
  metadata?: string | null
}

export interface AuditEventQuery {
  entityType?: string
  entityId?: string
  companyId?: string
  userId?: string
  correlationId?: string
  eventType?: string
  action?: string
  search?: string
  occurredFrom?: string
  occurredTo?: string
  orderBy?: string
  orderDirection?: string
}
