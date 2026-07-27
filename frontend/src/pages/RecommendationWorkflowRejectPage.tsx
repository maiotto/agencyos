import { useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import {
  getRecommendationWorkflow,
  rejectRecommendationWorkflow,
} from '../api/recommendationWorkflow'
import { getErrorMessage } from '../api/client'
import { RecommendationStatusBadge } from '../components/RecommendationStatusBadge'
import type { RecommendationWorkflow } from '../types/recommendationWorkflow'

export function RecommendationWorkflowRejectPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [item, setItem] = useState<RecommendationWorkflow | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [actor, setActor] = useState('approver@agencyos.local')
  const [comment, setComment] = useState('')

  useEffect(() => {
    if (!id) return
    let cancelled = false
    const load = async () => {
      setLoading(true)
      try {
        const workflow = await getRecommendationWorkflow(id)
        if (!cancelled) {
          setItem(workflow)
          if (workflow.status !== 'PendingApproval') {
            setError('Only Pending Approval recommendations can be rejected.')
          }
        }
      } catch (err) {
        if (!cancelled) setError(getErrorMessage(err, 'Failed to load workflow.'))
      } finally {
        if (!cancelled) setLoading(false)
      }
    }
    void load()
    return () => {
      cancelled = true
    }
  }, [id])

  const onReject = async () => {
    if (!id) return
    setSaving(true)
    setError(null)
    try {
      await rejectRecommendationWorkflow(id, {
        actor: actor.trim(),
        comment: comment.trim() || undefined,
      })
      navigate(`/recommendations/workflow/${id}`)
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to reject recommendation.'))
    } finally {
      setSaving(false)
    }
  }

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" py={8}>
        <CircularProgress />
      </Box>
    )
  }

  return (
    <Stack spacing={3}>
      <Box>
        <Button component={RouterLink} to={`/recommendations/workflow/${id}`} size="small" sx={{ mb: 1 }}>
          ← Back to detail
        </Button>
        <Typography variant="h4" gutterBottom>
          Reject Recommendation
        </Typography>
        {item && (
          <Stack direction="row" spacing={1} alignItems="center">
            <Typography>{item.title}</Typography>
            <RecommendationStatusBadge status={item.status} />
          </Stack>
        )}
      </Box>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 3 }}>
        <Stack spacing={2}>
          <TextField
            label="Actor"
            required
            value={actor}
            onChange={(event) => setActor(event.target.value)}
          />
          <TextField
            label="Comment"
            value={comment}
            onChange={(event) => setComment(event.target.value)}
            multiline
            minRows={3}
            helperText="Optional but recommended for rejection rationale."
          />
          <Stack direction="row" spacing={2}>
            <Button
              variant="contained"
              color="warning"
              disabled={saving || item?.status !== 'PendingApproval'}
              onClick={() => void onReject()}
            >
              {saving ? 'Rejecting…' : 'Confirm Rejection'}
            </Button>
            <Button component={RouterLink} to={`/recommendations/workflow/${id}`}>
              Cancel
            </Button>
          </Stack>
        </Stack>
      </Paper>
    </Stack>
  )
}
