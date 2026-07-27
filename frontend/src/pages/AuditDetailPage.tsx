import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link as RouterLink, useParams } from 'react-router-dom'
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
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import {
  getAuditByCorrelation,
  getAuditByEntity,
  getAuditEvent,
} from '../api/audit'
import { getErrorMessage } from '../api/client'
import type { AuditEvent } from '../types/audit'

function prettyJson(value?: string | null) {
  if (!value) return '—'
  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}

function TimelineTable({ items }: { items: AuditEvent[] }) {
  return (
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell>When</TableCell>
          <TableCell>Event</TableCell>
          <TableCell>Action</TableCell>
          <TableCell>User</TableCell>
          <TableCell>Source</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {items.map((item) => (
          <TableRow key={item.id} hover>
            <TableCell>{new Date(item.occurredAt).toLocaleString()}</TableCell>
            <TableCell>{item.eventType}</TableCell>
            <TableCell>
              <Button component={RouterLink} to={`/audit/${item.id}`} size="small">
                {item.action}
              </Button>
            </TableCell>
            <TableCell>{item.userName}</TableCell>
            <TableCell>{item.source}</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  )
}

export function AuditDetailPage() {
  const { id, entityId, correlationId } = useParams<{
    id?: string
    entityId?: string
    correlationId?: string
  }>()
  const mode = useMemo(() => {
    if (entityId) return 'entity'
    if (correlationId) return 'correlation'
    return 'detail'
  }, [entityId, correlationId])

  const [item, setItem] = useState<AuditEvent | null>(null)
  const [timeline, setTimeline] = useState<AuditEvent[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      if (mode === 'entity' && entityId) {
        setTimeline(await getAuditByEntity(entityId))
        setItem(null)
      } else if (mode === 'correlation' && correlationId) {
        setTimeline(await getAuditByCorrelation(correlationId))
        setItem(null)
      } else if (id) {
        const detail = await getAuditEvent(id)
        setItem(detail)
        setTimeline(await getAuditByEntity(detail.entityId))
      }
    } catch (err) {
      setItem(null)
      setTimeline([])
      setError(getErrorMessage(err, 'Failed to load audit data.'))
    } finally {
      setLoading(false)
    }
  }, [mode, id, entityId, correlationId])

  useEffect(() => {
    void load()
  }, [load])

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
        <Button component={RouterLink} to="/audit" size="small" sx={{ mb: 1 }}>
          ← Audit Trail
        </Button>
        <Typography variant="h4" gutterBottom>
          {mode === 'entity'
            ? 'Entity History'
            : mode === 'correlation'
              ? 'Correlation View'
              : 'Audit Event Detail'}
        </Typography>
        <Typography color="text.secondary">
          Immutable previous/current state snapshots with request correlation (BR-1507 / BR-1508).
        </Typography>
      </Box>

      {error && <Alert severity="error">{error}</Alert>}

      {item && (
        <>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              Summary
            </Typography>
            <Stack spacing={0.5}>
              <Typography variant="body2">Id: {item.id}</Typography>
              <Typography variant="body2">
                Entity: {item.entityType} / {item.entityId}
              </Typography>
              <Typography variant="body2">Event: {item.eventType}</Typography>
              <Typography variant="body2">Action: {item.action}</Typography>
              <Typography variant="body2">
                User: {item.userName} ({item.userId})
              </Typography>
              <Typography variant="body2">Source: {item.source}</Typography>
              <Typography variant="body2">
                Occurred: {new Date(item.occurredAt).toLocaleString()}
              </Typography>
              <Typography variant="body2">Correlation: {item.correlationId ?? '—'}</Typography>
              <Typography variant="body2">Session: {item.sessionId ?? '—'}</Typography>
              <Typography variant="body2">Request: {item.requestId ?? '—'}</Typography>
            </Stack>
          </Paper>

          <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
            <Paper sx={{ p: 2, flex: 1 }}>
              <Typography variant="h6" gutterBottom>
                Previous State
              </Typography>
              <Box
                component="pre"
                sx={{
                  m: 0,
                  p: 1.5,
                  overflow: 'auto',
                  bgcolor: 'rgba(15, 76, 92, 0.06)',
                  borderRadius: 1,
                  fontSize: 12,
                }}
              >
                {prettyJson(item.previousState)}
              </Box>
            </Paper>
            <Paper sx={{ p: 2, flex: 1 }}>
              <Typography variant="h6" gutterBottom>
                Current State
              </Typography>
              <Box
                component="pre"
                sx={{
                  m: 0,
                  p: 1.5,
                  overflow: 'auto',
                  bgcolor: 'rgba(15, 76, 92, 0.06)',
                  borderRadius: 1,
                  fontSize: 12,
                }}
              >
                {prettyJson(item.currentState)}
              </Box>
            </Paper>
          </Stack>

          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              Metadata
            </Typography>
            <Box
              component="pre"
              sx={{
                m: 0,
                p: 1.5,
                overflow: 'auto',
                bgcolor: 'rgba(15, 76, 92, 0.06)',
                borderRadius: 1,
                fontSize: 12,
              }}
            >
              {prettyJson(item.metadata)}
            </Box>
          </Paper>
        </>
      )}

      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>
          {mode === 'correlation' ? 'Correlated Timeline' : 'Entity Timeline'}
        </Typography>
        <TimelineTable items={timeline} />
      </Paper>
    </Stack>
  )
}
