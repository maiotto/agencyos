import { useState } from 'react'
import { Link as RouterLink, useNavigate } from 'react-router-dom'
import {
  Alert,
  Button,
  IconButton,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline'
import { createPortfolio } from '../api/portfolio'
import { getErrorMessage } from '../api/client'
import { DEFAULT_COMPANY_ID } from '../theme'

interface MissionRow {
  missionId: string
  priority: string
}

export function PortfolioFormPage() {
  const navigate = useNavigate()
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [companyId, setCompanyId] = useState(DEFAULT_COMPANY_ID)
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [periodStart, setPeriodStart] = useState('2026-07-01')
  const [periodEnd, setPeriodEnd] = useState('2026-09-30')
  const [planningTemplateId, setPlanningTemplateId] = useState('')
  const [missions, setMissions] = useState<MissionRow[]>([{ missionId: '', priority: '1' }])

  const onSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
    setSaving(true)
    setError(null)
    try {
      const created = await createPortfolio({
        companyId,
        name: name.trim(),
        description: description.trim() || undefined,
        planningPeriodStart: periodStart,
        planningPeriodEnd: periodEnd,
        planningTemplateId: planningTemplateId.trim() || undefined,
        missions: missions
          .filter((mission) => mission.missionId.trim())
          .map((mission) => ({
            missionId: mission.missionId.trim(),
            priority: Number(mission.priority) || 1,
          })),
      })
      navigate(`/portfolios/${created.id}`)
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to create portfolio.'))
    } finally {
      setSaving(false)
    }
  }

  return (
    <Stack spacing={3} component="form" onSubmit={onSubmit}>
      <Stack spacing={1}>
        <Button component={RouterLink} to="/portfolios" size="small">
          ← Portfolios
        </Button>
        <Typography variant="h4">Create Portfolio</Typography>
        <Typography color="text.secondary">
          Associate at least one Active Mission (PLANNED or IN_PROGRESS). Does not modify Mission
          planning.
        </Typography>
      </Stack>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 3 }}>
        <Stack spacing={2}>
          <TextField
            label="Company Id"
            required
            value={companyId}
            onChange={(event) => setCompanyId(event.target.value)}
          />
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
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <TextField
              label="Period Start"
              type="date"
              required
              InputLabelProps={{ shrink: true }}
              value={periodStart}
              onChange={(event) => setPeriodStart(event.target.value)}
              fullWidth
            />
            <TextField
              label="Period End"
              type="date"
              required
              InputLabelProps={{ shrink: true }}
              value={periodEnd}
              onChange={(event) => setPeriodEnd(event.target.value)}
              fullWidth
            />
          </Stack>
          <TextField
            label="Planning Template Id (optional)"
            value={planningTemplateId}
            onChange={(event) => setPlanningTemplateId(event.target.value)}
          />

          <Typography variant="h6">Missions</Typography>
          {missions.map((mission, index) => (
            <Stack key={index} direction={{ xs: 'column', sm: 'row' }} spacing={1}>
              <TextField
                label="Mission Id"
                required
                value={mission.missionId}
                onChange={(event) => {
                  const next = [...missions]
                  next[index] = { ...next[index], missionId: event.target.value }
                  setMissions(next)
                }}
                fullWidth
              />
              <TextField
                label="Priority"
                type="number"
                value={mission.priority}
                onChange={(event) => {
                  const next = [...missions]
                  next[index] = { ...next[index], priority: event.target.value }
                  setMissions(next)
                }}
                sx={{ width: { sm: 120 } }}
              />
              <IconButton
                aria-label="Remove mission row"
                disabled={missions.length === 1}
                onClick={() => setMissions(missions.filter((_, i) => i !== index))}
              >
                <DeleteOutlineIcon />
              </IconButton>
            </Stack>
          ))}
          <Button
            onClick={() => setMissions([...missions, { missionId: '', priority: String(missions.length + 1) }])}
          >
            Add Mission
          </Button>

          <Stack direction="row" spacing={1}>
            <Button type="submit" variant="contained" disabled={saving}>
              {saving ? 'Creating…' : 'Create Portfolio'}
            </Button>
            <Button component={RouterLink} to="/portfolios">
              Cancel
            </Button>
          </Stack>
        </Stack>
      </Paper>
    </Stack>
  )
}
