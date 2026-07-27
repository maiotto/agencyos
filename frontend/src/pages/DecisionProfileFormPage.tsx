import { useEffect, useMemo, useState } from 'react'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useFieldArray, useForm } from 'react-hook-form'
import { z } from 'zod'
import {
  Alert,
  Box,
  Button,
  Checkbox,
  CircularProgress,
  FormControlLabel,
  MenuItem,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import {
  createDecisionProfile,
  getDecisionProfile,
  updateDecisionProfile,
} from '../api/decisionProfiles'
import { getErrorMessage } from '../api/client'
import { DEFAULT_COMPANY_ID } from '../theme'
import { RANKING_DIMENSIONS } from '../types/decisionProfile'

const dimensionSchema = z.object({
  dimension: z.enum(RANKING_DIMENSIONS),
  weight: z.coerce.number().min(0).max(1),
  preferHigherValues: z.boolean(),
})

const formSchema = z
  .object({
    code: z.string().trim().min(1, 'Code is mandatory.').max(100),
    name: z.string().trim().min(1, 'Name is mandatory (BR-1902).').max(200),
    description: z.string().max(2000).optional(),
    priorityWeights: z.array(dimensionSchema).min(1),
    capacityWeight: z.coerce.number().min(0).max(1),
    workloadWeight: z.coerce.number().min(0).max(1),
    costWeight: z.coerce.number().min(0).max(1),
    riskWeight: z.coerce.number().min(0).max(1),
    qualityWeight: z.coerce.number().min(0).max(1),
    preferredStrategy: z.string().max(200).optional(),
    preferredCapacityThreshold: z
      .union([z.coerce.number().min(0).max(100), z.literal('')])
      .optional(),
    preferredWorkloadThreshold: z
      .union([z.coerce.number().min(0).max(100), z.literal('')])
      .optional(),
    defaultProfile: z.boolean(),
  })
  .superRefine((values, ctx) => {
    if (!values.priorityWeights.some((weight) => weight.weight > 0)) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'At least one dimension must have a positive weight.',
        path: ['priorityWeights'],
      })
    }
  })

type FormValues = z.infer<typeof formSchema>

const defaultDimensionWeight = 1 / RANKING_DIMENSIONS.length

const defaultValues: FormValues = {
  code: '',
  name: '',
  description: '',
  priorityWeights: RANKING_DIMENSIONS.map((dimension) => ({
    dimension,
    weight: Number(defaultDimensionWeight.toFixed(3)),
    preferHigherValues:
      dimension === 'HumanResourceUsage' ||
      dimension === 'AiResourceUsage' ||
      dimension === 'ExternalResourceUsage' ||
      dimension === 'AutomationUsage',
  })),
  capacityWeight: 0.2,
  workloadWeight: 0.2,
  costWeight: 0.2,
  riskWeight: 0.2,
  qualityWeight: 0.2,
  preferredStrategy: '',
  preferredCapacityThreshold: '',
  preferredWorkloadThreshold: '',
  defaultProfile: false,
}

export function DecisionProfileFormPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const isEdit = Boolean(id)
  const [loading, setLoading] = useState(isEdit)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

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

  const { fields } = useFieldArray({ control, name: 'priorityWeights' })

  useEffect(() => {
    if (!id) {
      return
    }

    let cancelled = false

    const load = async () => {
      setLoading(true)
      setError(null)

      try {
        const profile = await getDecisionProfile(id)
        if (cancelled) {
          return
        }

        reset({
          code: profile.code,
          name: profile.name,
          description: profile.description ?? '',
          priorityWeights: RANKING_DIMENSIONS.map((dimension) => {
            const existing = profile.dimensions.find((item) => item.dimension === dimension)
            return {
              dimension,
              weight: existing?.weight ?? 0,
              preferHigherValues: existing?.preferHigherValues ?? false,
            }
          }),
          capacityWeight: profile.capacityWeight,
          workloadWeight: profile.workloadWeight,
          costWeight: profile.costWeight,
          riskWeight: profile.riskWeight,
          qualityWeight: profile.qualityWeight,
          preferredStrategy: profile.preferredStrategy ?? '',
          preferredCapacityThreshold: profile.preferredCapacityThreshold ?? '',
          preferredWorkloadThreshold: profile.preferredWorkloadThreshold ?? '',
          defaultProfile: profile.defaultProfile,
        })
      } catch (err) {
        if (!cancelled) {
          setError(getErrorMessage(err, 'Failed to load decision profile.'))
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

  const title = useMemo(() => (isEdit ? 'Edit decision profile' : 'Create decision profile'), [isEdit])

  const weightSum = (weights: FormValues['priorityWeights']) =>
    weights.reduce((sum, weight) => sum + Number(weight.weight || 0), 0)

  const onSubmit = handleSubmit(async (values) => {
    setSubmitting(true)
    setError(null)

    const sharedPayload = {
      description: values.description?.trim() || null,
      priorityWeights: values.priorityWeights.map((weight) => ({
        dimension: weight.dimension,
        weight: Number(weight.weight),
        preferHigherValues: weight.preferHigherValues,
      })),
      capacityWeight: Number(values.capacityWeight),
      workloadWeight: Number(values.workloadWeight),
      costWeight: Number(values.costWeight),
      riskWeight: Number(values.riskWeight),
      qualityWeight: Number(values.qualityWeight),
      preferredStrategy: values.preferredStrategy?.trim() || null,
      preferredCapacityThreshold:
        values.preferredCapacityThreshold === '' || values.preferredCapacityThreshold === undefined
          ? null
          : Number(values.preferredCapacityThreshold),
      preferredWorkloadThreshold:
        values.preferredWorkloadThreshold === '' || values.preferredWorkloadThreshold === undefined
          ? null
          : Number(values.preferredWorkloadThreshold),
    }

    try {
      if (isEdit && id) {
        const updated = await updateDecisionProfile(id, {
          name: values.name.trim(),
          ...sharedPayload,
        })
        navigate(`/decision-profiles/${updated.id}`)
      } else {
        const created = await createDecisionProfile({
          companyId: DEFAULT_COMPANY_ID,
          code: values.code.trim(),
          name: values.name.trim(),
          defaultProfile: values.defaultProfile,
          ...sharedPayload,
        })
        navigate(`/decision-profiles/${created.id}`)
      }
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to save decision profile.'))
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
          Configure ranking dimension weights used by Delivery Strategy ranking and AI Decision
          Support.{' '}
          {isEdit
            ? 'Saving creates a new immutable version and deactivates the current one (BR-1905).'
            : ''}
        </Typography>
      </Box>

      {error ? <Alert severity="error">{error}</Alert> : null}

      <Paper sx={{ p: { xs: 2.5, md: 3.5 } }}>
        <Stack component="form" spacing={3} onSubmit={onSubmit} noValidate>
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <TextField
              label="Code"
              fullWidth
              disabled={isEdit}
              error={Boolean(errors.code)}
              helperText={errors.code?.message || (isEdit ? 'Code cannot change across versions.' : undefined)}
              {...register('code')}
            />
            <TextField
              label="Name"
              fullWidth
              error={Boolean(errors.name)}
              helperText={errors.name?.message}
              {...register('name')}
            />
          </Stack>

          <TextField
            label="Description"
            fullWidth
            multiline
            minRows={2}
            error={Boolean(errors.description)}
            helperText={errors.description?.message}
            {...register('description')}
          />

          <Box>
            <Typography variant="subtitle1" gutterBottom>
              Ranking dimension weights
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Weights should sum to approximately 1.0 across dimensions used for ranking. Current
              total:{' '}
              <Controller
                control={control}
                name="priorityWeights"
                render={({ field }) => <span>{weightSum(field.value).toFixed(3)}</span>}
              />
            </Typography>
            {errors.priorityWeights?.message ? (
              <Alert severity="warning" sx={{ mb: 2 }}>
                {errors.priorityWeights.message}
              </Alert>
            ) : null}
            <Stack direction="row" flexWrap="wrap" useFlexGap sx={{ gap: 2 }}>
              {fields.map((field, index) => (
                <Paper
                  key={field.id}
                  variant="outlined"
                  sx={{ p: 2, flex: '1 1 260px', minWidth: 260 }}
                >
                  <Typography fontWeight={600} gutterBottom>
                    {field.dimension}
                  </Typography>
                  <Stack direction="row" spacing={2} alignItems="center">
                    <TextField
                      label="Weight"
                      type="number"
                      size="small"
                      inputProps={{ step: 0.01, min: 0, max: 1 }}
                      error={Boolean(errors.priorityWeights?.[index]?.weight)}
                      {...register(`priorityWeights.${index}.weight` as const)}
                    />
                    <Controller
                      control={control}
                      name={`priorityWeights.${index}.preferHigherValues` as const}
                      render={({ field: checkboxField }) => (
                        <FormControlLabel
                          control={
                            <Checkbox
                              checked={checkboxField.value}
                              onChange={(event) => checkboxField.onChange(event.target.checked)}
                            />
                          }
                          label="Prefer higher"
                        />
                      )}
                    />
                  </Stack>
                </Paper>
              ))}
            </Stack>
          </Box>

          <Box>
            <Typography variant="subtitle1" gutterBottom>
              Category weights
            </Typography>
            <Stack direction="row" flexWrap="wrap" useFlexGap sx={{ gap: 2 }}>
              <TextField
                label="Capacity"
                type="number"
                sx={{ flex: '1 1 160px' }}
                inputProps={{ step: 0.01, min: 0, max: 1 }}
                error={Boolean(errors.capacityWeight)}
                {...register('capacityWeight')}
              />
              <TextField
                label="Workload"
                type="number"
                sx={{ flex: '1 1 160px' }}
                inputProps={{ step: 0.01, min: 0, max: 1 }}
                error={Boolean(errors.workloadWeight)}
                {...register('workloadWeight')}
              />
              <TextField
                label="Cost"
                type="number"
                sx={{ flex: '1 1 160px' }}
                inputProps={{ step: 0.01, min: 0, max: 1 }}
                error={Boolean(errors.costWeight)}
                {...register('costWeight')}
              />
              <TextField
                label="Risk"
                type="number"
                sx={{ flex: '1 1 160px' }}
                inputProps={{ step: 0.01, min: 0, max: 1 }}
                error={Boolean(errors.riskWeight)}
                {...register('riskWeight')}
              />
              <TextField
                label="Quality"
                type="number"
                sx={{ flex: '1 1 160px' }}
                inputProps={{ step: 0.01, min: 0, max: 1 }}
                error={Boolean(errors.qualityWeight)}
                {...register('qualityWeight')}
              />
            </Stack>
          </Box>

          <Box>
            <Typography variant="subtitle1" gutterBottom>
              Preferred strategy (optional)
            </Typography>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <Controller
                name="preferredStrategy"
                control={control}
                render={({ field }) => (
                  <TextField select label="Preferred strategy" fullWidth {...field} value={field.value ?? ''}>
                    <MenuItem value="">None</MenuItem>
                    <MenuItem value="Internal">Internal</MenuItem>
                    <MenuItem value="External">External</MenuItem>
                    <MenuItem value="Hybrid">Hybrid</MenuItem>
                    <MenuItem value="AI-Assisted">AI-Assisted</MenuItem>
                  </TextField>
                )}
              />
              <TextField
                label="Preferred capacity threshold (%)"
                type="number"
                fullWidth
                inputProps={{ step: 1, min: 0, max: 100 }}
                error={Boolean(errors.preferredCapacityThreshold)}
                {...register('preferredCapacityThreshold')}
              />
              <TextField
                label="Preferred workload threshold (%)"
                type="number"
                fullWidth
                inputProps={{ step: 1, min: 0, max: 100 }}
                error={Boolean(errors.preferredWorkloadThreshold)}
                {...register('preferredWorkloadThreshold')}
              />
            </Stack>
          </Box>

          {!isEdit ? (
            <Controller
              name="defaultProfile"
              control={control}
              render={({ field }) => (
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={field.value}
                      onChange={(event) => field.onChange(event.target.checked)}
                    />
                  }
                  label="Set as default profile for this company (BR-1901)"
                />
              )}
            />
          ) : null}

          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} justifyContent="flex-end">
            <Button component={RouterLink} to="/decision-profiles" disabled={submitting}>
              Cancel
            </Button>
            <Button type="submit" variant="contained" disabled={submitting}>
              {submitting ? 'Saving…' : isEdit ? 'Save as new version' : 'Create profile'}
            </Button>
          </Stack>
        </Stack>
      </Paper>
    </Stack>
  )
}
