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
  Typography,
} from '@mui/material'
import {
  activateHoliday,
  deactivateHoliday,
  deleteHoliday,
  getHoliday,
} from '../api/holidays'
import { getErrorMessage } from '../api/client'
import type { Holiday } from '../types/holiday'

export function HolidayDetailPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [holiday, setHoliday] = useState<Holiday | null>(null)
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
        const data = await getHoliday(id)
        if (!cancelled) {
          setHoliday(data)
        }
      } catch (err) {
        if (!cancelled) {
          setError(getErrorMessage(err, 'Failed to load holiday.'))
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
      const refreshed = await getHoliday(id)
      setHoliday(refreshed)
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

  if (!holiday) {
    return (
      <Stack spacing={2}>
        {error ? <Alert severity="error">{error}</Alert> : <Alert severity="warning">Holiday not found.</Alert>}
        <Button component={RouterLink} to="/holidays">
          Back to holidays
        </Button>
      </Stack>
    )
  }

  return (
    <Stack spacing={3} maxWidth={720}>
      <Stack
        direction={{ xs: 'column', sm: 'row' }}
        justifyContent="space-between"
        alignItems={{ xs: 'stretch', sm: 'flex-start' }}
        spacing={2}
      >
        <Box>
          <Typography variant="h4" gutterBottom>
            {holiday.name}
          </Typography>
          <Stack direction="row" spacing={1} alignItems="center">
            <Chip
              size="small"
              label={holiday.status}
              color={holiday.status === 'Active' ? 'success' : 'default'}
            />
            <Chip size="small" label={holiday.holidayType} variant="outlined" />
            {holiday.recurring ? <Chip size="small" label="Recurring" variant="outlined" /> : null}
          </Stack>
        </Box>
        <Button component={RouterLink} to="/holidays">
          Back
        </Button>
      </Stack>

      {error ? <Alert severity="error">{error}</Alert> : null}

      <Paper sx={{ p: 3 }}>
        <Stack spacing={1.5}>
          <DetailRow label="Date" value={holiday.holidayDate} />
          <DetailRow label="Description" value={holiday.description || '—'} />
          <DetailRow label="Company ID" value={holiday.companyId || '—'} />
          <DetailRow label="State" value={holiday.stateCode || '—'} />
          <DetailRow label="City" value={holiday.city || '—'} />
          <DetailRow label="Created" value={new Date(holiday.createdAt).toLocaleString()} />
          <DetailRow label="Updated" value={new Date(holiday.updatedAt).toLocaleString()} />
        </Stack>
      </Paper>

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} flexWrap="wrap" useFlexGap>
        <Button
          variant="outlined"
          onClick={() => navigate(`/holidays/${holiday.id}/edit`)}
          disabled={busy}
        >
          Edit
        </Button>
        {holiday.status === 'Inactive' ? (
          <Button
            variant="contained"
            onClick={() => void runAction(() => activateHoliday(holiday.id))}
            disabled={busy}
          >
            Activate
          </Button>
        ) : (
          <Button
            variant="contained"
            onClick={() => void runAction(() => deactivateHoliday(holiday.id))}
            disabled={busy}
          >
            Deactivate
          </Button>
        )}
        <Button
          color="error"
          variant="outlined"
          disabled={busy || holiday.status === 'Active'}
          onClick={() =>
            void runAction(async () => {
              await deleteHoliday(holiday.id)
              navigate('/holidays')
            })
          }
        >
          Delete
        </Button>
      </Stack>
    </Stack>
  )
}

function DetailRow({ label, value }: { label: string; value: string }) {
  return (
    <Box>
      <Typography variant="caption" color="text.secondary">
        {label}
      </Typography>
      <Typography>{value}</Typography>
    </Box>
  )
}
