import { useEffect, useMemo, useState } from 'react'
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  MenuItem,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material'
import { Link as RouterLink, useSearchParams } from 'react-router-dom'
import {
  compareRecommendationVersions,
  compareRecommendations,
} from '../api/recommendationComparison'
import { getErrorMessage } from '../api/client'
import type {
  RecommendationComparison,
  RecommendationComparisonFieldDiff,
} from '../types/recommendationComparison'

function formatDelta(value: number | null | undefined, suffix = '') {
  if (value == null) return '—'
  const sign = value > 0 ? '+' : ''
  return `${sign}${value}${suffix}`
}

function exportComparison(comparison: RecommendationComparison) {
  const blob = new Blob([JSON.stringify(comparison, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  const leftLabel = `${comparison.left.recommendationNumber}-v${comparison.left.recommendationVersion}`
  const rightLabel = `${comparison.right.recommendationNumber}-v${comparison.right.recommendationVersion}`
  anchor.href = url
  anchor.download = `recommendation-comparison-${leftLabel}-vs-${rightLabel}.json`
  anchor.click()
  URL.revokeObjectURL(url)
}

function DiffTable({
  title,
  fields,
  showUnchanged,
}: {
  title: string
  fields: RecommendationComparisonFieldDiff[]
  showUnchanged: boolean
}) {
  const rows = showUnchanged ? fields : fields.filter((field) => field.changed)
  if (rows.length === 0) {
    return null
  }

  return (
    <Paper sx={{ p: 2 }}>
      <Typography variant="h6" gutterBottom>
        {title}
      </Typography>
      <TableContainer>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Path</TableCell>
              <TableCell>Left</TableCell>
              <TableCell>Right</TableCell>
              <TableCell>Status</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {rows.map((field) => (
              <TableRow
                key={`${field.section}:${field.path}`}
                sx={
                  field.changed
                    ? { backgroundColor: 'rgba(211, 47, 47, 0.06)' }
                    : undefined
                }
              >
                <TableCell>
                  <Typography variant="body2" fontFamily="monospace">
                    {field.path}
                  </Typography>
                </TableCell>
                <TableCell sx={{ maxWidth: 280, wordBreak: 'break-word' }}>
                  {field.leftValue ?? '—'}
                </TableCell>
                <TableCell sx={{ maxWidth: 280, wordBreak: 'break-word' }}>
                  {field.rightValue ?? '—'}
                </TableCell>
                <TableCell>
                  <Chip
                    size="small"
                    label={field.changed ? 'Changed' : 'Same'}
                    color={field.changed ? 'warning' : 'default'}
                    variant="outlined"
                  />
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Paper>
  )
}

export function RecommendationComparisonPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const [leftId, setLeftId] = useState(searchParams.get('leftId') ?? '')
  const [rightId, setRightId] = useState(searchParams.get('rightId') ?? '')
  const [recommendationNumber, setRecommendationNumber] = useState(
    searchParams.get('recommendationNumber') ?? '',
  )
  const [leftVersion, setLeftVersion] = useState(searchParams.get('leftVersion') ?? '')
  const [rightVersion, setRightVersion] = useState(searchParams.get('rightVersion') ?? '')
  const [mode, setMode] = useState<'ids' | 'versions'>(
    searchParams.get('mode') === 'versions' ? 'versions' : 'ids',
  )
  const [showUnchanged, setShowUnchanged] = useState(false)
  const [comparison, setComparison] = useState<RecommendationComparison | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const canCompareIds = useMemo(
    () => Boolean(leftId && rightId && leftId !== rightId),
    [leftId, rightId],
  )

  const canCompareVersions = useMemo(
    () => Boolean(recommendationNumber.trim()),
    [recommendationNumber],
  )

  useEffect(() => {
    const nextLeft = searchParams.get('leftId') ?? ''
    const nextRight = searchParams.get('rightId') ?? ''
    const nextNumber = searchParams.get('recommendationNumber') ?? ''
    const nextLeftVersion = searchParams.get('leftVersion') ?? ''
    const nextRightVersion = searchParams.get('rightVersion') ?? ''
    const nextMode = searchParams.get('mode') === 'versions' ? 'versions' : 'ids'
    setLeftId(nextLeft)
    setRightId(nextRight)
    setRecommendationNumber(nextNumber)
    setLeftVersion(nextLeftVersion)
    setRightVersion(nextRightVersion)
    setMode(nextMode)
  }, [searchParams])

  useEffect(() => {
    let cancelled = false

    const load = async () => {
      if (mode === 'ids') {
        if (!canCompareIds) {
          setComparison(null)
          setError(
            leftId && rightId
              ? 'Left and right ids must be different.'
              : 'Provide leftId and rightId (history or recommendation ids).',
          )
          return
        }
      } else if (!canCompareVersions) {
        setComparison(null)
        setError('Provide a recommendation number to compare versions.')
        return
      }

      setLoading(true)
      setError(null)
      try {
        const result =
          mode === 'ids'
            ? await compareRecommendations(leftId, rightId)
            : await compareRecommendationVersions(
                recommendationNumber.trim(),
                leftVersion ? Number(leftVersion) : undefined,
                rightVersion ? Number(rightVersion) : undefined,
              )
        if (!cancelled) {
          setComparison(result)
        }
      } catch (err) {
        if (!cancelled) {
          setComparison(null)
          setError(getErrorMessage(err, 'Failed to compare recommendations.'))
        }
      } finally {
        if (!cancelled) {
          setLoading(false)
        }
      }
    }

    void load()
    return () => {
      cancelled = true
    }
  }, [
    mode,
    canCompareIds,
    canCompareVersions,
    leftId,
    rightId,
    recommendationNumber,
    leftVersion,
    rightVersion,
  ])

  const applyFilters = () => {
    const params = new URLSearchParams()
    params.set('mode', mode)
    if (mode === 'ids') {
      if (leftId) params.set('leftId', leftId)
      if (rightId) params.set('rightId', rightId)
    } else {
      if (recommendationNumber.trim()) {
        params.set('recommendationNumber', recommendationNumber.trim())
      }
      if (leftVersion) params.set('leftVersion', leftVersion)
      if (rightVersion) params.set('rightVersion', rightVersion)
    }
    setSearchParams(params)
  }

  return (
    <Stack spacing={3}>
      <Stack
        direction={{ xs: 'column', sm: 'row' }}
        justifyContent="space-between"
        spacing={2}
        alignItems={{ xs: 'stretch', sm: 'flex-start' }}
      >
        <Box>
          <Button component={RouterLink} to="/recommendations/history" size="small" sx={{ mb: 1 }}>
            ← Recommendation History
          </Button>
          <Typography variant="h4" gutterBottom>
            Recommendation Comparison
          </Typography>
          <Typography color="text.secondary">
            Side-by-side diff of immutable history snapshots (capacity, workload, payload, strategy,
            workflow).
          </Typography>
        </Box>
        {comparison && (
          <Button variant="outlined" onClick={() => exportComparison(comparison)}>
            Export JSON
          </Button>
        )}
      </Stack>

      <Paper sx={{ p: 2 }}>
        <Stack spacing={2}>
          <TextField
            select
            label="Mode"
            size="small"
            value={mode}
            onChange={(event) => setMode(event.target.value as 'ids' | 'versions')}
            sx={{ maxWidth: 260 }}
          >
            <MenuItem value="ids">Compare by Ids</MenuItem>
            <MenuItem value="versions">Compare Versions</MenuItem>
          </TextField>

          {mode === 'ids' ? (
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} useFlexGap flexWrap="wrap">
              <TextField
                label="Left Id"
                size="small"
                value={leftId}
                onChange={(event) => setLeftId(event.target.value)}
                sx={{ minWidth: 280 }}
                helperText="History id or recommendation id"
              />
              <TextField
                label="Right Id"
                size="small"
                value={rightId}
                onChange={(event) => setRightId(event.target.value)}
                sx={{ minWidth: 280 }}
                helperText="History id or recommendation id"
              />
            </Stack>
          ) : (
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} useFlexGap flexWrap="wrap">
              <TextField
                label="Recommendation Number"
                size="small"
                value={recommendationNumber}
                onChange={(event) => setRecommendationNumber(event.target.value)}
                sx={{ minWidth: 280 }}
              />
              <TextField
                label="Left Version"
                size="small"
                value={leftVersion}
                onChange={(event) => setLeftVersion(event.target.value)}
                sx={{ width: 140 }}
                helperText="Optional"
              />
              <TextField
                label="Right Version"
                size="small"
                value={rightVersion}
                onChange={(event) => setRightVersion(event.target.value)}
                sx={{ width: 140 }}
                helperText="Optional"
              />
            </Stack>
          )}

          <Stack direction="row" spacing={1}>
            <Button variant="contained" onClick={applyFilters}>
              Compare
            </Button>
            <Button
              variant="text"
              onClick={() => setShowUnchanged((current) => !current)}
            >
              {showUnchanged ? 'Hide unchanged' : 'Show unchanged'}
            </Button>
          </Stack>
        </Stack>
      </Paper>

      {error && <Alert severity="error">{error}</Alert>}

      {loading && (
        <Box display="flex" justifyContent="center" py={6}>
          <CircularProgress />
        </Box>
      )}

      {comparison && !loading && (
        <>
          <Stack direction={{ xs: 'column', md: 'row' }} spacing={1} useFlexGap flexWrap="wrap">
            <Chip
              label={comparison.hasDifferences ? 'Differences found' : 'Identical snapshots'}
              color={comparison.hasDifferences ? 'warning' : 'success'}
            />
            <Chip label={`Score Δ ${formatDelta(comparison.scoreDelta)}`} variant="outlined" />
            <Chip label={`Rank Δ ${formatDelta(comparison.rankDelta)}`} variant="outlined" />
            <Chip label={`Version Δ ${formatDelta(comparison.versionDelta)}`} variant="outlined" />
          </Stack>

          <TableContainer component={Paper}>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Metric</TableCell>
                  <TableCell align="right">Left</TableCell>
                  <TableCell align="right">Right</TableCell>
                  <TableCell align="right">Delta</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                <TableRow>
                  <TableCell>Number</TableCell>
                  <TableCell align="right">{comparison.left.recommendationNumber}</TableCell>
                  <TableCell align="right">{comparison.right.recommendationNumber}</TableCell>
                  <TableCell align="right">—</TableCell>
                </TableRow>
                <TableRow
                  sx={
                    comparison.versionDelta !== 0
                      ? { backgroundColor: 'rgba(211, 47, 47, 0.06)' }
                      : undefined
                  }
                >
                  <TableCell>Version</TableCell>
                  <TableCell align="right">v{comparison.left.recommendationVersion}</TableCell>
                  <TableCell align="right">v{comparison.right.recommendationVersion}</TableCell>
                  <TableCell align="right">{formatDelta(comparison.versionDelta)}</TableCell>
                </TableRow>
                <TableRow
                  sx={
                    comparison.scoreDelta
                      ? { backgroundColor: 'rgba(211, 47, 47, 0.06)' }
                      : undefined
                  }
                >
                  <TableCell>Score</TableCell>
                  <TableCell align="right">{comparison.left.score ?? '—'}</TableCell>
                  <TableCell align="right">{comparison.right.score ?? '—'}</TableCell>
                  <TableCell align="right">{formatDelta(comparison.scoreDelta)}</TableCell>
                </TableRow>
                <TableRow
                  sx={
                    comparison.rankDelta
                      ? { backgroundColor: 'rgba(211, 47, 47, 0.06)' }
                      : undefined
                  }
                >
                  <TableCell>Rank</TableCell>
                  <TableCell align="right">{comparison.left.rank ?? '—'}</TableCell>
                  <TableCell align="right">{comparison.right.rank ?? '—'}</TableCell>
                  <TableCell align="right">{formatDelta(comparison.rankDelta)}</TableCell>
                </TableRow>
                <TableRow>
                  <TableCell>Workflow</TableCell>
                  <TableCell align="right">{comparison.left.workflowStatus ?? '—'}</TableCell>
                  <TableCell align="right">{comparison.right.workflowStatus ?? '—'}</TableCell>
                  <TableCell align="right">
                    {comparison.left.workflowStatus === comparison.right.workflowStatus
                      ? 'Same'
                      : 'Changed'}
                  </TableCell>
                </TableRow>
                <TableRow>
                  <TableCell>Delivery Strategy</TableCell>
                  <TableCell align="right" sx={{ fontFamily: 'monospace', fontSize: 12 }}>
                    {comparison.left.deliveryStrategyId}
                  </TableCell>
                  <TableCell align="right" sx={{ fontFamily: 'monospace', fontSize: 12 }}>
                    {comparison.right.deliveryStrategyId}
                  </TableCell>
                  <TableCell align="right">
                    {comparison.left.deliveryStrategyId === comparison.right.deliveryStrategyId
                      ? 'Same'
                      : 'Changed'}
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </TableContainer>

          <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
            <Paper sx={{ p: 2, flex: 1 }}>
              <Typography variant="h6" gutterBottom>
                Left Snapshot
              </Typography>
              <Typography variant="body2">History: {comparison.left.id}</Typography>
              <Typography variant="body2">
                Recommendation: {comparison.left.recommendationId}
              </Typography>
              <Typography variant="body2">{comparison.left.title}</Typography>
              <Button
                component={RouterLink}
                to={`/recommendations/history/${comparison.left.id}`}
                size="small"
                sx={{ mt: 1 }}
              >
                Open snapshot
              </Button>
            </Paper>
            <Paper sx={{ p: 2, flex: 1 }}>
              <Typography variant="h6" gutterBottom>
                Right Snapshot
              </Typography>
              <Typography variant="body2">History: {comparison.right.id}</Typography>
              <Typography variant="body2">
                Recommendation: {comparison.right.recommendationId}
              </Typography>
              <Typography variant="body2">{comparison.right.title}</Typography>
              <Button
                component={RouterLink}
                to={`/recommendations/history/${comparison.right.id}`}
                size="small"
                sx={{ mt: 1 }}
              >
                Open snapshot
              </Button>
            </Paper>
          </Stack>

          {comparison.sections.map((section) => (
            <DiffTable
              key={section.section}
              title={`${section.section} comparison`}
              fields={section.fields}
              showUnchanged={showUnchanged}
            />
          ))}
        </>
      )}
    </Stack>
  )
}
