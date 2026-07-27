export type PlanningTemplateStatus = 'Active' | 'Inactive'

export interface PlanningTemplate {
  id: string
  companyId: string
  name: string
  description?: string | null
  status: PlanningTemplateStatus | string
  workingCalendarId: string
  workingHoursId: string
  resourceAvailabilityStrategy: string
  defaultPlanningWindowDays: number
  defaultPeriodStartOffsetDays: number
  utilizationWarningPercentage?: number | null
  includeAssignmentDistribution: boolean
  planningParametersJson?: string | null
  createdAt: string
  updatedAt: string
}

export interface PlanningTemplatePayload {
  companyId?: string
  name: string
  description?: string
  workingCalendarId: string
  workingHoursId: string
  resourceAvailabilityStrategy: string
  defaultPlanningWindowDays: number
  defaultPeriodStartOffsetDays: number
  utilizationWarningPercentage?: number | null
  includeAssignmentDistribution: boolean
  planningParametersJson?: string | null
}

export interface PlanningTemplateQuery {
  companyId?: string
  name?: string
  status?: string
  workingCalendarId?: string
  workingHoursId?: string
  search?: string
  orderBy?: string
  orderDirection?: string
}

export interface ApplyPlanningTemplatePayload {
  periodStartDate?: string
  periodEndDate?: string
  executionResourceId?: string
  excludeMissionId?: string
  calculateCapacity?: boolean
  calculateWorkload?: boolean
}

export interface AppliedPlanningConfiguration {
  sourceTemplateId: string
  sourceTemplateName: string
  companyId: string
  workingCalendarId: string
  workingHoursId: string
  resourceAvailabilityStrategy: string
  periodStartDate: string
  periodEndDate: string
  utilizationWarningPercentage?: number | null
  includeAssignmentDistribution: boolean
  planningParametersJson?: string | null
  executionResourceId?: string | null
  excludeMissionId?: string | null
  capacityResults?: unknown[] | null
  capacitySummary?: unknown | null
  workloadResults?: unknown[] | null
  workloadSummary?: unknown | null
}
