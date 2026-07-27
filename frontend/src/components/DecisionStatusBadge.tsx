import { Chip } from '@mui/material'

const DECISION_COLOR: Record<string, 'default' | 'success' | 'warning' | 'info' | 'error'> = {
  Created: 'info',
  InProgress: 'warning',
  Completed: 'success',
  Cancelled: 'default',
}

const IMPLEMENTATION_COLOR: Record<string, 'default' | 'success' | 'warning' | 'info' | 'error'> = {
  NotStarted: 'default',
  InProgress: 'warning',
  Completed: 'success',
  Cancelled: 'default',
}

export function DecisionStatusBadge({ status }: { status: string }) {
  return (
    <Chip
      size="small"
      label={status}
      color={DECISION_COLOR[status] ?? 'default'}
      variant="outlined"
    />
  )
}

export function ImplementationStatusBadge({ status }: { status: string }) {
  return (
    <Chip
      size="small"
      label={status}
      color={IMPLEMENTATION_COLOR[status] ?? 'default'}
      variant="outlined"
    />
  )
}
