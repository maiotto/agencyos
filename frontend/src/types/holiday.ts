export const HOLIDAY_TYPES = ['National', 'State', 'Municipal', 'Company'] as const

export type HolidayType = (typeof HOLIDAY_TYPES)[number]

export type HolidayStatus = 'Active' | 'Inactive'

export interface Holiday {
  id: string
  companyId: string | null
  name: string
  description: string | null
  holidayType: HolidayType | string
  holidayDate: string
  stateCode: string | null
  city: string | null
  recurring: boolean
  status: HolidayStatus | string
  createdAt: string
  updatedAt: string
}

export interface HolidayQuery {
  companyId?: string
  status?: string
  holidayType?: string
  name?: string
  stateCode?: string
  city?: string
  fromDate?: string
  toDate?: string
  recurring?: boolean
  orderBy?: string
  orderDirection?: 'asc' | 'desc'
}

export interface HolidayPayload {
  companyId?: string | null
  name: string
  description?: string | null
  holidayType: string
  holidayDate: string
  stateCode?: string | null
  city?: string | null
  recurring: boolean
}
