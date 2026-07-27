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
import { getPlanningWorkspace } from '../api/planningWorkspace'
import { getErrorMessage } from '../api/client'
import { useCompany } from '../context/CompanyContext'
import type { PlanningAction, PlanningWorkspace } from '../types/planningWorkspace'

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
    <Paper sx={{ p: 2, flex: '1 1 160px', minWidth: 140 }} variant="outlined">
      <Typography variant="subtitle2" color="text.secondary">
        {label}
      </Typography>
      <Typography variant="h5">{value}</Typography>
    </Paper>
  )
}

function ActionButton({ action }: { action: PlanningAction }) {
  return (
    <Button
      component={RouterLink}
      to={action.drillDownPath}
      size="small"
      variant={action.isAdvisory ? 'outlined' : 'text'}
      color={action.isAdvisory ? 'warning' : 'primary'}
      title={action.description}
    >
      {action.label}
      {action.isAdvisory ? ' (advisory)' : ''}
    </Button>
  )
}

export function PlanningWorkspacePage() {
  const { activeCompanyId } = useCompany()
  const initial = defaultDateRange()
  const [from, setFrom] = useState(initial.from)
  const [to, setTo] = useState(initial.to)
  const [periodStart, setPeriodStart] = useState(initial.from)
  const [periodEnd, setPeriodEnd] = useState(initial.to)
  const [workspace, setWorkspace] = useState<PlanningWorkspace | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await getPlanningWorkspace({
        companyId: activeCompanyId || undefined,
        from: from ? new Date(from).toISOString() : undefined,
        to: to ? new Date(`${to}T23:59:59.999Z`).toISOString() : undefined,
        periodStart,
        periodEnd,
      })
      setWorkspace(data)
    } catch (err) {
      setError(getErrorMessage(err))
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
        <Typography variant="h5">Planning Workspace</Typography>
        <Stack direction="row" spacing={1}>
          <Button component={RouterLink} to="/planning-templates" size="small">
            Templates
          </Button>
          <Button component={RouterLink} to="/cross-portfolio-planning" size="small">
            Cross-Portfolio
          </Button>
          <Button component={RouterLink} to="/enterprise-dashboard" size="small">
            Enterprise Dashboard
          </Button>
        </Stack>
      </Stack>

      <Alert severity="info">
        Planning Workspace orchestrates existing planning capabilities. Engines and history are unchanged.
        Cross-Portfolio Planning is advisory only.
      </Alert>

      <Paper sx={{ p: 2 }}>
        <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap alignItems="center">
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
            label="Period start"
            type="date"
            size="small"
            InputLabelProps={{ shrink: true }}
            value={periodStart}
            onChange={(event) => setPeriodStart(event.target.value)}
          />
          <TextField
            label="Period end"
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
          <Stack direction="row" spacing={1.5} flexWrap="wrap" useFlexGap>
            <KpiTile label="Templates" value={workspace.kpis.templateCount} />
            <KpiTile label="Active templates" value={workspace.kpis.activeTemplateCount} />
            <KpiTile label="Portfolios" value={workspace.kpis.portfolioCount} />
            <KpiTile label="Scenarios" value={workspace.kpis.scenarioCount} />
            <KpiTile label="Avg utilization %" value={workspace.kpis.averageUtilizationPercentage} />
            <KpiTile label="Avg workload %" value={workspace.kpis.averageWorkloadPercentage} />
            <KpiTile label="Overall health" value={workspace.kpis.overallHealth.label} />
          </Stack>

          <SectionCard title="Planning navigation">
            <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
              {workspace.navigation.actions.map((action) => (
                <ActionButton key={action.key} action={action} />
              ))}
            </Stack>
          </SectionCard>

          <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
            <SectionCard
              title="Capacity"
              action={<ActionButton action={workspace.capacity.action} />}
            >
              <Typography variant="body2">
                Records: {workspace.capacity.recordCount} · Avg utilization:{' '}
                {workspace.capacity.averageUtilizationPercentage}%
              </Typography>
              <LinearProgress
                variant="determinate"
                value={Math.min(100, Number(workspace.capacity.averageUtilizationPercentage) || 0)}
                sx={{ height: 8, borderRadius: 1 }}
              />
              <Chip size="small" label={workspace.capacity.utilizationHealth.label} />
            </SectionCard>

            <SectionCard
              title="Workload"
              action={<ActionButton action={workspace.workload.action} />}
            >
              <Typography variant="body2">
                Records: {workspace.workload.recordCount} · Avg workload:{' '}
                {workspace.workload.averageWorkloadPercentage}%
              </Typography>
              <LinearProgress
                variant="determinate"
                value={Math.min(100, Number(workspace.workload.averageWorkloadPercentage) || 0)}
                sx={{ height: 8, borderRadius: 1 }}
              />
              <Chip size="small" label={workspace.workload.workloadHealth.label} />
            </SectionCard>
          </Stack>

          <SectionCard
            title="Planning Templates"
            action={
              <Stack direction="row" spacing={1}>
                <ActionButton action={workspace.templates.manageAction} />
                <ActionButton action={workspace.templates.applyAction} />
              </Stack>
            }
          >
            {workspace.templates.templates.length === 0 ? (
              <Typography variant="body2" color="text.secondary">
                No templates for this company.
              </Typography>
            ) : (
              <Stack spacing={1}>
                {workspace.templates.templates.map((template) => (
                  <Stack
                    key={template.id}
                    direction="row"
                    justifyContent="space-between"
                    alignItems="center"
                    sx={{ borderBottom: '1px solid rgba(0,0,0,0.06)', pb: 0.5 }}
                  >
                    <Box>
                      <Typography variant="body2">{template.name}</Typography>
                      <Typography variant="caption" color="text.secondary">
                        {template.status} · window {template.defaultPlanningWindowDays}d
                      </Typography>
                    </Box>
                    <Button component={RouterLink} to={template.drillDownPath} size="small">
                      Open
                    </Button>
                  </Stack>
                ))}
              </Stack>
            )}
          </SectionCard>

          <SectionCard
            title="Portfolios"
            action={<ActionButton action={workspace.portfolios.action} />}
          >
            {workspace.portfolios.portfolios.length === 0 ? (
              <Typography variant="body2" color="text.secondary">
                No portfolios for this company.
              </Typography>
            ) : (
              <Stack spacing={1}>
                {workspace.portfolios.portfolios.map((portfolio) => (
                  <Stack
                    key={portfolio.id}
                    direction="row"
                    justifyContent="space-between"
                    alignItems="center"
                    sx={{ borderBottom: '1px solid rgba(0,0,0,0.06)', pb: 0.5 }}
                  >
                    <Box>
                      <Typography variant="body2">{portfolio.name}</Typography>
                      <Typography variant="caption" color="text.secondary">
                        {portfolio.portfolioHealth} · {portfolio.planningPeriodStart} →{' '}
                        {portfolio.planningPeriodEnd}
                      </Typography>
                    </Box>
                    <Button component={RouterLink} to={portfolio.drillDownPath} size="small">
                      Open
                    </Button>
                  </Stack>
                ))}
              </Stack>
            )}
          </SectionCard>

          <SectionCard
            title="Scenarios (advisory)"
            action={<ActionButton action={workspace.scenarios.comparisonAction} />}
          >
            <Alert severity="warning" sx={{ mb: 1 }}>
              {workspace.scenarios.advisoryDisclaimer}
            </Alert>
            {workspace.scenarios.scenarios.length === 0 ? (
              <Typography variant="body2" color="text.secondary">
                No temporary scenarios in memory for this company.
              </Typography>
            ) : (
              <Stack spacing={1}>
                {workspace.scenarios.scenarios.map((scenario) => (
                  <Box key={scenario.scenarioId}>
                    <Typography variant="body2">
                      {scenario.scenarioName || scenario.scenarioId}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      Portfolios: {scenario.portfolioCount} · Conflicts: {scenario.conflictCount}
                    </Typography>
                  </Box>
                ))}
              </Stack>
            )}
          </SectionCard>

          <SectionCard title="Planning History" action={<ActionButton action={workspace.history.action} />}>
            {workspace.history.items.length === 0 ? (
              <Typography variant="body2" color="text.secondary">
                No planning audit events in this period.
              </Typography>
            ) : (
              <Stack spacing={1} divider={<Divider flexItem />}>
                {workspace.history.items.slice(0, 20).map((item) => (
                  <Stack
                    key={item.id}
                    direction="row"
                    justifyContent="space-between"
                    alignItems="center"
                  >
                    <Box>
                      <Typography variant="body2">
                        {item.entityType} · {item.action}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {new Date(item.occurredAt).toLocaleString()} · {item.userId}
                      </Typography>
                    </Box>
                    <Button component={RouterLink} to={item.drillDownPath} size="small">
                      Audit
                    </Button>
                  </Stack>
                ))}
              </Stack>
            )}
          </SectionCard>
        </>
      )}
    </Stack>
  )
}
