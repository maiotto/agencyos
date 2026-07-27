import { useCallback, useEffect, useState } from 'react'
import { Link as RouterLink, useParams } from 'react-router-dom'
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
  archiveAIRecommendation,
  compareAIRecommendation,
  getAIRecommendation,
} from '../api/aiRecommendation'
import { getErrorMessage } from '../api/client'
import type {
  AIRecommendation,
  AIRecommendationComparison,
} from '../types/aiRecommendation'

function prettyJson(value: string) {
  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}

export function AIRecommendationDetailPage() {
  const { id } = useParams<{ id: string }>()
  const [item, setItem] = useState<AIRecommendation | null>(null)
  const [comparison, setComparison] = useState<AIRecommendationComparison | null>(null)
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    if (!id) return
    setLoading(true)
    setError(null)
    try {
      const detail = await getAIRecommendation(id)
      setItem(detail)
      setComparison(await compareAIRecommendation(id))
    } catch (err) {
      setItem(null)
      setComparison(null)
      setError(getErrorMessage(err, 'Failed to load AI recommendation.'))
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
      setItem(await archiveAIRecommendation(item.id))
    } catch (err) {
      setError(getErrorMessage(err))
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
        <Alert severity="error">{error ?? 'AI recommendation not found.'}</Alert>
        <Button component={RouterLink} to="/ai-recommendations" variant="outlined">
          Back to list
        </Button>
      </Stack>
    )
  }

  return (
    <Stack spacing={3}>
      <Box>
        <Button component={RouterLink} to="/ai-recommendations" size="small" sx={{ mb: 1 }}>
          ← AI Recommendations
        </Button>
        <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" spacing={2}>
          <Box>
            <Typography variant="h4" gutterBottom>
              {item.suggestedDeliveryStrategy}
            </Typography>
            <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap">
              <Chip size="small" label={item.status} color={item.archived ? 'default' : 'success'} />
              <Chip size="small" label={`g${item.generationVersion}`} variant="outlined" />
              <Chip size="small" label={item.modelVersion} variant="outlined" />
            </Stack>
          </Box>
          <Stack direction="row" spacing={1}>
            <Button
              variant="outlined"
              component={RouterLink}
              to={`/recommendations/${item.recommendationId}`}
            >
              Open Recommendation
            </Button>
            <Button
              variant="outlined"
              component={RouterLink}
              to={`/explainability/generate?recommendationId=${item.recommendationId}&aiRecommendationId=${item.id}`}
            >
              Explain
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
        Advisory only (BR-1602). AI does not replace the Recommendation and cannot approve or
        execute decisions (BR-1601 / BR-1603).
      </Alert>

      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>
          Confidence
        </Typography>
        <Typography variant="h4" gutterBottom>
          {item.confidenceScore.toFixed(1)}%
        </Typography>
        <LinearProgress
          variant="determinate"
          value={Math.min(100, Math.max(0, item.confidenceScore))}
          sx={{ height: 10, borderRadius: 1 }}
        />
      </Paper>

      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>
          Executive Summary
        </Typography>
        <Typography>{item.executiveSummary}</Typography>
      </Paper>

      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>
          Reasoning
        </Typography>
        <Typography whiteSpace="pre-wrap">{item.reasoning}</Typography>
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
          Alternative Strategies
        </Typography>
        <Box component="pre" sx={{ m: 0, fontSize: 12, overflow: 'auto' }}>
          {prettyJson(item.alternatives)}
        </Box>
      </Paper>

      <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
        <Paper sx={{ p: 2, flex: 1 }}>
          <Typography variant="h6" gutterBottom>
            Suggested Capacity Impact
          </Typography>
          <Box component="pre" sx={{ m: 0, fontSize: 12, overflow: 'auto' }}>
            {prettyJson(item.suggestedCapacityImpact)}
          </Box>
        </Paper>
        <Paper sx={{ p: 2, flex: 1 }}>
          <Typography variant="h6" gutterBottom>
            Suggested Workload Impact
          </Typography>
          <Box component="pre" sx={{ m: 0, fontSize: 12, overflow: 'auto' }}>
            {prettyJson(item.suggestedWorkloadImpact)}
          </Box>
        </Paper>
      </Stack>

      {comparison && (
        <Paper sx={{ p: 2 }}>
          <Typography variant="h6" gutterBottom>
            Comparison vs Recommendation
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Left = current Recommendation ({comparison.recommendationTitle}); Right = AI suggestion.
          </Typography>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Path</TableCell>
                <TableCell>Recommendation</TableCell>
                <TableCell>AI Suggestion</TableCell>
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
