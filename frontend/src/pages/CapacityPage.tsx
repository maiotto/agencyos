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
import { getCapacityForResource, getCapacitySummary, listCapacity } from '../api/capacity'
import { getErrorMessage } from '../api/client'
import type { Capacity, CapacitySummary } from '../types/capacity'

function defaultPeriod() {
  const start = new Date()
  const end = new Date()
  end.setDate(start.getDate() + 6)
  return {
    periodStartDate: start.toISOString().slice(0, 10),
    periodEndDate: end.toISOString().slice(0, 10),
  }
}

export function CapacityPage() {
  const initial = defaultPeriod()
  const [periodStartDate, setPeriodStartDate] = useState(initial.periodStartDate)
  const [periodEndDate, setPeriodEndDate] = useState(initial.periodEndDate)
  const [items, setItems] = useState<Capacity[]>([])
  const [summary, setSummary] = useState<CapacitySummary | null>(null)
  const [selected, setSelected] = useState<Capacity | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    setSelected(null)

    try {
      const query = { periodStartDate, periodEndDate }
      const [capacityList, capacitySummary] = await Promise.all([
        listCapacity(query),
        getCapacitySummary(query),
      ])
      setItems(capacityList)
      setSummary(capacitySummary)
    } catch (err) {
      setItems([])
      setSummary(null)
      setError(getErrorMessage(err, 'Failed to calculate capacity.'))
    } finally {
      setLoading(false)
    }
  }, [periodStartDate, periodEndDate])

  useEffect(() => {
    void load()
  }, [load])

  const openDetail = async (resourceId: string) => {
    setError(null)
    try {
      const detail = await getCapacityForResource(resourceId, { periodStartDate, periodEndDate })
      setSelected(detail)
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to load capacity detail.'))
    }
  }

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h4">Capacity Planning</Typography>
        <Typography color="text.secondary">
          Planned capacity from Working Calendar, Holidays, Working Hours, and Resource Availability
          (US-105). Successful calculations persist immutable history (US-106). No Monday–Friday
          fallback.
        </Typography>
        <Button component={RouterLink} to="/capacity/history" size="small" sx={{ mt: 1 }}>
          View Capacity History
        </Button>
      </Box>

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} alignItems={{ md: 'center' }}>
          <TextField
            label="Period Start"
            type="date"
            value={periodStartDate}
            onChange={(event) => setPeriodStartDate(event.target.value)}
            InputLabelProps={{ shrink: true }}
            size="small"
          />
          <TextField
            label="Period End"
            type="date"
            value={periodEndDate}
            onChange={(event) => setPeriodEndDate(event.target.value)}
            InputLabelProps={{ shrink: true }}
            size="small"
          />
          <Button variant="contained" onClick={() => void load()} disabled={loading}>
            Calculate
          </Button>
        </Stack>
      </Paper>

      {error && <Alert severity="error">{error}</Alert>}

      {summary && (
        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" sx={{ mb: 2 }}>
            Planned Capacity Summary
          </Typography>
          <Stack direction={{ xs: 'column', md: 'row' }} spacing={3}>
            <Typography variant="body2">Resources: {summary.activeResourceCount}</Typography>
            <Typography variant="body2">Planned Hours: {summary.totalCapacityHours}</Typography>
            <Typography variant="body2">Allocated: {summary.totalAllocatedHours}</Typography>
            <Typography variant="body2">Available: {summary.totalAvailableHours}</Typography>
            <Typography variant="body2">Utilization: {summary.overallUtilizationPercentage}%</Typography>
          </Stack>
        </Paper>
      )}

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
          <CircularProgress />
        </Box>
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Resource</TableCell>
                <TableCell>Operational Days</TableCell>
                <TableCell>Working Hours</TableCell>
                <TableCell>Holiday Impact</TableCell>
                <TableCell>RA Excluded</TableCell>
                <TableCell>Planned Capacity</TableCell>
                <TableCell>Allocated</TableCell>
                <TableCell>Available</TableCell>
                <TableCell align="right">Detail</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {items.map((item) => (
                <TableRow key={item.executionResourceId} hover>
                  <TableCell>
                    {item.executionResourceCode} — {item.executionResourceName}
                  </TableCell>
                  <TableCell>{item.operationalDayCount}</TableCell>
                  <TableCell>{item.configuredWorkingHoursTotal}</TableCell>
                  <TableCell>{item.holidayImpactDayCount}</TableCell>
                  <TableCell>{item.resourceAvailabilityExcludedDayCount}</TableCell>
                  <TableCell>{item.totalCapacityHours}</TableCell>
                  <TableCell>{item.allocatedHours}</TableCell>
                  <TableCell>{item.availableHours}</TableCell>
                  <TableCell align="right">
                    <Button size="small" onClick={() => void openDetail(item.executionResourceId)}>
                      Days
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
              {items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={9}>
                    <Typography color="text.secondary">
                      No capacity results. Ensure Active Resource Availability covers the period.
                    </Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {selected && (
        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" sx={{ mb: 1 }}>
            Operational Day Breakdown — {selected.executionResourceCode}
          </Typography>
          <Typography color="text.secondary" sx={{ mb: 2 }}>
            Shows calendar weekdays, holiday impact, resource availability impact, and planned capacity
            hours.
          </Typography>
          <TableContainer>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Date</TableCell>
                  <TableCell>Operational</TableCell>
                  <TableCell>Calendar Day</TableCell>
                  <TableCell>Holiday</TableCell>
                  <TableCell>RA Available</TableCell>
                  <TableCell>Planned Hours</TableCell>
                  <TableCell>Reason</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {selected.operationalDays.map((day) => (
                  <TableRow key={day.date}>
                    <TableCell>{day.date}</TableCell>
                    <TableCell>
                      <Chip
                        size="small"
                        label={day.isOperationalDay ? 'Yes' : 'No'}
                        color={day.isOperationalDay ? 'success' : 'default'}
                      />
                    </TableCell>
                    <TableCell>{day.isCalendarWorkingWeekday ? 'Yes' : 'No'}</TableCell>
                    <TableCell>{day.isHoliday ? 'Yes' : 'No'}</TableCell>
                    <TableCell>{day.isResourceAvailable ? 'Yes' : 'No'}</TableCell>
                    <TableCell>{day.plannedCapacityHours}</TableCell>
                    <TableCell>{day.exclusionReason ?? '—'}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </Paper>
      )}
    </Stack>
  )
}
