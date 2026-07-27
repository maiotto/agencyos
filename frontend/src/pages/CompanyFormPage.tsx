import { useEffect, useMemo, useState } from 'react'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
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
import { createCompany, getCompany, updateCompany } from '../api/companies'
import { getErrorMessage } from '../api/client'
import { useCompany } from '../context/CompanyContext'

const formSchema = z.object({
  companyCode: z.string().trim().min(1, 'CompanyCode is mandatory (BR-2002).').max(50),
  companyName: z.string().trim().min(1, 'CompanyName is mandatory (BR-2001).').max(200),
  legalName: z.string().max(300).optional(),
  timezone: z.string().trim().min(1, 'Timezone is mandatory.').max(100),
  country: z.string().max(100).optional(),
  language: z.string().max(20).optional(),
  currency: z.string().max(10).optional(),
})

type FormValues = z.infer<typeof formSchema>

const defaultValues: FormValues = {
  companyCode: '',
  companyName: '',
  legalName: '',
  timezone: 'UTC',
  country: '',
  language: '',
  currency: '',
}

export function CompanyFormPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { refreshCompanies } = useCompany()
  const isEdit = Boolean(id)
  const [loading, setLoading] = useState(isEdit)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const {
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
        const company = await getCompany(id)
        if (cancelled) {
          return
        }

        reset({
          companyCode: company.companyCode,
          companyName: company.companyName,
          legalName: company.legalName ?? '',
          timezone: company.timezone,
          country: company.country ?? '',
          language: company.language ?? '',
          currency: company.currency ?? '',
        })
      } catch (err) {
        if (!cancelled) {
          setError(getErrorMessage(err, 'Failed to load company.'))
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

  const title = useMemo(() => (isEdit ? 'Edit company' : 'Create company'), [isEdit])

  const onSubmit = handleSubmit(async (values) => {
    setSubmitting(true)
    setError(null)

    const sharedPayload = {
      companyName: values.companyName.trim(),
      legalName: values.legalName?.trim() || null,
      timezone: values.timezone.trim(),
      country: values.country?.trim() || null,
      language: values.language?.trim() || null,
      currency: values.currency?.trim() || null,
    }

    try {
      if (isEdit && id) {
        const updated = await updateCompany(id, sharedPayload)
        await refreshCompanies()
        navigate(`/companies/${updated.id}`)
      } else {
        const created = await createCompany({
          companyCode: values.companyCode.trim(),
          ...sharedPayload,
        })
        await refreshCompanies()
        navigate(`/companies/${created.id}`)
      }
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to save company.'))
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
          Configure the multi-company tenant used to scope AI-generated artifacts (US-402).
        </Typography>
      </Box>

      {error ? <Alert severity="error">{error}</Alert> : null}

      <Paper sx={{ p: { xs: 2.5, md: 3.5 } }}>
        <Stack component="form" spacing={3} onSubmit={onSubmit} noValidate>
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <TextField
              label="Company Code"
              fullWidth
              disabled={isEdit}
              error={Boolean(errors.companyCode)}
              helperText={
                errors.companyCode?.message ||
                (isEdit ? 'CompanyCode cannot be changed after creation (BR-2002).' : undefined)
              }
              {...register('companyCode')}
            />
            <TextField
              label="Company Name"
              fullWidth
              error={Boolean(errors.companyName)}
              helperText={errors.companyName?.message}
              {...register('companyName')}
            />
          </Stack>

          <TextField
            label="Legal Name"
            fullWidth
            error={Boolean(errors.legalName)}
            helperText={errors.legalName?.message}
            {...register('legalName')}
          />

          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <TextField
              label="Timezone"
              fullWidth
              error={Boolean(errors.timezone)}
              helperText={errors.timezone?.message}
              {...register('timezone')}
            />
            <TextField
              label="Country"
              fullWidth
              error={Boolean(errors.country)}
              helperText={errors.country?.message}
              {...register('country')}
            />
          </Stack>

          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <TextField
              label="Language"
              fullWidth
              error={Boolean(errors.language)}
              helperText={errors.language?.message}
              {...register('language')}
            />
            <TextField
              label="Currency"
              fullWidth
              error={Boolean(errors.currency)}
              helperText={errors.currency?.message}
              {...register('currency')}
            />
          </Stack>

          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} justifyContent="flex-end">
            <Button component={RouterLink} to="/companies" disabled={submitting}>
              Cancel
            </Button>
            <Button type="submit" variant="contained" disabled={submitting}>
              {submitting ? 'Saving…' : isEdit ? 'Save changes' : 'Create company'}
            </Button>
          </Stack>
        </Stack>
      </Paper>
    </Stack>
  )
}
