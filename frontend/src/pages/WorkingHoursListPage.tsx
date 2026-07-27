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
  activateWorkingHours,
  deactivateWorkingHours,
  deleteWorkingHours,
  listWorkingHours,
} from '../api/workingHours'
import { getErrorMessage } from '../api/client'
import type { WorkingHours } from '../types/workingHours'

export function WorkingHoursListPage() {
  const navigate = useNavigate()
  const [items, setItems] = useState<WorkingHours[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [nameFilter, setNameFilter] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [pendingDelete, setPendingDelete] = useState<WorkingHours | null>(null)
  const [busyId, setBusyId] = useState<string | null>(null)

  const loadItems = useCallback(async () => {
    setLoading(true)
    setError(null)

    try {
      const data = await listWorkingHours({
        name: nameFilter || undefined,
        status: statusFilter || undefined,
        orderBy: 'effectiveFrom',
        orderDirection: 'desc',
      })
      setItems(data)
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to load working hours.'))
    } finally {
      setLoading(false)
    }
  }, [nameFilter, statusFilter])

  useEffect(() => {
    void loadItems()
  }, [loadItems])

  const runAction = async (id: string, action: () => Promise<void>) => {
    setBusyId(id)
    setError(null)

    try {
      await action()
      await loadItems()
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusyId(null)
    }
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
            Working Hours
          </Typography>
          <Typography color="text.secondary">
            Configure standard weekday schedules associated with Working Calendars.
          </Typography>
        </Box>
        <Button
          component={RouterLink}
          to="/working-hours/new"
          variant="contained"
          startIcon={<AddIcon />}
        >
          Create working hours
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
          <FormControl fullWidth sx={{ maxWidth: { md: 220 } }}>
            <InputLabel id="hours-status-filter">Status</InputLabel>
            <Select
              labelId="hours-status-filter"
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
        ) : items.length === 0 ? (
          <Box sx={{ py: 8, px: 3, textAlign: 'center' }}>
            <Typography variant="h6" gutterBottom>
              No working hours yet
            </Typography>
            <Button component={RouterLink} to="/working-hours/new" variant="contained">
              Create working hours
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
                  <TableCell>Enabled days</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {items.map((item) => {
                  const busy = busyId === item.id
                  return (
                    <TableRow key={item.id} hover>
                      <TableCell>
                        <Typography fontWeight={600}>{item.name}</Typography>
                      </TableCell>
                      <TableCell>
                        <Chip
                          size="small"
                          label={item.status}
                          color={item.status === 'Active' ? 'success' : 'default'}
                        />
                      </TableCell>
                      <TableCell>
                        {item.effectiveFrom} → {item.effectiveTo ?? 'Open'}
                      </TableCell>
                      <TableCell>{item.days.filter((day) => day.enabled).length}</TableCell>
                      <TableCell align="right">
                        <Stack
                          direction="row"
                          spacing={1}
                          justifyContent="flex-end"
                          flexWrap="wrap"
                          useFlexGap
                        >
                          <Button size="small" onClick={() => navigate(`/working-hours/${item.id}`)}>
                            Detail
                          </Button>
                          <Button
                            size="small"
                            onClick={() => navigate(`/working-hours/${item.id}/edit`)}
                          >
                            Edit
                          </Button>
                          {item.status === 'Inactive' ? (
                            <Button
                              size="small"
                              disabled={busy}
                              onClick={() =>
                                void runAction(item.id, () => activateWorkingHours(item.id))
                              }
                            >
                              Activate
                            </Button>
                          ) : (
                            <Button
                              size="small"
                              disabled={busy}
                              onClick={() =>
                                void runAction(item.id, () => deactivateWorkingHours(item.id))
                              }
                            >
                              Deactivate
                            </Button>
                          )}
                          <Button
                            size="small"
                            color="error"
                            disabled={busy || item.status === 'Active'}
                            onClick={() => setPendingDelete(item)}
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
        <DialogTitle>Delete working hours?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            This permanently deletes &quot;{pendingDelete?.name}&quot;. Only inactive configurations
            that have not started can be deleted.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setPendingDelete(null)}>Cancel</Button>
          <Button
            color="error"
            variant="contained"
            onClick={() => {
              if (!pendingDelete) {
                return
              }
              const id = pendingDelete.id
              setPendingDelete(null)
              void runAction(id, () => deleteWorkingHours(id))
            }}
          >
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  )
}
