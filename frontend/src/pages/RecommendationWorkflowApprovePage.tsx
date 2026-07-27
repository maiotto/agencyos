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
  approveRecommendationWorkflow,
  getRecommendationWorkflow,
} from '../api/recommendationWorkflow'
import { getErrorMessage } from '../api/client'
import { RecommendationStatusBadge } from '../components/RecommendationStatusBadge'
import type { RecommendationWorkflow } from '../types/recommendationWorkflow'

export function RecommendationWorkflowApprovePage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [item, setItem] = useState<RecommendationWorkflow | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [approver, setApprover] = useState('approver@agencyos.local')
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
            setError('Only Pending Approval recommendations can be approved.')
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

  const onApprove = async () => {
    if (!id) return
    setSaving(true)
    setError(null)
    try {
      await approveRecommendationWorkflow(id, {
        approver: approver.trim(),
        comment: comment.trim() || undefined,
      })
      navigate(`/recommendations/workflow/${id}`)
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to approve recommendation.'))
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
          Approve Recommendation
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
            label="Approver"
            required
            value={approver}
            onChange={(event) => setApprover(event.target.value)}
          />
          <TextField
            label="Comment (optional)"
            value={comment}
            onChange={(event) => setComment(event.target.value)}
            multiline
            minRows={3}
          />
          <Stack direction="row" spacing={2}>
            <Button
              variant="contained"
              color="success"
              disabled={saving || item?.status !== 'PendingApproval'}
              onClick={() => void onApprove()}
            >
              {saving ? 'Approving…' : 'Confirm Approval'}
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
