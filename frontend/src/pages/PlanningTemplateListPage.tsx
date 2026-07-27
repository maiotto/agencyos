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
  activatePlanningTemplate,
  clonePlanningTemplate,
  deactivatePlanningTemplate,
  deletePlanningTemplate,
  filterPlanningTemplates,
} from '../api/planningTemplates'
import { getErrorMessage } from '../api/client'
import type { PlanningTemplate } from '../types/planningTemplate'

const DEFAULT_COMPANY_ID = 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa'

export function PlanningTemplateListPage() {
  const navigate = useNavigate()
  const [items, setItems] = useState<PlanningTemplate[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [pendingDelete, setPendingDelete] = useState<PlanningTemplate | null>(null)
  const [pendingClone, setPendingClone] = useState<PlanningTemplate | null>(null)
  const [cloneName, setCloneName] = useState('')
  const [busyId, setBusyId] = useState<string | null>(null)

  const loadItems = useCallback(async () => {
    setLoading(true)
    setError(null)

    try {
      const data = await filterPlanningTemplates({
        companyId: DEFAULT_COMPANY_ID,
        search: search || undefined,
        status: statusFilter || undefined,
        orderBy: 'name',
        orderDirection: 'asc',
      })
      setItems(data)
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to load planning templates.'))
    } finally {
      setLoading(false)
    }
  }, [search, statusFilter])

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
      <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" spacing={2}>
        <Box>
          <Typography variant="h4">Planning Templates</Typography>
          <Typography color="text.secondary">
            Reusable planning configuration referencing calendars and hours (US-108).
          </Typography>
        </Box>
        <Button
          component={RouterLink}
          to="/planning-templates/new"
          variant="contained"
          startIcon={<AddIcon />}
        >
          New Template
        </Button>
      </Stack>

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          <TextField
            label="Search"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            size="small"
            fullWidth
          />
          <FormControl size="small" sx={{ minWidth: 160 }}>
            <InputLabel>Status</InputLabel>
            <Select
              label="Status"
              value={statusFilter}
              onChange={(event) => setStatusFilter(event.target.value)}
            >
              <MenuItem value="">All</MenuItem>
              <MenuItem value="Active">Active</MenuItem>
              <MenuItem value="Inactive">Inactive</MenuItem>
            </Select>
          </FormControl>
          <Button variant="outlined" onClick={() => void loadItems()}>
            Refresh
          </Button>
        </Stack>
      </Paper>

      {error && <Alert severity="error">{error}</Alert>}

      {loading ? (
        <Box display="flex" justifyContent="center" py={6}>
          <CircularProgress />
        </Box>
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Window</TableCell>
                <TableCell>Strategy</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {items.map((item) => (
                <TableRow key={item.id} hover>
                  <TableCell>
                    <Button component={RouterLink} to={`/planning-templates/${item.id}`} size="small">
                      {item.name}
                    </Button>
                  </TableCell>
                  <TableCell>
                    <Chip
                      size="small"
                      label={item.status}
                      color={item.status === 'Active' ? 'success' : 'default'}
                    />
                  </TableCell>
                  <TableCell>{item.defaultPlanningWindowDays} days</TableCell>
                  <TableCell>{item.resourceAvailabilityStrategy}</TableCell>
                  <TableCell align="right">
                    <Stack direction="row" spacing={1} justifyContent="flex-end" flexWrap="wrap">
                      <Button
                        size="small"
                        onClick={() => navigate(`/planning-templates/${item.id}/edit`)}
                      >
                        Edit
                      </Button>
                      <Button
                        size="small"
                        disabled={busyId === item.id}
                        onClick={() =>
                          void runAction(item.id, () =>
                            item.status === 'Active'
                              ? deactivatePlanningTemplate(item.id)
                              : activatePlanningTemplate(item.id),
                          )
                        }
                      >
                        {item.status === 'Active' ? 'Deactivate' : 'Activate'}
                      </Button>
                      <Button
                        size="small"
                        onClick={() => {
                          setPendingClone(item)
                          setCloneName(`${item.name} (Copy)`)
                        }}
                      >
                        Clone
                      </Button>
                      <Button
                        size="small"
                        component={RouterLink}
                        to={`/planning-templates/${item.id}/apply`}
                        disabled={item.status !== 'Active'}
                      >
                        Apply
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
                    <Typography color="text.secondary" py={2}>
                      No planning templates found.
                    </Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      <Dialog open={Boolean(pendingDelete)} onClose={() => setPendingDelete(null)}>
        <DialogTitle>Delete template?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Delete inactive template “{pendingDelete?.name}”? This cannot be undone.
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
              void runAction(id, () => deletePlanningTemplate(id))
            }}
          >
            Delete
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={Boolean(pendingClone)} onClose={() => setPendingClone(null)}>
        <DialogTitle>Clone template</DialogTitle>
        <DialogContent>
          <TextField
            autoFocus
            margin="dense"
            label="New template name"
            fullWidth
            value={cloneName}
            onChange={(event) => setCloneName(event.target.value)}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setPendingClone(null)}>Cancel</Button>
          <Button
            variant="contained"
            disabled={!cloneName.trim()}
            onClick={() => {
              if (!pendingClone) return
              const source = pendingClone
              setPendingClone(null)
              void runAction(source.id, async () => {
                const cloned = await clonePlanningTemplate(source.id, cloneName.trim())
                navigate(`/planning-templates/${cloned.id}`)
              })
            }}
          >
            Clone
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  )
}
