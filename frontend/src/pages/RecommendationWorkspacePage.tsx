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
import { getRecommendationWorkspace } from '../api/recommendationWorkspace'
import { getErrorMessage } from '../api/client'
import { useCompany } from '../context/CompanyContext'
import type { RecommendationAction, RecommendationWorkspace } from '../types/recommendationWorkspace'

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

function ActionButton({ action }: { action: RecommendationAction }) {
  const color = action.requiresHumanApproval
    ? 'error'
    : action.isAdvisory
      ? 'warning'
      : action.isInformational
        ? 'info'
        : 'primary'
  const suffix = action.requiresHumanApproval
    ? ' (human approval)'
    : action.isAdvisory
      ? ' (advisory)'
      : action.isInformational
        ? ' (informational)'
        : ''
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

export function RecommendationWorkspacePage() {
  const { activeCompanyId } = useCompany()
  const initial = defaultDateRange()
  const [from, setFrom] = useState(initial.from)
  const [to, setTo] = useState(initial.to)
  const [leftRecommendationId, setLeftRecommendationId] = useState('')
  const [rightRecommendationId, setRightRecommendationId] = useState('')
  const [workspace, setWorkspace] = useState<RecommendationWorkspace | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await getRecommendationWorkspace({
        companyId: activeCompanyId || undefined,
        from: from ? new Date(from).toISOString() : undefined,
        to: to ? new Date(`${to}T23:59:59.999Z`).toISOString() : undefined,
        leftRecommendationId: leftRecommendationId || undefined,
        rightRecommendationId: rightRecommendationId || undefined,
      })
      setWorkspace(data)
    } catch (err) {
      setError(getErrorMessage(err))
      setWorkspace(null)
    } finally {
      setLoading(false)
    }
  }, [activeCompanyId, from, to, leftRecommendationId, rightRecommendationId])

  useEffect(() => {
    void load()
  }, [load])

  return (
    <Stack spacing={2.5}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" flexWrap="wrap" useFlexGap spacing={1}>
        <Typography variant="h5">Recommendation Workspace</Typography>
        <Stack direction="row" spacing={1}>
          <Button component={RouterLink} to="/recommendations" size="small">
            Recommendations
          </Button>
          <Button component={RouterLink} to="/recommendations/workflow" size="small">
            Approval Workflow
          </Button>
          <Button component={RouterLink} to="/decisions" size="small">
            Decisions
          </Button>
        </Stack>
      </Stack>

      <Alert severity="info">
        Recommendation Workspace orchestrates existing capabilities. Decision Engine and lifecycle unchanged. Human
        approval remains mandatory. AI is advisory; Explainability is informational.
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
            label="Compare: Left Recommendation Id"
            size="small"
            sx={{ minWidth: 260 }}
            value={leftRecommendationId}
            onChange={(event) => setLeftRecommendationId(event.target.value)}
          />
          <TextField
            label="Compare: Right Recommendation Id"
            size="small"
            sx={{ minWidth: 260 }}
            value={rightRecommendationId}
            onChange={(event) => setRightRecommendationId(event.target.value)}
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
            <KpiTile label="Active recommendations" value={workspace.kpis.activeRecommendationCount} />
            <KpiTile label="Archived" value={workspace.kpis.archivedRecommendationCount} />
            <KpiTile label="Pending approval" value={workspace.kpis.pendingApprovalCount} />
            <KpiTile label="AI recommendations" value={workspace.kpis.aiRecommendationCount} />
            <KpiTile label="Explainability" value={workspace.kpis.explainabilityCount} />
            <KpiTile label="Executive summaries" value={workspace.kpis.executiveSummaryCount} />
            <KpiTile label="History events" value={workspace.kpis.historyEventCount} />
          </Stack>

          <SectionCard title="Recommendation navigation">
            <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
              {workspace.navigation.actions.map((action) => (
                <ActionButton key={action.key} action={action} />
              ))}
            </Stack>
          </SectionCard>

          <SectionCard
            title="Recommendations"
            action={
              <Stack direction="row" spacing={1}>
                <ActionButton action={workspace.recommendations.generateAction} />
                <ActionButton action={workspace.recommendations.archiveRestoreAction} />
                <ActionButton action={workspace.recommendations.listAction} />
              </Stack>
            }
          >
            {workspace.recommendations.recommendations.length === 0 ? (
              <Typography variant="body2" color="text.secondary">
                No active Recommendations for this company.
              </Typography>
            ) : (
              <Stack spacing={1} divider={<Divider flexItem />}>
                {workspace.recommendations.recommendations.map((recommendation) => (
                  <Stack
                    key={recommendation.id}
                    direction="row"
                    justifyContent="space-between"
                    alignItems="center"
                  >
                    <Box>
                      <Typography variant="body2">
                        {recommendation.recommendationNumber} · {recommendation.title}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {recommendation.status} · v{recommendation.version} · score {recommendation.score ?? '—'} ·
                        rank {recommendation.rank ?? '—'}
                      </Typography>
                    </Box>
                    <Button component={RouterLink} to={recommendation.drillDownPath} size="small">
                      Open
                    </Button>
                  </Stack>
                ))}
              </Stack>
            )}
          </SectionCard>

          <SectionCard
            title="Approval Queue"
            action={
              <Stack direction="row" spacing={1}>
                <ActionButton action={workspace.approval.startWorkflowAction} />
                <ActionButton action={workspace.approval.approvalQueueAction} />
              </Stack>
            }
          >
            <Alert severity="error" sx={{ mb: 1 }}>
              {workspace.approval.humanApprovalDisclaimer}
            </Alert>
            {workspace.approval.pendingApprovals.length === 0 ? (
              <Typography variant="body2" color="text.secondary">
                No Recommendation Workflows pending approval.
              </Typography>
            ) : (
              <Stack spacing={1} divider={<Divider flexItem />}>
                {workspace.approval.pendingApprovals.map((approval) => (
                  <Stack
                    key={approval.workflowId}
                    direction="row"
                    justifyContent="space-between"
                    alignItems="center"
                    flexWrap="wrap"
                    useFlexGap
                  >
                    <Box>
                      <Typography variant="body2">{approval.title}</Typography>
                      <Typography variant="caption" color="text.secondary">
                        {approval.status} · created by {approval.createdBy} ·{' '}
                        {new Date(approval.createdAt).toLocaleString()}
                      </Typography>
                    </Box>
                    <Stack direction="row" spacing={1}>
                      <Button component={RouterLink} to={approval.approvePath} size="small" color="success">
                        Approve
                      </Button>
                      <Button component={RouterLink} to={approval.rejectPath} size="small" color="error">
                        Reject
                      </Button>
                      <Button component={RouterLink} to={approval.drillDownPath} size="small">
                        Open
                      </Button>
                    </Stack>
                  </Stack>
                ))}
              </Stack>
            )}
          </SectionCard>

          <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
            <SectionCard
              title="AI Recommendations (advisory)"
              action={
                <Stack direction="row" spacing={1}>
                  <ActionButton action={workspace.ai.generateAiAction} />
                  <ActionButton action={workspace.ai.aiListAction} />
                </Stack>
              }
            >
              <Alert severity="warning" sx={{ mb: 1 }}>
                {workspace.ai.aiAdvisoryDisclaimer}
              </Alert>
              {workspace.ai.aiRecommendations.length === 0 ? (
                <Typography variant="body2" color="text.secondary">
                  No AI Recommendations in this period.
                </Typography>
              ) : (
                <Stack spacing={1}>
                  {workspace.ai.aiRecommendations.map((ai) => (
                    <Stack key={ai.id} direction="row" justifyContent="space-between" alignItems="center">
                      <Typography variant="body2">
                        {ai.status} · confidence {ai.confidenceScore}
                      </Typography>
                      <Button component={RouterLink} to={ai.drillDownPath} size="small">
                        Open
                      </Button>
                    </Stack>
                  ))}
                </Stack>
              )}
            </SectionCard>

            <SectionCard
              title="Explainability (informational)"
              action={
                <Stack direction="row" spacing={1}>
                  <ActionButton action={workspace.ai.generateExplainabilityAction} />
                  <ActionButton action={workspace.ai.explainabilityListAction} />
                </Stack>
              }
            >
              <Alert severity="info" sx={{ mb: 1 }}>
                {workspace.ai.explainabilityInformationalDisclaimer}
              </Alert>
              {workspace.ai.explainability.length === 0 ? (
                <Typography variant="body2" color="text.secondary">
                  No Explainability records in this period.
                </Typography>
              ) : (
                <Stack spacing={1}>
                  {workspace.ai.explainability.map((item) => (
                    <Stack key={item.id} direction="row" justifyContent="space-between" alignItems="center">
                      <Typography variant="body2">
                        {item.explanationType} · {item.status}
                      </Typography>
                      <Button component={RouterLink} to={item.drillDownPath} size="small">
                        Open
                      </Button>
                    </Stack>
                  ))}
                </Stack>
              )}
            </SectionCard>
          </Stack>

          <SectionCard
            title="Executive Summaries"
            action={
              <Stack direction="row" spacing={1}>
                <ActionButton action={workspace.executiveSummary.generateAction} />
                <ActionButton action={workspace.executiveSummary.listAction} />
              </Stack>
            }
          >
            {workspace.executiveSummary.summaries.length === 0 ? (
              <Typography variant="body2" color="text.secondary">
                No Executive Summaries in this period.
              </Typography>
            ) : (
              <Stack spacing={1}>
                {workspace.executiveSummary.summaries.map((summary) => (
                  <Stack key={summary.id} direction="row" justifyContent="space-between" alignItems="center">
                    <Typography variant="body2">
                      v{summary.summaryVersion} · confidence {summary.confidenceLevel} · {summary.status}
                    </Typography>
                    <Button component={RouterLink} to={summary.drillDownPath} size="small">
                      Open
                    </Button>
                  </Stack>
                ))}
              </Stack>
            )}
          </SectionCard>

          <SectionCard title="Comparison" action={<ActionButton action={workspace.compare.action} />}>
            {!workspace.compare.hasComparison || !workspace.compare.comparison ? (
              <Typography variant="body2" color="text.secondary">
                Provide a Left and Right Recommendation Id above to preview a comparison.
              </Typography>
            ) : (
              <Stack spacing={1}>
                <Typography variant="body2">
                  {workspace.compare.comparison.left.title} vs {workspace.compare.comparison.right.title}
                </Typography>
                <Stack direction="row" spacing={1}>
                  <Chip
                    size="small"
                    label={workspace.compare.comparison.hasDifferences ? 'Has differences' : 'No differences'}
                    color={workspace.compare.comparison.hasDifferences ? 'warning' : 'success'}
                  />
                  <Chip size="small" label={`Score Δ ${workspace.compare.comparison.scoreDelta ?? '—'}`} />
                  <Chip size="small" label={`Rank Δ ${workspace.compare.comparison.rankDelta ?? '—'}`} />
                  <Chip size="small" label={`Version Δ ${workspace.compare.comparison.versionDelta}`} />
                </Stack>
              </Stack>
            )}
          </SectionCard>

          <SectionCard title="Recommendation History" action={<ActionButton action={workspace.history.action} />}>
            {workspace.history.items.length === 0 ? (
              <Typography variant="body2" color="text.secondary">
                No Recommendation History events in this period.
              </Typography>
            ) : (
              <Stack spacing={1} divider={<Divider flexItem />}>
                {workspace.history.items.slice(0, 20).map((item) => (
                  <Stack key={item.id} direction="row" justifyContent="space-between" alignItems="center">
                    <Box>
                      <Typography variant="body2">
                        {item.recommendationNumber} · {item.eventType}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {new Date(item.createdAt).toLocaleString()} · {item.createdBy}
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
