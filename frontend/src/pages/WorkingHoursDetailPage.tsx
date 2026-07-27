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
  activateWorkingHours,
  deactivateWorkingHours,
  deleteWorkingHours,
  getWorkingHours,
} from '../api/workingHours'
import { getErrorMessage } from '../api/client'
import type { WorkingHours } from '../types/workingHours'
import { toTimeInput } from '../types/workingHours'

export function WorkingHoursDetailPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [item, setItem] = useState<WorkingHours | null>(null)
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
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
        const data = await getWorkingHours(id)
        if (!cancelled) {
          setItem(data)
        }
      } catch (err) {
        if (!cancelled) {
          setError(getErrorMessage(err, 'Failed to load working hours.'))
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

  const runAction = async (action: () => Promise<void>) => {
    if (!id) {
      return
    }

    setBusy(true)
    setError(null)

    try {
      await action()
      setItem(await getWorkingHours(id))
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
        <CircularProgress />
      </Box>
    )
  }

  if (!item) {
    return (
      <Stack spacing={2}>
        {error ? <Alert severity="error">{error}</Alert> : <Alert severity="warning">Not found.</Alert>}
        <Button component={RouterLink} to="/working-hours">
          Back
        </Button>
      </Stack>
    )
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
          <Typography variant="h4" gutterBottom>
            {item.name}
          </Typography>
          <Stack direction="row" spacing={1}>
            <Chip
              size="small"
              label={item.status}
              color={item.status === 'Active' ? 'success' : 'default'}
            />
            <Chip
              size="small"
              variant="outlined"
              label={`${item.effectiveFrom} → ${item.effectiveTo ?? 'Open'}`}
            />
          </Stack>
        </Box>
        <Button component={RouterLink} to="/working-hours">
          Back
        </Button>
      </Stack>

      {error ? <Alert severity="error">{error}</Alert> : null}

      <Paper sx={{ p: 3, overflowX: 'auto' }}>
        <Typography variant="h6" gutterBottom>
          Weekday schedule
        </Typography>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Day</TableCell>
              <TableCell>Enabled</TableCell>
              <TableCell>Start</TableCell>
              <TableCell>End</TableCell>
              <TableCell>Break</TableCell>
              <TableCell>Net hours</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {item.days.map((day) => (
              <TableRow key={day.dayOfWeek}>
                <TableCell>{day.dayOfWeek}</TableCell>
                <TableCell>{day.enabled ? 'Yes' : 'No'}</TableCell>
                <TableCell>{toTimeInput(day.startTime) || '—'}</TableCell>
                <TableCell>{toTimeInput(day.endTime) || '—'}</TableCell>
                <TableCell>
                  {day.breakStart && day.breakEnd
                    ? `${toTimeInput(day.breakStart)} – ${toTimeInput(day.breakEnd)}`
                    : '—'}
                </TableCell>
                <TableCell>{day.netHours ?? '—'}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Paper>

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5}>
        <Button variant="outlined" onClick={() => navigate(`/working-hours/${item.id}/edit`)}>
          Edit
        </Button>
        {item.status === 'Inactive' ? (
          <Button
            variant="contained"
            disabled={busy}
            onClick={() => void runAction(() => activateWorkingHours(item.id))}
          >
            Activate
          </Button>
        ) : (
          <Button
            variant="contained"
            disabled={busy}
            onClick={() => void runAction(() => deactivateWorkingHours(item.id))}
          >
            Deactivate
          </Button>
        )}
        <Button
          color="error"
          variant="outlined"
          disabled={busy || item.status === 'Active'}
          onClick={() =>
            void runAction(async () => {
              await deleteWorkingHours(item.id)
              navigate('/working-hours')
            })
          }
        >
          Delete
        </Button>
      </Stack>
    </Stack>
  )
}
