export interface CapacityHistoryDay {
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

export interface CapacityHistory {
  historyId: string
  executionResourceId: string
  companyId: string
  calculationDate: string
  periodStart: string
  periodEnd: string
  workingDays: number
  holidayDays: number
  availableDays: number
  configuredHours: number
  availableHours: number
  capacityHours: number
  allocatedHours: number
  utilizationPercentage: number
  calculationVersion: string
  createdAt: string
  operationalDays: CapacityHistoryDay[]
}

export interface CapacityHistoryQuery {
  executionResourceId?: string
  companyId?: string
  periodStart?: string
  periodEnd?: string
  calculationVersion?: string
  calculatedFrom?: string
  calculatedTo?: string
}

export interface CapacityHistoryAggregate {
  companyId?: string | null
  executionResourceId?: string | null
  periodStart?: string | null
  periodEnd?: string | null
  calculationVersion?: string | null
  recordCount: number
  totalConfiguredHours: number
  totalAvailableHours: number
  totalCapacityHours: number
  totalAllocatedHours: number
  averageUtilizationPercentage: number
}

export interface CapacityHistoryCompare {
  left: CapacityHistory
  right: CapacityHistory
  capacityHoursDelta: number
  availableHoursDelta: number
  configuredHoursDelta: number
  utilizationPercentageDelta: number
  workingDaysDelta: number
  holidayDaysDelta: number
  availableDaysDelta: number
}
