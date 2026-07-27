import { useEffect, useMemo, useState } from 'react'
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
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { Link as RouterLink, useSearchParams } from 'react-router-dom'
import { compareCapacityHistory } from '../api/capacityHistory'
import { getErrorMessage } from '../api/client'
import type { CapacityHistoryCompare } from '../types/capacityHistory'

function formatDelta(value: number, suffix = '') {
  const sign = value > 0 ? '+' : ''
  return `${sign}${value}${suffix}`
}

export function CapacityHistoryComparePage() {
  const [searchParams] = useSearchParams()
  const leftHistoryId = searchParams.get('leftHistoryId') ?? ''
  const rightHistoryId = searchParams.get('rightHistoryId') ?? ''
  const [comparison, setComparison] = useState<CapacityHistoryCompare | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const canCompare = useMemo(
    () => Boolean(leftHistoryId && rightHistoryId && leftHistoryId !== rightHistoryId),
    [leftHistoryId, rightHistoryId],
  )

  useEffect(() => {
    if (!canCompare) {
      setComparison(null)
      setError(
        leftHistoryId && rightHistoryId
          ? 'Left and right history ids must be different.'
          : 'Provide leftHistoryId and rightHistoryId query parameters.',
      )
      return
    }

    let cancelled = false

    const load = async () => {
      setLoading(true)
      setError(null)
      try {
        const result = await compareCapacityHistory(leftHistoryId, rightHistoryId)
        if (!cancelled) {
          setComparison(result)
        }
      } catch (err) {
        if (!cancelled) {
          setComparison(null)
          setError(getErrorMessage(err, 'Failed to compare capacity history.'))
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
  }, [canCompare, leftHistoryId, rightHistoryId])

  return (
    <Stack spacing={3}>
      <Box>
        <Button component={RouterLink} to="/capacity/history" size="small" sx={{ mb: 1 }}>
          ← Capacity History
        </Button>
        <Typography variant="h4" gutterBottom>
          Capacity History Comparison
        </Typography>
        <Typography color="text.secondary">
          Side-by-side comparison of two immutable Capacity History records.
        </Typography>
      </Box>

      {error && <Alert severity="error">{error}</Alert>}

      {loading && (
        <Box display="flex" justifyContent="center" py={6}>
          <CircularProgress />
        </Box>
      )}

      {comparison && !loading && (
        <>
          <TableContainer component={Paper}>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Metric</TableCell>
                  <TableCell align="right">Left</TableCell>
                  <TableCell align="right">Right</TableCell>
                  <TableCell align="right">Delta (Right − Left)</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                <TableRow>
                  <TableCell>Capacity hours</TableCell>
                  <TableCell align="right">{comparison.left.capacityHours}</TableCell>
                  <TableCell align="right">{comparison.right.capacityHours}</TableCell>
                  <TableCell align="right">{formatDelta(comparison.capacityHoursDelta)}</TableCell>
                </TableRow>
                <TableRow>
                  <TableCell>Available hours</TableCell>
                  <TableCell align="right">{comparison.left.availableHours}</TableCell>
                  <TableCell align="right">{comparison.right.availableHours}</TableCell>
                  <TableCell align="right">{formatDelta(comparison.availableHoursDelta)}</TableCell>
                </TableRow>
                <TableRow>
                  <TableCell>Configured hours</TableCell>
                  <TableCell align="right">{comparison.left.configuredHours}</TableCell>
                  <TableCell align="right">{comparison.right.configuredHours}</TableCell>
                  <TableCell align="right">{formatDelta(comparison.configuredHoursDelta)}</TableCell>
                </TableRow>
                <TableRow>
                  <TableCell>Utilization %</TableCell>
                  <TableCell align="right">{comparison.left.utilizationPercentage}</TableCell>
                  <TableCell align="right">{comparison.right.utilizationPercentage}</TableCell>
                  <TableCell align="right">
                    {formatDelta(comparison.utilizationPercentageDelta, '%')}
                  </TableCell>
                </TableRow>
                <TableRow>
                  <TableCell>Working days</TableCell>
                  <TableCell align="right">{comparison.left.workingDays}</TableCell>
                  <TableCell align="right">{comparison.right.workingDays}</TableCell>
                  <TableCell align="right">{formatDelta(comparison.workingDaysDelta)}</TableCell>
                </TableRow>
                <TableRow>
                  <TableCell>Holiday days</TableCell>
                  <TableCell align="right">{comparison.left.holidayDays}</TableCell>
                  <TableCell align="right">{comparison.right.holidayDays}</TableCell>
                  <TableCell align="right">{formatDelta(comparison.holidayDaysDelta)}</TableCell>
                </TableRow>
                <TableRow>
                  <TableCell>Available days</TableCell>
                  <TableCell align="right">{comparison.left.availableDays}</TableCell>
                  <TableCell align="right">{comparison.right.availableDays}</TableCell>
                  <TableCell align="right">{formatDelta(comparison.availableDaysDelta)}</TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </TableContainer>

          <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
            <Paper sx={{ p: 2, flex: 1 }}>
              <Typography variant="h6" gutterBottom>
                Left
              </Typography>
              <Typography variant="body2">Id: {comparison.left.historyId}</Typography>
              <Typography variant="body2">
                Period: {comparison.left.periodStart} → {comparison.left.periodEnd}
              </Typography>
              <Typography variant="body2">Version: {comparison.left.calculationVersion}</Typography>
              <Button
                component={RouterLink}
                to={`/capacity/history/${comparison.left.historyId}`}
                size="small"
                sx={{ mt: 1 }}
              >
                Open detail
              </Button>
            </Paper>
            <Paper sx={{ p: 2, flex: 1 }}>
              <Typography variant="h6" gutterBottom>
                Right
              </Typography>
              <Typography variant="body2">Id: {comparison.right.historyId}</Typography>
              <Typography variant="body2">
                Period: {comparison.right.periodStart} → {comparison.right.periodEnd}
              </Typography>
              <Typography variant="body2">Version: {comparison.right.calculationVersion}</Typography>
              <Button
                component={RouterLink}
                to={`/capacity/history/${comparison.right.historyId}`}
                size="small"
                sx={{ mt: 1 }}
              >
                Open detail
              </Button>
            </Paper>
          </Stack>
        </>
      )}
    </Stack>
  )
}
