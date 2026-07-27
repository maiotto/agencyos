import { useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import {
  activateDecisionProfile,
  archiveDecisionProfile,
  cloneDecisionProfile,
  deactivateDecisionProfile,
  getDecisionProfile,
  setDefaultDecisionProfile,
} from '../api/decisionProfiles'
import { getErrorMessage } from '../api/client'
import type { CompanyDecisionProfile } from '../types/decisionProfile'

function statusColor(status: string): 'success' | 'default' | 'warning' {
  if (status === 'Active') return 'success'
  if (status === 'Archived') return 'default'
  return 'warning'
}

export function DecisionProfileDetailPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [profile, setProfile] = useState<CompanyDecisionProfile | null>(null)
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [cloneOpen, setCloneOpen] = useState(false)
  const [cloneName, setCloneName] = useState('')
  const [cloneCode, setCloneCode] = useState('')

  const load = async () => {
    if (!id) {
      return
    }

    setLoading(true)
    setError(null)

    try {
      const data = await getDecisionProfile(id)
      setProfile(data)
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to load decision profile.'))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id])

  const runAction = async (action: () => Promise<unknown>) => {
    if (!id) {
      return
    }

    setBusy(true)
    setError(null)

    try {
      await action()
      await load()
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  const submitClone = async () => {
    if (!id) {
      return
    }

    setBusy(true)
    setError(null)

    try {
      const clone = await cloneDecisionProfile(id, { name: cloneName.trim(), code: cloneCode.trim() })
      setCloneOpen(false)
      navigate(`/decision-profiles/${clone.id}`)
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to clone decision profile.'))
    } finally {
      setBusy(false)
    }
  }

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
        <CircularProgress />
      </Box>
    )
  }

  if (!profile) {
    return (
      <Stack spacing={2}>
        {error ? (
          <Alert severity="error">{error}</Alert>
        ) : (
          <Alert severity="warning">Decision profile not found.</Alert>
        )}
        <Button component={RouterLink} to="/decision-profiles">
          Back to decision profiles
        </Button>
      </Stack>
    )
  }

  return (
    <Stack spacing={3} maxWidth={860}>
      <Stack
        direction={{ xs: 'column', sm: 'row' }}
        justifyContent="space-between"
        alignItems={{ xs: 'stretch', sm: 'flex-start' }}
        spacing={2}
      >
        <Box>
          <Typography variant="h4" gutterBottom>
            {profile.name}
          </Typography>
          <Stack direction="row" spacing={1} alignItems="center" flexWrap="wrap" useFlexGap>
            <Chip size="small" label={profile.status} color={statusColor(profile.status)} />
            <Chip size="small" label={`v${profile.version}`} variant="outlined" />
            <Chip size="small" label={profile.code} variant="outlined" />
            {profile.defaultProfile ? (
              <Chip size="small" label="Default" color="secondary" />
            ) : null}
          </Stack>
        </Box>
        <Button component={RouterLink} to="/decision-profiles">
          Back
        </Button>
      </Stack>

      {error ? <Alert severity="error">{error}</Alert> : null}

      <Paper sx={{ p: 3 }}>
        <Stack spacing={1.5}>
          <DetailRow label="Description" value={profile.description || '—'} />
          <DetailRow label="Profile Family Id" value={profile.profileFamilyId} />
          <DetailRow
            label="Preferred Strategy"
            value={profile.preferredStrategy || '—'}
          />
          <DetailRow
            label="Preferred Capacity Threshold"
            value={
              profile.preferredCapacityThreshold != null
                ? `${profile.preferredCapacityThreshold}%`
                : '—'
            }
          />
          <DetailRow
            label="Preferred Workload Threshold"
            value={
              profile.preferredWorkloadThreshold != null
                ? `${profile.preferredWorkloadThreshold}%`
                : '—'
            }
          />
          <DetailRow label="Created" value={new Date(profile.createdAt).toLocaleString()} />
          <DetailRow label="Updated" value={new Date(profile.updatedAt).toLocaleString()} />
          {profile.archivedAt ? (
            <DetailRow label="Archived" value={new Date(profile.archivedAt).toLocaleString()} />
          ) : null}
        </Stack>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="subtitle1" gutterBottom>
          Ranking dimension weights
        </Typography>
        <Stack direction="row" flexWrap="wrap" useFlexGap sx={{ gap: 1 }}>
          {profile.dimensions.map((dimension) => (
            <Chip
              key={dimension.dimension}
              label={`${dimension.dimension}: ${dimension.weight}${
                dimension.preferHigherValues ? ' (higher preferred)' : ''
              }`}
              variant="outlined"
            />
          ))}
        </Stack>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="subtitle1" gutterBottom>
          Category weights
        </Typography>
        <Stack direction="row" flexWrap="wrap" useFlexGap sx={{ gap: 1 }}>
          <Chip label={`Capacity: ${profile.capacityWeight}`} variant="outlined" />
          <Chip label={`Workload: ${profile.workloadWeight}`} variant="outlined" />
          <Chip label={`Cost: ${profile.costWeight}`} variant="outlined" />
          <Chip label={`Risk: ${profile.riskWeight}`} variant="outlined" />
          <Chip label={`Quality: ${profile.qualityWeight}`} variant="outlined" />
        </Stack>
      </Paper>

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} flexWrap="wrap" useFlexGap>
        <Button
          variant="outlined"
          onClick={() => navigate(`/decision-profiles/${profile.id}/edit`)}
          disabled={busy || profile.status === 'Archived'}
        >
          Edit
        </Button>
        <Button
          variant="outlined"
          onClick={() => {
            setCloneName(`${profile.name} (copy)`)
            setCloneCode(`${profile.code}Copy`)
            setCloneOpen(true)
          }}
          disabled={busy}
        >
          Clone
        </Button>
        {!profile.defaultProfile && profile.status === 'Active' ? (
          <Button
            variant="contained"
            onClick={() => void runAction(() => setDefaultDecisionProfile(profile.id))}
            disabled={busy}
          >
            Set as default
          </Button>
        ) : null}
        {profile.status === 'Inactive' ? (
          <Button
            variant="contained"
            onClick={() => void runAction(() => activateDecisionProfile(profile.id))}
            disabled={busy}
          >
            Activate
          </Button>
        ) : null}
        {profile.status === 'Active' && !profile.defaultProfile ? (
          <Button
            variant="outlined"
            onClick={() => void runAction(() => deactivateDecisionProfile(profile.id))}
            disabled={busy}
          >
            Deactivate
          </Button>
        ) : null}
        {profile.status !== 'Archived' && !profile.defaultProfile ? (
          <Button
            color="error"
            variant="outlined"
            onClick={() => void runAction(() => archiveDecisionProfile(profile.id))}
            disabled={busy}
          >
            Archive
          </Button>
        ) : null}
      </Stack>

      <Dialog open={cloneOpen} onClose={() => setCloneOpen(false)}>
        <DialogTitle>Clone decision profile</DialogTitle>
        <DialogContent>
          <DialogContentText sx={{ mb: 2 }}>
            Creates a new profile lineage (version 1, not default) copying this profile&apos;s
            weights.
          </DialogContentText>
          <Stack spacing={2}>
            <TextField
              label="Name"
              value={cloneName}
              onChange={(event) => setCloneName(event.target.value)}
              fullWidth
              autoFocus
            />
            <TextField
              label="Code"
              value={cloneCode}
              onChange={(event) => setCloneCode(event.target.value)}
              fullWidth
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCloneOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={() => void submitClone()}
            disabled={busy || !cloneName.trim() || !cloneCode.trim()}
          >
            Clone
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  )
}

function DetailRow({ label, value }: { label: string; value: string }) {
  return (
    <Box>
      <Typography variant="caption" color="text.secondary">
        {label}
      </Typography>
      <Typography>{value}</Typography>
    </Box>
  )
}
