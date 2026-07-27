export interface WorkloadHistoryAssignment {
  assignmentId: string
  taskId: string
  assignmentRole: string
  plannedHours: number
  plannedStartDate: string
  plannedEndDate: string
  status: string
}

export interface WorkloadHistory {
  historyId: string
  executionResourceId: string
  companyId: string
  calculationDate: string
  periodStart: string
  periodEnd: string
  allocatedHours: number
  capacityHours: number
  workloadPercentage: number
  workingDays: number
  holidayDays: number
  availableDays: number
  calculationVersion: string
  createdAt: string
  assignmentCount: number
  averageHoursPerAssignment: number
  executionResourceCode: string
  executionResourceName: string
  assignmentDistribution: WorkloadHistoryAssignment[]
}

export interface WorkloadHistoryQuery {
  executionResourceId?: string
  companyId?: string
  periodStart?: string
  periodEnd?: string
  calculationVersion?: string
  calculatedFrom?: string
  calculatedTo?: string
}

export interface WorkloadHistoryAggregate {
  companyId?: string | null
  executionResourceId?: string | null
  periodStart?: string | null
  periodEnd?: string | null
  calculationVersion?: string | null
  recordCount: number
  totalAllocatedHours: number
  totalCapacityHours: number
  averageWorkloadPercentage: number
}

export interface WorkloadHistoryCompare {
  left: WorkloadHistory
  right: WorkloadHistory
  allocatedHoursDelta: number
  capacityHoursDelta: number
  workloadPercentageDelta: number
  workingDaysDelta: number
  holidayDaysDelta: number
  availableDaysDelta: number
}

export interface WorkloadHistoryTrendPoint {
  calculationDate: string
  periodStart: string
  periodEnd: string
  allocatedHours: number
  capacityHours: number
  workloadPercentage: number
  calculationVersion: string
}

export interface WorkloadHistoryTrend {
  companyId?: string | null
  executionResourceId?: string | null
  pointCount: number
  points: WorkloadHistoryTrendPoint[]
}
