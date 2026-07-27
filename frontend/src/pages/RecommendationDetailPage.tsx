import { useCallback, useEffect, useState } from 'react'
import {
  Alert,
  Box,
  Button,
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
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import {
  archiveRecommendation,
  getRecommendation,
  getRecommendationVersions,
  restoreRecommendation,
} from '../api/recommendation'
import { getErrorMessage } from '../api/client'
import { RecommendationPersistenceStatusBadge } from '../components/RecommendationPersistenceStatusBadge'
import type { Recommendation } from '../types/recommendation'

export function RecommendationDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [item, setItem] = useState<Recommendation | null>(null)
  const [versions, setVersions] = useState<Recommendation[]>([])
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    if (!id) return
    setLoading(true)
    setError(null)
    try {
      const [recommendation, versionList] = await Promise.all([
        getRecommendation(id),
        getRecommendationVersions(id),
      ])
      setItem(recommendation)
      setVersions(versionList)
    } catch (err) {
      setItem(null)
      setVersions([])
      setError(getErrorMessage(err, 'Failed to load recommendation.'))
    } finally {
      setLoading(false)
    }
  }, [id])

  useEffect(() => {
    void load()
  }, [load])

  const onArchive = async () => {
    if (!id) return
    setBusy(true)
    setError(null)
    try {
      await archiveRecommendation(id)
      await load()
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to archive recommendation.'))
    } finally {
      setBusy(false)
    }
  }

  const onRestore = async () => {
    if (!id) return
    setBusy(true)
    setError(null)
    try {
      await restoreRecommendation(id)
      await load()
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to restore recommendation.'))
    } finally {
      setBusy(false)
    }
  }

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
        <Button component={RouterLink} to="/recommendations" variant="outlined">
          ← Recommendations
        </Button>
      </Stack>
    )
  }

  return (
    <Stack spacing={3}>
      <Stack spacing={1}>
        <Button component={RouterLink} to="/recommendations" size="small" sx={{ alignSelf: 'flex-start' }}>
          ← Recommendations
        </Button>
        <Stack
          direction={{ xs: 'column', sm: 'row' }}
          justifyContent="space-between"
          alignItems={{ xs: 'stretch', sm: 'center' }}
          spacing={2}
        >
          <Box>
            <Typography variant="h4">{item.title}</Typography>
            <Typography color="text.secondary">
              {item.recommendationNumber} · v{item.version}
            </Typography>
          </Box>
          <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
            <RecommendationPersistenceStatusBadge status={item.status} />
            {!item.archived ? (
              <Button variant="outlined" color="warning" disabled={busy} onClick={() => void onArchive()}>
                Archive
              </Button>
            ) : (
              <Button variant="outlined" disabled={busy} onClick={() => void onRestore()}>
                Restore
              </Button>
            )}
            <Button
              variant="contained"
              component={RouterLink}
              to={`/recommendations/workflow/new?recommendationId=${item.id}`}
              disabled={item.archived}
            >
              Start Workflow
            </Button>
            <Button
              variant="outlined"
              component={RouterLink}
              to={`/recommendations/history/timeline/${item.id}`}
            >
              History
            </Button>
            <Button
              variant="outlined"
              component={RouterLink}
              to={`/decisions/new?recommendationId=${item.id}`}
              disabled={item.archived}
            >
              Track Decision
            </Button>
            <Button
              variant="outlined"
              component={RouterLink}
              to={`/ai-recommendations/generate?recommendationId=${item.id}`}
              disabled={item.archived}
            >
              Ask AI
            </Button>
            <Button
              variant="outlined"
              component={RouterLink}
              to={`/explainability/generate?recommendationId=${item.id}`}
              disabled={item.archived}
            >
              Explain
            </Button>
            <Button
              variant="outlined"
              component={RouterLink}
              to={`/executive-summaries/generate?recommendationId=${item.id}`}
              disabled={item.archived}
            >
              Exec Summary
            </Button>
          </Stack>
        </Stack>
      </Stack>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Summary
        </Typography>
        <Stack spacing={1}>
          <Typography>
            <strong>Reason:</strong> {item.reason ?? '—'}
          </Typography>
          <Typography>
            <strong>Summary:</strong> {item.summary ?? '—'}
          </Typography>
          <Typography>
            <strong>Rank / Score:</strong> {item.rank ?? '—'} / {item.score ?? '—'}
          </Typography>
          <Typography>
            <strong>Generated by:</strong> {item.generatedBy} at{' '}
            {new Date(item.generatedAt).toLocaleString()}
          </Typography>
          <Typography>
            <strong>Decision Engine:</strong> {item.decisionEngineVersion}
          </Typography>
          <Typography sx={{ wordBreak: 'break-all' }}>
            <strong>Delivery Strategy Id:</strong> {item.deliveryStrategyId}
          </Typography>
          <Typography sx={{ wordBreak: 'break-all' }}>
            <strong>Mission / Contract:</strong> {item.missionId} / {item.contractId}
          </Typography>
        </Stack>
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
              <TableCell>Status</TableCell>
              <TableCell>Generated</TableCell>
              <TableCell />
            </TableRow>
          </TableHead>
          <TableBody>
            {versions.map((version) => (
              <TableRow key={version.id} selected={version.id === item.id}>
                <TableCell>v{version.version}</TableCell>
                <TableCell>{version.title}</TableCell>
                <TableCell>{version.score ?? '—'}</TableCell>
                <TableCell>
                  <RecommendationPersistenceStatusBadge status={version.status} />
                </TableCell>
                <TableCell>{new Date(version.generatedAt).toLocaleString()}</TableCell>
                <TableCell align="right">
                  {version.id !== item.id && (
                    <Button size="small" onClick={() => navigate(`/recommendations/${version.id}`)}>
                      View
                    </Button>
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Recommendation Payload
        </Typography>
        <Box
          component="pre"
          sx={{
            m: 0,
            p: 2,
            bgcolor: 'action.hover',
            borderRadius: 1,
            overflow: 'auto',
            maxHeight: 360,
            fontSize: 12,
          }}
        >
          {formatJson(item.recommendationPayload)}
        </Box>
      </Paper>
    </Stack>
  )
}

function formatJson(value: string): string {
  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}
