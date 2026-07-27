import { useCallback, useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
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
import { listAuditEvents } from '../api/audit'
import { getErrorMessage } from '../api/client'
import type { AuditEvent } from '../types/audit'
import { DEFAULT_COMPANY_ID } from '../theme'

export function AuditListPage() {
  const navigate = useNavigate()
  const [companyId, setCompanyId] = useState(DEFAULT_COMPANY_ID)
  const [entityType, setEntityType] = useState('')
  const [eventType, setEventType] = useState('')
  const [userId, setUserId] = useState('')
  const [search, setSearch] = useState('')
  const [items, setItems] = useState<AuditEvent[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      setItems(
        await listAuditEvents({
          companyId: companyId || undefined,
          entityType: entityType || undefined,
          eventType: eventType || undefined,
          userId: userId || undefined,
          search: search || undefined,
          orderBy: 'occurredAt',
          orderDirection: 'desc',
        }),
      )
    } catch (err) {
      setItems([])
      setError(getErrorMessage(err, 'Failed to load audit events.'))
    } finally {
      setLoading(false)
    }
  }, [companyId, entityType, eventType, userId, search])

  useEffect(() => {
    void load()
  }, [load])

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h4" gutterBottom>
          Decision Audit Trail
        </Typography>
        <Typography color="text.secondary">
          Immutable governance log of recommendation and decision lifecycle actions (US-206).
        </Typography>
      </Box>

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
            label="Entity Type"
            size="small"
            value={entityType}
            onChange={(event) => setEntityType(event.target.value)}
            sx={{ minWidth: 180 }}
          >
            <MenuItem value="">All</MenuItem>
            <MenuItem value="Recommendation">Recommendation</MenuItem>
            <MenuItem value="RecommendationWorkflow">RecommendationWorkflow</MenuItem>
            <MenuItem value="Decision">Decision</MenuItem>
            <MenuItem value="AIRecommendation">AIRecommendation</MenuItem>
            <MenuItem value="Explainability">Explainability</MenuItem>
            <MenuItem value="ExecutiveRecommendationSummary">ExecutiveRecommendationSummary</MenuItem>
            <MenuItem value="PlanningTemplate">PlanningTemplate</MenuItem>
            <MenuItem value="Portfolio">Portfolio</MenuItem>
            <MenuItem value="CapacityHistory">CapacityHistory</MenuItem>
            <MenuItem value="WorkloadHistory">WorkloadHistory</MenuItem>
            <MenuItem value="CompanyDecisionProfile">CompanyDecisionProfile</MenuItem>
            <MenuItem value="Company">Company</MenuItem>
          </TextField>
          <TextField
            label="Event Type"
            size="small"
            value={eventType}
            onChange={(event) => setEventType(event.target.value)}
            sx={{ minWidth: 160 }}
          />
          <TextField
            label="User Id"
            size="small"
            value={userId}
            onChange={(event) => setUserId(event.target.value)}
            sx={{ minWidth: 180 }}
          />
          <TextField
            label="Search"
            size="small"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            sx={{ minWidth: 180 }}
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
                <TableCell>When</TableCell>
                <TableCell>Entity</TableCell>
                <TableCell>Event</TableCell>
                <TableCell>Action</TableCell>
                <TableCell>User</TableCell>
                <TableCell>Correlation</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {items.map((item) => (
                <TableRow key={item.id} hover>
                  <TableCell>{new Date(item.occurredAt).toLocaleString()}</TableCell>
                  <TableCell>
                    {item.entityType}
                    <Typography variant="caption" display="block" fontFamily="monospace">
                      {item.entityId}
                    </Typography>
                  </TableCell>
                  <TableCell>{item.eventType}</TableCell>
                  <TableCell>{item.action}</TableCell>
                  <TableCell>{item.userName}</TableCell>
                  <TableCell sx={{ fontFamily: 'monospace', fontSize: 12 }}>
                    {item.correlationId ?? '—'}
                  </TableCell>
                  <TableCell align="right">
                    <Button size="small" onClick={() => navigate(`/audit/${item.id}`)}>
                      Open
                    </Button>
                    <Button
                      size="small"
                      component={RouterLink}
                      to={`/audit/entity/${item.entityId}`}
                    >
                      Entity
                    </Button>
                    {item.correlationId && (
                      <Button
                        size="small"
                        component={RouterLink}
                        to={`/audit/correlation/${item.correlationId}`}
                      >
                        Corr.
                      </Button>
                    )}
                  </TableCell>
                </TableRow>
              ))}
              {items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={7}>
                    <Typography color="text.secondary" sx={{ py: 2 }}>
                      No audit events found.
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
