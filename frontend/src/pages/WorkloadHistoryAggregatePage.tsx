import { useCallback, useEffect, useState } from 'react'
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { Link as RouterLink } from 'react-router-dom'
import {
  aggregateWorkloadHistory,
  getWorkloadHistoryTrends,
} from '../api/workloadHistory'
import { getErrorMessage } from '../api/client'
import type { WorkloadHistoryAggregate, WorkloadHistoryTrend } from '../types/workloadHistory'

const DEFAULT_COMPANY_ID = 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa'

export function WorkloadHistoryAggregatePage() {
  const [companyId, setCompanyId] = useState(DEFAULT_COMPANY_ID)
  const [executionResourceId, setExecutionResourceId] = useState('')
  const [periodStart, setPeriodStart] = useState('')
  const [periodEnd, setPeriodEnd] = useState('')
  const [calculationVersion, setCalculationVersion] = useState('')
  const [aggregate, setAggregate] = useState<WorkloadHistoryAggregate | null>(null)
  const [trends, setTrends] = useState<WorkloadHistoryTrend | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)

    try {
      const query = {
        companyId: companyId || undefined,
        executionResourceId: executionResourceId || undefined,
        periodStart: periodStart || undefined,
        periodEnd: periodEnd || undefined,
        calculationVersion: calculationVersion || undefined,
      }
      const [historyAggregate, historyTrends] = await Promise.all([
        aggregateWorkloadHistory(query),
        getWorkloadHistoryTrends(query),
      ])
      setAggregate(historyAggregate)
      setTrends(historyTrends)
    } catch (err) {
      setAggregate(null)
      setTrends(null)
      setError(getErrorMessage(err, 'Failed to load workload aggregation.'))
    } finally {
      setLoading(false)
    }
  }, [companyId, executionResourceId, periodStart, periodEnd, calculationVersion])

  useEffect(() => {
    void load()
  }, [load])

  return (
    <Stack spacing={3}>
      <Box>
        <Button component={RouterLink} to="/workload/history" size="small" sx={{ mb: 1 }}>
          ← Workload History
        </Button>
        <Typography variant="h4" gutterBottom>
          Workload Aggregation
        </Typography>
        <Typography color="text.secondary">
          Aggregated historical workload totals and chronological trend points.
        </Typography>
      </Box>

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} useFlexGap flexWrap="wrap">
          <TextField
            label="Company Id"
            value={companyId}
            onChange={(event) => setCompanyId(event.target.value)}
            size="small"
            sx={{ minWidth: 280 }}
          />
          <TextField
            label="Execution Resource Id"
            value={executionResourceId}
            onChange={(event) => setExecutionResourceId(event.target.value)}
            size="small"
            sx={{ minWidth: 280 }}
          />
          <TextField
            label="Period Start"
            type="date"
            value={periodStart}
            onChange={(event) => setPeriodStart(event.target.value)}
            size="small"
            InputLabelProps={{ shrink: true }}
          />
          <TextField
            label="Period End"
            type="date"
            value={periodEnd}
            onChange={(event) => setPeriodEnd(event.target.value)}
            size="small"
            InputLabelProps={{ shrink: true }}
          />
          <TextField
            label="Calculation Version"
            value={calculationVersion}
            onChange={(event) => setCalculationVersion(event.target.value)}
            size="small"
          />
          <Button variant="contained" onClick={() => void load()} disabled={loading}>
            Refresh
          </Button>
        </Stack>
      </Paper>

      {error && <Alert severity="error">{error}</Alert>}

      {loading ? (
        <Box display="flex" justifyContent="center" py={6}>
          <CircularProgress />
        </Box>
      ) : (
        <>
          {aggregate && (
            <Paper sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom>
                Aggregate totals
              </Typography>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} useFlexGap flexWrap="wrap">
                <Chip label={`Records: ${aggregate.recordCount}`} color="primary" />
                <Chip label={`Allocated: ${aggregate.totalAllocatedHours}h`} />
                <Chip label={`Capacity: ${aggregate.totalCapacityHours}h`} />
                <Chip label={`Avg workload: ${aggregate.averageWorkloadPercentage}%`} />
              </Stack>
            </Paper>
          )}

          {trends && (
            <Paper sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom>
                Trend points ({trends.pointCount})
              </Typography>
              {trends.points.length === 0 ? (
                <Typography color="text.secondary">No trend points for this filter set.</Typography>
              ) : (
                <Stack spacing={1}>
                  {trends.points.map((point, index) => (
                    <Typography key={`${point.calculationDate}-${index}`} variant="body2">
                      {new Date(point.calculationDate).toLocaleString()} — {point.periodStart} →{' '}
                      {point.periodEnd}: {point.allocatedHours}h / {point.capacityHours}h (
                      {point.workloadPercentage}%)
                    </Typography>
                  ))}
                </Stack>
              )}
            </Paper>
          )}
        </>
      )}
    </Stack>
  )
}
