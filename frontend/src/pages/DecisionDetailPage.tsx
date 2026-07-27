import { useCallback, useEffect, useState } from 'react'
import { Link as RouterLink, useParams } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  LinearProgress,
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
  cancelDecision,
  completeDecision,
  getDecision,
  recordDecisionOutcome,
  startDecision,
} from '../api/decision'
import { getErrorMessage } from '../api/client'
import { DecisionStatusBadge, ImplementationStatusBadge } from '../components/DecisionStatusBadge'
import type { Decision } from '../types/decision'

function implementationProgress(status: string): number {
  switch (status) {
    case 'NotStarted':
      return 5
    case 'InProgress':
      return 55
    case 'Completed':
      return 100
    case 'Cancelled':
      return 0
    default:
      return 0
  }
}

export function DecisionDetailPage() {
  const { id } = useParams<{ id: string }>()
  const [item, setItem] = useState<Decision | null>(null)
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [actor, setActor] = useState('planner@agencyos.local')
  const [comment, setComment] = useState('')
  const [outcome, setOutcome] = useState('')
  const [businessValue, setBusinessValue] = useState('')

  const load = useCallback(async () => {
    if (!id) return
    setLoading(true)
    setError(null)
    try {
      const decision = await getDecision(id)
      setItem(decision)
      setOutcome(decision.outcome ?? '')
      setBusinessValue(decision.businessValue ?? '')
    } catch (err) {
      setItem(null)
      setError(getErrorMessage(err, 'Failed to load decision.'))
    } finally {
      setLoading(false)
    }
  }, [id])

  useEffect(() => {
    void load()
  }, [load])

  const run = async (action: () => Promise<Decision>) => {
    setBusy(true)
    setError(null)
    try {
      const updated = await action()
      setItem(updated)
      setComment('')
      setOutcome(updated.outcome ?? '')
      setBusinessValue(updated.businessValue ?? '')
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
        <Alert severity="error">{error ?? 'Decision not found.'}</Alert>
        <Button component={RouterLink} to="/decisions" variant="outlined">
          Back to list
        </Button>
      </Stack>
    )
  }

  const canStart = item.decisionStatus === 'Created' && item.implementationStatus === 'NotStarted'
  const canComplete =
    item.decisionStatus === 'InProgress' && item.implementationStatus === 'InProgress'
  const canCancel =
    item.decisionStatus === 'Created' || item.decisionStatus === 'InProgress'
  const canRecordOutcome = item.decisionStatus === 'Completed'

  return (
    <Stack spacing={3}>
      <Box>
        <Button component={RouterLink} to="/decisions" size="small" sx={{ mb: 1 }}>
          ← Decisions
        </Button>
        <Typography variant="h4" gutterBottom>
          Decision Detail
        </Typography>
        <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap">
          <DecisionStatusBadge status={item.decisionStatus} />
          <ImplementationStatusBadge status={item.implementationStatus} />
        </Stack>
      </Box>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>
          Summary
        </Typography>
        <Stack spacing={0.5}>
          <Typography variant="body2">Id: {item.id}</Typography>
          <Typography variant="body2">
            Recommendation:{' '}
            <Button
              component={RouterLink}
              to={`/recommendations/${item.recommendationId}`}
              size="small"
            >
              {item.recommendationId}
            </Button>
          </Typography>
          <Typography variant="body2">Company: {item.companyId}</Typography>
          <Typography variant="body2">Mission: {item.missionId}</Typography>
          <Typography variant="body2">Contract: {item.contractId}</Typography>
          <Typography variant="body2">
            Decision date: {new Date(item.decisionDate).toLocaleString()}
          </Typography>
          <Typography variant="body2">
            Implementation date:{' '}
            {item.implementationDate ? new Date(item.implementationDate).toLocaleString() : '—'}
          </Typography>
          <Typography variant="body2">
            Completed date:{' '}
            {item.completedDate ? new Date(item.completedDate).toLocaleString() : '—'}
          </Typography>
          <Typography variant="body2">Created by: {item.createdBy}</Typography>
        </Stack>
      </Paper>

      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>
          Implementation Progress
        </Typography>
        <LinearProgress
          variant="determinate"
          value={implementationProgress(item.implementationStatus)}
          sx={{ height: 10, borderRadius: 1, mb: 1 }}
        />
        <Typography variant="body2" color="text.secondary">
          {item.implementationStatus}
        </Typography>
      </Paper>

      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>
          Actions
        </Typography>
        <Stack spacing={2}>
          <TextField
            label="Actor"
            size="small"
            value={actor}
            onChange={(event) => setActor(event.target.value)}
            sx={{ maxWidth: 320 }}
          />
          <TextField
            label="Comment"
            size="small"
            value={comment}
            onChange={(event) => setComment(event.target.value)}
            fullWidth
          />
          <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap">
            <Button
              variant="contained"
              disabled={busy || !canStart || !actor}
              onClick={() => void run(() => startDecision(item.id, { actor, comment }))}
            >
              Start Implementation
            </Button>
            <Button
              variant="contained"
              color="success"
              disabled={busy || !canComplete || !actor}
              onClick={() => void run(() => completeDecision(item.id, { actor, comment }))}
            >
              Complete
            </Button>
            <Button
              variant="outlined"
              color="inherit"
              disabled={busy || !canCancel || !actor}
              onClick={() => void run(() => cancelDecision(item.id, { actor, comment }))}
            >
              Cancel
            </Button>
          </Stack>
        </Stack>
      </Paper>

      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>
          Outcome
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Outcome can be recorded only after completion (BR-1406).
        </Typography>
        <Stack spacing={2}>
          <TextField
            label="Outcome"
            value={outcome}
            onChange={(event) => setOutcome(event.target.value)}
            disabled={!canRecordOutcome}
            fullWidth
            multiline
            minRows={2}
          />
          <TextField
            label="Business Value"
            value={businessValue}
            onChange={(event) => setBusinessValue(event.target.value)}
            disabled={!canRecordOutcome}
            fullWidth
          />
          <Button
            variant="contained"
            disabled={busy || !canRecordOutcome || !outcome || !actor}
            onClick={() =>
              void run(() =>
                recordDecisionOutcome(item.id, {
                  outcome,
                  businessValue: businessValue || undefined,
                  actor,
                  comment,
                }),
              )
            }
          >
            Record Outcome
          </Button>
        </Stack>
      </Paper>

      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>
          Decision Timeline
        </Typography>
        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>When</TableCell>
                <TableCell>Event</TableCell>
                <TableCell>Decision</TableCell>
                <TableCell>Implementation</TableCell>
                <TableCell>Actor</TableCell>
                <TableCell>Comment</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {item.timeline.map((entry) => (
                <TableRow key={entry.id}>
                  <TableCell>{new Date(entry.occurredAt).toLocaleString()}</TableCell>
                  <TableCell>{entry.eventType}</TableCell>
                  <TableCell>
                    {entry.fromDecisionStatus} → {entry.toDecisionStatus}
                  </TableCell>
                  <TableCell>
                    {entry.fromImplementationStatus} → {entry.toImplementationStatus}
                  </TableCell>
                  <TableCell>{entry.actor}</TableCell>
                  <TableCell>{entry.comment ?? '—'}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>
    </Stack>
  )
}
