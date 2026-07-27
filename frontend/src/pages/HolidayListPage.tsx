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
  activateHoliday,
  deactivateHoliday,
  deleteHoliday,
  filterHolidays,
} from '../api/holidays'
import { getErrorMessage } from '../api/client'
import { DEFAULT_COMPANY_ID } from '../theme'
import { HOLIDAY_TYPES, type Holiday } from '../types/holiday'

export function HolidayListPage() {
  const navigate = useNavigate()
  const [holidays, setHolidays] = useState<Holiday[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [nameFilter, setNameFilter] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [typeFilter, setTypeFilter] = useState('')
  const [pendingDelete, setPendingDelete] = useState<Holiday | null>(null)
  const [busyId, setBusyId] = useState<string | null>(null)

  const loadHolidays = useCallback(async () => {
    setLoading(true)
    setError(null)

    try {
      const data = await filterHolidays({
        companyId: DEFAULT_COMPANY_ID,
        name: nameFilter || undefined,
        status: statusFilter || undefined,
        holidayType: typeFilter || undefined,
        orderBy: 'holidayDate',
        orderDirection: 'asc',
      })
      setHolidays(data)
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to load holidays.'))
    } finally {
      setLoading(false)
    }
  }, [nameFilter, statusFilter, typeFilter])

  useEffect(() => {
    void loadHolidays()
  }, [loadHolidays])

  const runAction = async (id: string, action: () => Promise<void>) => {
    setBusyId(id)
    setError(null)

    try {
      await action()
      await loadHolidays()
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
    await runAction(id, () => deleteHoliday(id))
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
            Holidays
          </Typography>
          <Typography color="text.secondary">
            Manage non-working days used with Working Calendars for Capacity Planning.
          </Typography>
        </Box>
        <Button
          component={RouterLink}
          to="/holidays/new"
          variant="contained"
          startIcon={<AddIcon />}
        >
          Create holiday
        </Button>
      </Stack>

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          <TextField
            label="Search by name"
            value={nameFilter}
            onChange={(event) => setNameFilter(event.target.value)}
            fullWidth
          />
          <FormControl fullWidth sx={{ maxWidth: { md: 200 } }}>
            <InputLabel id="holiday-status-filter">Status</InputLabel>
            <Select
              labelId="holiday-status-filter"
              label="Status"
              value={statusFilter}
              onChange={(event) => setStatusFilter(event.target.value)}
            >
              <MenuItem value="">All</MenuItem>
              <MenuItem value="Active">Active</MenuItem>
              <MenuItem value="Inactive">Inactive</MenuItem>
            </Select>
          </FormControl>
          <FormControl fullWidth sx={{ maxWidth: { md: 220 } }}>
            <InputLabel id="holiday-type-filter">Type</InputLabel>
            <Select
              labelId="holiday-type-filter"
              label="Type"
              value={typeFilter}
              onChange={(event) => setTypeFilter(event.target.value)}
            >
              <MenuItem value="">All</MenuItem>
              {HOLIDAY_TYPES.map((type) => (
                <MenuItem key={type} value={type}>
                  {type}
                </MenuItem>
              ))}
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
        ) : holidays.length === 0 ? (
          <Box sx={{ py: 8, px: 3, textAlign: 'center' }}>
            <Typography variant="h6" gutterBottom>
              No holidays found
            </Typography>
            <Typography color="text.secondary" sx={{ mb: 2 }}>
              Create national, state, municipal, or company holidays.
            </Typography>
            <Button component={RouterLink} to="/holidays/new" variant="contained">
              Create holiday
            </Button>
          </Box>
        ) : (
          <TableContainer sx={{ overflowX: 'auto' }}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Type</TableCell>
                  <TableCell>Date</TableCell>
                  <TableCell>Scope</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {holidays.map((holiday) => {
                  const busy = busyId === holiday.id

                  return (
                    <TableRow key={holiday.id} hover>
                      <TableCell>
                        <Typography fontWeight={600}>{holiday.name}</Typography>
                        {holiday.recurring ? (
                          <Typography variant="caption" color="text.secondary">
                            Recurring
                          </Typography>
                        ) : null}
                      </TableCell>
                      <TableCell>{holiday.holidayType}</TableCell>
                      <TableCell>{holiday.holidayDate}</TableCell>
                      <TableCell>
                        {[holiday.stateCode, holiday.city].filter(Boolean).join(' / ') || '—'}
                      </TableCell>
                      <TableCell>
                        <Chip
                          size="small"
                          label={holiday.status}
                          color={holiday.status === 'Active' ? 'success' : 'default'}
                        />
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
                            onClick={() => navigate(`/holidays/${holiday.id}`)}
                            disabled={busy}
                          >
                            Detail
                          </Button>
                          <Button
                            size="small"
                            onClick={() => navigate(`/holidays/${holiday.id}/edit`)}
                            disabled={busy}
                          >
                            Edit
                          </Button>
                          {holiday.status === 'Inactive' ? (
                            <Button
                              size="small"
                              onClick={() =>
                                void runAction(holiday.id, () => activateHoliday(holiday.id))
                              }
                              disabled={busy}
                            >
                              Activate
                            </Button>
                          ) : (
                            <Button
                              size="small"
                              onClick={() =>
                                void runAction(holiday.id, () => deactivateHoliday(holiday.id))
                              }
                              disabled={busy}
                            >
                              Deactivate
                            </Button>
                          )}
                          <Button
                            size="small"
                            color="error"
                            onClick={() => setPendingDelete(holiday)}
                            disabled={busy || holiday.status === 'Active'}
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
        <DialogTitle>Delete holiday?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            This permanently deletes &quot;{pendingDelete?.name}&quot;. Active holidays must be
            deactivated first.
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
