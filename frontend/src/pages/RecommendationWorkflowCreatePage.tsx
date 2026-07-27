import { useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate, useSearchParams } from 'react-router-dom'
import {
  Alert,
  Button,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { createRecommendationWorkflow } from '../api/recommendationWorkflow'
import { getRecommendation } from '../api/recommendation'
import { getErrorMessage } from '../api/client'

export function RecommendationWorkflowCreatePage() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [previewTitle, setPreviewTitle] = useState('')

  const [recommendationId, setRecommendationId] = useState(
    searchParams.get('recommendationId') ?? '',
  )
  const [createdBy, setCreatedBy] = useState('planner@agencyos.local')
  const [title, setTitle] = useState('')
  const [summary, setSummary] = useState('')

  useEffect(() => {
    const id = recommendationId.trim()
    if (!id) {
      setPreviewTitle('')
      return
    }

    let cancelled = false
    void getRecommendation(id)
      .then((recommendation) => {
        if (cancelled) return
        setPreviewTitle(recommendation.title)
        if (!title) setTitle(recommendation.title)
        if (!summary && recommendation.summary) setSummary(recommendation.summary)
      })
      .catch(() => {
        if (!cancelled) setPreviewTitle('')
      })

    return () => {
      cancelled = true
    }
  }, [recommendationId])

  const onSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
    setSaving(true)
    setError(null)

    try {
      const created = await createRecommendationWorkflow({
        recommendationId: recommendationId.trim(),
        createdBy: createdBy.trim(),
        title: title.trim() || undefined,
        summary: summary.trim() || undefined,
      })
      navigate(`/recommendations/workflow/${created.id}`)
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to create recommendation workflow.'))
    } finally {
      setSaving(false)
    }
  }

  return (
    <Stack spacing={3} component="form" onSubmit={onSubmit}>
      <Stack spacing={1}>
        <Button component={RouterLink} to="/recommendations/workflow" size="small">
          ← Recommendation Workflow
        </Button>
        <Typography variant="h4">Create Recommendation Workflow</Typography>
        <Typography color="text.secondary">
          Attach governance to a persisted Recommendation (US-202). This does not generate
          strategies.
        </Typography>
      </Stack>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 3 }}>
        <Stack spacing={2}>
          <TextField
            label="Recommendation Id"
            required
            value={recommendationId}
            onChange={(event) => setRecommendationId(event.target.value)}
            helperText={previewTitle ? `Loaded: ${previewTitle}` : 'Paste a persisted recommendation id'}
          />
          <TextField
            label="Title override (optional)"
            value={title}
            onChange={(event) => setTitle(event.target.value)}
          />
          <TextField
            label="Summary override (optional)"
            value={summary}
            onChange={(event) => setSummary(event.target.value)}
            multiline
            minRows={2}
          />
          <TextField
            label="Created By"
            required
            value={createdBy}
            onChange={(event) => setCreatedBy(event.target.value)}
          />
          <Stack direction="row" spacing={1}>
            <Button type="submit" variant="contained" disabled={saving}>
              {saving ? 'Creating…' : 'Create Workflow'}
            </Button>
            <Button component={RouterLink} to="/recommendations/workflow">
              Cancel
            </Button>
          </Stack>
        </Stack>
      </Paper>
    </Stack>
  )
}
