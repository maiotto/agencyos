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
import { Link as RouterLink } from 'react-router-dom'
import { getMyWorkDashboard } from '../api/myWork'
import { getErrorMessage } from '../api/client'
import { useCompany } from '../context/CompanyContext'
import { getMyWorkUserId, setMyWorkUserId } from '../myWorkStorage'
import type { MyWorkDashboard, MyWorkDeadlineItem } from '../types/myWork'

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
    <Paper sx={{ p: 2.5, flex: '1 1 320px', minWidth: 320 }}>
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
    <Paper sx={{ p: 2, flex: '1 1 180px', minWidth: 160 }} variant="outlined">
      <Typography variant="subtitle2" color="text.secondary">
        {label}
      </Typography>
      <Typography variant="h5">{value}</Typography>
    </Paper>
  )
}

function DeadlineList({ items, emptyLabel }: { items: MyWorkDeadlineItem[]; emptyLabel: string }) {
  if (items.length === 0) {
    return (
      <Typography variant="body2" color="text.secondary">
        {emptyLabel}
      </Typography>
    )
  }

  return (
    <Stack spacing={1}>
      {items.map((item) => (
        <Stack
          key={item.taskId}
          direction="row"
          justifyContent="space-between"
          alignItems="center"
          sx={{ borderBottom: '1px solid rgba(0,0,0,0.06)', pb: 0.5 }}
        >
          <Box>
            <Typography variant="body2">{item.taskName}</Typography>
            <Typography variant="caption" color="text.secondary">
              {item.missionName} · due {item.plannedEnd}
            </Typography>
          </Box>
          <Stack direction="row" spacing={1} alignItems="center">
            <Chip
              size="small"
              label={item.isOverdue ? `${Math.abs(item.daysRemaining)}d overdue` : `${item.daysRemaining}d left`}
              color={item.isOverdue ? 'error' : 'warning'}
              variant="outlined"
            />
            <Button component={RouterLink} to={item.drillDownPath} size="small">
              View
            </Button>
          </Stack>
        </Stack>
      ))}
    </Stack>
  )
}

export function MyWorkDashboardPage() {
  const { activeCompanyId } = useCompany()
  const initialRange = defaultDateRange()
  const [userId, setUserId] = useState(getMyWorkUserId())
  const [executionResourceId, setExecutionResourceId] = useState('')
  const [from, setFrom] = useState(initialRange.from)
  const [to, setTo] = useState(initialRange.to)
  const [periodStart, setPeriodStart] = useState(initialRange.from)
  const [periodEnd, setPeriodEnd] = useState(initialRange.to)
  const [dashboard, setDashboard] = useState<MyWorkDashboard | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)

    try {
      const data = await getMyWorkDashboard({
        companyId: activeCompanyId || undefined,
        userId: userId || undefined,
        executionResourceId: executionResourceId || undefined,
        from,
        to,
        periodStart,
        periodEnd,
      })
      setDashboard(data)
    } catch (err) {
      setDashboard(null)
      setError(getErrorMessage(err, 'Failed to load the My Work Dashboard.'))
    } finally {
      setLoading(false)
    }
  }, [activeCompanyId, userId, executionResourceId, from, to, periodStart, periodEnd])

  useEffect(() => {
    void load()
  }, [load])

  const handleUserIdChange = (value: string) => {
    setUserId(value)
    setMyWorkUserId(value)
  }

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h4">My Work</Typography>
        <Typography color="text.secondary">
          Personal operational dashboard consolidating your assigned Missions/Tasks, pending Recommendations
          and Decisions, Capacity/Workload, and recent activity (US-501).
        </Typography>
      </Box>

      <Alert severity="info">
        Operational workspace — read-only. Drill-down does not modify data.
      </Alert>

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} alignItems={{ md: 'center' }} flexWrap="wrap" useFlexGap>
          <TextField
            label="User Id"
            value={userId}
            onChange={(event) => handleUserIdChange(event.target.value)}
            helperText="Also sent as X-User-Id header"
            size="small"
          />
          <TextField
            label="Execution Resource Id"
            value={executionResourceId}
            onChange={(event) => setExecutionResourceId(event.target.value)}
            helperText="Optional — overrides Code-based lookup"
            size="small"
            sx={{ minWidth: 260 }}
          />
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
          <Typography variant="body2" color="text.secondary">
            Resolved identity — Company: {dashboard.companyId} · User: {dashboard.userId} · Execution Resource:{' '}
            {dashboard.executionResourceId ?? 'unresolved'}
          </Typography>

          <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
            <KpiTile label="Assigned Tasks" value={dashboard.kpis.assignedTaskCount} />
            <KpiTile label="Assigned Missions" value={dashboard.kpis.assignedMissionCount} />
            <KpiTile label="Pending Recommendations" value={dashboard.kpis.pendingRecommendationCount} />
            <KpiTile label="Pending Decisions" value={dashboard.kpis.pendingDecisionCount} />
            <KpiTile label="Overdue Tasks" value={dashboard.kpis.overdueTaskCount} />
            <KpiTile label="Upcoming Deadlines" value={dashboard.kpis.upcomingDeadlineCount} />
          </Stack>

          <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
            <SectionCard title="Capacity">
              {dashboard.capacity.hasData ? (
                <>
                  <Stack direction="row" justifyContent="space-between">
                    <Typography variant="body2">Utilization</Typography>
                    <Typography variant="body2">{dashboard.capacity.utilizationPercentage}%</Typography>
                  </Stack>
                  <LinearProgress
                    variant="determinate"
                    value={Math.min(dashboard.capacity.utilizationPercentage, 100)}
                    sx={{ height: 8, borderRadius: 4 }}
                  />
                  <Typography variant="caption" color="text.secondary">
                    {dashboard.capacity.allocatedHours} / {dashboard.capacity.totalCapacityHours} hours allocated
                  </Typography>
                </>
              ) : (
                <Typography variant="body2" color="text.secondary">
                  No Execution Resource resolved — capacity unavailable.
                </Typography>
              )}
              <Button component={RouterLink} to={dashboard.capacity.drillDownPath} size="small" sx={{ alignSelf: 'flex-start' }}>
                View Capacity
              </Button>
            </SectionCard>

            <SectionCard title="Workload">
              {dashboard.workload.hasData ? (
                <>
                  <Stack direction="row" justifyContent="space-between">
                    <Typography variant="body2">Workload</Typography>
                    <Typography variant="body2">{dashboard.workload.workloadPercentage}%</Typography>
                  </Stack>
                  <LinearProgress
                    variant="determinate"
                    value={Math.min(dashboard.workload.workloadPercentage, 100)}
                    sx={{ height: 8, borderRadius: 4 }}
                    color="secondary"
                  />
                  <Typography variant="caption" color="text.secondary">
                    {dashboard.workload.totalPlannedHours} planned hours across {dashboard.workload.assignmentCount}{' '}
                    assignments
                  </Typography>
                </>
              ) : (
                <Typography variant="body2" color="text.secondary">
                  No Execution Resource resolved — workload unavailable.
                </Typography>
              )}
              <Button component={RouterLink} to={dashboard.workload.drillDownPath} size="small" sx={{ alignSelf: 'flex-start' }}>
                View Workload History
              </Button>
            </SectionCard>
          </Stack>

          <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
            <SectionCard title={`Overdue Tasks (${dashboard.overdueTasks.length})`}>
              <DeadlineList items={dashboard.overdueTasks} emptyLabel="No overdue tasks." />
            </SectionCard>
            <SectionCard title={`Upcoming Deadlines (${dashboard.upcomingDeadlines.length})`}>
              <DeadlineList items={dashboard.upcomingDeadlines} emptyLabel="No upcoming deadlines in the next 14 days." />
            </SectionCard>
          </Stack>

          <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
            <SectionCard title={`Missions (${dashboard.missions.length})`}>
              {dashboard.missions.length === 0 ? (
                <Typography variant="body2" color="text.secondary">
                  No active Missions for the resolved Execution Resource.
                </Typography>
              ) : (
                <Stack spacing={1}>
                  {dashboard.missions.map((mission) => (
                    <Stack key={mission.id} direction="row" justifyContent="space-between" alignItems="center">
                      <Box>
                        <Typography variant="body2">{mission.name}</Typography>
                        <Typography variant="caption" color="text.secondary">
                          {mission.code} · {mission.priority} · {mission.activeTaskCount} active tasks
                        </Typography>
                      </Box>
                      <Button component={RouterLink} to={mission.drillDownPath} size="small">
                        View
                      </Button>
                    </Stack>
                  ))}
                </Stack>
              )}
            </SectionCard>

            <SectionCard title={`Tasks (${dashboard.tasks.length})`}>
              {dashboard.tasks.length === 0 ? (
                <Typography variant="body2" color="text.secondary">
                  No active Tasks for the resolved Execution Resource.
                </Typography>
              ) : (
                <Stack spacing={1}>
                  {dashboard.tasks.map((task) => (
                    <Stack key={task.id} direction="row" justifyContent="space-between" alignItems="center">
                      <Box>
                        <Typography variant="body2">{task.name}</Typography>
                        <Typography variant="caption" color="text.secondary">
                          {task.missionName} · {task.status ?? 'Unknown'} · {task.priority}
                          {task.plannedEnd ? ` · due ${task.plannedEnd}` : ''}
                        </Typography>
                      </Box>
                      <Stack direction="row" spacing={1} alignItems="center">
                        {task.isOverdue && <Chip size="small" label="Overdue" color="error" variant="outlined" />}
                        <Button component={RouterLink} to={task.drillDownPath} size="small">
                          View
                        </Button>
                      </Stack>
                    </Stack>
                  ))}
                </Stack>
              )}
            </SectionCard>
          </Stack>

          <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
            <SectionCard title={`Pending Recommendations (${dashboard.recommendations.length})`}>
              {dashboard.recommendations.length === 0 ? (
                <Typography variant="body2" color="text.secondary">
                  No pending Recommendations.
                </Typography>
              ) : (
                <Stack spacing={1}>
                  {dashboard.recommendations.map((recommendation) => (
                    <Stack key={recommendation.id} direction="row" justifyContent="space-between" alignItems="center">
                      <Box>
                        <Typography variant="body2">{recommendation.title}</Typography>
                        <Typography variant="caption" color="text.secondary">
                          {recommendation.status} · by {recommendation.createdBy}
                        </Typography>
                      </Box>
                      <Button component={RouterLink} to={recommendation.drillDownPath} size="small">
                        Review
                      </Button>
                    </Stack>
                  ))}
                </Stack>
              )}
              <Button component={RouterLink} to="/recommendations/workflow" size="small" sx={{ alignSelf: 'flex-start' }}>
                View Approval Workflow
              </Button>
            </SectionCard>

            <SectionCard title={`Pending Decisions (${dashboard.decisions.length})`}>
              {dashboard.decisions.length === 0 ? (
                <Typography variant="body2" color="text.secondary">
                  No pending Decisions.
                </Typography>
              ) : (
                <Stack spacing={1}>
                  {dashboard.decisions.map((decision) => (
                    <Stack key={decision.id} direction="row" justifyContent="space-between" alignItems="center">
                      <Box>
                        <Typography variant="body2">Decision {decision.id.slice(0, 8)}</Typography>
                        <Typography variant="caption" color="text.secondary">
                          {decision.decisionStatus} / {decision.implementationStatus} · by {decision.createdBy}
                        </Typography>
                      </Box>
                      <Button component={RouterLink} to={decision.drillDownPath} size="small">
                        View
                      </Button>
                    </Stack>
                  ))}
                </Stack>
              )}
              <Button component={RouterLink} to="/decisions" size="small" sx={{ alignSelf: 'flex-start' }}>
                View All Decisions
              </Button>
            </SectionCard>
          </Stack>

          <Paper sx={{ p: 2.5 }}>
            <Stack spacing={1.5}>
              <Typography variant="subtitle1">Activity Timeline ({dashboard.activity.length})</Typography>
              {dashboard.activity.length === 0 ? (
                <Typography variant="body2" color="text.secondary">
                  No recent activity for the selected window.
                </Typography>
              ) : (
                <Stack spacing={1}>
                  {dashboard.activity.map((item) => (
                    <Stack
                      key={item.id}
                      direction="row"
                      justifyContent="space-between"
                      alignItems="center"
                      sx={{ borderBottom: '1px solid rgba(0,0,0,0.06)', pb: 0.5 }}
                    >
                      <Box>
                        <Typography variant="body2">{item.summary}</Typography>
                        <Typography variant="caption" color="text.secondary">
                          {new Date(item.occurredAt).toLocaleString()}
                        </Typography>
                      </Box>
                      <Button component={RouterLink} to={item.drillDownPath} size="small">
                        View
                      </Button>
                    </Stack>
                  ))}
                </Stack>
              )}
              <Divider />
              <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
                <Button component={RouterLink} to="/audit" size="small">
                  View Full Audit Log
                </Button>
                <Button component={RouterLink} to="/enterprise-dashboard" size="small">
                  Enterprise Dashboard
                </Button>
                <Button component={RouterLink} to="/portfolios" size="small">
                  Portfolios
                </Button>
                <Button component={RouterLink} to="/recommendations" size="small">
                  Recommendations
                </Button>
                <Button component={RouterLink} to="/decisions" size="small">
                  Decisions
                </Button>
              </Stack>
            </Stack>
          </Paper>
        </>
      ) : null}
    </Stack>
  )
}
