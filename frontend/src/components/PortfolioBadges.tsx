import { Chip } from '@mui/material'

const HEALTH_COLOR: Record<string, 'default' | 'success' | 'warning' | 'error' | 'info'> = {
  Unknown: 'default',
  Underutilized: 'info',
  Healthy: 'success',
  AtRisk: 'warning',
  Overloaded: 'error',
}

const STATUS_COLOR: Record<string, 'default' | 'success' | 'warning'> = {
  Active: 'success',
  Inactive: 'warning',
}

export function PortfolioHealthBadge({ health }: { health: string }) {
  return (
    <Chip size="small" label={health} color={HEALTH_COLOR[health] ?? 'default'} variant="outlined" />
  )
}

export function PortfolioStatusBadge({ status }: { status: string }) {
  return (
    <Chip size="small" label={status} color={STATUS_COLOR[status] ?? 'default'} variant="outlined" />
  )
}
