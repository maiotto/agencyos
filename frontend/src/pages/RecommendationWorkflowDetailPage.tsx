import { useCallback, useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  CircularProgress,
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
import {
  cancelRecommendationWorkflow,
  getRecommendationWorkflow,
  reopenRecommendationWorkflow,
  submitRecommendationWorkflow,
} from '../api/recommendationWorkflow'
import { getErrorMessage } from '../api/client'
import { RecommendationStatusBadge } from '../components/RecommendationStatusBadge'
import type { RecommendationWorkflow } from '../types/recommendationWorkflow'

export function RecommendationWorkflowDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [item, setItem] = useState<RecommendationWorkflow | null>(null)
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [actor, setActor] = useState('approver@agencyos.local')
  const [comment, setComment] = useState('')

  const load = useCallback(async () => {
    if (!id) return
    setLoading(true)
    setError(null)
    try {
      setItem(await getRecommendationWorkflow(id))
    } catch (err) {
      setItem(null)
      setError(getErrorMessage(err, 'Failed to load workflow.'))
    } finally {
      setLoading(false)
    }
  }, [id])

  useEffect(() => {
    void load()
  }, [load])

  const run = async (action: () => Promise<RecommendationWorkflow>) => {
    setBusy(true)
    setError(null)
    try {
      setItem(await action())
      setComment('')
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" py={8}>
        <CircularProgress />
      </Box>
    )
  }

  if (!item) {
    return (
      <Stack spacing={2}>
        <Alert severity="error">{error ?? 'Workflow not found.'}</Alert>
        <Button component={RouterLink} to="/recommendations/workflow" variant="outlined">
          Back to list
        </Button>
      </Stack>
    )
  }

  return (
    <Stack spacing={3}>
      <Box>
        <Button component={RouterLink} to="/recommendations/workflow" size="small" sx={{ mb: 1 }}>
          ← Recommendation Workflow
        </Button>
        <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" spacing={2}>
          <Box>
            <Typography variant="h4" gutterBottom>
              {item.title}
            </Typography>
            <RecommendationStatusBadge status={item.status} />
          </Box>
          <Stack direction="row" spacing={1} flexWrap="wrap">
            {(item.status === 'Draft' || item.status === 'Reopened') && (
              <Button
                variant="contained"
                disabled={busy}
                onClick={() =>
                  void run(() =>
                    submitRecommendationWorkflow(item.id, { actor, comment: comment || undefined }),
                  )
                }
              >
                Submit
              </Button>
            )}
            {item.status === 'PendingApproval' && (
              <>
                <Button
                  variant="contained"
                  color="success"
                  onClick={() => navigate(`/recommendations/workflow/${item.id}/approve`)}
                >
                  Approve
                </Button>
                <Button
                  variant="outlined"
                  color="warning"
                  onClick={() => navigate(`/recommendations/workflow/${item.id}/reject`)}
                >
                  Reject
                </Button>
              </>
            )}
            {(item.status === 'Draft' ||
              item.status === 'PendingApproval' ||
              item.status === 'Reopened') && (
              <Button
                color="error"
                disabled={busy}
                onClick={() =>
                  void run(() =>
                    cancelRecommendationWorkflow(item.id, { actor, comment: comment || undefined }),
                  )
                }
              >
                Cancel
              </Button>
            )}
            {item.status === 'Rejected' && (
              <Button
                variant="outlined"
                disabled={busy}
                onClick={() =>
                  void run(() =>
                    reopenRecommendationWorkflow(item.id, { actor, comment: comment || undefined }),
                  )
                }
              >
                Reopen
              </Button>
            )}
          </Stack>
        </Stack>
      </Box>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 3 }}>
        <Stack spacing={1.5}>
          <Typography variant="body2">Summary: {item.summary || '—'}</Typography>
          <Typography variant="body2">Recommendation Id: {item.recommendationId}</Typography>
          <Typography variant="body2">Delivery Strategy Id: {item.deliveryStrategyId}</Typography>
          <Typography variant="body2">Contract: {item.contractId}</Typography>
          <Typography variant="body2">Mission: {item.missionId}</Typography>
          <Typography variant="body2">Created by: {item.createdBy}</Typography>
          {item.approver && (
            <Typography variant="body2">
              Approver: {item.approver} ({item.approvalDate ? new Date(item.approvalDate).toLocaleString() : '—'})
            </Typography>
          )}
          {item.approvalComment && (
            <Typography variant="body2">Approval comment: {item.approvalComment}</Typography>
          )}
        </Stack>
      </Paper>

      <Paper sx={{ p: 2 }}>
        <Typography variant="subtitle1" gutterBottom>
          Action actor / comment
        </Typography>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
          <TextField
            label="Actor"
            size="small"
            value={actor}
            onChange={(event) => setActor(event.target.value)}
            sx={{ minWidth: 260 }}
          />
          <TextField
            label="Comment"
            size="small"
            fullWidth
            value={comment}
            onChange={(event) => setComment(event.target.value)}
          />
        </Stack>
      </Paper>

      <Box>
        <Typography variant="h6" gutterBottom>
          Approval Timeline
        </Typography>
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>When</TableCell>
                <TableCell>From</TableCell>
                <TableCell>To</TableCell>
                <TableCell>Actor</TableCell>
                <TableCell>Comment</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {item.transitions.map((transition) => (
                <TableRow key={transition.id}>
                  <TableCell>{new Date(transition.occurredAt).toLocaleString()}</TableCell>
                  <TableCell>{transition.fromStatus}</TableCell>
                  <TableCell>{transition.toStatus}</TableCell>
                  <TableCell>{transition.actor}</TableCell>
                  <TableCell>{transition.comment ?? '—'}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Box>
    </Stack>
  )
}
