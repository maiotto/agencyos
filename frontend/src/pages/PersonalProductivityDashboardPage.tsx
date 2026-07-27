import { useCallback, useEffect, useState, type ReactNode } from 'react'
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  LinearProgress,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { Link as RouterLink } from 'react-router-dom'
import { getPersonalProductivityDashboard } from '../api/personalProductivity'
import { getErrorMessage } from '../api/client'
import { useCompany } from '../context/CompanyContext'
import { getMyWorkUserId, setMyWorkUserId } from '../myWorkStorage'
import type {
  PersonalProductivityActivityItem,
  PersonalProductivityDashboard,
  PersonalProductivityTrendPoint,
} from '../types/personalProductivity'

function defaultDateRange() {
  const to = new Date()
  const from = new Date()
  from.setDate(to.getDate() - 30)
  return {
    from: from.toISOString().slice(0, 10),
    to: to.toISOString().slice(0, 10),
  }
}

function SectionCard({ title, action, children }: { title: string; action?: ReactNode; children: ReactNode }) {
  return (
    <Paper sx={{ p: 2.5, flex: '1 1 320px', minWidth: 280 }}>
      <Stack spacing={1.5}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="subtitle1">{title}</Typography>
          {action}
        </Stack>
        {children}
      </Stack>
    </Paper>
  )
}

function KpiTile({ label, value }: { label: string; value: string | number }) {
  return (
    <Paper sx={{ p: 2, flex: '1 1 160px', minWidth: 140 }} variant="outlined">
      <Typography variant="subtitle2" color="text.secondary">
        {label}
      </Typography>
      <Typography variant="h5">{value}</Typography>
    </Paper>
  )
}

function formatPercent(value?: number | null) {
  return value == null ? '—' : `${value.toFixed(1)}%`
}

function TrendRow({ point }: { point: PersonalProductivityTrendPoint }) {
  const color =
    point.direction === 'Up' ? 'success.main' : point.direction === 'Down' ? 'warning.main' : 'text.secondary'
  return (
    <Stack
      direction="row"
      justifyContent="space-between"
      alignItems="center"
      sx={{ borderBottom: '1px solid rgba(0,0,0,0.06)', py: 0.75 }}
    >
      <Box>
        <Typography variant="body2">{point.metric}</Typography>
        <Typography variant="caption" color="text.secondary">
          Current {point.currentValue ?? '—'} · Previous {point.previousValue ?? '—'} ({point.unit})
        </Typography>
      </Box>
      <Chip
        size="small"
        label={`${point.direction}${point.delta != null ? ` ${point.delta > 0 ? '+' : ''}${point.delta}` : ''}`}
        sx={{ color }}
      />
    </Stack>
  )
}

function ActivityList({
  items,
  emptyLabel,
}: {
  items: PersonalProductivityActivityItem[]
  emptyLabel: string
}) {
  if (items.length === 0) {
    return (
      <Typography variant="body2" color="text.secondary">
        {emptyLabel}
      </Typography>
    )
  }

  return (
    <Stack spacing={1}>
      {items.slice(0, 8).map((item) => (
        <Stack
          key={`${item.kind}-${item.id}`}
          direction="row"
          justifyContent="space-between"
          alignItems="center"
          sx={{ borderBottom: '1px solid rgba(0,0,0,0.06)', pb: 0.5 }}
        >
          <Box>
            <Typography variant="body2">{item.title}</Typography>
            <Typography variant="caption" color="text.secondary">
              {item.kind} · {item.status}
            </Typography>
          </Box>
          <Button size="small" component={RouterLink} to={item.drillDownPath}>
            Open
          </Button>
        </Stack>
      ))}
    </Stack>
  )
}

export function PersonalProductivityDashboardPage() {
  const { activeCompanyId } = useCompany()
  const defaults = defaultDateRange()
  const [userId, setUserId] = useState(getMyWorkUserId())
  const [from, setFrom] = useState(defaults.from)
  const [to, setTo] = useState(defaults.to)
  const [periodStart, setPeriodStart] = useState(defaults.from)
  const [periodEnd, setPeriodEnd] = useState(defaults.to)
  const [dashboard, setDashboard] = useState<PersonalProductivityDashboard | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      setMyWorkUserId(userId)
      setDashboard(
        await getPersonalProductivityDashboard({
          companyId: activeCompanyId || undefined,
          userId: userId || undefined,
          from: from ? `${from}T00:00:00.000Z` : undefined,
          to: to ? `${to}T23:59:59.999Z` : undefined,
          periodStart: periodStart || undefined,
          periodEnd: periodEnd || undefined,
        }),
      )
    } catch (err) {
      setDashboard(null)
      setError(getErrorMessage(err, 'Failed to load Personal Productivity Dashboard.'))
    } finally {
      setLoading(false)
    }
  }, [activeCompanyId, from, periodEnd, periodStart, to, userId])

  useEffect(() => {
    void load()
  }, [load])

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h4" gutterBottom>
          Personal Productivity Dashboard
        </Typography>
        <Typography color="text.secondary">
          Analytical, user-focused view of execution, capacity utilization, workload trends, completed and
          pending work, and personal operational metrics (US-507). Read-only — never changes business
          behavior.
        </Typography>
      </Box>

      <Alert severity="info">
        Metrics originate from existing Assignments, Tasks, Decisions, Capacity/Workload engines, and Audit
        Trail. Identity uses the same User Id fallback as My Work (DEC-501-001). Dashboard usage is audited.
      </Alert>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} useFlexGap flexWrap="wrap">
          <TextField
            label="User Id"
            size="small"
            value={userId}
            onChange={(event) => setUserId(event.target.value)}
            helperText="Persisted locally; sent as X-User-Id"
          />
          <TextField
            label="Activity From"
            type="date"
            size="small"
            InputLabelProps={{ shrink: true }}
            value={from}
            onChange={(event) => setFrom(event.target.value)}
          />
          <TextField
            label="Activity To"
            type="date"
            size="small"
            InputLabelProps={{ shrink: true }}
            value={to}
            onChange={(event) => setTo(event.target.value)}
          />
          <TextField
            label="Capacity Period Start"
            type="date"
            size="small"
            InputLabelProps={{ shrink: true }}
            value={periodStart}
            onChange={(event) => setPeriodStart(event.target.value)}
          />
          <TextField
            label="Capacity Period End"
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

      {loading && !dashboard ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
          <CircularProgress />
        </Box>
      ) : dashboard ? (
        <>
          <Stack direction="row" spacing={2} useFlexGap flexWrap="wrap">
            <KpiTile label="Pending tasks" value={dashboard.kpis.assignedTaskCount} />
            <KpiTile label="Pending decisions" value={dashboard.kpis.pendingDecisionCount} />
            <KpiTile label="Completed decisions" value={dashboard.kpis.completedDecisionCount} />
            <KpiTile label="Overdue" value={dashboard.kpis.overdueTaskCount} />
            <KpiTile label="Completion rate" value={formatPercent(dashboard.kpis.completionRatePercentage)} />
            <KpiTile label="Activity events" value={dashboard.kpis.activityEventCount} />
          </Stack>

          <Stack direction="row" spacing={2} useFlexGap flexWrap="wrap">
            <SectionCard
              title="Capacity utilization"
              action={
                dashboard.capacity.drillDownPath ? (
                  <Button size="small" component={RouterLink} to={dashboard.capacity.drillDownPath}>
                    Open
                  </Button>
                ) : undefined
              }
            >
              {dashboard.capacity.hasData ? (
                <Stack spacing={1}>
                  <Typography variant="h5">
                    {formatPercent(dashboard.capacity.utilizationPercentage)}
                  </Typography>
                  <LinearProgress
                    variant="determinate"
                    value={Math.min(100, Number(dashboard.capacity.utilizationPercentage) || 0)}
                  />
                  <Typography variant="body2" color="text.secondary">
                    {dashboard.capacity.allocatedHours} allocated / {dashboard.capacity.totalCapacityHours}{' '}
                    capacity hours
                  </Typography>
                </Stack>
              ) : (
                <Typography variant="body2" color="text.secondary">
                  No Execution Resource resolved for capacity.
                </Typography>
              )}
            </SectionCard>

            <SectionCard
              title="Workload utilization"
              action={
                dashboard.workload.drillDownPath ? (
                  <Button size="small" component={RouterLink} to={dashboard.workload.drillDownPath}>
                    Open
                  </Button>
                ) : undefined
              }
            >
              {dashboard.workload.hasData ? (
                <Stack spacing={1}>
                  <Typography variant="h5">{formatPercent(dashboard.workload.workloadPercentage)}</Typography>
                  <LinearProgress
                    variant="determinate"
                    value={Math.min(100, Number(dashboard.workload.workloadPercentage) || 0)}
                    color="secondary"
                  />
                  <Typography variant="body2" color="text.secondary">
                    {dashboard.workload.totalPlannedHours} planned hours across{' '}
                    {dashboard.workload.assignmentCount} assignments
                  </Typography>
                </Stack>
              ) : (
                <Typography variant="body2" color="text.secondary">
                  No Execution Resource resolved for workload.
                </Typography>
              )}
            </SectionCard>

            <SectionCard title="Performance indicators">
              <Stack spacing={0.75}>
                <Typography variant="body2">
                  On-time tasks: {formatPercent(dashboard.performance.onTimeTaskPercentage)}
                </Typography>
                <Typography variant="body2">
                  Completion: {formatPercent(dashboard.performance.completionRatePercentage)}
                </Typography>
                <Typography variant="body2">
                  Activity intensity: {dashboard.performance.activityIntensity}
                </Typography>
                <Chip size="small" label={dashboard.performance.focusHint} color="info" sx={{ alignSelf: 'flex-start' }} />
              </Stack>
            </SectionCard>
          </Stack>

          <Stack direction="row" spacing={2} useFlexGap flexWrap="wrap">
            <SectionCard title="Productivity trends">
              <Typography variant="caption" color="text.secondary">
                {dashboard.trends.currentPeriodStart} → {dashboard.trends.currentPeriodEnd} vs prior period
              </Typography>
              <Stack spacing={0.5} sx={{ mt: 1 }}>
                {dashboard.trends.points.map((point) => (
                  <TrendRow key={point.metric} point={point} />
                ))}
              </Stack>
            </SectionCard>

            <SectionCard title="Pending work">
              <ActivityList items={dashboard.activity.pending} emptyLabel="No pending work." />
            </SectionCard>

            <SectionCard title="Completed work">
              <ActivityList items={dashboard.activity.completed} emptyLabel="No completed decisions in period." />
            </SectionCard>
          </Stack>

          <Stack direction="row" spacing={2} useFlexGap flexWrap="wrap">
            <SectionCard title="Activity timeline">
              {dashboard.activity.timeline.length === 0 ? (
                <Typography variant="body2" color="text.secondary">
                  No activity in the selected window.
                </Typography>
              ) : (
                <Stack spacing={1}>
                  {dashboard.activity.timeline.slice(0, 10).map((item) => (
                    <Stack
                      key={item.id}
                      direction="row"
                      justifyContent="space-between"
                      sx={{ borderBottom: '1px solid rgba(0,0,0,0.06)', pb: 0.5 }}
                    >
                      <Box>
                        <Typography variant="body2">{item.summary || item.action}</Typography>
                        <Typography variant="caption" color="text.secondary">
                          {new Date(item.occurredAt).toLocaleString()} · {item.entityType}
                        </Typography>
                      </Box>
                      <Button size="small" component={RouterLink} to={item.drillDownPath}>
                        Open
                      </Button>
                    </Stack>
                  ))}
                </Stack>
              )}
            </SectionCard>

            <SectionCard title="Operational statistics">
              <Stack spacing={0.75}>
                <Typography variant="body2">Missions: {dashboard.statistics.missionCount}</Typography>
                <Typography variant="body2">
                  Pending planned hours: {dashboard.statistics.totalPendingPlannedHours}
                </Typography>
                <Typography variant="body2">
                  Capacity hours: {dashboard.statistics.capacityHours ?? '—'}
                </Typography>
                <Typography variant="body2">
                  Workload hours: {dashboard.statistics.workloadHours ?? '—'}
                </Typography>
                <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap" sx={{ pt: 1 }}>
                  {dashboard.statistics.navigationLinks.map((link) => (
                    <Button key={link.path} size="small" component={RouterLink} to={link.path} variant="outlined">
                      {link.label}
                    </Button>
                  ))}
                </Stack>
              </Stack>
            </SectionCard>
          </Stack>
        </>
      ) : null}
    </Stack>
  )
}
