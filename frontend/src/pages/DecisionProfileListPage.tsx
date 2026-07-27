import { useCallback, useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
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
import StarIcon from '@mui/icons-material/Star'
import {
  activateDecisionProfile,
  archiveDecisionProfile,
  deactivateDecisionProfile,
  filterDecisionProfiles,
  setDefaultDecisionProfile,
} from '../api/decisionProfiles'
import { getErrorMessage } from '../api/client'
import { DEFAULT_COMPANY_ID } from '../theme'
import type { CompanyDecisionProfile } from '../types/decisionProfile'

function statusColor(status: string): 'success' | 'default' | 'warning' {
  if (status === 'Active') return 'success'
  if (status === 'Archived') return 'default'
  return 'warning'
}

export function DecisionProfileListPage() {
  const navigate = useNavigate()
  const [profiles, setProfiles] = useState<CompanyDecisionProfile[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [nameFilter, setNameFilter] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [busyId, setBusyId] = useState<string | null>(null)

  const loadProfiles = useCallback(async () => {
    setLoading(true)
    setError(null)

    try {
      const data = await filterDecisionProfiles({
        companyId: DEFAULT_COMPANY_ID,
        name: nameFilter || undefined,
        status: statusFilter || undefined,
        orderBy: 'name',
        orderDirection: 'asc',
      })
      setProfiles(data)
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to load decision profiles.'))
    } finally {
      setLoading(false)
    }
  }, [nameFilter, statusFilter])

  useEffect(() => {
    void loadProfiles()
  }, [loadProfiles])

  const runAction = async (id: string, action: () => Promise<unknown>) => {
    setBusyId(id)
    setError(null)

    try {
      await action()
      await loadProfiles()
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
            Decision Profiles
          </Typography>
          <Typography color="text.secondary">
            Company-specific ranking preferences that influence Delivery Strategy ranking and AI
            Decision Support (US-401).
          </Typography>
        </Box>
        <Button
          component={RouterLink}
          to="/decision-profiles/new"
          variant="contained"
          startIcon={<AddIcon />}
        >
          Create profile
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
            <InputLabel id="profile-status-filter">Status</InputLabel>
            <Select
              labelId="profile-status-filter"
              label="Status"
              value={statusFilter}
              onChange={(event) => setStatusFilter(event.target.value)}
            >
              <MenuItem value="">All</MenuItem>
              <MenuItem value="Active">Active</MenuItem>
              <MenuItem value="Inactive">Inactive</MenuItem>
              <MenuItem value="Archived">Archived</MenuItem>
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
        ) : profiles.length === 0 ? (
          <Box sx={{ py: 8, px: 3, textAlign: 'center' }}>
            <Typography variant="h6" gutterBottom>
              No decision profiles found
            </Typography>
            <Typography color="text.secondary" sx={{ mb: 2 }}>
              Create a profile to customize ranking weights for this company.
            </Typography>
            <Button component={RouterLink} to="/decision-profiles/new" variant="contained">
              Create profile
            </Button>
          </Box>
        ) : (
          <TableContainer sx={{ overflowX: 'auto' }}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Code</TableCell>
                  <TableCell>Version</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {profiles.map((profile) => {
                  const busy = busyId === profile.id

                  return (
                    <TableRow key={profile.id} hover>
                      <TableCell>
                        <Stack direction="row" spacing={1} alignItems="center">
                          {profile.defaultProfile ? (
                            <StarIcon fontSize="small" color="secondary" titleAccess="Default" />
                          ) : null}
                          <Typography fontWeight={600}>{profile.name}</Typography>
                        </Stack>
                      </TableCell>
                      <TableCell>{profile.code}</TableCell>
                      <TableCell>v{profile.version}</TableCell>
                      <TableCell>
                        <Chip size="small" label={profile.status} color={statusColor(profile.status)} />
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
                            onClick={() => navigate(`/decision-profiles/${profile.id}`)}
                            disabled={busy}
                          >
                            Detail
                          </Button>
                          <Button
                            size="small"
                            onClick={() => navigate(`/decision-profiles/${profile.id}/edit`)}
                            disabled={busy || profile.status === 'Archived'}
                          >
                            Edit
                          </Button>
                          {!profile.defaultProfile && profile.status === 'Active' ? (
                            <Button
                              size="small"
                              onClick={() =>
                                void runAction(profile.id, () => setDefaultDecisionProfile(profile.id))
                              }
                              disabled={busy}
                            >
                              Set default
                            </Button>
                          ) : null}
                          {profile.status === 'Inactive' ? (
                            <Button
                              size="small"
                              onClick={() =>
                                void runAction(profile.id, () => activateDecisionProfile(profile.id))
                              }
                              disabled={busy}
                            >
                              Activate
                            </Button>
                          ) : null}
                          {profile.status === 'Active' && !profile.defaultProfile ? (
                            <Button
                              size="small"
                              onClick={() =>
                                void runAction(profile.id, () => deactivateDecisionProfile(profile.id))
                              }
                              disabled={busy}
                            >
                              Deactivate
                            </Button>
                          ) : null}
                          {profile.status !== 'Archived' && !profile.defaultProfile ? (
                            <Button
                              size="small"
                              color="error"
                              onClick={() =>
                                void runAction(profile.id, () => archiveDecisionProfile(profile.id))
                              }
                              disabled={busy}
                            >
                              Archive
                            </Button>
                          ) : null}
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
    </Stack>
  )
}
