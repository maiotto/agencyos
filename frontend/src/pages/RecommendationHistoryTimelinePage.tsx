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
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import {
  getRecommendationHistoryTimeline,
  getRecommendationHistoryVersions,
} from '../api/recommendationHistory'
import { getErrorMessage } from '../api/client'
import type {
  RecommendationHistory,
  RecommendationHistoryTimelineEntry,
} from '../types/recommendationHistory'

export function RecommendationHistoryTimelinePage() {
  const { recommendationId } = useParams<{ recommendationId: string }>()
  const [timeline, setTimeline] = useState<RecommendationHistoryTimelineEntry[]>([])
  const [versions, setVersions] = useState<RecommendationHistory[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    if (!recommendationId) return
    setLoading(true)
    setError(null)
    try {
      const [timelineData, versionData] = await Promise.all([
        getRecommendationHistoryTimeline(recommendationId),
        getRecommendationHistoryVersions(recommendationId),
      ])
      setTimeline(timelineData)
      setVersions(versionData)
    } catch (err) {
      setTimeline([])
      setVersions([])
      setError(getErrorMessage(err, 'Failed to load recommendation timeline.'))
    } finally {
      setLoading(false)
    }
  }, [recommendationId])

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
        <Typography variant="h4">Recommendation Timeline</Typography>
        <Typography color="text.secondary" sx={{ wordBreak: 'break-all' }}>
          Recommendation Id: {recommendationId}
        </Typography>
      </Stack>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Combined Timeline (Versions + Workflow)
        </Typography>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>When</TableCell>
              <TableCell>Event</TableCell>
              <TableCell>Version</TableCell>
              <TableCell>Workflow</TableCell>
              <TableCell>Actor</TableCell>
              <TableCell>Title</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {timeline.map((entry) => (
              <TableRow key={entry.id}>
                <TableCell>{new Date(entry.createdAt).toLocaleString()}</TableCell>
                <TableCell>
                  <Chip size="small" label={entry.eventType} variant="outlined" />
                </TableCell>
                <TableCell>v{entry.recommendationVersion}</TableCell>
                <TableCell>{entry.workflowStatus ?? '—'}</TableCell>
                <TableCell>{entry.approver ?? entry.createdBy}</TableCell>
                <TableCell>{entry.title}</TableCell>
              </TableRow>
            ))}
            {timeline.length === 0 && (
              <TableRow>
                <TableCell colSpan={6}>
                  <Typography color="text.secondary">No timeline events.</Typography>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Version Viewer
        </Typography>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Version</TableCell>
              <TableCell>Title</TableCell>
              <TableCell>Score</TableCell>
              <TableCell>Created</TableCell>
              <TableCell align="right">Snapshot</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {versions.map((version) => (
              <TableRow key={version.id}>
                <TableCell>v{version.recommendationVersion}</TableCell>
                <TableCell>{version.title}</TableCell>
                <TableCell>{version.score ?? '—'}</TableCell>
                <TableCell>{new Date(version.createdAt).toLocaleString()}</TableCell>
                <TableCell align="right">
                  <Button
                    size="small"
                    component={RouterLink}
                    to={`/recommendations/history/${version.id}`}
                  >
                    View
                  </Button>
                </TableCell>
              </TableRow>
            ))}
            {versions.length === 0 && (
              <TableRow>
                <TableCell colSpan={5}>
                  <Typography color="text.secondary">No version history.</Typography>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </Paper>
    </Stack>
  )
}
