import { useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Paper,
  Stack,
  Typography,
} from '@mui/material'
import {
  activatePlanningTemplate,
  deactivatePlanningTemplate,
  getPlanningTemplate,
} from '../api/planningTemplates'
import { getErrorMessage } from '../api/client'
import type { PlanningTemplate } from '../types/planningTemplate'

export function PlanningTemplateDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [item, setItem] = useState<PlanningTemplate | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    if (!id) return
    let cancelled = false

    const load = async () => {
      setLoading(true)
      setError(null)
      try {
        const template = await getPlanningTemplate(id)
        if (!cancelled) setItem(template)
      } catch (err) {
        if (!cancelled) {
          setItem(null)
          setError(getErrorMessage(err, 'Failed to load planning template.'))
        }
      } finally {
        if (!cancelled) setLoading(false)
      }
    }

    void load()
    return () => {
      cancelled = true
    }
  }, [id])

  const toggleStatus = async () => {
    if (!item) return
    setBusy(true)
    setError(null)
    try {
      if (item.status === 'Active') {
        await deactivatePlanningTemplate(item.id)
      } else {
        await activatePlanningTemplate(item.id)
      }
      setItem(await getPlanningTemplate(item.id))
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

  if (error || !item) {
    return (
      <Stack spacing={2}>
        <Alert severity="error">{error ?? 'Template not found.'}</Alert>
        <Button component={RouterLink} to="/planning-templates" variant="outlined">
          Back to list
        </Button>
      </Stack>
    )
  }

  return (
    <Stack spacing={3}>
      <Box>
        <Button component={RouterLink} to="/planning-templates" size="small" sx={{ mb: 1 }}>
          ← Planning Templates
        </Button>
        <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" spacing={2}>
          <Box>
            <Typography variant="h4" gutterBottom>
              {item.name}
            </Typography>
            <Chip
              label={item.status}
              color={item.status === 'Active' ? 'success' : 'default'}
              size="small"
            />
          </Box>
          <Stack direction="row" spacing={1} flexWrap="wrap">
            <Button
              variant="outlined"
              onClick={() => navigate(`/planning-templates/${item.id}/edit`)}
            >
              Edit
            </Button>
            <Button variant="outlined" disabled={busy} onClick={() => void toggleStatus()}>
              {item.status === 'Active' ? 'Deactivate' : 'Activate'}
            </Button>
            <Button
              variant="contained"
              component={RouterLink}
              to={`/planning-templates/${item.id}/apply`}
              disabled={item.status !== 'Active'}
            >
              Apply
            </Button>
          </Stack>
        </Stack>
      </Box>

      <Paper sx={{ p: 3 }}>
        <Stack spacing={1.5}>
          <Typography variant="body2">Description: {item.description || '—'}</Typography>
          <Typography variant="body2">Company: {item.companyId}</Typography>
          <Typography variant="body2">Working Calendar: {item.workingCalendarId}</Typography>
          <Typography variant="body2">Working Hours: {item.workingHoursId}</Typography>
          <Typography variant="body2">
            Resource Availability Strategy: {item.resourceAvailabilityStrategy}
          </Typography>
          <Typography variant="body2">
            Default window: {item.defaultPlanningWindowDays} days (offset{' '}
            {item.defaultPeriodStartOffsetDays})
          </Typography>
          <Typography variant="body2">
            Utilization warning: {item.utilizationWarningPercentage ?? '—'}%
          </Typography>
          <Typography variant="body2">
            Include assignment distribution: {item.includeAssignmentDistribution ? 'Yes' : 'No'}
          </Typography>
          <Typography variant="body2">
            Updated: {new Date(item.updatedAt).toLocaleString()}
          </Typography>
        </Stack>
      </Paper>
    </Stack>
  )
}
