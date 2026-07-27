import { useCallback, useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  Chip,
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
import { listRecommendationHistory } from '../api/recommendationHistory'
import { getErrorMessage } from '../api/client'
import type { RecommendationHistory } from '../types/recommendationHistory'
import { DEFAULT_COMPANY_ID } from '../theme'

const EVENT_COLOR: Record<string, 'default' | 'success' | 'warning' | 'info' | 'error'> = {
  VersionCreated: 'success',
  Archived: 'warning',
  Restored: 'info',
  WorkflowTransition: 'default',
}

export function RecommendationHistoryListPage() {
  const navigate = useNavigate()
  const [companyId, setCompanyId] = useState(DEFAULT_COMPANY_ID)
  const [search, setSearch] = useState('')
  const [eventType, setEventType] = useState('')
  const [workflowStatus, setWorkflowStatus] = useState('')
  const [leftCompareId, setLeftCompareId] = useState('')
  const [rightCompareId, setRightCompareId] = useState('')
  const [items, setItems] = useState<RecommendationHistory[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await listRecommendationHistory({
        companyId: companyId || undefined,
        search: search || undefined,
        eventType: eventType || undefined,
        workflowStatus: workflowStatus || undefined,
        orderBy: 'createdAt',
        orderDirection: 'desc',
      })
      setItems(data)
    } catch (err) {
      setItems([])
      setError(getErrorMessage(err, 'Failed to load recommendation history.'))
    } finally {
      setLoading(false)
    }
  }, [companyId, search, eventType, workflowStatus])

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
            Recommendation History
          </Typography>
          <Typography color="text.secondary">
            Immutable version and workflow timeline for persisted recommendations (US-203).
          </Typography>
        </Box>
        <Stack direction="row" spacing={1}>
          <Button component={RouterLink} to="/recommendations" variant="outlined">
            Recommendations
          </Button>
          <Button component={RouterLink} to="/recommendations/compare" variant="contained">
            Compare
          </Button>
        </Stack>
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
            label="Event Type"
            size="small"
            value={eventType}
            onChange={(event) => setEventType(event.target.value)}
            sx={{ minWidth: 180 }}
          >
            <MenuItem value="">All</MenuItem>
            <MenuItem value="VersionCreated">VersionCreated</MenuItem>
            <MenuItem value="Archived">Archived</MenuItem>
            <MenuItem value="Restored">Restored</MenuItem>
            <MenuItem value="WorkflowTransition">WorkflowTransition</MenuItem>
          </TextField>
          <TextField
            label="Workflow Status"
            size="small"
            value={workflowStatus}
            onChange={(event) => setWorkflowStatus(event.target.value)}
            sx={{ minWidth: 160 }}
          />
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

      <Paper sx={{ p: 2 }}>
        <Typography variant="subtitle1" gutterBottom>
          Compare snapshots
        </Typography>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} useFlexGap flexWrap="wrap">
          <TextField
            label="Left History Id"
            size="small"
            value={leftCompareId}
            onChange={(event) => setLeftCompareId(event.target.value)}
            sx={{ minWidth: 280 }}
          />
          <TextField
            label="Right History Id"
            size="small"
            value={rightCompareId}
            onChange={(event) => setRightCompareId(event.target.value)}
            sx={{ minWidth: 280 }}
          />
          <Button
            variant="outlined"
            disabled={!leftCompareId || !rightCompareId || leftCompareId === rightCompareId}
            component={RouterLink}
            to={`/recommendations/compare?mode=ids&leftId=${encodeURIComponent(leftCompareId)}&rightId=${encodeURIComponent(rightCompareId)}`}
          >
            Open comparison
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
                <TableCell>When</TableCell>
                <TableCell>Number</TableCell>
                <TableCell>Version</TableCell>
                <TableCell>Event</TableCell>
                <TableCell>Workflow</TableCell>
                <TableCell>Title</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {items.map((item) => (
                <TableRow key={item.id} hover>
                  <TableCell>{new Date(item.createdAt).toLocaleString()}</TableCell>
                  <TableCell>{item.recommendationNumber}</TableCell>
                  <TableCell>v{item.recommendationVersion}</TableCell>
                  <TableCell>
                    <Chip
                      size="small"
                      label={item.eventType}
                      color={EVENT_COLOR[item.eventType] ?? 'default'}
                      variant="outlined"
                    />
                  </TableCell>
                  <TableCell>{item.workflowStatus ?? '—'}</TableCell>
                  <TableCell>{item.title}</TableCell>
                  <TableCell align="right">
                    <Button
                      size="small"
                      onClick={() => navigate(`/recommendations/history/${item.id}`)}
                    >
                      Open
                    </Button>
                    <Button
                      size="small"
                      onClick={() =>
                        navigate(`/recommendations/history/timeline/${item.recommendationId}`)
                      }
                    >
                      Timeline
                    </Button>
                    <Button
                      size="small"
                      onClick={() => {
                        if (!leftCompareId || leftCompareId === rightCompareId) {
                          setLeftCompareId(item.id)
                        } else {
                          setRightCompareId(item.id)
                        }
                      }}
                    >
                      Use in compare
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
              {items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={7}>
                    <Typography color="text.secondary" sx={{ py: 2 }}>
                      No recommendation history found.
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
