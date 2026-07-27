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
  activateResourceAvailability,
  deactivateResourceAvailability,
  deleteResourceAvailability,
  listResourceAvailabilities,
} from '../api/resourceAvailabilities'
import { getErrorMessage } from '../api/client'
import type { ResourceAvailability } from '../types/resourceAvailability'

export function ResourceAvailabilityListPage() {
  const navigate = useNavigate()
  const [items, setItems] = useState<ResourceAvailability[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [nameFilter, setNameFilter] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [pendingDelete, setPendingDelete] = useState<ResourceAvailability | null>(null)
  const [busyId, setBusyId] = useState<string | null>(null)

  const loadItems = useCallback(async () => {
    setLoading(true)
    setError(null)

    try {
      const data = await listResourceAvailabilities({
        name: nameFilter || undefined,
        status: statusFilter || undefined,
        orderBy: 'effectiveFrom',
        orderDirection: 'desc',
      })
      setItems(data)
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to load resource availabilities.'))
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
      <Box sx={{ display: 'flex', justifyContent: 'space-between', gap: 2, flexWrap: 'wrap' }}>
        <Box>
          <Typography variant="h4">Resource Availability</Typography>
          <Typography color="text.secondary">
            Planned individual availability linked to calendars and working hours (US-104).
          </Typography>
        </Box>
        <Button
          component={RouterLink}
          to="/resource-availabilities/new"
          variant="contained"
          startIcon={<AddIcon />}
        >
          New Availability
        </Button>
      </Box>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          <TextField
            label="Name"
            value={nameFilter}
            onChange={(event) => setNameFilter(event.target.value)}
            size="small"
            fullWidth
          />
          <FormControl size="small" sx={{ minWidth: 160 }}>
            <InputLabel id="ra-status-filter">Status</InputLabel>
            <Select
              labelId="ra-status-filter"
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

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
          <CircularProgress />
        </Box>
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Effective From</TableCell>
                <TableCell>Effective To</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {items.map((item) => (
                <TableRow key={item.id} hover>
                  <TableCell>{item.name}</TableCell>
                  <TableCell>
                    <Chip
                      size="small"
                      label={item.status}
                      color={item.status === 'Active' ? 'success' : 'default'}
                    />
                  </TableCell>
                  <TableCell>{item.effectiveFrom}</TableCell>
                  <TableCell>{item.effectiveTo ?? 'Open'}</TableCell>
                  <TableCell align="right">
                    <Stack direction="row" spacing={1} justifyContent="flex-end">
                      <Button size="small" onClick={() => navigate(`/resource-availabilities/${item.id}`)}>
                        View
                      </Button>
                      <Button
                        size="small"
                        disabled={busyId === item.id}
                        onClick={() =>
                          void runAction(
                            item.id,
                            item.status === 'Active'
                              ? () => deactivateResourceAvailability(item.id)
                              : () => activateResourceAvailability(item.id),
                          )
                        }
                      >
                        {item.status === 'Active' ? 'Deactivate' : 'Activate'}
                      </Button>
                      <Button
                        size="small"
                        color="error"
                        disabled={item.status === 'Active' || busyId === item.id}
                        onClick={() => setPendingDelete(item)}
                      >
                        Delete
                      </Button>
                    </Stack>
                  </TableCell>
                </TableRow>
              ))}
              {items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={5}>
                    <Typography color="text.secondary">No resource availabilities found.</Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      <Dialog open={Boolean(pendingDelete)} onClose={() => setPendingDelete(null)}>
        <DialogTitle>Delete resource availability?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Delete inactive configuration &quot;{pendingDelete?.name}&quot;? Historical records cannot be
            deleted.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setPendingDelete(null)}>Cancel</Button>
          <Button
            color="error"
            onClick={() => {
              if (!pendingDelete) return
              const id = pendingDelete.id
              setPendingDelete(null)
              void runAction(id, () => deleteResourceAvailability(id))
            }}
          >
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  )
}
