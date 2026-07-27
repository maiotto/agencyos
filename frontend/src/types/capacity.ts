export interface CapacityDayBreakdown {
  date: string
  isOperationalDay: boolean
  isCalendarWorkingWeekday: boolean
  isHoliday: boolean
  isResourceAvailable: boolean
  plannedCapacityHours: number
  resourceAvailabilityId?: string | null
  workingCalendarId?: string | null
  workingHoursId?: string | null
  exclusionReason?: string | null
}

export interface Capacity {
  executionResourceId: string
  executionResourceCode: string
  executionResourceName: string
  periodStartDate: string
  periodEndDate: string
  operationalDayCount: number
  holidayImpactDayCount: number
  resourceAvailabilityExcludedDayCount: number
  configuredWorkingHoursTotal: number
  totalCapacityHours: number
  allocatedHours: number
  availableHours: number
  utilizationPercentage: number
  remainingCapacityHours: number
  operationalDays: CapacityDayBreakdown[]
}

export interface CapacitySummary {
  periodStartDate: string
  periodEndDate: string
  activeResourceCount: number
  totalCapacityHours: number
  totalAllocatedHours: number
  totalAvailableHours: number
  overallUtilizationPercentage: number
  totalRemainingCapacityHours: number
}

export interface CapacityQuery {
  periodStartDate: string
  periodEndDate: string
  excludeMissionId?: string
}
