import { FormEvent, useEffect, useMemo, useState } from 'react'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  Checkbox,
  FormControl,
  FormControlLabel,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material'
import {
  createResourceAvailability,
  getResourceAvailability,
  listExecutionResourcesForSelect,
  listWorkingCalendarsForSelect,
  listWorkingHoursForSelect,
  updateResourceAvailability,
} from '../api/resourceAvailabilities'
import { getErrorMessage } from '../api/client'
import {
  defaultWeeklyAvailability,
  type ExecutionResourceOption,
  type ResourceAvailabilityDayOverride,
  type ResourceAvailabilityPayload,
} from '../types/resourceAvailability'
import type { WorkingCalendar } from '../types/workingCalendar'
import type { WorkingHours } from '../types/workingHours'
import { fromTimeInput, toTimeInput } from '../types/workingHours'

export function ResourceAvailabilityFormPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const isEdit = Boolean(id)

  const [resources, setResources] = useState<ExecutionResourceOption[]>([])
  const [calendars, setCalendars] = useState<WorkingCalendar[]>([])
  const [workingHours, setWorkingHours] = useState<WorkingHours[]>([])
  const [executionResourceId, setExecutionResourceId] = useState('')
  const [workingCalendarId, setWorkingCalendarId] = useState('')
  const [workingHoursId, setWorkingHoursId] = useState('')
  const [name, setName] = useState('')
  const [effectiveFrom, setEffectiveFrom] = useState('2026-01-01')
  const [effectiveTo, setEffectiveTo] = useState('')
  const [weeklyAvailability, setWeeklyAvailability] = useState(defaultWeeklyAvailability())
  const [dailyOverrides, setDailyOverrides] = useState<ResourceAvailabilityDayOverride[]>([])
  const [error, setError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  const filteredHours = useMemo(
    () => workingHours.filter((item) => !workingCalendarId || item.workingCalendarId === workingCalendarId),
    [workingHours, workingCalendarId],
  )

  useEffect(() => {
    void (async () => {
      try {
        const [resourceData, calendarData, hoursData] = await Promise.all([
          listExecutionResourcesForSelect(),
          listWorkingCalendarsForSelect(),
          listWorkingHoursForSelect(),
        ])
        setResources(resourceData)
        setCalendars(calendarData)
        setWorkingHours(hoursData)
      } catch (err) {
        setError(getErrorMessage(err, 'Failed to load lookup data.'))
      }
    })()
  }, [])

  useEffect(() => {
    if (!id) return

    void (async () => {
      try {
        const item = await getResourceAvailability(id)
        setExecutionResourceId(item.executionResourceId)
        setWorkingCalendarId(item.workingCalendarId)
        setWorkingHoursId(item.workingHoursId)
        setName(item.name)
        setEffectiveFrom(item.effectiveFrom)
        setEffectiveTo(item.effectiveTo ?? '')
        setWeeklyAvailability(item.weeklyAvailability)
        setDailyOverrides(item.dailyOverrides)
      } catch (err) {
        setError(getErrorMessage(err, 'Failed to load resource availability.'))
      }
    })()
  }, [id])

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setSaving(true)
    setError(null)

    const payload: ResourceAvailabilityPayload = {
      executionResourceId: isEdit ? undefined : executionResourceId,
      workingCalendarId,
      workingHoursId,
      name,
      effectiveFrom,
      effectiveTo: effectiveTo || null,
      weeklyAvailability,
      dailyOverrides,
    }

    try {
      if (isEdit && id) {
        await updateResourceAvailability(id, payload)
        navigate(`/resource-availabilities/${id}`)
      } else {
        const created = await createResourceAvailability({
          ...payload,
          executionResourceId,
        })
        navigate(`/resource-availabilities/${created.id}`)
      }
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to save resource availability.'))
    } finally {
      setSaving(false)
    }
  }

  return (
    <Stack spacing={3} component="form" onSubmit={onSubmit}>
      <Box>
        <Typography variant="h4">{isEdit ? 'Edit Resource Availability' : 'New Resource Availability'}</Typography>
        <Typography color="text.secondary">
          Link an execution resource to a working calendar and working hours schedule.
        </Typography>
      </Box>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 3 }}>
        <Stack spacing={2}>
          {!isEdit && (
            <FormControl fullWidth required>
              <InputLabel id="ra-resource">Execution Resource</InputLabel>
              <Select
                labelId="ra-resource"
                label="Execution Resource"
                value={executionResourceId}
                onChange={(event) => setExecutionResourceId(event.target.value)}
              >
                {resources.map((resource) => (
                  <MenuItem key={resource.id} value={resource.id}>
                    {resource.code} — {resource.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          )}

          <TextField
            label="Name"
            value={name}
            onChange={(event) => setName(event.target.value)}
            required
            fullWidth
          />

          <FormControl fullWidth required>
            <InputLabel id="ra-calendar">Working Calendar</InputLabel>
            <Select
              labelId="ra-calendar"
              label="Working Calendar"
              value={workingCalendarId}
              onChange={(event) => {
                setWorkingCalendarId(event.target.value)
                setWorkingHoursId('')
              }}
            >
              {calendars.map((calendar) => (
                <MenuItem key={calendar.id} value={calendar.id}>
                  {calendar.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <FormControl fullWidth required>
            <InputLabel id="ra-hours">Working Hours</InputLabel>
            <Select
              labelId="ra-hours"
              label="Working Hours"
              value={workingHoursId}
              onChange={(event) => setWorkingHoursId(event.target.value)}
            >
              {filteredHours.map((hours) => (
                <MenuItem key={hours.id} value={hours.id}>
                  {hours.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
            <TextField
              label="Effective From"
              type="date"
              value={effectiveFrom}
              onChange={(event) => setEffectiveFrom(event.target.value)}
              InputLabelProps={{ shrink: true }}
              required
              fullWidth
            />
            <TextField
              label="Effective To"
              type="date"
              value={effectiveTo}
              onChange={(event) => setEffectiveTo(event.target.value)}
              InputLabelProps={{ shrink: true }}
              fullWidth
            />
          </Stack>
        </Stack>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" sx={{ mb: 2 }}>
          Weekly Availability
        </Typography>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Day</TableCell>
              <TableCell>Available</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {weeklyAvailability.map((day, index) => (
              <TableRow key={day.dayOfWeek}>
                <TableCell>{day.dayOfWeek}</TableCell>
                <TableCell>
                  <FormControlLabel
                    control={
                      <Checkbox
                        checked={day.enabled}
                        onChange={(event) => {
                          const next = [...weeklyAvailability]
                          next[index] = { ...day, enabled: event.target.checked }
                          setWeeklyAvailability(next)
                        }}
                      />
                    }
                    label={day.enabled ? 'Yes' : 'No'}
                  />
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
          <Typography variant="h6">Daily Overrides</Typography>
          <Button
            size="small"
            onClick={() =>
              setDailyOverrides([
                ...dailyOverrides,
                {
                  overrideDate: effectiveFrom,
                  available: false,
                  startTime: null,
                  endTime: null,
                  notes: null,
                },
              ])
            }
          >
            Add Override
          </Button>
        </Stack>
        <Stack spacing={2}>
          {dailyOverrides.map((override, index) => (
            <Stack key={`${override.overrideDate}-${index}`} direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <TextField
                label="Date"
                type="date"
                value={override.overrideDate}
                onChange={(event) => {
                  const next = [...dailyOverrides]
                  next[index] = { ...override, overrideDate: event.target.value }
                  setDailyOverrides(next)
                }}
                InputLabelProps={{ shrink: true }}
              />
              <FormControlLabel
                control={
                  <Checkbox
                    checked={override.available}
                    onChange={(event) => {
                      const next = [...dailyOverrides]
                      next[index] = {
                        ...override,
                        available: event.target.checked,
                        startTime: event.target.checked ? override.startTime : null,
                        endTime: event.target.checked ? override.endTime : null,
                      }
                      setDailyOverrides(next)
                    }}
                  />
                }
                label="Available"
              />
              <TextField
                label="Start"
                type="time"
                value={toTimeInput(override.startTime)}
                disabled={!override.available}
                onChange={(event) => {
                  const next = [...dailyOverrides]
                  next[index] = { ...override, startTime: fromTimeInput(event.target.value) }
                  setDailyOverrides(next)
                }}
                InputLabelProps={{ shrink: true }}
              />
              <TextField
                label="End"
                type="time"
                value={toTimeInput(override.endTime)}
                disabled={!override.available}
                onChange={(event) => {
                  const next = [...dailyOverrides]
                  next[index] = { ...override, endTime: fromTimeInput(event.target.value) }
                  setDailyOverrides(next)
                }}
                InputLabelProps={{ shrink: true }}
              />
              <Button color="error" onClick={() => setDailyOverrides(dailyOverrides.filter((_, i) => i !== index))}>
                Remove
              </Button>
            </Stack>
          ))}
        </Stack>
      </Paper>

      <Stack direction="row" spacing={2}>
        <Button type="submit" variant="contained" disabled={saving}>
          {saving ? 'Saving…' : 'Save'}
        </Button>
        <Button component={RouterLink} to="/resource-availabilities">
          Cancel
        </Button>
      </Stack>
    </Stack>
  )
}
