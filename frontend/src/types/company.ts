export type CompanyStatus = 'Active' | 'Inactive' | 'Archived'

export interface Company {
  id: string
  companyCode: string
  companyName: string
  legalName: string | null
  status: CompanyStatus | string
  timezone: string
  country: string | null
  language: string | null
  currency: string | null
  planningConfiguration: string
  decisionProfileId: string | null
  defaultCalendarId: string | null
  defaultPlanningTemplateId: string | null
  createdAt: string
  updatedAt: string
  archivedAt: string | null
}

export interface CompanyQuery {
  status?: string
  search?: string
  orderBy?: string
  orderDirection?: 'asc' | 'desc'
  includeArchived?: boolean
}

export interface CreateCompanyPayload {
  companyCode: string
  companyName: string
  legalName?: string | null
  timezone: string
  country?: string | null
  language?: string | null
  currency?: string | null
  planningConfiguration?: string | null
  decisionProfileId?: string | null
  defaultCalendarId?: string | null
  defaultPlanningTemplateId?: string | null
}

export interface UpdateCompanyPayload {
  companyName: string
  legalName?: string | null
  timezone: string
  country?: string | null
  language?: string | null
  currency?: string | null
  planningConfiguration?: string | null
  decisionProfileId?: string | null
  defaultCalendarId?: string | null
  defaultPlanningTemplateId?: string | null
}
