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
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import {
  createWorkingCalendar,
  getWorkingCalendar,
  updateWorkingCalendar,
} from '../api/workingCalendars'
import { getErrorMessage } from '../api/client'
import { WorkingDaySelector } from '../components/WorkingDaySelector'
import { DEFAULT_COMPANY_ID } from '../theme'
import { WORKING_DAYS } from '../types/workingCalendar'

const formSchema = z
  .object({
    name: z.string().trim().min(1, 'Calendar Name is mandatory.').max(200),
    effectiveFrom: z.string().min(1, 'EffectiveFrom is mandatory.'),
    effectiveTo: z.string().optional(),
    workingDays: z
      .array(z.string())
      .min(1, 'Calendar must contain at least one working day.')
      .refine((days) => new Set(days.map((day) => day.toLowerCase())).size === days.length, {
        message: 'Working days cannot be duplicated.',
      })
      .refine((days) => days.every((day) => WORKING_DAYS.includes(day as (typeof WORKING_DAYS)[number])), {
        message: 'Working day must be a valid weekday name.',
      }),
  })
  .refine(
    (values) =>
      !values.effectiveTo ||
      values.effectiveTo.length === 0 ||
      values.effectiveTo >= values.effectiveFrom,
    {
      message: 'EffectiveTo cannot be earlier than EffectiveFrom.',
      path: ['effectiveTo'],
    },
  )

type FormValues = z.infer<typeof formSchema>

const defaultValues: FormValues = {
  name: '',
  effectiveFrom: '',
  effectiveTo: '',
  workingDays: ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday'],
}

export function WorkingCalendarFormPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const isEdit = Boolean(id)
  const [loading, setLoading] = useState(isEdit)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [historicalLock, setHistoricalLock] = useState(false)

  const {
    control,
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues,
  })

  useEffect(() => {
    if (!id) {
      return
    }

    let cancelled = false

    const load = async () => {
      setLoading(true)
      setError(null)

      try {
        const calendar = await getWorkingCalendar(id)
        if (cancelled) {
          return
        }

        const today = new Date().toISOString().slice(0, 10)
        setHistoricalLock(calendar.effectiveFrom <= today)
        reset({
          name: calendar.name,
          effectiveFrom: calendar.effectiveFrom,
          effectiveTo: calendar.effectiveTo ?? '',
          workingDays: calendar.workingDays,
        })
      } catch (err) {
        if (!cancelled) {
          setError(getErrorMessage(err, 'Failed to load working calendar.'))
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
    () => (isEdit ? 'Edit working calendar' : 'Create working calendar'),
    [isEdit],
  )

  const onSubmit = handleSubmit(async (values) => {
    setSubmitting(true)
    setError(null)

    const payload = {
      name: values.name.trim(),
      effectiveFrom: values.effectiveFrom,
      effectiveTo: values.effectiveTo ? values.effectiveTo : null,
      workingDays: values.workingDays,
    }

    try {
      if (isEdit && id) {
        await updateWorkingCalendar(id, payload)
      } else {
        await createWorkingCalendar({
          ...payload,
          companyId: DEFAULT_COMPANY_ID,
        })
      }

      navigate('/working-calendars')
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to save working calendar.'))
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
    <Stack spacing={3} maxWidth={720}>
      <Box>
        <Typography variant="h4" gutterBottom>
          {title}
        </Typography>
        <Typography color="text.secondary">
          Define the calendar name, validity period, and working days for Capacity Planning.
        </Typography>
      </Box>

      {historicalLock ? (
        <Alert severity="warning">
          This calendar has already started. Structural changes are blocked to protect historical
          planning (BR-109).
        </Alert>
      ) : null}

      {error ? <Alert severity="error">{error}</Alert> : null}

      <Paper sx={{ p: { xs: 2.5, md: 3.5 } }}>
        <Stack component="form" spacing={3} onSubmit={onSubmit} noValidate>
          <TextField
            label="Calendar name"
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
            name="workingDays"
            control={control}
            render={({ field }) => (
              <WorkingDaySelector
                value={field.value}
                onChange={field.onChange}
                disabled={historicalLock}
                error={errors.workingDays?.message}
              />
            )}
          />

          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} justifyContent="flex-end">
            <Button component={RouterLink} to="/working-calendars" disabled={submitting}>
              Cancel
            </Button>
            <Button type="submit" variant="contained" disabled={submitting || historicalLock}>
              {submitting ? 'Saving…' : isEdit ? 'Save changes' : 'Create calendar'}
            </Button>
          </Stack>
        </Stack>
      </Paper>
    </Stack>
  )
}
