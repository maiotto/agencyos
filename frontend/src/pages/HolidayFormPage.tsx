import { useEffect, useMemo, useState } from 'react'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'
import {
  Alert,
  Box,
  Button,
  Checkbox,
  CircularProgress,
  FormControl,
  FormControlLabel,
  FormHelperText,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { createHoliday, getHoliday, updateHoliday } from '../api/holidays'
import { getErrorMessage } from '../api/client'
import { DEFAULT_COMPANY_ID } from '../theme'
import { HOLIDAY_TYPES } from '../types/holiday'

const formSchema = z
  .object({
    name: z.string().trim().min(1, 'Holiday Name is mandatory.').max(200),
    description: z.string().max(4000).optional(),
    holidayType: z.enum(HOLIDAY_TYPES),
    holidayDate: z.string().min(1, 'Holiday Date is mandatory.'),
    companyId: z.string().optional(),
    stateCode: z.string().optional(),
    city: z.string().optional(),
    recurring: z.boolean(),
  })
  .superRefine((values, ctx) => {
    if (values.holidayType === 'Company' && !values.companyId?.trim()) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'Company Holiday requires CompanyId.',
        path: ['companyId'],
      })
    }

    if (
      (values.holidayType === 'State' || values.holidayType === 'Municipal') &&
      !values.stateCode?.trim()
    ) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'StateCode is required for State and Municipal holidays.',
        path: ['stateCode'],
      })
    }

    if (values.holidayType === 'Municipal' && !values.city?.trim()) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'Municipal Holiday requires StateCode and City.',
        path: ['city'],
      })
    }
  })

type FormValues = z.infer<typeof formSchema>

const defaultValues: FormValues = {
  name: '',
  description: '',
  holidayType: 'National',
  holidayDate: '',
  companyId: DEFAULT_COMPANY_ID,
  stateCode: '',
  city: '',
  recurring: true,
}

export function HolidayFormPage() {
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
    watch,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues,
  })

  const holidayType = watch('holidayType')

  useEffect(() => {
    if (!id) {
      return
    }

    let cancelled = false

    const load = async () => {
      setLoading(true)
      setError(null)

      try {
        const holiday = await getHoliday(id)
        if (cancelled) {
          return
        }

        const today = new Date().toISOString().slice(0, 10)
        const occurrence = holiday.recurring
          ? `${today.slice(0, 4)}-${holiday.holidayDate.slice(5)}`
          : holiday.holidayDate
        setHistoricalLock(occurrence <= today)

        reset({
          name: holiday.name,
          description: holiday.description ?? '',
          holidayType: (HOLIDAY_TYPES.includes(holiday.holidayType as (typeof HOLIDAY_TYPES)[number])
            ? holiday.holidayType
            : 'National') as FormValues['holidayType'],
          holidayDate: holiday.holidayDate,
          companyId: holiday.companyId ?? DEFAULT_COMPANY_ID,
          stateCode: holiday.stateCode ?? '',
          city: holiday.city ?? '',
          recurring: holiday.recurring,
        })
      } catch (err) {
        if (!cancelled) {
          setError(getErrorMessage(err, 'Failed to load holiday.'))
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

  const title = useMemo(() => (isEdit ? 'Edit holiday' : 'Create holiday'), [isEdit])

  const onSubmit = handleSubmit(async (values) => {
    setSubmitting(true)
    setError(null)

    const payload = {
      name: values.name.trim(),
      description: values.description?.trim() || null,
      holidayType: values.holidayType,
      holidayDate: values.holidayDate,
      companyId:
        values.holidayType === 'Company' || values.companyId
          ? values.companyId || DEFAULT_COMPANY_ID
          : null,
      stateCode: values.holidayType === 'National' ? null : values.stateCode?.trim() || null,
      city: values.holidayType === 'Municipal' ? values.city?.trim() || null : null,
      recurring: values.recurring,
    }

    try {
      if (isEdit && id) {
        await updateHoliday(id, payload)
        navigate(`/holidays/${id}`)
      } else {
        const created = await createHoliday(payload)
        navigate(`/holidays/${created.id}`)
      }
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to save holiday.'))
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
          Define holiday type, date, recurrence, and geographic or company scope.
        </Typography>
      </Box>

      {historicalLock ? (
        <Alert severity="warning">
          This holiday has already occurred. Structural changes are blocked to protect historical
          planning (BR-213).
        </Alert>
      ) : null}

      {error ? <Alert severity="error">{error}</Alert> : null}

      <Paper sx={{ p: { xs: 2.5, md: 3.5 } }}>
        <Stack component="form" spacing={3} onSubmit={onSubmit} noValidate>
          <TextField
            label="Holiday name"
            fullWidth
            disabled={historicalLock}
            error={Boolean(errors.name)}
            helperText={errors.name?.message}
            {...register('name')}
          />

          <TextField
            label="Description"
            fullWidth
            multiline
            minRows={2}
            disabled={historicalLock}
            error={Boolean(errors.description)}
            helperText={errors.description?.message}
            {...register('description')}
          />

          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <Controller
              name="holidayType"
              control={control}
              render={({ field }) => (
                <FormControl fullWidth error={Boolean(errors.holidayType)} disabled={historicalLock}>
                  <InputLabel id="holiday-type-label">Holiday type</InputLabel>
                  <Select labelId="holiday-type-label" label="Holiday type" {...field}>
                    {HOLIDAY_TYPES.map((type) => (
                      <MenuItem key={type} value={type}>
                        {type}
                      </MenuItem>
                    ))}
                  </Select>
                  {errors.holidayType ? (
                    <FormHelperText>{errors.holidayType.message}</FormHelperText>
                  ) : null}
                </FormControl>
              )}
            />

            <TextField
              label="Holiday date"
              type="date"
              fullWidth
              InputLabelProps={{ shrink: true }}
              disabled={historicalLock}
              error={Boolean(errors.holidayDate)}
              helperText={errors.holidayDate?.message}
              {...register('holidayDate')}
            />
          </Stack>

          {(holidayType === 'Company' || holidayType === 'National' || holidayType === 'State' || holidayType === 'Municipal') && (
            <TextField
              label="Company ID"
              fullWidth
              disabled={historicalLock}
              error={Boolean(errors.companyId)}
              helperText={
                errors.companyId?.message ||
                (holidayType === 'Company'
                  ? 'Required for company holidays.'
                  : 'Optional association with a company configuration.')
              }
              {...register('companyId')}
            />
          )}

          {(holidayType === 'State' || holidayType === 'Municipal') && (
            <TextField
              label="State code"
              fullWidth
              disabled={historicalLock}
              error={Boolean(errors.stateCode)}
              helperText={errors.stateCode?.message}
              {...register('stateCode')}
            />
          )}

          {holidayType === 'Municipal' && (
            <TextField
              label="City"
              fullWidth
              disabled={historicalLock}
              error={Boolean(errors.city)}
              helperText={errors.city?.message}
              {...register('city')}
            />
          )}

          <Controller
            name="recurring"
            control={control}
            render={({ field }) => (
              <FormControlLabel
                control={
                  <Checkbox
                    checked={field.value}
                    onChange={(event) => field.onChange(event.target.checked)}
                    disabled={historicalLock}
                  />
                }
                label="Recurring every year"
              />
            )}
          />

          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} justifyContent="flex-end">
            <Button component={RouterLink} to="/holidays" disabled={submitting}>
              Cancel
            </Button>
            <Button type="submit" variant="contained" disabled={submitting || historicalLock}>
              {submitting ? 'Saving…' : isEdit ? 'Save changes' : 'Create holiday'}
            </Button>
          </Stack>
        </Stack>
      </Paper>
    </Stack>
  )
}
