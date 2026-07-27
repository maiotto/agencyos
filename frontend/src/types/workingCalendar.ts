export const WORKING_DAYS = [
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
  'Sunday',
] as const

export type WorkingDay = (typeof WORKING_DAYS)[number]

export type WorkingCalendarStatus = 'Active' | 'Inactive'

export interface WorkingCalendar {
  id: string
  companyId: string
  name: string
  status: WorkingCalendarStatus
  effectiveFrom: string
  effectiveTo: string | null
  workingDays: string[]
  createdAt: string
  updatedAt: string
}

export interface WorkingCalendarQuery {
  companyId?: string
  status?: string
  name?: string
  orderBy?: string
  orderDirection?: 'asc' | 'desc'
}

export interface CreateWorkingCalendarPayload {
  companyId: string
  name: string
  effectiveFrom: string
  effectiveTo?: string | null
  workingDays: string[]
}

export interface UpdateWorkingCalendarPayload {
  name: string
  effectiveFrom: string
  effectiveTo?: string | null
  workingDays: string[]
}
