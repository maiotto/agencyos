import { useCallback, useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
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
import AddIcon from '@mui/icons-material/Add'
import {
  activateWorkingCalendar,
  deactivateWorkingCalendar,
  deleteWorkingCalendar,
  listWorkingCalendars,
} from '../api/workingCalendars'
import { getErrorMessage } from '../api/client'
import { DEFAULT_COMPANY_ID } from '../theme'
import type { WorkingCalendar } from '../types/workingCalendar'

export function WorkingCalendarListPage() {
  const navigate = useNavigate()
  const [calendars, setCalendars] = useState<WorkingCalendar[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [statusFilter, setStatusFilter] = useState('')
  const [nameFilter, setNameFilter] = useState('')
  const [pendingDelete, setPendingDelete] = useState<WorkingCalendar | null>(null)
  const [busyId, setBusyId] = useState<string | null>(null)

  const loadCalendars = useCallback(async () => {
    setLoading(true)
    setError(null)

    try {
      const data = await listWorkingCalendars({
        companyId: DEFAULT_COMPANY_ID,
        status: statusFilter || undefined,
        name: nameFilter || undefined,
        orderBy: 'effectiveFrom',
        orderDirection: 'desc',
      })
      setCalendars(data)
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to load working calendars.'))
    } finally {
      setLoading(false)
    }
  }, [nameFilter, statusFilter])

  useEffect(() => {
    void loadCalendars()
  }, [loadCalendars])

  const runAction = async (id: string, action: () => Promise<void>) => {
    setBusyId(id)
    setError(null)

    try {
      await action()
      await loadCalendars()
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusyId(null)
    }
  }

  const confirmDelete = async () => {
    if (!pendingDelete) {
      return
    }

    const id = pendingDelete.id
    setPendingDelete(null)
    await runAction(id, () => deleteWorkingCalendar(id))
  }

  return (
    <Stack spacing={3}>
      <Stack
        direction={{ xs: 'column', sm: 'row' }}
        justifyContent="space-between"
        alignItems={{ xs: 'stretch', sm: 'flex-start' }}
        spacing={2}
      >
        <Box>
          <Typography variant="h4" gutterBottom>
            Working Calendars
          </Typography>
          <Typography color="text.secondary">
            Configure company working days and validity periods used by Capacity Planning.
          </Typography>
        </Box>
        <Button
          component={RouterLink}
          to="/working-calendars/new"
          variant="contained"
          startIcon={<AddIcon />}
        >
          Create calendar
        </Button>
      </Stack>

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          <TextField
            label="Filter by name"
            value={nameFilter}
            onChange={(event) => setNameFilter(event.target.value)}
            fullWidth
          />
          <FormControl fullWidth sx={{ maxWidth: { md: 220 } }}>
            <InputLabel id="status-filter-label">Status</InputLabel>
            <Select
              labelId="status-filter-label"
              label="Status"
              value={statusFilter}
              onChange={(event) => setStatusFilter(event.target.value)}
            >
              <MenuItem value="">All</MenuItem>
              <MenuItem value="Active">Active</MenuItem>
              <MenuItem value="Inactive">Inactive</MenuItem>
            </Select>
          </FormControl>
        </Stack>
      </Paper>

      {error ? <Alert severity="error">{error}</Alert> : null}

      <Paper>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
            <CircularProgress />
          </Box>
        ) : calendars.length === 0 ? (
          <Box sx={{ py: 8, px: 3, textAlign: 'center' }}>
            <Typography variant="h6" gutterBottom>
              No working calendars yet
            </Typography>
            <Typography color="text.secondary" sx={{ mb: 2 }}>
              Create the first company calendar to define operational working days.
            </Typography>
            <Button component={RouterLink} to="/working-calendars/new" variant="contained">
              Create calendar
            </Button>
          </Box>
        ) : (
          <TableContainer sx={{ overflowX: 'auto' }}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Effective period</TableCell>
                  <TableCell>Working days</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {calendars.map((calendar) => {
                  const busy = busyId === calendar.id

                  return (
                    <TableRow key={calendar.id} hover>
                      <TableCell>
                        <Typography fontWeight={600}>{calendar.name}</Typography>
                      </TableCell>
                      <TableCell>
                        <Chip
                          size="small"
                          label={calendar.status}
                          color={calendar.status === 'Active' ? 'success' : 'default'}
                        />
                      </TableCell>
                      <TableCell>
                        {calendar.effectiveFrom}
                        {' → '}
                        {calendar.effectiveTo ?? 'Open'}
                      </TableCell>
                      <TableCell>
                        <Stack direction="row" gap={0.5} flexWrap="wrap" useFlexGap>
                          {calendar.workingDays.map((day) => (
                            <Chip key={day} size="small" label={day.slice(0, 3)} variant="outlined" />
                          ))}
                        </Stack>
                      </TableCell>
                      <TableCell align="right">
                        <Stack
                          direction="row"
                          spacing={1}
                          justifyContent="flex-end"
                          flexWrap="wrap"
                          useFlexGap
                        >
                          <Button
                            size="small"
                            onClick={() => navigate(`/working-calendars/${calendar.id}/edit`)}
                            disabled={busy}
                          >
                            Edit
                          </Button>
                          {calendar.status === 'Inactive' ? (
                            <Button
                              size="small"
                              onClick={() =>
                                void runAction(calendar.id, () => activateWorkingCalendar(calendar.id))
                              }
                              disabled={busy}
                            >
                              Activate
                            </Button>
                          ) : (
                            <Button
                              size="small"
                              onClick={() =>
                                void runAction(calendar.id, () =>
                                  deactivateWorkingCalendar(calendar.id),
                                )
                              }
                              disabled={busy}
                            >
                              Deactivate
                            </Button>
                          )}
                          <Button
                            size="small"
                            color="error"
                            onClick={() => setPendingDelete(calendar)}
                            disabled={busy || calendar.status === 'Active'}
                          >
                            Delete
                          </Button>
                        </Stack>
                      </TableCell>
                    </TableRow>
                  )
                })}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </Paper>

      <Dialog open={Boolean(pendingDelete)} onClose={() => setPendingDelete(null)}>
        <DialogTitle>Delete working calendar?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            This permanently deletes &quot;{pendingDelete?.name}&quot;. Only inactive calendars that
            have not yet started can be deleted.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setPendingDelete(null)}>Cancel</Button>
          <Button color="error" variant="contained" onClick={() => void confirmDelete()}>
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  )
}
