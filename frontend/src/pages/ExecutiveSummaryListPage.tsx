import { useCallback, useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  FormControlLabel,
  LinearProgress,
  MenuItem,
  Paper,
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
import { listExecutiveSummaries } from '../api/executiveSummary'
import { getErrorMessage } from '../api/client'
import type { ExecutiveRecommendationSummary } from '../types/executiveSummary'

export function ExecutiveSummaryListPage() {
  const navigate = useNavigate()
  const [status, setStatus] = useState('')
  const [search, setSearch] = useState('')
  const [includeArchived, setIncludeArchived] = useState(true)
  const [items, setItems] = useState<ExecutiveRecommendationSummary[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      setItems(
        await listExecutiveSummaries({
          status: status || undefined,
          search: search || undefined,
          includeArchived,
          orderBy: 'generatedAt',
          orderDirection: 'desc',
        }),
      )
    } catch (err) {
      setItems([])
      setError(getErrorMessage(err, 'Failed to load executive summaries.'))
    } finally {
      setLoading(false)
    }
  }, [status, search, includeArchived])

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
            Executive Recommendation Summaries
          </Typography>
          <Typography color="text.secondary">
            Concise executive briefings consolidating Recommendation, AI, Explainability, and
            Decision context (US-303). Completes EPIC-03.
          </Typography>
        </Box>
        <Button component={RouterLink} to="/executive-summaries/generate" variant="contained">
          Generate
        </Button>
      </Stack>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} useFlexGap flexWrap="wrap" alignItems="center">
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
            label="Search"
            size="small"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            sx={{ minWidth: 220 }}
          />
          <FormControlLabel
            control={
              <Switch
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
                <TableCell>Generated</TableCell>
                <TableCell>Summary</TableCell>
                <TableCell>Confidence</TableCell>
                <TableCell>Version</TableCell>
                <TableCell>Status</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {items.map((item) => (
                <TableRow key={item.id} hover>
                  <TableCell>{new Date(item.generatedAt).toLocaleString()}</TableCell>
                  <TableCell sx={{ maxWidth: 360 }}>
                    <Typography variant="body2" noWrap>
                      {item.executiveSummary}
                    </Typography>
                  </TableCell>
                  <TableCell sx={{ minWidth: 140 }}>
                    <Typography variant="body2">{item.confidenceLevel.toFixed(1)}%</Typography>
                    <LinearProgress
                      variant="determinate"
                      value={Math.min(100, Math.max(0, item.confidenceLevel))}
                      sx={{ height: 6, borderRadius: 1 }}
                    />
                  </TableCell>
                  <TableCell>v{item.summaryVersion}</TableCell>
                  <TableCell>{item.status}</TableCell>
                  <TableCell align="right">
                    <Button size="small" onClick={() => navigate(`/executive-summaries/${item.id}`)}>
                      Open
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
              {items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={6}>
                    <Typography color="text.secondary" sx={{ py: 2 }}>
                      No executive summaries found.
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
