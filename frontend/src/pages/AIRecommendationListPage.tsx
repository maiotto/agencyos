import { useCallback, useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  LinearProgress,
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
import { listAIRecommendations } from '../api/aiRecommendation'
import { getErrorMessage } from '../api/client'
import type { AIRecommendation } from '../types/aiRecommendation'

export function AIRecommendationListPage() {
  const navigate = useNavigate()
  const [status, setStatus] = useState('Active')
  const [search, setSearch] = useState('')
  const [items, setItems] = useState<AIRecommendation[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      setItems(
        await listAIRecommendations({
          status: status || undefined,
          search: search || undefined,
          orderBy: 'generatedAt',
          orderDirection: 'desc',
        }),
      )
    } catch (err) {
      setItems([])
      setError(getErrorMessage(err, 'Failed to load AI recommendations.'))
    } finally {
      setLoading(false)
    }
  }, [status, search])

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
            AI-assisted Recommendations
          </Typography>
          <Typography color="text.secondary">
            Advisory alternatives with confidence, assumptions, and risks (US-301). Human approval
            remains mandatory.
          </Typography>
        </Box>
        <Button component={RouterLink} to="/ai-recommendations/generate" variant="contained">
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
                <TableCell>Strategy</TableCell>
                <TableCell>Confidence</TableCell>
                <TableCell>Generation</TableCell>
                <TableCell>Status</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {items.map((item) => (
                <TableRow key={item.id} hover>
                  <TableCell>{new Date(item.generatedAt).toLocaleString()}</TableCell>
                  <TableCell>{item.suggestedDeliveryStrategy}</TableCell>
                  <TableCell sx={{ minWidth: 140 }}>
                    <Typography variant="body2">{item.confidenceScore.toFixed(1)}%</Typography>
                    <LinearProgress
                      variant="determinate"
                      value={Math.min(100, Math.max(0, item.confidenceScore))}
                      sx={{ height: 6, borderRadius: 1 }}
                    />
                  </TableCell>
                  <TableCell>g{item.generationVersion}</TableCell>
                  <TableCell>{item.status}</TableCell>
                  <TableCell align="right">
                    <Button size="small" onClick={() => navigate(`/ai-recommendations/${item.id}`)}>
                      Open
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
              {items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={6}>
                    <Typography color="text.secondary" sx={{ py: 2 }}>
                      No AI recommendations found.
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
