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
import { listExplainability } from '../api/explainability'
import { getErrorMessage } from '../api/client'
import type { Explainability } from '../types/explainability'

export function ExplainabilityListPage() {
  const navigate = useNavigate()
  const [status, setStatus] = useState('Active')
  const [explanationType, setExplanationType] = useState('')
  const [search, setSearch] = useState('')
  const [items, setItems] = useState<Explainability[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      setItems(
        await listExplainability({
          status: status || undefined,
          explanationType: explanationType || undefined,
          search: search || undefined,
          orderBy: 'generatedAt',
          orderDirection: 'desc',
        }),
      )
    } catch (err) {
      setItems([])
      setError(getErrorMessage(err, 'Failed to load explainability records.'))
    } finally {
      setLoading(false)
    }
  }, [status, explanationType, search])

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
            LLM Explainability
          </Typography>
          <Typography color="text.secondary">
            Natural-language explanations for Recommendations and AI Recommendations (US-302).
            Informational only — never changes Decision Engine output.
          </Typography>
        </Box>
        <Button component={RouterLink} to="/explainability/generate" variant="contained">
          Generate
        </Button>
      </Stack>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} useFlexGap flexWrap="wrap">
          <TextField
            select
            label="Status"
            size="small"
            value={status}
            onChange={(event) => setStatus(event.target.value)}
            sx={{ minWidth: 140 }}
          >
            <MenuItem value="">All</MenuItem>
            <MenuItem value="Active">Active</MenuItem>
            <MenuItem value="Archived">Archived</MenuItem>
          </TextField>
          <TextField
            select
            label="Type"
            size="small"
            value={explanationType}
            onChange={(event) => setExplanationType(event.target.value)}
            sx={{ minWidth: 180 }}
          >
            <MenuItem value="">All</MenuItem>
            <MenuItem value="Recommendation">Recommendation</MenuItem>
            <MenuItem value="AIRecommendation">AI Recommendation</MenuItem>
          </TextField>
          <TextField
            label="Search"
            size="small"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            sx={{ minWidth: 220 }}
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
                <TableCell>Generated</TableCell>
                <TableCell>Type</TableCell>
                <TableCell>Summary</TableCell>
                <TableCell>Version</TableCell>
                <TableCell>Status</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {items.map((item) => (
                <TableRow key={item.id} hover>
                  <TableCell>{new Date(item.generatedAt).toLocaleString()}</TableCell>
                  <TableCell>{item.explanationType}</TableCell>
                  <TableCell sx={{ maxWidth: 360 }}>
                    <Typography variant="body2" noWrap>
                      {item.executiveSummary}
                    </Typography>
                  </TableCell>
                  <TableCell>g{item.generationVersion}</TableCell>
                  <TableCell>{item.status}</TableCell>
                  <TableCell align="right">
                    <Button size="small" onClick={() => navigate(`/explainability/${item.id}`)}>
                      Open
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
              {items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={6}>
                    <Typography color="text.secondary" sx={{ py: 2 }}>
                      No explainability records found.
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
