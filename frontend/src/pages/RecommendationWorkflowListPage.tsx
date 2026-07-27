import { useCallback, useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
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
import { listRecommendationWorkflows } from '../api/recommendationWorkflow'
import { getErrorMessage } from '../api/client'
import { RecommendationStatusBadge } from '../components/RecommendationStatusBadge'
import type { RecommendationWorkflow } from '../types/recommendationWorkflow'

export function RecommendationWorkflowListPage() {
  const navigate = useNavigate()
  const [items, setItems] = useState<RecommendationWorkflow[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await listRecommendationWorkflows({
        search: search || undefined,
        status: status || undefined,
        orderBy: 'createdAt',
        orderDirection: 'desc',
      })
      setItems(data)
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to load recommendation workflows.'))
    } finally {
      setLoading(false)
    }
  }, [search, status])

  useEffect(() => {
    void load()
  }, [load])

  return (
    <Stack spacing={3}>
      <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" spacing={2}>
        <Box>
          <Typography variant="h4">Recommendation Workflow</Typography>
          <Typography color="text.secondary">
            Governance layer for Decision Engine recommendations (US-201).
          </Typography>
        </Box>
        <Button
          component={RouterLink}
          to="/recommendations/workflow/new"
          variant="contained"
          startIcon={<AddIcon />}
        >
          Submit Recommendation
        </Button>
      </Stack>

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          <TextField
            label="Search"
            size="small"
            fullWidth
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
          <FormControl size="small" sx={{ minWidth: 180 }}>
            <InputLabel>Status</InputLabel>
            <Select
              label="Status"
              value={status}
              onChange={(event) => setStatus(event.target.value)}
            >
              <MenuItem value="">All</MenuItem>
              <MenuItem value="Draft">Draft</MenuItem>
              <MenuItem value="PendingApproval">Pending Approval</MenuItem>
              <MenuItem value="Approved">Approved</MenuItem>
              <MenuItem value="Rejected">Rejected</MenuItem>
              <MenuItem value="Cancelled">Cancelled</MenuItem>
              <MenuItem value="Reopened">Reopened</MenuItem>
            </Select>
          </FormControl>
          <Button variant="outlined" onClick={() => void load()}>
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
                <TableCell>Title</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Created By</TableCell>
                <TableCell>Updated</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {items.map((item) => (
                <TableRow key={item.id} hover>
                  <TableCell>
                    <Button
                      component={RouterLink}
                      to={`/recommendations/workflow/${item.id}`}
                      size="small"
                    >
                      {item.title}
                    </Button>
                  </TableCell>
                  <TableCell>
                    <RecommendationStatusBadge status={item.status} />
                  </TableCell>
                  <TableCell>{item.createdBy}</TableCell>
                  <TableCell>{new Date(item.updatedAt).toLocaleString()}</TableCell>
                  <TableCell align="right">
                    <Stack direction="row" spacing={1} justifyContent="flex-end">
                      {item.status === 'PendingApproval' && (
                        <>
                          <Button
                            size="small"
                            onClick={() =>
                              navigate(`/recommendations/workflow/${item.id}/approve`)
                            }
                          >
                            Approve
                          </Button>
                          <Button
                            size="small"
                            onClick={() =>
                              navigate(`/recommendations/workflow/${item.id}/reject`)
                            }
                          >
                            Reject
                          </Button>
                        </>
                      )}
                      <Button
                        size="small"
                        component={RouterLink}
                        to={`/recommendations/workflow/${item.id}`}
                      >
                        Detail
                      </Button>
                    </Stack>
                  </TableCell>
                </TableRow>
              ))}
              {items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={5}>
                    <Typography color="text.secondary" py={2}>
                      No recommendation workflows found.
                    </Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Stack>
  )
}
