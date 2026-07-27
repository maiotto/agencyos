export interface PersonalProductivityQuery {
  companyId?: string
  userId?: string
  executionResourceId?: string
  from?: string
  to?: string
  periodStart?: string
  periodEnd?: string
}

export interface PersonalProductivityKpi {
  assignedTaskCount: number
  assignedMissionCount: number
  pendingRecommendationCount: number
  pendingDecisionCount: number
  completedDecisionCount: number
  overdueTaskCount: number
  upcomingDeadlineCount: number
  activityEventCount: number
  utilizationPercentage?: number | null
  workloadPercentage?: number | null
  completionRatePercentage?: number | null
}

export interface PersonalProductivityTrendPoint {
  metric: string
  currentValue?: number | null
  previousValue?: number | null
  delta?: number | null
  direction: string
  unit: string
}

export interface PersonalProductivityTrends {
  currentPeriodStart: string
  currentPeriodEnd: string
  previousPeriodStart: string
  previousPeriodEnd: string
  points: PersonalProductivityTrendPoint[]
}

export interface PersonalProductivityActivityItem {
  id: string
  kind: string
  title: string
  status: string
  occurredAt?: string | null
  drillDownPath: string
}

export interface PersonalProductivityActivity {
  pending: PersonalProductivityActivityItem[]
  completed: PersonalProductivityActivityItem[]
  timeline: Array<{
    id: string
    occurredAt: string
    entityType: string
    entityId: string
    eventType: string
    action: string
    summary: string
    drillDownPath: string
  }>
}

export interface PersonalProductivityPerformance {
  utilizationPercentage?: number | null
  workloadPercentage?: number | null
  completionRatePercentage?: number | null
  onTimeTaskPercentage?: number | null
  overdueTaskCount: number
  activityIntensity: number
  focusHint: string
}

export interface PersonalProductivityStatistics {
  missionCount: number
  pendingTaskCount: number
  pendingRecommendationCount: number
  pendingDecisionCount: number
  completedDecisionCount: number
  overdueTaskCount: number
  upcomingDeadlineCount: number
  activityEventCount: number
  totalPendingPlannedHours: number
  capacityHours?: number | null
  workloadHours?: number | null
  navigationLinks: Array<{ label: string; path: string; category: string }>
}

export interface PersonalProductivityCapacity {
  hasData: boolean
  executionResourceId?: string | null
  periodStartDate: string
  periodEndDate: string
  totalCapacityHours: number
  allocatedHours: number
  availableHours: number
  utilizationPercentage: number
  drillDownPath: string
}

export interface PersonalProductivityWorkload {
  hasData: boolean
  executionResourceId?: string | null
  periodStartDate: string
  periodEndDate: string
  totalPlannedHours: number
  assignmentCount: number
  workloadPercentage: number
  drillDownPath: string
}

export interface PersonalProductivityDashboard {
  companyId: string
  userId: string
  executionResourceId?: string | null
  generatedAt: string
  from: string
  to: string
  periodStart: string
  periodEnd: string
  summary: {
    pendingWorkCount: number
    completedWorkCount: number
    activityEventCount: number
    kpis: PersonalProductivityKpi
    capacity: PersonalProductivityCapacity
    workload: PersonalProductivityWorkload
  }
  kpis: PersonalProductivityKpi
  trends: PersonalProductivityTrends
  capacity: PersonalProductivityCapacity
  workload: PersonalProductivityWorkload
  activity: PersonalProductivityActivity
  performance: PersonalProductivityPerformance
  statistics: PersonalProductivityStatistics
}
