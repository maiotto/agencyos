import { useState } from 'react'
import { Link as RouterLink, useNavigate, useSearchParams } from 'react-router-dom'
import { Alert, Box, Button, Paper, Stack, TextField, Typography } from '@mui/material'
import { createDecision } from '../api/decision'
import { getErrorMessage } from '../api/client'

export function DecisionCreatePage() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const [recommendationId, setRecommendationId] = useState(
    searchParams.get('recommendationId') ?? '',
  )
  const [createdBy, setCreatedBy] = useState('planner@agencyos.local')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const submit = async () => {
    setBusy(true)
    setError(null)
    try {
      const created = await createDecision({
        recommendationId,
        createdBy,
      })
      navigate(`/decisions/${created.id}`)
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to create decision.'))
    } finally {
      setBusy(false)
    }
  }

  return (
    <Stack spacing={3} maxWidth={640}>
      <Box>
        <Button component={RouterLink} to="/decisions" size="small" sx={{ mb: 1 }}>
          ← Decisions
        </Button>
        <Typography variant="h4" gutterBottom>
          Create Decision
        </Typography>
        <Typography color="text.secondary">
          Link a Decision to an Approved Recommendation (BR-1401). One Decision per Recommendation.
        </Typography>
      </Box>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 3 }}>
        <Stack spacing={2}>
          <TextField
            label="Recommendation Id"
            value={recommendationId}
            onChange={(event) => setRecommendationId(event.target.value)}
            required
            fullWidth
          />
          <TextField
            label="Created By"
            value={createdBy}
            onChange={(event) => setCreatedBy(event.target.value)}
            required
            fullWidth
          />
          <Stack direction="row" spacing={1}>
            <Button variant="contained" disabled={busy || !recommendationId || !createdBy} onClick={() => void submit()}>
              Create
            </Button>
            <Button component={RouterLink} to="/decisions" disabled={busy}>
              Cancel
            </Button>
          </Stack>
        </Stack>
      </Paper>
    </Stack>
  )
}
