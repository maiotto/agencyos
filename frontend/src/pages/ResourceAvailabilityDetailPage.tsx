import { useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
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
  activateResourceAvailability,
  deactivateResourceAvailability,
  getResourceAvailability,
} from '../api/resourceAvailabilities'
import { getErrorMessage } from '../api/client'
import type { ResourceAvailability } from '../types/resourceAvailability'

export function ResourceAvailabilityDetailPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [item, setItem] = useState<ResourceAvailability | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    if (!id) return

    void (async () => {
      setLoading(true)
      try {
        setItem(await getResourceAvailability(id))
      } catch (err) {
        setError(getErrorMessage(err, 'Failed to load resource availability.'))
      } finally {
        setLoading(false)
      }
    })()
  }, [id])

  const toggleStatus = async () => {
    if (!item) return
    setBusy(true)
    setError(null)

    try {
      if (item.status === 'Active') {
        await deactivateResourceAvailability(item.id)
      } else {
        await activateResourceAvailability(item.id)
      }
      setItem(await getResourceAvailability(item.id))
    } catch (err) {
      setError(getErrorMessage(err))
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
    return <Alert severity="error">{error ?? 'Resource availability not found.'}</Alert>
  }

  return (
    <Stack spacing={3}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', gap: 2, flexWrap: 'wrap' }}>
        <Box>
          <Typography variant="h4">{item.name}</Typography>
          <Stack direction="row" spacing={1} alignItems="center" sx={{ mt: 1 }}>
            <Chip label={item.status} color={item.status === 'Active' ? 'success' : 'default'} size="small" />
            <Typography color="text.secondary">
              {item.effectiveFrom} → {item.effectiveTo ?? 'Open'}
            </Typography>
          </Stack>
        </Box>
        <Stack direction="row" spacing={1}>
          <Button variant="outlined" onClick={() => navigate(`/resource-availabilities/${item.id}/edit`)}>
            Edit
          </Button>
          <Button variant="contained" disabled={busy} onClick={() => void toggleStatus()}>
            {item.status === 'Active' ? 'Deactivate' : 'Activate'}
          </Button>
          <Button component={RouterLink} to="/resource-availabilities">
            Back
          </Button>
        </Stack>
      </Box>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" sx={{ mb: 2 }}>
          Associations
        </Typography>
        <Typography variant="body2">Execution Resource: {item.executionResourceId}</Typography>
        <Typography variant="body2">Working Calendar: {item.workingCalendarId}</Typography>
        <Typography variant="body2">Working Hours: {item.workingHoursId}</Typography>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" sx={{ mb: 2 }}>
          Weekly Availability
        </Typography>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Day</TableCell>
              <TableCell>Available</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {item.weeklyAvailability.map((day) => (
              <TableRow key={day.dayOfWeek}>
                <TableCell>{day.dayOfWeek}</TableCell>
                <TableCell>{day.enabled ? 'Yes' : 'No'}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" sx={{ mb: 2 }}>
          Daily Overrides
        </Typography>
        {item.dailyOverrides.length === 0 ? (
          <Typography color="text.secondary">No overrides configured.</Typography>
        ) : (
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Date</TableCell>
                <TableCell>Available</TableCell>
                <TableCell>Start</TableCell>
                <TableCell>End</TableCell>
                <TableCell>Notes</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {item.dailyOverrides.map((day) => (
                <TableRow key={day.overrideDate}>
                  <TableCell>{day.overrideDate}</TableCell>
                  <TableCell>{day.available ? 'Yes' : 'No'}</TableCell>
                  <TableCell>{day.startTime ?? '—'}</TableCell>
                  <TableCell>{day.endTime ?? '—'}</TableCell>
                  <TableCell>{day.notes ?? '—'}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </Paper>
    </Stack>
  )
}
