import { useCallback, useEffect, useState } from 'react'
import {
  Alert,
  Box,
  Button,
  Checkbox,
  CircularProgress,
  FormControlLabel,
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
import { Link as RouterLink, useNavigate } from 'react-router-dom'
import { listRecommendations } from '../api/recommendation'
import { getErrorMessage } from '../api/client'
import { RecommendationPersistenceStatusBadge } from '../components/RecommendationPersistenceStatusBadge'
import type { Recommendation } from '../types/recommendation'
import { DEFAULT_COMPANY_ID } from '../theme'

export function RecommendationListPage() {
  const navigate = useNavigate()
  const [companyId, setCompanyId] = useState(DEFAULT_COMPANY_ID)
  const [missionId, setMissionId] = useState('')
  const [contractId, setContractId] = useState('')
  const [status, setStatus] = useState('')
  const [search, setSearch] = useState('')
  const [includeArchived, setIncludeArchived] = useState(false)
  const [items, setItems] = useState<Recommendation[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await listRecommendations({
        companyId: companyId || undefined,
        missionId: missionId || undefined,
        contractId: contractId || undefined,
        status: status || undefined,
        search: search || undefined,
        includeArchived,
        orderBy: 'generatedAt',
        orderDirection: 'desc',
      })
      setItems(data)
    } catch (err) {
      setItems([])
      setError(getErrorMessage(err, 'Failed to load recommendations.'))
    } finally {
      setLoading(false)
    }
  }, [companyId, missionId, contractId, status, search, includeArchived])

  useEffect(() => {
    void load()
  }, [load])

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
            Recommendations
          </Typography>
          <Typography color="text.secondary">
            Persisted Decision Engine recommendations (US-202). Immutable snapshots with versioning
            and archival.
          </Typography>
        </Box>
        <Stack direction="row" spacing={1}>
          <Button component={RouterLink} to="/recommendations/workflow" variant="outlined">
            Workflow
          </Button>
        </Stack>
      </Stack>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} useFlexGap flexWrap="wrap">
          <TextField
            label="Company Id"
            value={companyId}
            onChange={(event) => setCompanyId(event.target.value)}
            size="small"
            sx={{ minWidth: 280 }}
          />
          <TextField
            label="Mission Id"
            value={missionId}
            onChange={(event) => setMissionId(event.target.value)}
            size="small"
            sx={{ minWidth: 280 }}
          />
          <TextField
            label="Contract Id"
            value={contractId}
            onChange={(event) => setContractId(event.target.value)}
            size="small"
            sx={{ minWidth: 280 }}
          />
          <TextField
            select
            label="Status"
            value={status}
            onChange={(event) => setStatus(event.target.value)}
            size="small"
            sx={{ minWidth: 160 }}
          >
            <MenuItem value="">All</MenuItem>
            <MenuItem value="Active">Active</MenuItem>
            <MenuItem value="Archived">Archived</MenuItem>
          </TextField>
          <TextField
            label="Search"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            size="small"
            sx={{ minWidth: 200 }}
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={includeArchived}
                onChange={(event) => setIncludeArchived(event.target.checked)}
              />
            }
            label="Include archived"
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
                <TableCell>Number</TableCell>
                <TableCell>Title</TableCell>
                <TableCell>Rank</TableCell>
                <TableCell>Score</TableCell>
                <TableCell>Version</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Generated</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {items.map((item) => (
                <TableRow key={item.id} hover>
                  <TableCell>
                    <Button
                      component={RouterLink}
                      to={`/recommendations/${item.id}`}
                      size="small"
                      sx={{ textTransform: 'none' }}
                    >
                      {item.recommendationNumber}
                    </Button>
                  </TableCell>
                  <TableCell>{item.title}</TableCell>
                  <TableCell>{item.rank ?? '—'}</TableCell>
                  <TableCell>{item.score ?? '—'}</TableCell>
                  <TableCell>v{item.version}</TableCell>
                  <TableCell>
                    <RecommendationPersistenceStatusBadge status={item.status} />
                  </TableCell>
                  <TableCell>{new Date(item.generatedAt).toLocaleString()}</TableCell>
                  <TableCell align="right">
                    <Button
                      size="small"
                      onClick={() => navigate(`/recommendations/${item.id}`)}
                    >
                      Open
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
              {items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={8}>
                    <Typography color="text.secondary" sx={{ py: 2 }}>
                      No recommendations found.
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
