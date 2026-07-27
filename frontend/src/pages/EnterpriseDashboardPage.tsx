import { useCallback, useEffect, useState, type ReactNode } from 'react'
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Divider,
  LinearProgress,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import TrendingDownIcon from '@mui/icons-material/TrendingDown'
import TrendingFlatIcon from '@mui/icons-material/TrendingFlat'
import TrendingUpIcon from '@mui/icons-material/TrendingUp'
import { Link as RouterLink } from 'react-router-dom'
import { getEnterpriseDashboard } from '../api/enterpriseDashboard'
import { getErrorMessage } from '../api/client'
import { useCompany } from '../context/CompanyContext'
import type {
  EnterpriseDashboard,
  HealthIndicator,
  StatusCountItem,
  TrendIndicator,
} from '../types/enterpriseDashboard'

const HEALTH_COLOR: Record<string, 'default' | 'success' | 'warning' | 'error' | 'info'> = {
  Unknown: 'default',
  Underutilized: 'info',
  Healthy: 'success',
  AtRisk: 'warning',
  Overloaded: 'error',
}

function HealthChip({ health }: { health: HealthIndicator }) {
  return (
    <Chip
      size="small"
      label={health.label}
      color={HEALTH_COLOR[health.status] ?? 'default'}
      variant="outlined"
      title={health.detail ?? undefined}
    />
  )
}

function TrendChip({ trend }: { trend: TrendIndicator }) {
  const icon =
    trend.direction === 'Up' ? (
      <TrendingUpIcon fontSize="small" />
    ) : trend.direction === 'Down' ? (
      <TrendingDownIcon fontSize="small" />
    ) : (
      <TrendingFlatIcon fontSize="small" />
    )

  const color = trend.direction === 'Up' ? 'success' : trend.direction === 'Down' ? 'error' : 'default'

  return (
    <Chip
      size="small"
      icon={icon}
      color={color}
      variant="outlined"
      label={`${trend.deltaPercent > 0 ? '+' : ''}${trend.deltaPercent}%`}
    />
  )
}

function StatusBreakdownList({ items }: { items: StatusCountItem[] }) {
  if (items.length === 0) {
    return (
      <Typography variant="body2" color="text.secondary">
        No records for the selected period.
      </Typography>
    )
  }

  return (
    <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
      {items.map((item) => (
        <Chip key={item.status} size="small" label={`${item.status}: ${item.count}`} variant="outlined" />
      ))}
    </Stack>
  )
}

function KpiCard({
  title,
  value,
  trend,
  health,
  drillDownPath,
  children,
}: {
  title: string
  value: string | number
  trend?: TrendIndicator
  health?: HealthIndicator
  drillDownPath?: string
  children?: ReactNode
}) {
  return (
    <Paper sx={{ p: 2.5, flex: '1 1 260px', minWidth: 260 }}>
      <Stack spacing={1.5}>
        <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
          <Typography variant="subtitle2" color="text.secondary">
            {title}
          </Typography>
          {health && <HealthChip health={health} />}
        </Stack>
        <Typography variant="h4">{value}</Typography>
        <Stack direction="row" spacing={1} alignItems="center">
          {trend && <TrendChip trend={trend} />}
        </Stack>
        {children}
        {drillDownPath && (
          <Button component={RouterLink} to={drillDownPath} size="small" sx={{ alignSelf: 'flex-start' }}>
            View details
          </Button>
        )}
      </Stack>
    </Paper>
  )
}

function defaultDateRange() {
  const to = new Date()
  const from = new Date()
  from.setDate(to.getDate() - 30)
  return {
    from: from.toISOString().slice(0, 10),
    to: to.toISOString().slice(0, 10),
  }
}

export function EnterpriseDashboardPage() {
  const { activeCompanyId } = useCompany()
  const initialRange = defaultDateRange()
  const [from, setFrom] = useState(initialRange.from)
  const [to, setTo] = useState(initialRange.to)
  const [periodStart, setPeriodStart] = useState(initialRange.from)
  const [periodEnd, setPeriodEnd] = useState(initialRange.to)
  const [dashboard, setDashboard] = useState<EnterpriseDashboard | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)

    try {
      const data = await getEnterpriseDashboard({
        companyId: activeCompanyId || undefined,
        from,
        to,
        periodStart,
        periodEnd,
      })
      setDashboard(data)
    } catch (err) {
      setDashboard(null)
      setError(getErrorMessage(err, 'Failed to load the Enterprise Dashboard.'))
    } finally {
      setLoading(false)
    }
  }, [activeCompanyId, from, to, periodStart, periodEnd])

  useEffect(() => {
    void load()
  }, [load])

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h4">Enterprise Dashboard</Typography>
        <Typography color="text.secondary">
          Read-only rollup of Portfolios, Capacity/Workload history, Planning Templates, Recommendations,
          Decisions, AI Decision Support, and Audit activity (US-403). KPIs are aggregated directly from
          existing history — nothing here is recalculated or stored separately.
        </Typography>
      </Box>

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} alignItems={{ md: 'center' }} flexWrap="wrap">
          <TextField
            label="From"
            type="date"
            value={from}
            onChange={(event) => setFrom(event.target.value)}
            InputLabelProps={{ shrink: true }}
            size="small"
          />
          <TextField
            label="To"
            type="date"
            value={to}
            onChange={(event) => setTo(event.target.value)}
            InputLabelProps={{ shrink: true }}
            size="small"
          />
          <TextField
            label="Period Start"
            type="date"
            value={periodStart}
            onChange={(event) => setPeriodStart(event.target.value)}
            InputLabelProps={{ shrink: true }}
            size="small"
          />
          <TextField
            label="Period End"
            type="date"
            value={periodEnd}
            onChange={(event) => setPeriodEnd(event.target.value)}
            InputLabelProps={{ shrink: true }}
            size="small"
          />
          <Button variant="contained" onClick={() => void load()} disabled={loading}>
            Refresh
          </Button>
        </Stack>
      </Paper>

      {error && <Alert severity="error">{error}</Alert>}

      {loading && !dashboard ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
          <CircularProgress />
        </Box>
      ) : dashboard ? (
        <>
          <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
            <KpiCard
              title="Portfolios"
              value={`${dashboard.summary.activePortfolioCount} / ${dashboard.summary.portfolioCount}`}
              health={dashboard.summary.overallHealth}
              drillDownPath="/portfolios"
            >
              <Typography variant="caption" color="text.secondary">
                Active / Total
              </Typography>
            </KpiCard>
            <KpiCard
              title="Planning Templates"
              value={dashboard.summary.planningTemplateCount}
              drillDownPath="/planning-templates"
            />
            <KpiCard
              title="Recommendations"
              value={dashboard.summary.recommendationCount}
              trend={dashboard.summary.recommendationTrend}
              drillDownPath={dashboard.recommendations.drillDownPath}
            />
            <KpiCard
              title="Decisions"
              value={dashboard.summary.decisionCount}
              trend={dashboard.summary.decisionTrend}
              drillDownPath={dashboard.decisions.drillDownPath}
            >
              <Typography variant="caption" color="text.secondary">
                {dashboard.summary.pendingDecisionCount} pending, {dashboard.summary.completedDecisionCount}{' '}
                completed
              </Typography>
            </KpiCard>
          </Stack>

          {dashboard.summary.defaultDecisionProfileName && (
            <Typography variant="body2" color="text.secondary">
              Default Decision Profile: {dashboard.summary.defaultDecisionProfileName}
            </Typography>
          )}

          <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
            <Paper sx={{ p: 2.5, flex: '1 1 320px', minWidth: 320 }}>
              <Stack spacing={1.5}>
                <Stack direction="row" justifyContent="space-between" alignItems="center">
                  <Typography variant="subtitle1">Portfolio Health</Typography>
                  <HealthChip health={dashboard.portfolio.overallHealth} />
                </Stack>
                <Box>
                  <Stack direction="row" justifyContent="space-between">
                    <Typography variant="body2">Average Utilization</Typography>
                    <Typography variant="body2">{dashboard.portfolio.averageUtilizationPercentage}%</Typography>
                  </Stack>
                  <LinearProgress
                    variant="determinate"
                    value={Math.min(dashboard.portfolio.averageUtilizationPercentage, 100)}
                    sx={{ height: 8, borderRadius: 4 }}
                  />
                </Box>
                <Box>
                  <Stack direction="row" justifyContent="space-between">
                    <Typography variant="body2">Average Workload</Typography>
                    <Typography variant="body2">{dashboard.portfolio.averageWorkloadPercentage}%</Typography>
                  </Stack>
                  <LinearProgress
                    variant="determinate"
                    value={Math.min(dashboard.portfolio.averageWorkloadPercentage, 100)}
                    sx={{ height: 8, borderRadius: 4 }}
                    color="secondary"
                  />
                </Box>
                <Divider />
                <StatusBreakdownList items={dashboard.portfolio.statusBreakdown} />
                <Button component={RouterLink} to={dashboard.portfolio.drillDownPath} size="small" sx={{ alignSelf: 'flex-start' }}>
                  View Portfolios
                </Button>
              </Stack>
            </Paper>

            <Paper sx={{ p: 2.5, flex: '1 1 320px', minWidth: 320 }}>
              <Stack spacing={1.5}>
                <Stack direction="row" justifyContent="space-between" alignItems="center">
                  <Typography variant="subtitle1">Capacity</Typography>
                  <HealthChip health={dashboard.capacity.utilizationHealth} />
                </Stack>
                <Box>
                  <Stack direction="row" justifyContent="space-between">
                    <Typography variant="body2">Average Utilization</Typography>
                    <Stack direction="row" spacing={1} alignItems="center">
                      <Typography variant="body2">{dashboard.capacity.averageUtilizationPercentage}%</Typography>
                      <TrendChip trend={dashboard.capacity.utilizationTrend} />
                    </Stack>
                  </Stack>
                  <LinearProgress
                    variant="determinate"
                    value={Math.min(dashboard.capacity.averageUtilizationPercentage, 100)}
                    sx={{ height: 8, borderRadius: 4 }}
                  />
                </Box>
                <Typography variant="body2" color="text.secondary">
                  {dashboard.capacity.recordCount} history records · {dashboard.capacity.totalAllocatedHours} /{' '}
                  {dashboard.capacity.totalCapacityHours} hours allocated
                </Typography>
                <Button component={RouterLink} to={dashboard.capacity.drillDownPath} size="small" sx={{ alignSelf: 'flex-start' }}>
                  View Capacity History
                </Button>
              </Stack>
            </Paper>

            <Paper sx={{ p: 2.5, flex: '1 1 320px', minWidth: 320 }}>
              <Stack spacing={1.5}>
                <Stack direction="row" justifyContent="space-between" alignItems="center">
                  <Typography variant="subtitle1">Workload</Typography>
                  <HealthChip health={dashboard.workload.workloadHealth} />
                </Stack>
                <Box>
                  <Stack direction="row" justifyContent="space-between">
                    <Typography variant="body2">Average Workload</Typography>
                    <Stack direction="row" spacing={1} alignItems="center">
                      <Typography variant="body2">{dashboard.workload.averageWorkloadPercentage}%</Typography>
                      <TrendChip trend={dashboard.workload.workloadTrend} />
                    </Stack>
                  </Stack>
                  <LinearProgress
                    variant="determinate"
                    value={Math.min(dashboard.workload.averageWorkloadPercentage, 100)}
                    sx={{ height: 8, borderRadius: 4 }}
                    color="secondary"
                  />
                </Box>
                <Typography variant="body2" color="text.secondary">
                  {dashboard.workload.recordCount} history records · {dashboard.workload.totalAllocatedHours} /{' '}
                  {dashboard.workload.totalCapacityHours} hours allocated
                </Typography>
                <Button component={RouterLink} to={dashboard.workload.drillDownPath} size="small" sx={{ alignSelf: 'flex-start' }}>
                  View Workload History
                </Button>
              </Stack>
            </Paper>
          </Stack>

          <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
            <Paper sx={{ p: 2.5, flex: '1 1 320px', minWidth: 320 }}>
              <Stack spacing={1.5}>
                <Typography variant="subtitle1">Recommendations</Typography>
                <Stack direction="row" spacing={3}>
                  <Typography variant="body2">Active: {dashboard.recommendations.activeCount}</Typography>
                  <Typography variant="body2">Archived: {dashboard.recommendations.archivedCount}</Typography>
                  <Typography variant="body2">
                    Avg. Score: {dashboard.recommendations.averageScore ?? '—'}
                  </Typography>
                </Stack>
                <StatusBreakdownList items={dashboard.recommendations.statusBreakdown} />
                <Button
                  component={RouterLink}
                  to={dashboard.recommendations.drillDownPath}
                  size="small"
                  sx={{ alignSelf: 'flex-start' }}
                >
                  View Recommendations
                </Button>
              </Stack>
            </Paper>

            <Paper sx={{ p: 2.5, flex: '1 1 320px', minWidth: 320 }}>
              <Stack spacing={1.5}>
                <Typography variant="subtitle1">Decisions</Typography>
                <Stack direction="row" spacing={3}>
                  <Typography variant="body2">Completed: {dashboard.decisions.completedCount}</Typography>
                  <Typography variant="body2">Cancelled: {dashboard.decisions.cancelledCount}</Typography>
                </Stack>
                <StatusBreakdownList items={dashboard.decisions.decisionStatusBreakdown} />
                <Button
                  component={RouterLink}
                  to={dashboard.decisions.drillDownPath}
                  size="small"
                  sx={{ alignSelf: 'flex-start' }}
                >
                  View Decisions
                </Button>
              </Stack>
            </Paper>

            <Paper sx={{ p: 2.5, flex: '1 1 320px', minWidth: 320 }}>
              <Stack spacing={1.5}>
                <Typography variant="subtitle1">AI Decision Support</Typography>
                <Stack direction="row" spacing={3} flexWrap="wrap">
                  <Typography variant="body2">
                    AI Recs: {dashboard.ai.aiRecommendationCount} (avg. confidence{' '}
                    {dashboard.ai.averageConfidenceScore}%)
                  </Typography>
                </Stack>
                <Stack direction="row" spacing={3} flexWrap="wrap">
                  <Typography variant="body2">Explainability: {dashboard.ai.explainabilityCount}</Typography>
                  <Typography variant="body2">
                    Executive Summaries: {dashboard.ai.executiveSummaryCount} (avg. confidence{' '}
                    {dashboard.ai.averageExecutiveSummaryConfidenceLevel}%)
                  </Typography>
                </Stack>
                <Button
                  component={RouterLink}
                  to={dashboard.ai.drillDownPath}
                  size="small"
                  sx={{ alignSelf: 'flex-start' }}
                >
                  View AI Recommendations
                </Button>
              </Stack>
            </Paper>
          </Stack>

          <Paper sx={{ p: 2.5 }}>
            <Stack spacing={1.5}>
              <Stack direction="row" justifyContent="space-between" alignItems="center">
                <Typography variant="subtitle1">Audit Activity</Typography>
                <TrendChip trend={dashboard.audit.trend} />
              </Stack>
              <Typography variant="body2" color="text.secondary">
                {dashboard.audit.eventCount} events ·{' '}
                {dashboard.audit.lastEventAt
                  ? `last recorded ${new Date(dashboard.audit.lastEventAt).toLocaleString()}`
                  : 'no events in the selected period'}
              </Typography>
              <StatusBreakdownList items={dashboard.audit.entityTypeBreakdown} />
              <Button component={RouterLink} to={dashboard.audit.drillDownPath} size="small" sx={{ alignSelf: 'flex-start' }}>
                View Audit Log
              </Button>
            </Stack>
          </Paper>
        </>
      ) : null}
    </Stack>
  )
}
