import { useCallback, useEffect, useState } from 'react'
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
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material'
import { Link as RouterLink } from 'react-router-dom'
import {
  aggregateCapacityHistory,
  listCapacityHistory,
} from '../api/capacityHistory'
import { getErrorMessage } from '../api/client'
import type { CapacityHistory, CapacityHistoryAggregate } from '../types/capacityHistory'

const DEFAULT_COMPANY_ID = 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa'

export function CapacityHistoryPage() {
  const [companyId, setCompanyId] = useState(DEFAULT_COMPANY_ID)
  const [executionResourceId, setExecutionResourceId] = useState('')
  const [periodStart, setPeriodStart] = useState('')
  const [periodEnd, setPeriodEnd] = useState('')
  const [calculationVersion, setCalculationVersion] = useState('')
  const [items, setItems] = useState<CapacityHistory[]>([])
  const [aggregate, setAggregate] = useState<CapacityHistoryAggregate | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [leftId, setLeftId] = useState('')
  const [rightId, setRightId] = useState('')

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
      const [history, historyAggregate] = await Promise.all([
        listCapacityHistory(query),
        aggregateCapacityHistory(query),
      ])
      setItems(history)
      setAggregate(historyAggregate)
    } catch (err) {
      setItems([])
      setAggregate(null)
      setError(getErrorMessage(err, 'Failed to load capacity history.'))
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
        <Typography variant="h4" gutterBottom>
          Capacity History
        </Typography>
        <Typography color="text.secondary">
          Immutable snapshots of completed Capacity Engine calculations (US-106).
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
            placeholder="1.1.0"
          />
          <Button variant="contained" onClick={() => void load()} disabled={loading}>
            Apply Filters
          </Button>
        </Stack>
      </Paper>

      {error && <Alert severity="error">{error}</Alert>}

      {aggregate && (
        <Paper sx={{ p: 2 }}>
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} useFlexGap flexWrap="wrap">
            <Chip label={`Records: ${aggregate.recordCount}`} />
            <Chip label={`Capacity: ${aggregate.totalCapacityHours}h`} />
            <Chip label={`Available: ${aggregate.totalAvailableHours}h`} />
            <Chip label={`Avg utilization: ${aggregate.averageUtilizationPercentage}%`} />
          </Stack>
        </Paper>
      )}

      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>
          Compare periods
        </Typography>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ sm: 'center' }}>
          <TextField
            label="Left History Id"
            value={leftId}
            onChange={(event) => setLeftId(event.target.value)}
            size="small"
            sx={{ minWidth: 280 }}
          />
          <TextField
            label="Right History Id"
            value={rightId}
            onChange={(event) => setRightId(event.target.value)}
            size="small"
            sx={{ minWidth: 280 }}
          />
          <Button
            component={RouterLink}
            to={`/capacity/history/compare?leftHistoryId=${encodeURIComponent(leftId)}&rightHistoryId=${encodeURIComponent(rightId)}`}
            variant="outlined"
            disabled={!leftId || !rightId}
          >
            Open Comparison
          </Button>
        </Stack>
      </Paper>

      {loading ? (
        <Box display="flex" justifyContent="center" py={6}>
          <CircularProgress />
        </Box>
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Calculated</TableCell>
                <TableCell>Resource</TableCell>
                <TableCell>Period</TableCell>
                <TableCell align="right">Capacity</TableCell>
                <TableCell align="right">Available</TableCell>
                <TableCell align="right">Utilization</TableCell>
                <TableCell>Version</TableCell>
                <TableCell />
              </TableRow>
            </TableHead>
            <TableBody>
              {items.map((item) => (
                <TableRow key={item.historyId} hover>
                  <TableCell>{new Date(item.calculationDate).toLocaleString()}</TableCell>
                  <TableCell sx={{ fontFamily: 'monospace', fontSize: 12 }}>
                    {item.executionResourceId}
                  </TableCell>
                  <TableCell>
                    {item.periodStart} → {item.periodEnd}
                  </TableCell>
                  <TableCell align="right">{item.capacityHours}</TableCell>
                  <TableCell align="right">{item.availableHours}</TableCell>
                  <TableCell align="right">{item.utilizationPercentage}%</TableCell>
                  <TableCell>{item.calculationVersion}</TableCell>
                  <TableCell align="right">
                    <Button
                      component={RouterLink}
                      to={`/capacity/history/${item.historyId}`}
                      size="small"
                    >
                      Detail
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
              {items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={8}>
                    <Typography color="text.secondary" py={2}>
                      No historical capacity records match the current filters.
                    </Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Stack>
  )
}
