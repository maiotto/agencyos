import { CssBaseline, ThemeProvider } from '@mui/material'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AppLayout } from './components/AppLayout'
import { CompanyProvider } from './context/CompanyContext'
import { EnterpriseDashboardPage } from './pages/EnterpriseDashboardPage'
import { MyWorkDashboardPage } from './pages/MyWorkDashboardPage'
import { PlanningWorkspacePage } from './pages/PlanningWorkspacePage'
import { RecommendationWorkspacePage } from './pages/RecommendationWorkspacePage'
import { DecisionWorkspacePage } from './pages/DecisionWorkspacePage'
import { ExecutiveWorkspacePage } from './pages/ExecutiveWorkspacePage'
import { NotificationCenterPage } from './pages/NotificationCenterPage'
import { PersonalProductivityDashboardPage } from './pages/PersonalProductivityDashboardPage'
import { CompanyDetailPage } from './pages/CompanyDetailPage'
import { CompanyFormPage } from './pages/CompanyFormPage'
import { CompanyListPage } from './pages/CompanyListPage'
import { CapacityPage } from './pages/CapacityPage'
import { CapacityHistoryComparePage } from './pages/CapacityHistoryComparePage'
import { CapacityHistoryDetailPage } from './pages/CapacityHistoryDetailPage'
import { CapacityHistoryPage } from './pages/CapacityHistoryPage'
import { HolidayDetailPage } from './pages/HolidayDetailPage'
import { HolidayFormPage } from './pages/HolidayFormPage'
import { HolidayListPage } from './pages/HolidayListPage'
import { DecisionProfileDetailPage } from './pages/DecisionProfileDetailPage'
import { DecisionProfileFormPage } from './pages/DecisionProfileFormPage'
import { DecisionProfileListPage } from './pages/DecisionProfileListPage'
import { ResourceAvailabilityDetailPage } from './pages/ResourceAvailabilityDetailPage'
import { ResourceAvailabilityFormPage } from './pages/ResourceAvailabilityFormPage'
import { ResourceAvailabilityListPage } from './pages/ResourceAvailabilityListPage'
import { WorkingCalendarFormPage } from './pages/WorkingCalendarFormPage'
import { WorkingCalendarListPage } from './pages/WorkingCalendarListPage'
import { WorkingHoursDetailPage } from './pages/WorkingHoursDetailPage'
import { WorkingHoursFormPage } from './pages/WorkingHoursFormPage'
import { WorkingHoursListPage } from './pages/WorkingHoursListPage'
import { WorkloadHistoryAggregatePage } from './pages/WorkloadHistoryAggregatePage'
import { WorkloadHistoryComparePage } from './pages/WorkloadHistoryComparePage'
import { WorkloadHistoryDetailPage } from './pages/WorkloadHistoryDetailPage'
import { WorkloadHistoryPage } from './pages/WorkloadHistoryPage'
import { PlanningTemplateApplyPage } from './pages/PlanningTemplateApplyPage'
import { PlanningTemplateDetailPage } from './pages/PlanningTemplateDetailPage'
import { PlanningTemplateFormPage } from './pages/PlanningTemplateFormPage'
import { PlanningTemplateListPage } from './pages/PlanningTemplateListPage'
import { PortfolioDetailPage } from './pages/PortfolioDetailPage'
import { PortfolioFormPage } from './pages/PortfolioFormPage'
import { PortfolioListPage } from './pages/PortfolioListPage'
import { PortfolioAnalyticsPage } from './pages/PortfolioAnalyticsPage'
import { CrossPortfolioPlanningPage } from './pages/CrossPortfolioPlanningPage'
import { AIRecommendationDetailPage } from './pages/AIRecommendationDetailPage'
import { AIRecommendationGeneratePage } from './pages/AIRecommendationGeneratePage'
import { AIRecommendationListPage } from './pages/AIRecommendationListPage'
import { AuditDetailPage } from './pages/AuditDetailPage'
import { AuditListPage } from './pages/AuditListPage'
import { DecisionCreatePage } from './pages/DecisionCreatePage'
import { DecisionDetailPage } from './pages/DecisionDetailPage'
import { DecisionListPage } from './pages/DecisionListPage'
import { ExplainabilityDetailPage } from './pages/ExplainabilityDetailPage'
import { ExplainabilityGeneratePage } from './pages/ExplainabilityGeneratePage'
import { ExplainabilityListPage } from './pages/ExplainabilityListPage'
import { ExecutiveSummaryDetailPage } from './pages/ExecutiveSummaryDetailPage'
import { ExecutiveSummaryGeneratePage } from './pages/ExecutiveSummaryGeneratePage'
import { ExecutiveSummaryListPage } from './pages/ExecutiveSummaryListPage'
import { RecommendationDetailPage } from './pages/RecommendationDetailPage'
import { RecommendationComparisonPage } from './pages/RecommendationComparisonPage'
import { RecommendationHistoryDetailPage } from './pages/RecommendationHistoryDetailPage'
import { RecommendationHistoryListPage } from './pages/RecommendationHistoryListPage'
import { RecommendationHistoryTimelinePage } from './pages/RecommendationHistoryTimelinePage'
import { RecommendationListPage } from './pages/RecommendationListPage'
import { RecommendationWorkflowApprovePage } from './pages/RecommendationWorkflowApprovePage'
import { RecommendationWorkflowCreatePage } from './pages/RecommendationWorkflowCreatePage'
import { RecommendationWorkflowDetailPage } from './pages/RecommendationWorkflowDetailPage'
import { RecommendationWorkflowListPage } from './pages/RecommendationWorkflowListPage'
import { RecommendationWorkflowRejectPage } from './pages/RecommendationWorkflowRejectPage'
import { theme } from './theme'

export default function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <CompanyProvider>
        <BrowserRouter>
          <AppLayout>
            <Routes>
            <Route path="/" element={<Navigate to="/enterprise-dashboard" replace />} />
            <Route path="/enterprise-dashboard" element={<EnterpriseDashboardPage />} />
            <Route path="/my-work" element={<MyWorkDashboardPage />} />
            <Route path="/planning-workspace" element={<PlanningWorkspacePage />} />
            <Route path="/recommendation-workspace" element={<RecommendationWorkspacePage />} />
            <Route path="/decision-workspace" element={<DecisionWorkspacePage />} />
            <Route path="/executive-workspace" element={<ExecutiveWorkspacePage />} />
            <Route path="/notifications" element={<NotificationCenterPage />} />
            <Route path="/notifications/:id" element={<NotificationCenterPage />} />
            <Route path="/personal-dashboard" element={<PersonalProductivityDashboardPage />} />
            <Route path="/companies" element={<CompanyListPage />} />
            <Route path="/companies/new" element={<CompanyFormPage />} />
            <Route path="/companies/:id" element={<CompanyDetailPage />} />
            <Route path="/companies/:id/edit" element={<CompanyFormPage />} />
            <Route path="/working-calendars" element={<WorkingCalendarListPage />} />
            <Route path="/working-calendars/new" element={<WorkingCalendarFormPage />} />
            <Route path="/working-calendars/:id/edit" element={<WorkingCalendarFormPage />} />
            <Route path="/holidays" element={<HolidayListPage />} />
            <Route path="/holidays/new" element={<HolidayFormPage />} />
            <Route path="/holidays/:id" element={<HolidayDetailPage />} />
            <Route path="/holidays/:id/edit" element={<HolidayFormPage />} />
            <Route path="/working-hours" element={<WorkingHoursListPage />} />
            <Route path="/working-hours/new" element={<WorkingHoursFormPage />} />
            <Route path="/working-hours/:id" element={<WorkingHoursDetailPage />} />
            <Route path="/working-hours/:id/edit" element={<WorkingHoursFormPage />} />
            <Route path="/resource-availabilities" element={<ResourceAvailabilityListPage />} />
            <Route path="/resource-availabilities/new" element={<ResourceAvailabilityFormPage />} />
            <Route path="/resource-availabilities/:id" element={<ResourceAvailabilityDetailPage />} />
            <Route path="/resource-availabilities/:id/edit" element={<ResourceAvailabilityFormPage />} />
            <Route path="/capacity" element={<CapacityPage />} />
            <Route path="/capacity/history" element={<CapacityHistoryPage />} />
            <Route path="/capacity/history/compare" element={<CapacityHistoryComparePage />} />
            <Route path="/capacity/history/:id" element={<CapacityHistoryDetailPage />} />
            <Route path="/workload/history" element={<WorkloadHistoryPage />} />
            <Route path="/workload/history/aggregate" element={<WorkloadHistoryAggregatePage />} />
            <Route path="/workload/history/compare" element={<WorkloadHistoryComparePage />} />
            <Route path="/workload/history/:id" element={<WorkloadHistoryDetailPage />} />
            <Route path="/planning-templates" element={<PlanningTemplateListPage />} />
            <Route path="/planning-templates/new" element={<PlanningTemplateFormPage />} />
            <Route path="/planning-templates/:id/edit" element={<PlanningTemplateFormPage />} />
            <Route path="/planning-templates/:id/apply" element={<PlanningTemplateApplyPage />} />
            <Route path="/planning-templates/:id" element={<PlanningTemplateDetailPage />} />
            <Route path="/portfolios" element={<PortfolioListPage />} />
            <Route path="/portfolios/new" element={<PortfolioFormPage />} />
            <Route path="/portfolios/:id" element={<PortfolioDetailPage />} />
            <Route path="/portfolio-analytics" element={<PortfolioAnalyticsPage />} />
            <Route path="/cross-portfolio-planning" element={<CrossPortfolioPlanningPage />} />
            <Route path="/decisions" element={<DecisionListPage />} />
            <Route path="/decisions/new" element={<DecisionCreatePage />} />
            <Route path="/decisions/:id" element={<DecisionDetailPage />} />
            <Route path="/decision-profiles" element={<DecisionProfileListPage />} />
            <Route path="/decision-profiles/new" element={<DecisionProfileFormPage />} />
            <Route path="/decision-profiles/:id" element={<DecisionProfileDetailPage />} />
            <Route path="/decision-profiles/:id/edit" element={<DecisionProfileFormPage />} />
            <Route path="/audit" element={<AuditListPage />} />
            <Route path="/audit/entity/:entityId" element={<AuditDetailPage />} />
            <Route path="/audit/correlation/:correlationId" element={<AuditDetailPage />} />
            <Route path="/audit/:id" element={<AuditDetailPage />} />
            <Route path="/ai-recommendations" element={<AIRecommendationListPage />} />
            <Route path="/ai-recommendations/generate" element={<AIRecommendationGeneratePage />} />
            <Route path="/ai-recommendations/:id" element={<AIRecommendationDetailPage />} />
            <Route path="/explainability" element={<ExplainabilityListPage />} />
            <Route path="/explainability/generate" element={<ExplainabilityGeneratePage />} />
            <Route path="/explainability/:id" element={<ExplainabilityDetailPage />} />
            <Route path="/executive-summaries" element={<ExecutiveSummaryListPage />} />
            <Route path="/executive-summaries/generate" element={<ExecutiveSummaryGeneratePage />} />
            <Route path="/executive-summaries/:id" element={<ExecutiveSummaryDetailPage />} />
            <Route path="/recommendations" element={<RecommendationListPage />} />
            <Route path="/recommendations/compare" element={<RecommendationComparisonPage />} />
            <Route path="/recommendations/history" element={<RecommendationHistoryListPage />} />
            <Route
              path="/recommendations/history/timeline/:recommendationId"
              element={<RecommendationHistoryTimelinePage />}
            />
            <Route
              path="/recommendations/history/:id"
              element={<RecommendationHistoryDetailPage />}
            />
            <Route path="/recommendations/workflow" element={<RecommendationWorkflowListPage />} />
            <Route path="/recommendations/workflow/new" element={<RecommendationWorkflowCreatePage />} />
            <Route
              path="/recommendations/workflow/:id/approve"
              element={<RecommendationWorkflowApprovePage />}
            />
            <Route
              path="/recommendations/workflow/:id/reject"
              element={<RecommendationWorkflowRejectPage />}
            />
            <Route
              path="/recommendations/workflow/:id"
              element={<RecommendationWorkflowDetailPage />}
            />
            <Route path="/recommendations/:id" element={<RecommendationDetailPage />} />
            <Route path="*" element={<Navigate to="/enterprise-dashboard" replace />} />
            </Routes>
          </AppLayout>
        </BrowserRouter>
      </CompanyProvider>
    </ThemeProvider>
  )
}
