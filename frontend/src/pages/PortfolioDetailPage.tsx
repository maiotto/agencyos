import { useCallback, useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Paper,
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
  activatePortfolio,
  assignPortfolioPlanningTemplate,
  associatePortfolioMission,
  calculatePortfolioCapacity,
  calculatePortfolioHealth,
  calculatePortfolioWorkload,
  deactivatePortfolio,
  deletePortfolio,
  getPortfolio,
  getPortfolioHealth,
  getPortfolioSummary,
  removePortfolioMission,
  updatePortfolio,
} from '../api/portfolio'
import { getErrorMessage } from '../api/client'
import { PortfolioHealthBadge, PortfolioStatusBadge } from '../components/PortfolioBadges'
import type { Portfolio, PortfolioHealthResponse, PortfolioSummary } from '../types/portfolio'

export function PortfolioDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [item, setItem] = useState<Portfolio | null>(null)
  const [summary, setSummary] = useState<PortfolioSummary | null>(null)
  const [health, setHealth] = useState<PortfolioHealthResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [periodStart, setPeriodStart] = useState('')
  const [periodEnd, setPeriodEnd] = useState('')
  const [templateId, setTemplateId] = useState('')
  const [missionId, setMissionId] = useState('')
  const [missionPriority, setMissionPriority] = useState('1')

  const load = useCallback(async () => {
    if (!id) return
    setLoading(true)
    setError(null)
    try {
      const [portfolio, portfolioSummary, portfolioHealth] = await Promise.all([
        getPortfolio(id),
        getPortfolioSummary(id),
        getPortfolioHealth(id),
      ])
      setItem(portfolio)
      setSummary(portfolioSummary)
      setHealth(portfolioHealth)
      setName(portfolio.name)
      setDescription(portfolio.description ?? '')
      setPeriodStart(portfolio.planningPeriodStart)
      setPeriodEnd(portfolio.planningPeriodEnd)
      setTemplateId(portfolio.planningTemplateId ?? '')
    } catch (err) {
      setItem(null)
      setSummary(null)
      setHealth(null)
      setError(getErrorMessage(err, 'Failed to load portfolio.'))
    } finally {
      setLoading(false)
    }
  }, [id])

  useEffect(() => {
    void load()
  }, [load])

  const run = async (action: () => Promise<unknown>, fallback: string) => {
    setBusy(true)
    setError(null)
    try {
      await action()
      await load()
    } catch (err) {
      setError(getErrorMessage(err, fallback))
    } finally {
      setBusy(false)
    }
  }

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
        <CircularProgress />
      </Box>
    )
  }

  if (!item) {
    return (
      <Stack spacing={2}>
        {error && <Alert severity="error">{error}</Alert>}
        <Button component={RouterLink} to="/portfolios" variant="outlined">
          ← Portfolios
        </Button>
      </Stack>
    )
  }

  const isActive = item.status === 'Active'

  return (
    <Stack spacing={3}>
      <Stack spacing={1}>
        <Button component={RouterLink} to="/portfolios" size="small" sx={{ alignSelf: 'flex-start' }}>
          ← Portfolios
        </Button>
        <Stack
          direction={{ xs: 'column', sm: 'row' }}
          justifyContent="space-between"
          spacing={2}
          alignItems={{ xs: 'stretch', sm: 'center' }}
        >
          <Box>
            <Typography variant="h4">{item.name}</Typography>
            <Typography color="text.secondary">
              {item.planningPeriodStart} → {item.planningPeriodEnd}
            </Typography>
          </Box>
          <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
            <PortfolioStatusBadge status={item.status} />
            <PortfolioHealthBadge health={item.portfolioHealth} />
            {isActive ? (
              <Button
                variant="outlined"
                disabled={busy}
                onClick={() => void run(() => deactivatePortfolio(item.id), 'Failed to deactivate.')}
              >
                Deactivate
              </Button>
            ) : (
              <Button
                variant="outlined"
                disabled={busy}
                onClick={() => void run(() => activatePortfolio(item.id), 'Failed to activate.')}
              >
                Activate
              </Button>
            )}
            <Button
              color="error"
              variant="outlined"
              disabled={busy || isActive}
              onClick={() =>
                void run(async () => {
                  await deletePortfolio(item.id)
                  navigate('/portfolios')
                }, 'Failed to delete portfolio.')
              }
            >
              Delete
            </Button>
          </Stack>
        </Stack>
      </Stack>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Portfolio Summary
        </Typography>
        <Stack spacing={1}>
          <Typography>
            <strong>Missions:</strong> {summary?.missionCount ?? item.missions.length}
          </Typography>
          <Typography>
            <strong>Capacity utilization:</strong>{' '}
            {summary?.capacity?.overallUtilizationPercentage ?? '—'}%
          </Typography>
          <Typography>
            <strong>Workload:</strong> {summary?.workload?.overallWorkloadPercentage ?? '—'}%
          </Typography>
          <Typography>
            <strong>Historical capacity avg:</strong>{' '}
            {summary?.historicalCapacity?.averageUtilizationPercentage ?? '—'}%
          </Typography>
          <Typography>
            <strong>Historical workload avg:</strong>{' '}
            {summary?.historicalWorkload?.averageWorkloadPercentage ?? '—'}%
          </Typography>
        </Stack>
        <Stack direction="row" spacing={1} sx={{ mt: 2 }} flexWrap="wrap" useFlexGap>
          <Button
            size="small"
            variant="contained"
            disabled={busy}
            onClick={() =>
              void run(() => calculatePortfolioCapacity(item.id), 'Failed to calculate capacity.')
            }
          >
            Calculate Capacity
          </Button>
          <Button
            size="small"
            variant="contained"
            disabled={busy}
            onClick={() =>
              void run(() => calculatePortfolioWorkload(item.id), 'Failed to calculate workload.')
            }
          >
            Calculate Workload
          </Button>
          <Button
            size="small"
            variant="contained"
            disabled={busy}
            onClick={() =>
              void run(() => calculatePortfolioHealth(item.id), 'Failed to calculate health.')
            }
          >
            Calculate Health
          </Button>
        </Stack>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Portfolio Health
        </Typography>
        <Stack spacing={1}>
          <Typography>
            <strong>Status:</strong> {health?.portfolioHealth ?? item.portfolioHealth}
          </Typography>
          <Typography>
            <strong>Utilization / Workload:</strong> {health?.utilizationPercentage ?? '—'}% /{' '}
            {health?.workloadPercentage ?? '—'}%
          </Typography>
          <Typography>
            <strong>Warning threshold:</strong> {health?.warningPercentage ?? '—'}%
          </Typography>
        </Stack>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Edit Portfolio
        </Typography>
        <Stack spacing={2}>
          <TextField
            label="Name"
            value={name}
            onChange={(event) => setName(event.target.value)}
            disabled={!isActive || busy}
          />
          <TextField
            label="Description"
            value={description}
            onChange={(event) => setDescription(event.target.value)}
            disabled={!isActive || busy}
            multiline
            minRows={2}
          />
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <TextField
              label="Period Start"
              type="date"
              InputLabelProps={{ shrink: true }}
              value={periodStart}
              onChange={(event) => setPeriodStart(event.target.value)}
              disabled={!isActive || busy}
              fullWidth
            />
            <TextField
              label="Period End"
              type="date"
              InputLabelProps={{ shrink: true }}
              value={periodEnd}
              onChange={(event) => setPeriodEnd(event.target.value)}
              disabled={!isActive || busy}
              fullWidth
            />
          </Stack>
          <Button
            variant="outlined"
            disabled={!isActive || busy}
            onClick={() =>
              void run(
                () =>
                  updatePortfolio(item.id, {
                    name: name.trim(),
                    description: description.trim() || undefined,
                    planningPeriodStart: periodStart,
                    planningPeriodEnd: periodEnd,
                  }),
                'Failed to update portfolio.',
              )
            }
          >
            Save Changes
          </Button>
        </Stack>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Planning Template
        </Typography>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
          <TextField
            label="Planning Template Id"
            value={templateId}
            onChange={(event) => setTemplateId(event.target.value)}
            disabled={!isActive || busy}
            fullWidth
          />
          <Button
            variant="outlined"
            disabled={!isActive || busy}
            onClick={() =>
              void run(
                () => assignPortfolioPlanningTemplate(item.id, templateId.trim() || null),
                'Failed to assign template.',
              )
            }
          >
            Assign
          </Button>
        </Stack>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Missions
        </Typography>
        <Table size="small" sx={{ mb: 2 }}>
          <TableHead>
            <TableRow>
              <TableCell>Mission Id</TableCell>
              <TableCell>Priority</TableCell>
              <TableCell>Included</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {item.missions.map((mission) => (
              <TableRow key={mission.id}>
                <TableCell sx={{ wordBreak: 'break-all' }}>{mission.missionId}</TableCell>
                <TableCell>{mission.priority}</TableCell>
                <TableCell>{new Date(mission.includedAt).toLocaleString()}</TableCell>
                <TableCell align="right">
                  <Button
                    size="small"
                    color="error"
                    disabled={!isActive || busy || item.missions.length <= 1}
                    onClick={() =>
                      void run(
                        () => removePortfolioMission(item.id, mission.missionId),
                        'Failed to remove mission.',
                      )
                    }
                  >
                    Remove
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
          <TextField
            label="Mission Id"
            value={missionId}
            onChange={(event) => setMissionId(event.target.value)}
            disabled={!isActive || busy}
            fullWidth
          />
          <TextField
            label="Priority"
            type="number"
            value={missionPriority}
            onChange={(event) => setMissionPriority(event.target.value)}
            disabled={!isActive || busy}
            sx={{ width: { sm: 120 } }}
          />
          <Button
            variant="contained"
            disabled={!isActive || busy || !missionId.trim()}
            onClick={() =>
              void run(async () => {
                await associatePortfolioMission(item.id, {
                  missionId: missionId.trim(),
                  priority: Number(missionPriority) || 1,
                })
                setMissionId('')
              }, 'Failed to associate mission.')
            }
          >
            Associate
          </Button>
        </Stack>
      </Paper>
    </Stack>
  )
}
