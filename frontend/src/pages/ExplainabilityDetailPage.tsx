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
  Typography,
} from '@mui/material'
import { archiveExplainability, getExplainability } from '../api/explainability'
import { getErrorMessage } from '../api/client'
import type { Explainability } from '../types/explainability'

function prettyJson(value: string) {
  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}

function confidencePercent(text: string): number | null {
  const match = text.match(/(\d+(?:\.\d+)?)\s*%/)
  if (!match) return null
  const value = Number(match[1])
  return Number.isFinite(value) ? Math.min(100, Math.max(0, value)) : null
}

export function ExplainabilityDetailPage() {
  const { id } = useParams<{ id: string }>()
  const [item, setItem] = useState<Explainability | null>(null)
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    if (!id) return
    setLoading(true)
    setError(null)
    try {
      setItem(await getExplainability(id))
    } catch (err) {
      setItem(null)
      setError(getErrorMessage(err, 'Failed to load explanation.'))
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
      setItem(await archiveExplainability(item.id))
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
        <Alert severity="error">{error ?? 'Explanation not found.'}</Alert>
        <Button component={RouterLink} to="/explainability" variant="outlined">
          Back to list
        </Button>
      </Stack>
    )
  }

  const confidence = confidencePercent(item.confidenceExplanation)

  return (
    <Stack spacing={3}>
      <Box>
        <Button component={RouterLink} to="/explainability" size="small" sx={{ mb: 1 }}>
          ← Explainability
        </Button>
        <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" spacing={2}>
          <Box>
            <Typography variant="h4" gutterBottom>
              Explanation g{item.generationVersion}
            </Typography>
            <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap">
              <Chip size="small" label={item.status} color={item.archived ? 'default' : 'success'} />
              <Chip size="small" label={item.explanationType} variant="outlined" />
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
            {item.aiRecommendationId && (
              <Button
                variant="outlined"
                component={RouterLink}
                to={`/ai-recommendations/${item.aiRecommendationId}`}
              >
                Open AI Recommendation
              </Button>
            )}
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
        Informational only (BR-1701). Explainability never changes Recommendations or Decision
        Engine calculations.
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
        <Typography sx={{ mb: 1 }}>{item.confidenceExplanation}</Typography>
        {confidence !== null && (
          <LinearProgress
            variant="determinate"
            value={confidence}
            sx={{ height: 10, borderRadius: 1 }}
          />
        )}
      </Paper>

      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>
          Detailed Explanation
        </Typography>
        <Typography whiteSpace="pre-wrap">{item.detailedExplanation}</Typography>
      </Paper>

      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>
          Decision Factors
        </Typography>
        <Box component="pre" sx={{ m: 0, fontSize: 12, overflow: 'auto' }}>
          {prettyJson(item.decisionFactors)}
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

      <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
        <Paper sx={{ p: 2, flex: 1 }}>
          <Typography variant="h6" gutterBottom>
            Capacity Impact
          </Typography>
          <Typography whiteSpace="pre-wrap">{item.capacityExplanation}</Typography>
        </Paper>
        <Paper sx={{ p: 2, flex: 1 }}>
          <Typography variant="h6" gutterBottom>
            Workload Impact
          </Typography>
          <Typography whiteSpace="pre-wrap">{item.workloadExplanation}</Typography>
        </Paper>
      </Stack>

      <Paper sx={{ p: 2 }}>
        <Typography variant="body2">Prompt: {item.promptVersion}</Typography>
        <Typography variant="body2">Model: {item.modelVersion}</Typography>
        <Typography variant="body2">Generated by: {item.generatedBy}</Typography>
      </Paper>
    </Stack>
  )
}
