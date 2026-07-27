import { WEEKDAYS, type Weekday } from './workingHours'

export interface ResourceAvailabilityWeekDay {
  dayOfWeek: string
  enabled: boolean
}

export interface ResourceAvailabilityDayOverride {
  overrideDate: string
  available: boolean
  startTime: string | null
  endTime: string | null
  notes: string | null
}

export interface ResourceAvailability {
  id: string
  executionResourceId: string
  workingCalendarId: string
  workingHoursId: string
  name: string
  status: string
  effectiveFrom: string
  effectiveTo: string | null
  weeklyAvailability: ResourceAvailabilityWeekDay[]
  dailyOverrides: ResourceAvailabilityDayOverride[]
  createdAt: string
  updatedAt: string
}

export interface ResourceAvailabilityQuery {
  executionResourceId?: string
  workingCalendarId?: string
  status?: string
  name?: string
  orderBy?: string
  orderDirection?: 'asc' | 'desc'
}

export interface ResourceAvailabilityPayload {
  executionResourceId?: string
  workingCalendarId: string
  workingHoursId: string
  name: string
  effectiveFrom: string
  effectiveTo?: string | null
  weeklyAvailability: ResourceAvailabilityWeekDay[]
  dailyOverrides: ResourceAvailabilityDayOverride[]
}

export interface ExecutionResourceOption {
  id: string
  code: string
  name: string
  status: string
}

export function defaultWeeklyAvailability(): ResourceAvailabilityWeekDay[] {
  return WEEKDAYS.map((day: Weekday) => ({
    dayOfWeek: day,
    enabled: day !== 'Saturday' && day !== 'Sunday',
  }))
}
