import { useEffect, useMemo, useState } from 'react'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  FormControl,
  FormHelperText,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { WeekdayScheduleEditor } from '../components/WeekdayScheduleEditor'
import {
  createWorkingHours,
  getWorkingHours,
  listWorkingCalendarsForSelect,
  updateWorkingHours,
} from '../api/workingHours'
import { getErrorMessage } from '../api/client'
import type { WorkingCalendar } from '../types/workingCalendar'
import {
  WEEKDAYS,
  defaultWeekdaySchedule,
  type WorkingHoursDay,
} from '../types/workingHours'

const daySchema = z.object({
  dayOfWeek: z.string(),
  enabled: z.boolean(),
  startTime: z.string().nullable(),
  endTime: z.string().nullable(),
  breakStart: z.string().nullable(),
  breakEnd: z.string().nullable(),
})

const formSchema = z
  .object({
    workingCalendarId: z.string().min(1, 'Working Calendar is mandatory.'),
    name: z.string().trim().min(1, 'Working Hours name is mandatory.').max(200),
    effectiveFrom: z.string().min(1, 'EffectiveFrom is mandatory.'),
    effectiveTo: z.string().optional(),
    days: z.array(daySchema),
  })
  .superRefine((values, ctx) => {
    if (!values.days.some((day) => day.enabled)) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'At least one enabled weekday is required.',
        path: ['days'],
      })
    }

    if (values.effectiveTo && values.effectiveTo < values.effectiveFrom) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'EffectiveTo cannot be earlier than EffectiveFrom.',
        path: ['effectiveTo'],
      })
    }

    values.days.forEach((day, index) => {
      if (!day.enabled) {
        return
      }

      if (!day.startTime || !day.endTime) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'Enabled weekdays require StartTime and EndTime.',
          path: ['days', index],
        })
        return
      }

      if (day.startTime >= day.endTime) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'StartTime must be earlier than EndTime.',
          path: ['days', index],
        })
      }

      if (Boolean(day.breakStart) !== Boolean(day.breakEnd)) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'If BreakStart exists, BreakEnd is mandatory.',
          path: ['days', index],
        })
      }

      if (day.breakStart && day.breakEnd) {
        if (day.breakStart >= day.breakEnd) {
          ctx.addIssue({
            code: z.ZodIssueCode.custom,
            message: 'BreakStart must be earlier than BreakEnd.',
            path: ['days', index],
          })
        }

        if (day.breakStart < day.startTime || day.breakEnd > day.endTime) {
          ctx.addIssue({
            code: z.ZodIssueCode.custom,
            message: 'Break period must be inside working period.',
            path: ['days', index],
          })
        }
      }
    })
  })

type FormValues = z.infer<typeof formSchema>

export function WorkingHoursFormPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const isEdit = Boolean(id)
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [historicalLock, setHistoricalLock] = useState(false)
  const [calendars, setCalendars] = useState<WorkingCalendar[]>([])

  const {
    control,
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      workingCalendarId: '',
      name: '',
      effectiveFrom: '',
      effectiveTo: '',
      days: defaultWeekdaySchedule(),
    },
  })

  useEffect(() => {
    let cancelled = false

    const load = async () => {
      setLoading(true)
      setError(null)

      try {
        const calendarList = await listWorkingCalendarsForSelect()
        if (cancelled) {
          return
        }

        setCalendars(calendarList)

        if (!id) {
          reset({
            workingCalendarId: calendarList[0]?.id ?? '',
            name: '',
            effectiveFrom: '',
            effectiveTo: '',
            days: defaultWeekdaySchedule(),
          })
          return
        }

        const item = await getWorkingHours(id)
        if (cancelled) {
          return
        }

        const today = new Date().toISOString().slice(0, 10)
        setHistoricalLock(item.effectiveFrom <= today)

        const daysByName = new Map(item.days.map((day) => [day.dayOfWeek, day]))
        const days: WorkingHoursDay[] = WEEKDAYS.map(
          (day) =>
            daysByName.get(day) ?? {
              dayOfWeek: day,
              enabled: false,
              startTime: null,
              endTime: null,
              breakStart: null,
              breakEnd: null,
            },
        )

        reset({
          workingCalendarId: item.workingCalendarId,
          name: item.name,
          effectiveFrom: item.effectiveFrom,
          effectiveTo: item.effectiveTo ?? '',
          days,
        })
      } catch (err) {
        if (!cancelled) {
          setError(getErrorMessage(err, 'Failed to load working hours form.'))
        }
      } finally {
        if (!cancelled) {
          setLoading(false)
        }
      }
    }

    void load()
    return () => {
      cancelled = true
    }
  }, [id, reset])

  const title = useMemo(
    () => (isEdit ? 'Edit working hours' : 'Create working hours'),
    [isEdit],
  )

  const onSubmit = handleSubmit(async (values) => {
    setSubmitting(true)
    setError(null)

    const payload = {
      workingCalendarId: values.workingCalendarId,
      name: values.name.trim(),
      effectiveFrom: values.effectiveFrom,
      effectiveTo: values.effectiveTo || null,
      days: values.days,
    }

    try {
      if (isEdit && id) {
        await updateWorkingHours(id, payload)
        navigate(`/working-hours/${id}`)
      } else {
        const created = await createWorkingHours(payload)
        navigate(`/working-hours/${created.id}`)
      }
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to save working hours.'))
    } finally {
      setSubmitting(false)
    }
  })

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
        <CircularProgress />
      </Box>
    )
  }

  return (
    <Stack spacing={3} maxWidth={960}>
      <Box>
        <Typography variant="h4" gutterBottom>
          {title}
        </Typography>
        <Typography color="text.secondary">
          Associate a weekday schedule with a Working Calendar and define validity.
        </Typography>
      </Box>

      {historicalLock ? (
        <Alert severity="warning">
          This configuration has already started. Structural changes are blocked (BR-309).
        </Alert>
      ) : null}

      {error ? <Alert severity="error">{error}</Alert> : null}

      <Paper sx={{ p: { xs: 2.5, md: 3.5 } }}>
        <Stack component="form" spacing={3} onSubmit={onSubmit} noValidate>
          <Controller
            name="workingCalendarId"
            control={control}
            render={({ field }) => (
              <FormControl fullWidth error={Boolean(errors.workingCalendarId)} disabled={isEdit || historicalLock}>
                <InputLabel id="calendar-label">Working calendar</InputLabel>
                <Select labelId="calendar-label" label="Working calendar" {...field}>
                  {calendars.map((calendar) => (
                    <MenuItem key={calendar.id} value={calendar.id}>
                      {calendar.name}
                    </MenuItem>
                  ))}
                </Select>
                {errors.workingCalendarId ? (
                  <FormHelperText>{errors.workingCalendarId.message}</FormHelperText>
                ) : null}
              </FormControl>
            )}
          />

          <TextField
            label="Name"
            fullWidth
            disabled={historicalLock}
            error={Boolean(errors.name)}
            helperText={errors.name?.message}
            {...register('name')}
          />

          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <TextField
              label="Effective from"
              type="date"
              fullWidth
              InputLabelProps={{ shrink: true }}
              disabled={historicalLock}
              error={Boolean(errors.effectiveFrom)}
              helperText={errors.effectiveFrom?.message}
              {...register('effectiveFrom')}
            />
            <TextField
              label="Effective to (optional)"
              type="date"
              fullWidth
              InputLabelProps={{ shrink: true }}
              disabled={historicalLock}
              error={Boolean(errors.effectiveTo)}
              helperText={errors.effectiveTo?.message}
              {...register('effectiveTo')}
            />
          </Stack>

          <Controller
            name="days"
            control={control}
            render={({ field }) => (
              <WeekdayScheduleEditor
                value={field.value}
                onChange={field.onChange}
                disabled={historicalLock}
                error={errors.days?.message || errors.days?.root?.message}
              />
            )}
          />

          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} justifyContent="flex-end">
            <Button component={RouterLink} to="/working-hours" disabled={submitting}>
              Cancel
            </Button>
            <Button type="submit" variant="contained" disabled={submitting || historicalLock}>
              {submitting ? 'Saving…' : isEdit ? 'Save changes' : 'Create working hours'}
            </Button>
          </Stack>
        </Stack>
      </Paper>
    </Stack>
  )
}
