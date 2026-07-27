import { useCallback, useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  MenuItem,
  Paper,
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
import { listDecisions } from '../api/decision'
import { getErrorMessage } from '../api/client'
import { DecisionStatusBadge, ImplementationStatusBadge } from '../components/DecisionStatusBadge'
import type { Decision } from '../types/decision'
import { DEFAULT_COMPANY_ID } from '../theme'

export function DecisionListPage() {
  const navigate = useNavigate()
  const [companyId, setCompanyId] = useState(DEFAULT_COMPANY_ID)
  const [decisionStatus, setDecisionStatus] = useState('')
  const [implementationStatus, setImplementationStatus] = useState('')
  const [search, setSearch] = useState('')
  const [items, setItems] = useState<Decision[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await listDecisions({
        companyId: companyId || undefined,
        decisionStatus: decisionStatus || undefined,
        implementationStatus: implementationStatus || undefined,
        search: search || undefined,
        orderBy: 'decisionDate',
        orderDirection: 'desc',
      })
      setItems(data)
    } catch (err) {
      setItems([])
      setError(getErrorMessage(err, 'Failed to load decisions.'))
    } finally {
      setLoading(false)
    }
  }, [companyId, decisionStatus, implementationStatus, search])

  useEffect(() => {
    void load()
  }, [load])

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
            Decision Tracking
          </Typography>
          <Typography color="text.secondary">
            Lifecycle monitoring of business decisions derived from approved recommendations
            (US-205).
          </Typography>
        </Box>
        <Button component={RouterLink} to="/decisions/new" variant="contained">
          New Decision
        </Button>
      </Stack>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} useFlexGap flexWrap="wrap">
          <TextField
            label="Company Id"
            size="small"
            value={companyId}
            onChange={(event) => setCompanyId(event.target.value)}
            sx={{ minWidth: 280 }}
          />
          <TextField
            select
            label="Decision Status"
            size="small"
            value={decisionStatus}
            onChange={(event) => setDecisionStatus(event.target.value)}
            sx={{ minWidth: 160 }}
          >
            <MenuItem value="">All</MenuItem>
            <MenuItem value="Created">Created</MenuItem>
            <MenuItem value="InProgress">InProgress</MenuItem>
            <MenuItem value="Completed">Completed</MenuItem>
            <MenuItem value="Cancelled">Cancelled</MenuItem>
          </TextField>
          <TextField
            select
            label="Implementation"
            size="small"
            value={implementationStatus}
            onChange={(event) => setImplementationStatus(event.target.value)}
            sx={{ minWidth: 160 }}
          >
            <MenuItem value="">All</MenuItem>
            <MenuItem value="NotStarted">NotStarted</MenuItem>
            <MenuItem value="InProgress">InProgress</MenuItem>
            <MenuItem value="Completed">Completed</MenuItem>
            <MenuItem value="Cancelled">Cancelled</MenuItem>
          </TextField>
          <TextField
            label="Search"
            size="small"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            sx={{ minWidth: 200 }}
          />
          <Button variant="contained" onClick={() => void load()} disabled={loading}>
            Refresh
          </Button>
        </Stack>
      </Paper>

      <TableContainer component={Paper}>
        {loading ? (
          <Box sx={{ p: 4, display: 'flex', justifyContent: 'center' }}>
            <CircularProgress size={28} />
          </Box>
        ) : (
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Decision Date</TableCell>
                <TableCell>Decision</TableCell>
                <TableCell>Implementation</TableCell>
                <TableCell>Recommendation</TableCell>
                <TableCell>Outcome</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {items.map((item) => (
                <TableRow key={item.id} hover>
                  <TableCell>{new Date(item.decisionDate).toLocaleString()}</TableCell>
                  <TableCell>
                    <DecisionStatusBadge status={item.decisionStatus} />
                  </TableCell>
                  <TableCell>
                    <ImplementationStatusBadge status={item.implementationStatus} />
                  </TableCell>
                  <TableCell sx={{ fontFamily: 'monospace', fontSize: 12 }}>
                    {item.recommendationId}
                  </TableCell>
                  <TableCell>{item.outcome ?? '—'}</TableCell>
                  <TableCell align="right">
                    <Button size="small" onClick={() => navigate(`/decisions/${item.id}`)}>
                      Open
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
              {items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={6}>
                    <Typography color="text.secondary" sx={{ py: 2 }}>
                      No decisions found.
                    </Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        )}
      </TableContainer>
    </Stack>
  )
}
