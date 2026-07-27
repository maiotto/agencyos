import { useEffect, useState } from 'react'
import { Link as RouterLink, useParams } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  Checkbox,
  CircularProgress,
  FormControlLabel,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { applyPlanningTemplate, getPlanningTemplate } from '../api/planningTemplates'
import { getErrorMessage } from '../api/client'
import type {
  AppliedPlanningConfiguration,
  PlanningTemplate,
} from '../types/planningTemplate'

export function PlanningTemplateApplyPage() {
  const { id } = useParams<{ id: string }>()
  const [template, setTemplate] = useState<PlanningTemplate | null>(null)
  const [loading, setLoading] = useState(true)
  const [applying, setApplying] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [result, setResult] = useState<AppliedPlanningConfiguration | null>(null)

  const [periodStartDate, setPeriodStartDate] = useState('')
  const [periodEndDate, setPeriodEndDate] = useState('')
  const [executionResourceId, setExecutionResourceId] = useState('')
  const [calculateCapacity, setCalculateCapacity] = useState(true)
  const [calculateWorkload, setCalculateWorkload] = useState(false)

  useEffect(() => {
    if (!id) return
    let cancelled = false

    const load = async () => {
      setLoading(true)
      setError(null)
      try {
        const item = await getPlanningTemplate(id)
        if (cancelled) return
        setTemplate(item)
        if (item.status !== 'Active') {
          setError('Only Active templates can be applied.')
        }
      } catch (err) {
        if (!cancelled) setError(getErrorMessage(err, 'Failed to load template.'))
      } finally {
        if (!cancelled) setLoading(false)
      }
    }

    void load()
    return () => {
      cancelled = true
    }
  }, [id])

  const onApply = async () => {
    if (!id) return
    setApplying(true)
    setError(null)
    setResult(null)

    try {
      const applied = await applyPlanningTemplate(id, {
        periodStartDate: periodStartDate || undefined,
        periodEndDate: periodEndDate || undefined,
        executionResourceId: executionResourceId || undefined,
        calculateCapacity,
        calculateWorkload,
      })
      setResult(applied)
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to apply planning template.'))
    } finally {
      setApplying(false)
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
        <Button component={RouterLink} to="/planning-templates" size="small" sx={{ mb: 1 }}>
          ← Planning Templates
        </Button>
        <Typography variant="h4" gutterBottom>
          Apply Template
        </Typography>
        <Typography color="text.secondary">
          {template
            ? `Produces a new planning configuration from “${template.name}” without modifying the template.`
            : 'Apply planning template'}
        </Typography>
      </Box>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 3 }}>
        <Stack spacing={2}>
          <TextField
            label="Period Start (optional)"
            type="date"
            value={periodStartDate}
            onChange={(event) => setPeriodStartDate(event.target.value)}
            InputLabelProps={{ shrink: true }}
            helperText="Leave blank to use the template default window from today."
          />
          <TextField
            label="Period End (optional)"
            type="date"
            value={periodEndDate}
            onChange={(event) => setPeriodEndDate(event.target.value)}
            InputLabelProps={{ shrink: true }}
          />
          <TextField
            label="Execution Resource Id (optional)"
            value={executionResourceId}
            onChange={(event) => setExecutionResourceId(event.target.value)}
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={calculateCapacity}
                onChange={(event) => setCalculateCapacity(event.target.checked)}
              />
            }
            label="Calculate Capacity"
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={calculateWorkload}
                onChange={(event) => setCalculateWorkload(event.target.checked)}
              />
            }
            label="Calculate Workload"
          />
          <Button
            variant="contained"
            disabled={applying || template?.status !== 'Active'}
            onClick={() => void onApply()}
          >
            {applying ? 'Applying…' : 'Apply Template'}
          </Button>
        </Stack>
      </Paper>

      {result && (
        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom>
            Applied planning configuration
          </Typography>
          <Stack spacing={1}>
            <Typography variant="body2">
              Period: {result.periodStartDate} → {result.periodEndDate}
            </Typography>
            <Typography variant="body2">Working Calendar: {result.workingCalendarId}</Typography>
            <Typography variant="body2">Working Hours: {result.workingHoursId}</Typography>
            <Typography variant="body2">
              Strategy: {result.resourceAvailabilityStrategy}
            </Typography>
            <Typography variant="body2">
              Capacity results: {result.capacityResults?.length ?? 0}
            </Typography>
            <Typography variant="body2">
              Workload results: {result.workloadResults?.length ?? 0}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Template was not modified. Historical Capacity/Workload remain immutable; new
              calculations may append history.
            </Typography>
          </Stack>
        </Paper>
      )}
    </Stack>
  )
}
