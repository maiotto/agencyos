import { useEffect, useState } from 'react'
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { Link as RouterLink, useParams } from 'react-router-dom'
import { getWorkloadHistory } from '../api/workloadHistory'
import { getErrorMessage } from '../api/client'
import type { WorkloadHistory } from '../types/workloadHistory'

export function WorkloadHistoryDetailPage() {
  const { id } = useParams<{ id: string }>()
  const [item, setItem] = useState<WorkloadHistory | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!id) {
      return
    }

    let cancelled = false

    const load = async () => {
      setLoading(true)
      setError(null)
      try {
        const history = await getWorkloadHistory(id)
        if (!cancelled) {
          setItem(history)
        }
      } catch (err) {
        if (!cancelled) {
          setItem(null)
          setError(getErrorMessage(err, 'Failed to load workload history detail.'))
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
  }, [id])

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" py={8}>
        <CircularProgress />
      </Box>
    )
  }

  if (error || !item) {
    return (
      <Stack spacing={2}>
        <Alert severity="error">{error ?? 'History record not found.'}</Alert>
        <Button component={RouterLink} to="/workload/history" variant="outlined">
          Back to history
        </Button>
      </Stack>
    )
  }

  return (
    <Stack spacing={3}>
      <Box>
        <Button component={RouterLink} to="/workload/history" size="small" sx={{ mb: 1 }}>
          ← Workload History
        </Button>
        <Typography variant="h4" gutterBottom>
          History Detail
        </Typography>
        <Typography color="text.secondary" sx={{ fontFamily: 'monospace' }}>
          {item.historyId}
        </Typography>
      </Box>

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} useFlexGap flexWrap="wrap">
          <Chip label={`Version ${item.calculationVersion}`} color="primary" variant="outlined" />
          <Chip label={`${item.periodStart} → ${item.periodEnd}`} />
          <Chip label={`Allocated: ${item.allocatedHours}h`} />
          <Chip label={`Capacity: ${item.capacityHours}h`} />
          <Chip label={`Workload: ${item.workloadPercentage}%`} />
          <Chip label={`Working days: ${item.workingDays}`} />
          <Chip label={`Holiday days: ${item.holidayDays}`} />
          <Chip label={`Available days: ${item.availableDays}`} />
        </Stack>
      </Paper>

      <Paper sx={{ p: 2 }}>
        <Typography variant="subtitle1" gutterBottom>
          Identifiers
        </Typography>
        <Typography variant="body2">
          Resource: {item.executionResourceName} ({item.executionResourceCode})
        </Typography>
        <Typography variant="body2">Execution Resource Id: {item.executionResourceId}</Typography>
        <Typography variant="body2">Company: {item.companyId}</Typography>
        <Typography variant="body2">
          Calculated: {new Date(item.calculationDate).toLocaleString()}
        </Typography>
      </Paper>

      <Box>
        <Typography variant="h6" gutterBottom>
          Assignment distribution snapshot
        </Typography>
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Role</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Period</TableCell>
                <TableCell align="right">Planned hours</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {item.assignmentDistribution.map((assignment) => (
                <TableRow key={assignment.assignmentId}>
                  <TableCell>{assignment.assignmentRole}</TableCell>
                  <TableCell>{assignment.status}</TableCell>
                  <TableCell>
                    {assignment.plannedStartDate} → {assignment.plannedEndDate}
                  </TableCell>
                  <TableCell align="right">{assignment.plannedHours}</TableCell>
                </TableRow>
              ))}
              {item.assignmentDistribution.length === 0 && (
                <TableRow>
                  <TableCell colSpan={4}>
                    <Typography color="text.secondary" py={2}>
                      No assignment snapshot was preserved for this record.
                    </Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Box>
    </Stack>
  )
}
