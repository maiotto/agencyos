import { useCallback, useEffect, useState } from 'react'
import { Link as RouterLink, useParams } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Paper,
  Stack,
  Typography,
} from '@mui/material'
import { getRecommendationHistoryEntry } from '../api/recommendationHistory'
import { getErrorMessage } from '../api/client'
import type { RecommendationHistory } from '../types/recommendationHistory'

export function RecommendationHistoryDetailPage() {
  const { id } = useParams<{ id: string }>()
  const [item, setItem] = useState<RecommendationHistory | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    if (!id) return
    setLoading(true)
    setError(null)
    try {
      setItem(await getRecommendationHistoryEntry(id))
    } catch (err) {
      setItem(null)
      setError(getErrorMessage(err, 'Failed to load history entry.'))
    } finally {
      setLoading(false)
    }
  }, [id])

  useEffect(() => {
    void load()
  }, [load])

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
        <CircularProgress />
      </Box>
    )
  }

  if (!item) {
    return (
      <Stack spacing={2}>
        {error && <Alert severity="error">{error}</Alert>}
        <Button component={RouterLink} to="/recommendations/history" variant="outlined">
          ← History
        </Button>
      </Stack>
    )
  }

  return (
    <Stack spacing={3}>
      <Stack spacing={1}>
        <Button
          component={RouterLink}
          to="/recommendations/history"
          size="small"
          sx={{ alignSelf: 'flex-start' }}
        >
          ← History
        </Button>
        <Typography variant="h4">{item.title}</Typography>
        <Typography color="text.secondary">
          {item.recommendationNumber} · v{item.recommendationVersion} · {item.eventType}
        </Typography>
        <Stack direction="row" spacing={1}>
          <Chip size="small" label={item.eventType} variant="outlined" />
          <Chip size="small" label={item.recommendationStatus} variant="outlined" />
          {item.workflowStatus && (
            <Chip size="small" label={`Workflow: ${item.workflowStatus}`} variant="outlined" />
          )}
        </Stack>
      </Stack>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Snapshot Summary
        </Typography>
        <Stack spacing={1}>
          <Typography>
            <strong>Created by:</strong> {item.createdBy} at{' '}
            {new Date(item.createdAt).toLocaleString()}
          </Typography>
          <Typography>
            <strong>Decision Engine:</strong> {item.decisionEngineVersion}
          </Typography>
          <Typography>
            <strong>Rank / Score:</strong> {item.rank ?? '—'} / {item.score ?? '—'}
          </Typography>
          <Typography>
            <strong>Approver:</strong> {item.approver ?? '—'}
          </Typography>
          <Typography>
            <strong>Approval comment:</strong> {item.approvalComment ?? '—'}
          </Typography>
          <Typography sx={{ wordBreak: 'break-all' }}>
            <strong>Recommendation Id:</strong> {item.recommendationId}
          </Typography>
          <Button
            component={RouterLink}
            to={`/recommendations/history/timeline/${item.recommendationId}`}
            size="small"
            sx={{ alignSelf: 'flex-start' }}
          >
            Open Timeline
          </Button>
        </Stack>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Recommendation Payload Snapshot
        </Typography>
        <Box component="pre" sx={preSx}>
          {formatJson(item.recommendationPayload)}
        </Box>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Capacity Snapshot
        </Typography>
        <Box component="pre" sx={preSx}>
          {formatJson(item.capacitySnapshot)}
        </Box>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Workload Snapshot
        </Typography>
        <Box component="pre" sx={preSx}>
          {formatJson(item.workloadSnapshot)}
        </Box>
      </Paper>
    </Stack>
  )
}

const preSx = {
  m: 0,
  p: 2,
  bgcolor: 'action.hover',
  borderRadius: 1,
  overflow: 'auto',
  maxHeight: 320,
  fontSize: 12,
}

function formatJson(value: string): string {
  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}
