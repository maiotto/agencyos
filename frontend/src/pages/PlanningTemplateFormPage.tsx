import { useEffect, useMemo, useState } from 'react'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  Checkbox,
  CircularProgress,
  FormControl,
  FormControlLabel,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import {
  createPlanningTemplate,
  getPlanningTemplate,
  listWorkingCalendarsForTemplate,
  listWorkingHoursForTemplate,
  updatePlanningTemplate,
} from '../api/planningTemplates'
import { getErrorMessage } from '../api/client'
import type { WorkingCalendar } from '../types/workingCalendar'
import type { WorkingHours } from '../types/workingHours'

const DEFAULT_COMPANY_ID = 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa'

export function PlanningTemplateFormPage() {
  const { id } = useParams<{ id: string }>()
  const isEdit = Boolean(id)
  const navigate = useNavigate()

  const [calendars, setCalendars] = useState<WorkingCalendar[]>([])
  const [hours, setHours] = useState<WorkingHours[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [workingCalendarId, setWorkingCalendarId] = useState('')
  const [workingHoursId, setWorkingHoursId] = useState('')
  const [strategy, setStrategy] = useState('RequireActiveConfiguration')
  const [windowDays, setWindowDays] = useState(7)
  const [offsetDays, setOffsetDays] = useState(0)
  const [warningPct, setWarningPct] = useState<string>('')
  const [includeDistribution, setIncludeDistribution] = useState(true)

  const filteredHours = useMemo(
    () => hours.filter((item) => !workingCalendarId || item.workingCalendarId === workingCalendarId),
    [hours, workingCalendarId],
  )

  useEffect(() => {
    let cancelled = false

    const load = async () => {
      setLoading(true)
      setError(null)
      try {
        const [calendarList, hoursList] = await Promise.all([
          listWorkingCalendarsForTemplate(),
          listWorkingHoursForTemplate(),
        ])
        if (cancelled) return
        setCalendars(calendarList)
        setHours(hoursList)

        if (id) {
          const template = await getPlanningTemplate(id)
          if (cancelled) return
          setName(template.name)
          setDescription(template.description ?? '')
          setWorkingCalendarId(template.workingCalendarId)
          setWorkingHoursId(template.workingHoursId)
          setStrategy(template.resourceAvailabilityStrategy)
          setWindowDays(template.defaultPlanningWindowDays)
          setOffsetDays(template.defaultPeriodStartOffsetDays)
          setWarningPct(
            template.utilizationWarningPercentage == null
              ? ''
              : String(template.utilizationWarningPercentage),
          )
          setIncludeDistribution(template.includeAssignmentDistribution)
        }
      } catch (err) {
        if (!cancelled) setError(getErrorMessage(err, 'Failed to load form data.'))
      } finally {
        if (!cancelled) setLoading(false)
      }
    }

    void load()
    return () => {
      cancelled = true
    }
  }, [id])

  const onSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
    setSaving(true)
    setError(null)

    const payload = {
      name: name.trim(),
      description: description.trim() || undefined,
      workingCalendarId,
      workingHoursId,
      resourceAvailabilityStrategy: strategy,
      defaultPlanningWindowDays: windowDays,
      defaultPeriodStartOffsetDays: offsetDays,
      utilizationWarningPercentage: warningPct === '' ? null : Number(warningPct),
      includeAssignmentDistribution: includeDistribution,
    }

    try {
      if (isEdit && id) {
        await updatePlanningTemplate(id, payload)
        navigate(`/planning-templates/${id}`)
      } else {
        const created = await createPlanningTemplate({
          ...payload,
          companyId: DEFAULT_COMPANY_ID,
        })
        navigate(`/planning-templates/${created.id}`)
      }
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to save planning template.'))
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
    <Stack spacing={3} component="form" onSubmit={onSubmit}>
      <Box>
        <Button component={RouterLink} to="/planning-templates" size="small" sx={{ mb: 1 }}>
          ← Planning Templates
        </Button>
        <Typography variant="h4">{isEdit ? 'Edit Template' : 'Create Template'}</Typography>
        <Typography color="text.secondary">
          Templates reference existing calendars and hours — they do not copy operational data.
        </Typography>
      </Box>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 3 }}>
        <Stack spacing={2}>
          <TextField
            label="Name"
            required
            value={name}
            onChange={(event) => setName(event.target.value)}
          />
          <TextField
            label="Description"
            value={description}
            onChange={(event) => setDescription(event.target.value)}
            multiline
            minRows={2}
          />
          <FormControl required>
            <InputLabel>Working Calendar</InputLabel>
            <Select
              label="Working Calendar"
              value={workingCalendarId}
              onChange={(event) => {
                setWorkingCalendarId(event.target.value)
                setWorkingHoursId('')
              }}
            >
              {calendars.map((calendar) => (
                <MenuItem key={calendar.id} value={calendar.id}>
                  {calendar.name} ({calendar.status})
                </MenuItem>
              ))}
            </Select>
          </FormControl>
          <FormControl required>
            <InputLabel>Working Hours</InputLabel>
            <Select
              label="Working Hours"
              value={workingHoursId}
              onChange={(event) => setWorkingHoursId(event.target.value)}
            >
              {filteredHours.map((item) => (
                <MenuItem key={item.id} value={item.id}>
                  {item.name} ({item.status})
                </MenuItem>
              ))}
            </Select>
          </FormControl>
          <FormControl>
            <InputLabel>Resource Availability Strategy</InputLabel>
            <Select
              label="Resource Availability Strategy"
              value={strategy}
              onChange={(event) => setStrategy(event.target.value)}
            >
              <MenuItem value="RequireActiveConfiguration">Require Active Configuration</MenuItem>
            </Select>
          </FormControl>
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <TextField
              label="Default Planning Window (days)"
              type="number"
              required
              value={windowDays}
              onChange={(event) => setWindowDays(Number(event.target.value))}
              inputProps={{ min: 1 }}
            />
            <TextField
              label="Period Start Offset (days)"
              type="number"
              value={offsetDays}
              onChange={(event) => setOffsetDays(Number(event.target.value))}
              inputProps={{ min: 0 }}
            />
            <TextField
              label="Utilization Warning %"
              type="number"
              value={warningPct}
              onChange={(event) => setWarningPct(event.target.value)}
              inputProps={{ min: 0, max: 100 }}
            />
          </Stack>
          <FormControlLabel
            control={
              <Checkbox
                checked={includeDistribution}
                onChange={(event) => setIncludeDistribution(event.target.checked)}
              />
            }
            label="Include assignment distribution in applied planning"
          />
          <Stack direction="row" spacing={2}>
            <Button type="submit" variant="contained" disabled={saving}>
              {saving ? 'Saving…' : 'Save'}
            </Button>
            <Button component={RouterLink} to="/planning-templates" disabled={saving}>
              Cancel
            </Button>
          </Stack>
        </Stack>
      </Paper>
    </Stack>
  )
}
