import { useEffect, useState } from 'react'
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
  Typography,
} from '@mui/material'
import { Link as RouterLink, useParams } from 'react-router-dom'
import { getCapacityHistory } from '../api/capacityHistory'
import { getErrorMessage } from '../api/client'
import type { CapacityHistory } from '../types/capacityHistory'

export function CapacityHistoryDetailPage() {
  const { id } = useParams<{ id: string }>()
  const [item, setItem] = useState<CapacityHistory | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!id) {
      return
    }

    let cancelled = false

    const load = async () => {
      setLoading(true)
      setError(null)
      try {
        const history = await getCapacityHistory(id)
        if (!cancelled) {
          setItem(history)
        }
      } catch (err) {
        if (!cancelled) {
          setItem(null)
          setError(getErrorMessage(err, 'Failed to load capacity history detail.'))
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
  }, [id])

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" py={8}>
        <CircularProgress />
      </Box>
    )
  }

  if (error || !item) {
    return (
      <Stack spacing={2}>
        <Alert severity="error">{error ?? 'History record not found.'}</Alert>
        <Button component={RouterLink} to="/capacity/history" variant="outlined">
          Back to history
        </Button>
      </Stack>
    )
  }

  return (
    <Stack spacing={3}>
      <Box>
        <Button component={RouterLink} to="/capacity/history" size="small" sx={{ mb: 1 }}>
          ← Capacity History
        </Button>
        <Typography variant="h4" gutterBottom>
          History Detail
        </Typography>
        <Typography color="text.secondary" sx={{ fontFamily: 'monospace' }}>
          {item.historyId}
        </Typography>
      </Box>

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} useFlexGap flexWrap="wrap">
          <Chip label={`Version ${item.calculationVersion}`} color="primary" variant="outlined" />
          <Chip label={`${item.periodStart} → ${item.periodEnd}`} />
          <Chip label={`Working days: ${item.workingDays}`} />
          <Chip label={`Holiday days: ${item.holidayDays}`} />
          <Chip label={`Available days: ${item.availableDays}`} />
          <Chip label={`Capacity: ${item.capacityHours}h`} />
          <Chip label={`Available: ${item.availableHours}h`} />
          <Chip label={`Utilization: ${item.utilizationPercentage}%`} />
        </Stack>
      </Paper>

      <Paper sx={{ p: 2 }}>
        <Typography variant="subtitle1" gutterBottom>
          Identifiers
        </Typography>
        <Typography variant="body2">Execution Resource: {item.executionResourceId}</Typography>
        <Typography variant="body2">Company: {item.companyId}</Typography>
        <Typography variant="body2">
          Calculated: {new Date(item.calculationDate).toLocaleString()}
        </Typography>
      </Paper>

      <Box>
        <Typography variant="h6" gutterBottom>
          Daily operational capacity
        </Typography>
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Date</TableCell>
                <TableCell>Operational</TableCell>
                <TableCell>Calendar day</TableCell>
                <TableCell>Holiday</TableCell>
                <TableCell>Resource available</TableCell>
                <TableCell align="right">Planned hours</TableCell>
                <TableCell>Exclusion</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {item.operationalDays.map((day) => (
                <TableRow key={day.date}>
                  <TableCell>{day.date}</TableCell>
                  <TableCell>{day.isOperationalDay ? 'Yes' : 'No'}</TableCell>
                  <TableCell>{day.isCalendarWorkingWeekday ? 'Yes' : 'No'}</TableCell>
                  <TableCell>{day.isHoliday ? 'Yes' : 'No'}</TableCell>
                  <TableCell>{day.isResourceAvailable ? 'Yes' : 'No'}</TableCell>
                  <TableCell align="right">{day.plannedCapacityHours}</TableCell>
                  <TableCell>{day.exclusionReason ?? '—'}</TableCell>
                </TableRow>
              ))}
              {item.operationalDays.length === 0 && (
                <TableRow>
                  <TableCell colSpan={7}>
                    <Typography color="text.secondary" py={2}>
                      No daily snapshot was preserved for this record.
                    </Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Box>
    </Stack>
  )
}
