export const WEEKDAYS = [
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
  'Sunday',
] as const

export type Weekday = (typeof WEEKDAYS)[number]

export interface WorkingHoursDay {
  dayOfWeek: string
  enabled: boolean
  startTime: string | null
  endTime: string | null
  breakStart: string | null
  breakEnd: string | null
  netHours?: number | null
}

export interface WorkingHours {
  id: string
  workingCalendarId: string
  name: string
  status: string
  effectiveFrom: string
  effectiveTo: string | null
  days: WorkingHoursDay[]
  createdAt: string
  updatedAt: string
}

export interface WorkingHoursQuery {
  workingCalendarId?: string
  status?: string
  name?: string
  orderBy?: string
  orderDirection?: 'asc' | 'desc'
}

export interface WorkingHoursPayload {
  workingCalendarId?: string
  name: string
  effectiveFrom: string
  effectiveTo?: string | null
  days: WorkingHoursDay[]
}

export function defaultWeekdaySchedule(): WorkingHoursDay[] {
  return WEEKDAYS.map((day) => {
    const weekday = day !== 'Saturday' && day !== 'Sunday'
    return {
      dayOfWeek: day,
      enabled: weekday,
      startTime: weekday ? '09:00:00' : null,
      endTime: weekday ? '18:00:00' : null,
      breakStart: weekday ? '12:00:00' : null,
      breakEnd: weekday ? '13:00:00' : null,
    }
  })
}

export function toTimeInput(value: string | null | undefined): string {
  if (!value) {
    return ''
  }

  return value.slice(0, 5)
}

export function fromTimeInput(value: string): string | null {
  if (!value) {
    return null
  }

  return value.length === 5 ? `${value}:00` : value
}
