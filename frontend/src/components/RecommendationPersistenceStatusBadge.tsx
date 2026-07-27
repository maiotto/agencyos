import { Chip } from '@mui/material'

const STATUS_COLOR: Record<string, 'default' | 'success' | 'warning'> = {
  Active: 'success',
  Archived: 'warning',
}

export function RecommendationPersistenceStatusBadge({ status }: { status: string }) {
  return (
    <Chip
      size="small"
      label={status}
      color={STATUS_COLOR[status] ?? 'default'}
      variant="outlined"
    />
  )
}
