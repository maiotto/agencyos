import { useCallback, useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  FormControl,
  FormControlLabel,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Stack,
  Switch,
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
  activateCompany,
  archiveCompany,
  deactivateCompany,
  listCompanies,
} from '../api/companies'
import { getErrorMessage } from '../api/client'
import { useCompany } from '../context/CompanyContext'
import type { Company } from '../types/company'

function statusColor(status: string): 'success' | 'default' | 'warning' {
  if (status === 'Active') return 'success'
  if (status === 'Archived') return 'default'
  return 'warning'
}

export function CompanyListPage() {
  const navigate = useNavigate()
  const { activeCompanyId, refreshCompanies } = useCompany()
  const [companies, setCompanies] = useState<Company[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [searchFilter, setSearchFilter] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [includeArchived, setIncludeArchived] = useState(false)
  const [busyId, setBusyId] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)

    try {
      const data = await listCompanies({
        search: searchFilter || undefined,
        status: statusFilter || undefined,
        includeArchived,
        orderBy: 'companyName',
        orderDirection: 'asc',
      })
      setCompanies(data)
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to load companies.'))
    } finally {
      setLoading(false)
    }
  }, [searchFilter, statusFilter, includeArchived])

  useEffect(() => {
    void load()
  }, [load])

  const runAction = async (id: string, action: () => Promise<unknown>) => {
    setBusyId(id)
    setError(null)

    try {
      await action()
      await load()
      await refreshCompanies()
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
            Companies
          </Typography>
          <Typography color="text.secondary">
            First-class multi-company tenant configuration (US-402). Company-scoped modules remain
            unchanged; select a Company to scope AI-generated artifacts and future requests.
          </Typography>
        </Box>
        <Button component={RouterLink} to="/companies/new" variant="contained" startIcon={<AddIcon />}>
          Create company
        </Button>
      </Stack>

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} alignItems={{ md: 'center' }}>
          <TextField
            label="Search by name or code"
            value={searchFilter}
            onChange={(event) => setSearchFilter(event.target.value)}
            fullWidth
          />
          <FormControl fullWidth sx={{ maxWidth: { md: 200 } }}>
            <InputLabel id="company-status-filter">Status</InputLabel>
            <Select
              labelId="company-status-filter"
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
          <FormControlLabel
            control={
              <Switch
                checked={includeArchived}
                onChange={(event) => setIncludeArchived(event.target.checked)}
              />
            }
            label="Include archived (BR-2009)"
          />
        </Stack>
      </Paper>

      {error ? <Alert severity="error">{error}</Alert> : null}

      <Paper>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
            <CircularProgress />
          </Box>
        ) : companies.length === 0 ? (
          <Box sx={{ py: 8, px: 3, textAlign: 'center' }}>
            <Typography variant="h6" gutterBottom>
              No companies found
            </Typography>
            <Typography color="text.secondary" sx={{ mb: 2 }}>
              Create a Company to configure a new multi-company tenant.
            </Typography>
            <Button component={RouterLink} to="/companies/new" variant="contained">
              Create company
            </Button>
          </Box>
        ) : (
          <TableContainer sx={{ overflowX: 'auto' }}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Code</TableCell>
                  <TableCell>Timezone</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {companies.map((company) => {
                  const busy = busyId === company.id
                  const isActiveSelection = company.id === activeCompanyId

                  return (
                    <TableRow key={company.id} hover selected={isActiveSelection}>
                      <TableCell>
                        <Stack direction="row" spacing={1} alignItems="center">
                          <Typography fontWeight={600}>{company.companyName}</Typography>
                          {isActiveSelection ? (
                            <Chip size="small" label="Selected" color="secondary" />
                          ) : null}
                        </Stack>
                      </TableCell>
                      <TableCell>{company.companyCode}</TableCell>
                      <TableCell>{company.timezone}</TableCell>
                      <TableCell>
                        <Chip size="small" label={company.status} color={statusColor(company.status)} />
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
                            onClick={() => navigate(`/companies/${company.id}`)}
                            disabled={busy}
                          >
                            Detail
                          </Button>
                          <Button
                            size="small"
                            onClick={() => navigate(`/companies/${company.id}/edit`)}
                            disabled={busy || company.status === 'Archived'}
                          >
                            Edit
                          </Button>
                          {company.status === 'Inactive' ? (
                            <Button
                              size="small"
                              onClick={() => void runAction(company.id, () => activateCompany(company.id))}
                              disabled={busy}
                            >
                              Activate
                            </Button>
                          ) : null}
                          {company.status === 'Active' ? (
                            <Button
                              size="small"
                              onClick={() =>
                                void runAction(company.id, () => deactivateCompany(company.id))
                              }
                              disabled={busy}
                            >
                              Deactivate
                            </Button>
                          ) : null}
                          {company.status !== 'Archived' ? (
                            <Button
                              size="small"
                              color="error"
                              onClick={() => void runAction(company.id, () => archiveCompany(company.id))}
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
