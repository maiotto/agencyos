export interface MyWorkDashboardQuery {
  companyId?: string
  userId?: string
  executionResourceId?: string
  from?: string
  to?: string
  periodStart?: string
  periodEnd?: string
}

export interface MyWorkMissionCard {
  id: string
  code: string
  name: string
  priority: string
  startDate?: string | null
  endDate?: string | null
  activeTaskCount: number
  drillDownPath: string
}

export interface MyWorkTaskCard {
  id: string
  missionId: string
  missionName: string
  code: string
  name: string
  status?: string | null
  priority: string
  plannedStart?: string | null
  plannedEnd?: string | null
  estimatedHours: number
  assignmentId: string
  assignmentStatus: string
  assignmentPlannedHours: number
  isOverdue: boolean
  drillDownPath: string
}

export interface MyWorkRecommendationCard {
  id: string
  recommendationId: string
  missionId: string
  title: string
  status: string
  createdBy: string
  createdAt: string
  drillDownPath: string
}

export interface MyWorkDecisionCard {
  id: string
  recommendationId: string
  missionId: string
  decisionStatus: string
  implementationStatus: string
  decisionDate: string
  createdBy: string
  drillDownPath: string
}

export interface MyWorkCapacitySummary {
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

export interface MyWorkWorkloadSummary {
  hasData: boolean
  executionResourceId?: string | null
  periodStartDate: string
  periodEndDate: string
  totalPlannedHours: number
  assignmentCount: number
  workloadPercentage: number
  drillDownPath: string
}

export interface MyWorkActivityItem {
  id: string
  occurredAt: string
  entityType: string
  entityId: string
  eventType: string
  action: string
  summary: string
  drillDownPath: string
}

export interface MyWorkDeadlineItem {
  taskId: string
  missionId: string
  taskName: string
  missionName: string
  plannedEnd: string
  isOverdue: boolean
  daysRemaining: number
  drillDownPath: string
}

export interface MyWorkKpiSummary {
  assignedTaskCount: number
  assignedMissionCount: number
  pendingRecommendationCount: number
  pendingDecisionCount: number
  overdueTaskCount: number
  upcomingDeadlineCount: number
  utilizationPercentage?: number | null
  workloadPercentage?: number | null
}

export interface MyWorkTasksSection {
  tasks: MyWorkTaskCard[]
  overdueTasks: MyWorkDeadlineItem[]
  upcomingDeadlines: MyWorkDeadlineItem[]
}

export interface MyWorkMissionsSection {
  missions: MyWorkMissionCard[]
}

export interface MyWorkRecommendationsSection {
  recommendations: MyWorkRecommendationCard[]
}

export interface MyWorkDecisionsSection {
  decisions: MyWorkDecisionCard[]
}

export interface MyWorkActivitySection {
  items: MyWorkActivityItem[]
}

export interface MyWorkSummary {
  companyId: string
  userId: string
  executionResourceId?: string | null
  generatedAt: string
  missionCount: number
  taskCount: number
  recommendationCount: number
  decisionCount: number
  kpis: MyWorkKpiSummary
  capacity: MyWorkCapacitySummary
  workload: MyWorkWorkloadSummary
}

export interface MyWorkDashboard {
  companyId: string
  userId: string
  executionResourceId?: string | null
  generatedAt: string
  from: string
  to: string
  periodStart: string
  periodEnd: string
  kpis: MyWorkKpiSummary
  capacity: MyWorkCapacitySummary
  workload: MyWorkWorkloadSummary
  missions: MyWorkMissionCard[]
  tasks: MyWorkTaskCard[]
  recommendations: MyWorkRecommendationCard[]
  decisions: MyWorkDecisionCard[]
  overdueTasks: MyWorkDeadlineItem[]
  upcomingDeadlines: MyWorkDeadlineItem[]
  activity: MyWorkActivityItem[]
}
