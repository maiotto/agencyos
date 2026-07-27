import { useCallback, useEffect, useState, type ReactNode } from 'react'
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Divider,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { Link as RouterLink } from 'react-router-dom'
import { getExecutiveWorkspace } from '../api/executiveWorkspace'
import { getErrorMessage } from '../api/client'
import { useCompany } from '../context/CompanyContext'
import type { ExecutiveAction, ExecutiveWorkspace } from '../types/executiveWorkspace'
import type { HealthIndicator, StatusCountItem } from '../types/enterpriseDashboard'

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
      label={health.label || health.status}
      color={HEALTH_COLOR[health.status] ?? 'default'}
      variant="outlined"
      title={health.detail ?? undefined}
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

function KpiTile({ label, value }: { label: string; value: string | number }) {
  return (
    <Paper sx={{ p: 2, flex: '1 1 170px', minWidth: 150 }} variant="outlined">
      <Typography variant="subtitle2" color="text.secondary">
        {label}
      </Typography>
      <Typography variant="h5">{value}</Typography>
    </Paper>
  )
}

function ActionButton({ action }: { action: ExecutiveAction }) {
  return (
    <Button
      component={RouterLink}
      to={action.drillDownPath}
      size="small"
      variant="outlined"
      color={action.isAdvisory ? 'secondary' : 'primary'}
      title={action.description}
    >
      {action.label}
      {action.isAdvisory ? ' (advisory)' : ''}
    </Button>
  )
}

function SectionCard({
  title,
  health,
  drillDownPath,
  drillDownLabel,
  children,
}: {
  title: string
  health?: HealthIndicator
  drillDownPath: string
  drillDownLabel: string
  children: ReactNode
}) {
  return (
    <Paper sx={{ p: 2.5, flex: '1 1 320px', minWidth: 320 }}>
      <Stack spacing={1.5}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="subtitle1">{title}</Typography>
          {health && <HealthChip health={health} />}
        </Stack>
        {children}
        <Button component={RouterLink} to={drillDownPath} size="small" sx={{ alignSelf: 'flex-start' }}>
          {drillDownLabel}
        </Button>
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

export function ExecutiveWorkspacePage() {
  const { activeCompanyId } = useCompany()
  const initialRange = defaultDateRange()
  const [from, setFrom] = useState(initialRange.from)
  const [to, setTo] = useState(initialRange.to)
  const [periodStart, setPeriodStart] = useState(initialRange.from)
  const [periodEnd, setPeriodEnd] = useState(initialRange.to)
  const [workspace, setWorkspace] = useState<ExecutiveWorkspace | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await getExecutiveWorkspace({
        companyId: activeCompanyId || undefined,
        from: from ? new Date(from).toISOString() : undefined,
        to: to ? new Date(`${to}T23:59:59.999Z`).toISOString() : undefined,
        periodStart,
        periodEnd,
      })
      setWorkspace(data)
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to load the Executive Workspace.'))
      setWorkspace(null)
    } finally {
      setLoading(false)
    }
  }, [activeCompanyId, from, to, periodStart, periodEnd])

  useEffect(() => {
    void load()
  }, [load])

  return (
    <Stack spacing={2.5}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" flexWrap="wrap" useFlexGap spacing={1}>
        <Typography variant="h5">Executive Workspace</Typography>
        <Stack direction="row" spacing={1}>
          <Button component={RouterLink} to="/enterprise-dashboard" size="small">
            Enterprise Dashboard
          </Button>
          <Button component={RouterLink} to="/decision-workspace" size="small">
            Decision Workspace
          </Button>
        </Stack>
      </Stack>

      <Alert severity="info">
        Executive Workspace consolidates existing enterprise signals. Read-only. Drill-down opens operational
        workspaces and dashboards without modifying data.
      </Alert>

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} alignItems={{ md: 'center' }} flexWrap="wrap">
          <TextField
            label="From"
            type="date"
            size="small"
            InputLabelProps={{ shrink: true }}
            value={from}
            onChange={(event) => setFrom(event.target.value)}
          />
          <TextField
            label="To"
            type="date"
            size="small"
            InputLabelProps={{ shrink: true }}
            value={to}
            onChange={(event) => setTo(event.target.value)}
          />
          <TextField
            label="Period Start"
            type="date"
            size="small"
            InputLabelProps={{ shrink: true }}
            value={periodStart}
            onChange={(event) => setPeriodStart(event.target.value)}
          />
          <TextField
            label="Period End"
            type="date"
            size="small"
            InputLabelProps={{ shrink: true }}
            value={periodEnd}
            onChange={(event) => setPeriodEnd(event.target.value)}
          />
          <Button variant="contained" onClick={() => void load()} disabled={loading}>
            Refresh
          </Button>
        </Stack>
      </Paper>

      {loading && (
        <Box display="flex" justifyContent="center" py={4}>
          <CircularProgress />
        </Box>
      )}

      {error && <Alert severity="error">{error}</Alert>}

      {workspace && !loading && (
        <>
          <Paper sx={{ p: 2.5 }}>
            <Stack spacing={1.5}>
              <Stack direction="row" justifyContent="space-between" alignItems="center" flexWrap="wrap" useFlexGap>
                <Typography variant="subtitle1">Executive Overview</Typography>
                <HealthChip health={workspace.overview.overallHealth} />
              </Stack>
              <Typography variant="body2">{workspace.overview.narrative}</Typography>
              <Typography variant="caption" color="text.secondary">
                {workspace.overview.navigationTip}
              </Typography>
            </Stack>
          </Paper>

          <Stack direction="row" spacing={1.5} flexWrap="wrap" useFlexGap>
            <KpiTile label="Portfolios" value={`${workspace.kpis.activePortfolioCount} / ${workspace.kpis.portfolioCount}`} />
            <KpiTile label="Recommendations" value={workspace.kpis.recommendationCount} />
            <KpiTile
              label="Decisions"
              value={`${workspace.kpis.decisionCount} (${workspace.kpis.pendingDecisionCount} pending)`}
            />
            <KpiTile label="Capacity Utilization" value={`${workspace.kpis.capacityUtilizationPercentage}%`} />
            <KpiTile label="Workload" value={`${workspace.kpis.workloadPercentage}%`} />
            <KpiTile label="Audit Events" value={workspace.kpis.auditEventCount} />
          </Stack>

          <SectionCard title="Executive Navigation" drillDownPath="/enterprise-dashboard" drillDownLabel="Open Enterprise Dashboard">
            <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
              {workspace.navigation.actions.map((action) => (
                <ActionButton key={action.key} action={action} />
              ))}
            </Stack>
          </SectionCard>

          <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
            <SectionCard
              title="Portfolios"
              health={workspace.portfolios.portfolio.overallHealth}
              drillDownPath={workspace.portfolios.drillDownPath}
              drillDownLabel="Open Portfolio Analytics"
            >
              <Typography variant="body2">
                {workspace.portfolios.portfolio.portfolioCount} portfolios · avg. utilization{' '}
                {workspace.portfolios.portfolio.averageUtilizationPercentage}% · avg. workload{' '}
                {workspace.portfolios.portfolio.averageWorkloadPercentage}%
              </Typography>
              <StatusBreakdownList items={workspace.portfolios.portfolio.healthBreakdown} />
            </SectionCard>

            <SectionCard
              title="Recommendations"
              drillDownPath={workspace.recommendations.drillDownPath}
              drillDownLabel="Open Recommendation Workspace"
            >
              <Typography variant="body2">
                {workspace.recommendations.recommendations.totalCount} total · active{' '}
                {workspace.recommendations.recommendations.activeCount} · archived{' '}
                {workspace.recommendations.recommendations.archivedCount}
              </Typography>
              <StatusBreakdownList items={workspace.recommendations.recommendations.statusBreakdown} />
            </SectionCard>

            <SectionCard
              title="Decisions"
              drillDownPath={workspace.decisions.drillDownPath}
              drillDownLabel="Open Decision Workspace"
            >
              <Typography variant="body2">
                {workspace.decisions.decisions.totalCount} total · completed{' '}
                {workspace.decisions.decisions.completedCount} · cancelled {workspace.decisions.decisions.cancelledCount}
              </Typography>
              <StatusBreakdownList items={workspace.decisions.decisions.decisionStatusBreakdown} />
            </SectionCard>
          </Stack>

          <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
            <SectionCard
              title="Capacity"
              health={workspace.capacity.capacity.utilizationHealth}
              drillDownPath={workspace.capacity.drillDownPath}
              drillDownLabel="Open Capacity History"
            >
              <Typography variant="body2">
                {workspace.capacity.capacity.recordCount} history records · avg. utilization{' '}
                {workspace.capacity.capacity.averageUtilizationPercentage}%
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {workspace.capacity.capacity.totalAllocatedHours} / {workspace.capacity.capacity.totalCapacityHours} hours
                allocated
              </Typography>
            </SectionCard>

            <SectionCard
              title="Workload"
              health={workspace.workload.workload.workloadHealth}
              drillDownPath={workspace.workload.drillDownPath}
              drillDownLabel="Open Workload History"
            >
              <Typography variant="body2">
                {workspace.workload.workload.recordCount} history records · avg. workload{' '}
                {workspace.workload.workload.averageWorkloadPercentage}%
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {workspace.workload.workload.totalAllocatedHours} / {workspace.workload.workload.totalCapacityHours} hours
                allocated
              </Typography>
            </SectionCard>

            <SectionCard title="AI Decision Support" drillDownPath={workspace.ai.drillDownPath} drillDownLabel="Open AI Recommendations">
              <Typography variant="body2">
                {workspace.ai.ai.aiRecommendationCount} AI Recommendations (avg. confidence{' '}
                {workspace.ai.ai.averageConfidenceScore}%)
              </Typography>
              <Typography variant="body2">
                Explainability: {workspace.ai.ai.explainabilityCount} · Executive Summaries:{' '}
                {workspace.ai.ai.executiveSummaryCount}
              </Typography>
            </SectionCard>
          </Stack>

          <SectionCard title="Audit Trail" drillDownPath={workspace.audit.drillDownPath} drillDownLabel="Open Audit Trail">
            <Typography variant="body2" color="text.secondary">
              {workspace.audit.audit.eventCount} events ·{' '}
              {workspace.audit.audit.lastEventAt
                ? `last recorded ${new Date(workspace.audit.audit.lastEventAt).toLocaleString()}`
                : 'no events in the selected period'}
            </Typography>
            <StatusBreakdownList items={workspace.audit.audit.entityTypeBreakdown} />
          </SectionCard>

          <Divider />
          <Typography variant="caption" color="text.secondary">
            Generated {new Date(workspace.generatedAt).toLocaleString()}
          </Typography>
        </>
      )}
    </Stack>
  )
}
