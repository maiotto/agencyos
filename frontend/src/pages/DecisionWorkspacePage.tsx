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
import { getDecisionWorkspace } from '../api/decisionWorkspace'
import { getErrorMessage } from '../api/client'
import { useCompany } from '../context/CompanyContext'
import type { DecisionAction, DecisionCard, DecisionWorkspace } from '../types/decisionWorkspace'

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

function ActionButton({ action }: { action: DecisionAction }) {
  const color = action.requiresHumanApproval ? 'error' : 'primary'
  const suffix = action.requiresHumanApproval ? ' (human approval)' : ''
  return (
    <Button
      component={RouterLink}
      to={action.drillDownPath}
      size="small"
      variant="outlined"
      color={color}
      title={action.description}
    >
      {action.label}
      {suffix}
    </Button>
  )
}

function DecisionQueueList({ decisions, emptyLabel }: { decisions: DecisionCard[]; emptyLabel: string }) {
  if (decisions.length === 0) {
    return (
      <Typography variant="body2" color="text.secondary">
        {emptyLabel}
      </Typography>
    )
  }

  return (
    <Stack spacing={1} divider={<Divider flexItem />}>
      {decisions.map((decision) => (
        <Stack key={decision.id} direction="row" justifyContent="space-between" alignItems="center" flexWrap="wrap" useFlexGap>
          <Box>
            <Typography variant="body2">
              {decision.decisionStatus} · Implementation: {decision.implementationStatus}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Decision date {new Date(decision.decisionDate).toLocaleDateString()} · by {decision.createdBy}
              {decision.outcome ? ` · outcome: ${decision.outcome}` : ''}
            </Typography>
          </Box>
          <Stack direction="row" spacing={1}>
            <Button component={RouterLink} to={decision.recommendationDrillDownPath} size="small">
              Recommendation
            </Button>
            <Button component={RouterLink} to={decision.drillDownPath} size="small" variant="outlined">
              Open
            </Button>
          </Stack>
        </Stack>
      ))}
    </Stack>
  )
}

export function DecisionWorkspacePage() {
  const { activeCompanyId } = useCompany()
  const initial = defaultDateRange()
  const [from, setFrom] = useState(initial.from)
  const [to, setTo] = useState(initial.to)
  const [decisionId, setDecisionId] = useState('')
  const [workspace, setWorkspace] = useState<DecisionWorkspace | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await getDecisionWorkspace({
        companyId: activeCompanyId || undefined,
        from: from ? new Date(from).toISOString() : undefined,
        to: to ? new Date(`${to}T23:59:59.999Z`).toISOString() : undefined,
        decisionId: decisionId || undefined,
      })
      setWorkspace(data)
    } catch (err) {
      setError(getErrorMessage(err))
      setWorkspace(null)
    } finally {
      setLoading(false)
    }
  }, [activeCompanyId, from, to, decisionId])

  useEffect(() => {
    void load()
  }, [load])

  return (
    <Stack spacing={2.5}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" flexWrap="wrap" useFlexGap spacing={1}>
        <Typography variant="h5">Decision Workspace</Typography>
        <Stack direction="row" spacing={1}>
          <Button component={RouterLink} to="/decisions" size="small">
            Decisions
          </Button>
          <Button component={RouterLink} to="/recommendation-workspace" size="small">
            Recommendation Workspace
          </Button>
          <Button component={RouterLink} to="/enterprise-dashboard" size="small">
            Enterprise Dashboard
          </Button>
        </Stack>
      </Stack>

      <Alert severity="info">
        Decision Workspace orchestrates existing Decision Tracking. Lifecycle unchanged. Create/Start/Complete/Cancel/Record
        Outcome navigate to existing Decision pages — nothing executes automatically.
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
            label="Focus Timeline: Decision Id"
            size="small"
            sx={{ minWidth: 280 }}
            value={decisionId}
            onChange={(event) => setDecisionId(event.target.value)}
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
            <KpiTile label="Total decisions" value={workspace.kpis.totalCount} />
            <KpiTile label="Pending" value={workspace.kpis.pendingCount} />
            <KpiTile label="In progress" value={workspace.kpis.inProgressCount} />
            <KpiTile label="Completed" value={workspace.kpis.completedCount} />
            <KpiTile label="Cancelled" value={workspace.kpis.cancelledCount} />
            <KpiTile label="With outcome" value={workspace.kpis.withOutcomeCount} />
            <KpiTile label="Implementation not started" value={workspace.kpis.implementationNotStartedCount} />
          </Stack>

          <Alert severity="warning">{workspace.overview.humanApprovalDisclaimer}</Alert>

          <SectionCard title="Decision navigation">
            <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
              {workspace.navigation.actions.map((action) => (
                <ActionButton key={action.key} action={action} />
              ))}
            </Stack>
          </SectionCard>

          <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
            <SectionCard
              title={`Pending (${workspace.decisions.pending.length})`}
              action={<ActionButton action={workspace.decisions.createAction} />}
            >
              <DecisionQueueList decisions={workspace.decisions.pending} emptyLabel="No pending Decisions." />
            </SectionCard>

            <SectionCard title={`In Progress (${workspace.decisions.inProgress.length})`}>
              <DecisionQueueList decisions={workspace.decisions.inProgress} emptyLabel="No Decisions in progress." />
            </SectionCard>

            <SectionCard
              title={`Completed (${workspace.decisions.completed.length})`}
              action={<ActionButton action={workspace.decisions.listAction} />}
            >
              <DecisionQueueList decisions={workspace.decisions.completed} emptyLabel="No completed Decisions." />
            </SectionCard>
          </Stack>

          <SectionCard
            title={workspace.timeline.decisionId ? 'Timeline (focused Decision)' : 'Timeline (recent Decisions)'}
            action={<ActionButton action={workspace.timeline.action} />}
          >
            {workspace.timeline.items.length === 0 ? (
              <Typography variant="body2" color="text.secondary">
                No Decision Timeline events in this period.
              </Typography>
            ) : (
              <Stack spacing={1} divider={<Divider flexItem />}>
                {workspace.timeline.items.map((item, index) => (
                  <Stack key={`${item.decisionId}-${index}`} direction="row" justifyContent="space-between" alignItems="center">
                    <Box>
                      <Typography variant="body2">
                        {item.eventType} · {item.fromDecisionStatus} → {item.toDecisionStatus}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {new Date(item.occurredAt).toLocaleString()} · {item.actor}
                        {item.comment ? ` · ${item.comment}` : ''}
                      </Typography>
                    </Box>
                    <Button component={RouterLink} to={item.drillDownPath} size="small">
                      Open
                    </Button>
                  </Stack>
                ))}
              </Stack>
            )}
          </SectionCard>

          <SectionCard title="Outcomes" action={<ActionButton action={workspace.outcomes.recordOutcomeAction} />}>
            {workspace.outcomes.outcomes.length === 0 ? (
              <Typography variant="body2" color="text.secondary">
                No Decisions with a recorded Outcome in this period.
              </Typography>
            ) : (
              <Stack spacing={1} divider={<Divider flexItem />}>
                {workspace.outcomes.outcomes.map((outcome) => (
                  <Stack key={outcome.id} direction="row" justifyContent="space-between" alignItems="center" flexWrap="wrap" useFlexGap>
                    <Box>
                      <Typography variant="body2">{outcome.outcome}</Typography>
                      <Typography variant="caption" color="text.secondary">
                        {outcome.decisionStatus} · {outcome.implementationStatus}
                        {outcome.completedDate ? ` · completed ${new Date(outcome.completedDate).toLocaleDateString()}` : ''}
                        {outcome.businessValue ? ` · value: ${outcome.businessValue}` : ''}
                      </Typography>
                    </Box>
                    <Stack direction="row" spacing={1}>
                      <Chip size="small" label="Outcome recorded" color="success" />
                      <Button component={RouterLink} to={outcome.drillDownPath} size="small">
                        Open
                      </Button>
                    </Stack>
                  </Stack>
                ))}
              </Stack>
            )}
          </SectionCard>

          <SectionCard title="Audit Trail" action={<ActionButton action={workspace.audit.action} />}>
            {workspace.audit.items.length === 0 ? (
              <Typography variant="body2" color="text.secondary">
                No Decision Audit events in this period.
              </Typography>
            ) : (
              <Stack spacing={1} divider={<Divider flexItem />}>
                {workspace.audit.items.slice(0, 20).map((item) => (
                  <Stack key={item.id} direction="row" justifyContent="space-between" alignItems="center">
                    <Box>
                      <Typography variant="body2">
                        {item.eventType} · {item.action}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {new Date(item.occurredAt).toLocaleString()} · {item.userId}
                      </Typography>
                    </Box>
                    <Button component={RouterLink} to={item.drillDownPath} size="small">
                      Open
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
