import { useCallback, useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  LinearProgress,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import {
  archiveExecutiveSummary,
  compareExecutiveSummary,
  createExecutiveSummaryVersion,
  getExecutiveSummary,
} from '../api/executiveSummary'
import { getErrorMessage } from '../api/client'
import type {
  ExecutiveRecommendationSummary,
  ExecutiveRecommendationSummaryComparison,
} from '../types/executiveSummary'

function prettyJson(value: string) {
  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}

export function ExecutiveSummaryDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [item, setItem] = useState<ExecutiveRecommendationSummary | null>(null)
  const [comparison, setComparison] = useState<ExecutiveRecommendationSummaryComparison | null>(null)
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    if (!id) return
    setLoading(true)
    setError(null)
    try {
      const detail = await getExecutiveSummary(id)
      setItem(detail)
      setComparison(await compareExecutiveSummary(id))
    } catch (err) {
      setItem(null)
      setComparison(null)
      setError(getErrorMessage(err, 'Failed to load executive summary.'))
    } finally {
      setLoading(false)
    }
  }, [id])

  useEffect(() => {
    void load()
  }, [load])

  const onArchive = async () => {
    if (!item) return
    setBusy(true)
    setError(null)
    try {
      setItem(await archiveExecutiveSummary(item.id))
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  const onRegenerate = async () => {
    if (!item) return
    setBusy(true)
    setError(null)
    try {
      const created = await createExecutiveSummaryVersion(item.id, {
        generatedBy: item.generatedBy || 'executive@agencyos.local',
        aiRecommendationId: item.aiRecommendationId,
        explainabilityId: item.explainabilityId,
      })
      navigate(`/executive-summaries/${created.id}`)
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to regenerate executive summary.'))
    } finally {
      setBusy(false)
    }
  }

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" py={8}>
        <CircularProgress />
      </Box>
    )
  }

  if (!item) {
    return (
      <Stack spacing={2}>
        <Alert severity="error">{error ?? 'Executive summary not found.'}</Alert>
        <Button component={RouterLink} to="/executive-summaries" variant="outlined">
          Back to list
        </Button>
      </Stack>
    )
  }

  return (
    <Stack spacing={3}>
      <Box>
        <Button component={RouterLink} to="/executive-summaries" size="small" sx={{ mb: 1 }}>
          ← Executive Summaries
        </Button>
        <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" spacing={2}>
          <Box>
            <Typography variant="h4" gutterBottom>
              Executive Briefing v{item.summaryVersion}
            </Typography>
            <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap">
              <Chip size="small" label={item.status} color={item.archived ? 'default' : 'success'} />
              <Chip size="small" label={item.modelVersion} variant="outlined" />
            </Stack>
          </Box>
          <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap">
            <Button
              variant="outlined"
              component={RouterLink}
              to={`/recommendations/${item.recommendationId}`}
            >
              Open Recommendation
            </Button>
            <Button variant="contained" disabled={busy} onClick={() => void onRegenerate()}>
              Regenerate
            </Button>
            {!item.archived && (
              <Button variant="outlined" disabled={busy} onClick={() => void onArchive()}>
                Archive
              </Button>
            )}
          </Stack>
        </Stack>
      </Box>

      {error && <Alert severity="error">{error}</Alert>}

      <Alert severity="info">
        Informational only (BR-1801). Executive summaries never change Recommendations, AI
        Recommendations, or Decisions.
      </Alert>

      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>
          Executive Summary
        </Typography>
        <Typography>{item.executiveSummary}</Typography>
      </Paper>

      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>
          Confidence
        </Typography>
        <Typography variant="h4" gutterBottom>
          {item.confidenceLevel.toFixed(1)}%
        </Typography>
        <LinearProgress
          variant="determinate"
          value={Math.min(100, Math.max(0, item.confidenceLevel))}
          sx={{ height: 10, borderRadius: 1 }}
        />
      </Paper>

      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>
          Business Impact
        </Typography>
        <Typography whiteSpace="pre-wrap">{item.businessImpact}</Typography>
      </Paper>

      <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
        <Paper sx={{ p: 2, flex: 1 }}>
          <Typography variant="h6" gutterBottom>
            Capacity Impact
          </Typography>
          <Typography whiteSpace="pre-wrap">{item.capacityImpact}</Typography>
        </Paper>
        <Paper sx={{ p: 2, flex: 1 }}>
          <Typography variant="h6" gutterBottom>
            Workload Impact
          </Typography>
          <Typography whiteSpace="pre-wrap">{item.workloadImpact}</Typography>
        </Paper>
      </Stack>

      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>
          Key Decision Factors
        </Typography>
        <Box component="pre" sx={{ m: 0, fontSize: 12, overflow: 'auto' }}>
          {prettyJson(item.keyDecisionFactors)}
        </Box>
      </Paper>

      <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
        <Paper sx={{ p: 2, flex: 1 }}>
          <Typography variant="h6" gutterBottom>
            Assumptions
          </Typography>
          <Box component="pre" sx={{ m: 0, fontSize: 12, overflow: 'auto' }}>
            {prettyJson(item.assumptions)}
          </Box>
        </Paper>
        <Paper sx={{ p: 2, flex: 1 }}>
          <Typography variant="h6" gutterBottom>
            Risks
          </Typography>
          <Box component="pre" sx={{ m: 0, fontSize: 12, overflow: 'auto' }}>
            {prettyJson(item.risks)}
          </Box>
        </Paper>
      </Stack>

      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>
          Recommended Actions
        </Typography>
        <Box component="pre" sx={{ m: 0, fontSize: 12, overflow: 'auto' }}>
          {prettyJson(item.recommendedActions)}
        </Box>
      </Paper>

      {comparison && (
        <Paper sx={{ p: 2 }}>
          <Typography variant="h6" gutterBottom>
            Comparison vs Recommendation
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Left = Recommendation ({comparison.recommendationTitle}); Right = Executive Summary.
          </Typography>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Path</TableCell>
                <TableCell>Recommendation</TableCell>
                <TableCell>Executive Summary</TableCell>
                <TableCell>Status</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {comparison.differences.map((field) => (
                <TableRow
                  key={field.path}
                  sx={field.changed ? { backgroundColor: 'rgba(211, 47, 47, 0.06)' } : undefined}
                >
                  <TableCell>{field.path}</TableCell>
                  <TableCell sx={{ maxWidth: 280, wordBreak: 'break-word' }}>
                    {field.leftValue ?? '—'}
                  </TableCell>
                  <TableCell sx={{ maxWidth: 280, wordBreak: 'break-word' }}>
                    {field.rightValue ?? '—'}
                  </TableCell>
                  <TableCell>{field.changed ? 'Changed' : 'Same'}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Paper>
      )}

      <Paper sx={{ p: 2 }}>
        <Typography variant="body2">Prompt: {item.promptVersion}</Typography>
        <Typography variant="body2">Model: {item.modelVersion}</Typography>
        <Typography variant="body2">Generated by: {item.generatedBy}</Typography>
      </Paper>
    </Stack>
  )
}
