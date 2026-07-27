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
import AddIcon from '@mui/icons-material/Add'
import { listPortfolios } from '../api/portfolio'
import { getErrorMessage } from '../api/client'
import { PortfolioHealthBadge, PortfolioStatusBadge } from '../components/PortfolioBadges'
import type { Portfolio } from '../types/portfolio'
import { DEFAULT_COMPANY_ID } from '../theme'

export function PortfolioListPage() {
  const navigate = useNavigate()
  const [companyId, setCompanyId] = useState(DEFAULT_COMPANY_ID)
  const [status, setStatus] = useState('')
  const [search, setSearch] = useState('')
  const [items, setItems] = useState<Portfolio[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await listPortfolios({
        companyId: companyId || undefined,
        status: status || undefined,
        search: search || undefined,
        orderBy: 'updatedAt',
        orderDirection: 'desc',
      })
      setItems(data)
    } catch (err) {
      setItems([])
      setError(getErrorMessage(err, 'Failed to load portfolios.'))
    } finally {
      setLoading(false)
    }
  }, [companyId, status, search])

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
            Portfolios
          </Typography>
          <Typography color="text.secondary">
            Consolidate Missions for capacity and workload planning (US-109). Completes EPIC-01
            Advanced Planning.
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          component={RouterLink}
          to="/portfolios/new"
        >
          New Portfolio
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
            label="Status"
            size="small"
            value={status}
            onChange={(event) => setStatus(event.target.value)}
            sx={{ minWidth: 160 }}
          >
            <MenuItem value="">All</MenuItem>
            <MenuItem value="Active">Active</MenuItem>
            <MenuItem value="Inactive">Inactive</MenuItem>
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
                <TableCell>Name</TableCell>
                <TableCell>Period</TableCell>
                <TableCell>Missions</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Health</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {items.map((item) => (
                <TableRow key={item.id} hover>
                  <TableCell>
                    <Button
                      component={RouterLink}
                      to={`/portfolios/${item.id}`}
                      size="small"
                      sx={{ textTransform: 'none' }}
                    >
                      {item.name}
                    </Button>
                  </TableCell>
                  <TableCell>
                    {item.planningPeriodStart} → {item.planningPeriodEnd}
                  </TableCell>
                  <TableCell>{item.missions.length}</TableCell>
                  <TableCell>
                    <PortfolioStatusBadge status={item.status} />
                  </TableCell>
                  <TableCell>
                    <PortfolioHealthBadge health={item.portfolioHealth} />
                  </TableCell>
                  <TableCell align="right">
                    <Button size="small" onClick={() => navigate(`/portfolios/${item.id}`)}>
                      Open
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
              {items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={6}>
                    <Typography color="text.secondary" sx={{ py: 2 }}>
                      No portfolios found.
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
