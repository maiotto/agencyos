import { Chip } from '@mui/material'

const STATUS_COLOR: Record<string, 'default' | 'info' | 'success' | 'warning' | 'error'> = {
  Draft: 'default',
  PendingApproval: 'info',
  Approved: 'success',
  Rejected: 'warning',
  Cancelled: 'error',
  Reopened: 'info',
}

export function RecommendationStatusBadge({ status }: { status: string }) {
  return (
    <Chip
      size="small"
      label={status}
      color={STATUS_COLOR[status] ?? 'default'}
      variant={status === 'Draft' ? 'outlined' : 'filled'}
    />
  )
}
