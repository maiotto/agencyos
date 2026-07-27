import { useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Paper,
  Stack,
  Typography,
} from '@mui/material'
import { activateCompany, archiveCompany, deactivateCompany, getCompany } from '../api/companies'
import { getErrorMessage } from '../api/client'
import { useCompany } from '../context/CompanyContext'
import type { Company } from '../types/company'

function statusColor(status: string): 'success' | 'default' | 'warning' {
  if (status === 'Active') return 'success'
  if (status === 'Archived') return 'default'
  return 'warning'
}

export function CompanyDetailPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { activeCompanyId, setActiveCompanyId, refreshCompanies } = useCompany()
  const [company, setCompany] = useState<Company | null>(null)
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = async () => {
    if (!id) {
      return
    }

    setLoading(true)
    setError(null)

    try {
      const data = await getCompany(id)
      setCompany(data)
    } catch (err) {
      setError(getErrorMessage(err, 'Failed to load company.'))
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
      await refreshCompanies()
    } catch (err) {
      setError(getErrorMessage(err))
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

  if (!company) {
    return (
      <Stack spacing={2}>
        {error ? <Alert severity="error">{error}</Alert> : <Alert severity="warning">Company not found.</Alert>}
        <Button component={RouterLink} to="/companies">
          Back to companies
        </Button>
      </Stack>
    )
  }

  const isSelected = company.id === activeCompanyId

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
            {company.companyName}
          </Typography>
          <Stack direction="row" spacing={1} alignItems="center" flexWrap="wrap" useFlexGap>
            <Chip size="small" label={company.status} color={statusColor(company.status)} />
            <Chip size="small" label={company.companyCode} variant="outlined" />
            {isSelected ? <Chip size="small" label="Selected" color="secondary" /> : null}
          </Stack>
        </Box>
        <Button component={RouterLink} to="/companies">
          Back
        </Button>
      </Stack>

      {error ? <Alert severity="error">{error}</Alert> : null}

      <Paper sx={{ p: 3 }}>
        <Stack spacing={1.5}>
          <DetailRow label="Legal Name" value={company.legalName || '—'} />
          <DetailRow label="Timezone" value={company.timezone} />
          <DetailRow label="Country" value={company.country || '—'} />
          <DetailRow label="Language" value={company.language || '—'} />
          <DetailRow label="Currency" value={company.currency || '—'} />
          <DetailRow label="Decision Profile Id" value={company.decisionProfileId || '—'} />
          <DetailRow label="Default Planning Template Id" value={company.defaultPlanningTemplateId || '—'} />
          <DetailRow label="Created" value={new Date(company.createdAt).toLocaleString()} />
          <DetailRow label="Updated" value={new Date(company.updatedAt).toLocaleString()} />
          {company.archivedAt ? (
            <DetailRow label="Archived" value={new Date(company.archivedAt).toLocaleString()} />
          ) : null}
        </Stack>
      </Paper>

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} flexWrap="wrap" useFlexGap>
        <Button
          variant="outlined"
          onClick={() => navigate(`/companies/${company.id}/edit`)}
          disabled={busy || company.status === 'Archived'}
        >
          Edit
        </Button>
        {!isSelected && company.status === 'Active' ? (
          <Button variant="contained" onClick={() => setActiveCompanyId(company.id)} disabled={busy}>
            Select as active company
          </Button>
        ) : null}
        {company.status === 'Inactive' ? (
          <Button
            variant="contained"
            onClick={() => void runAction(() => activateCompany(company.id))}
            disabled={busy}
          >
            Activate
          </Button>
        ) : null}
        {company.status === 'Active' ? (
          <Button
            variant="outlined"
            onClick={() => void runAction(() => deactivateCompany(company.id))}
            disabled={busy}
          >
            Deactivate
          </Button>
        ) : null}
        {company.status !== 'Archived' ? (
          <Button
            color="error"
            variant="outlined"
            onClick={() => void runAction(() => archiveCompany(company.id))}
            disabled={busy}
          >
            Archive
          </Button>
        ) : null}
      </Stack>
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
