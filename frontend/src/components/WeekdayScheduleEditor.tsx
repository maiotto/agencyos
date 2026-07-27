import {
  Checkbox,
  FormControlLabel,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material'
import type { WorkingHoursDay } from '../types/workingHours'
import { fromTimeInput, toTimeInput } from '../types/workingHours'

interface WeekdayScheduleEditorProps {
  value: WorkingHoursDay[]
  onChange: (days: WorkingHoursDay[]) => void
  disabled?: boolean
  error?: string
}

export function WeekdayScheduleEditor({
  value,
  onChange,
  disabled = false,
  error,
}: WeekdayScheduleEditorProps) {
  const updateDay = (index: number, patch: Partial<WorkingHoursDay>) => {
    const next = value.map((day, dayIndex) => (dayIndex === index ? { ...day, ...patch } : day))
    onChange(next)
  }

  return (
    <Stack spacing={1}>
      <Typography fontWeight={600}>Weekday schedule</Typography>
      {error ? (
        <Typography color="error" variant="body2">
          {error}
        </Typography>
      ) : null}
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Day</TableCell>
            <TableCell>Enabled</TableCell>
            <TableCell>Start</TableCell>
            <TableCell>End</TableCell>
            <TableCell>Break start</TableCell>
            <TableCell>Break end</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {value.map((day, index) => (
            <TableRow key={day.dayOfWeek}>
              <TableCell>{day.dayOfWeek}</TableCell>
              <TableCell>
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={day.enabled}
                      disabled={disabled}
                      onChange={(event) =>
                        updateDay(index, {
                          enabled: event.target.checked,
                          startTime: event.target.checked ? day.startTime ?? '09:00:00' : null,
                          endTime: event.target.checked ? day.endTime ?? '18:00:00' : null,
                          breakStart: event.target.checked ? day.breakStart : null,
                          breakEnd: event.target.checked ? day.breakEnd : null,
                        })
                      }
                    />
                  }
                  label=""
                />
              </TableCell>
              <TableCell>
                <TextField
                  type="time"
                  size="small"
                  disabled={disabled || !day.enabled}
                  value={toTimeInput(day.startTime)}
                  onChange={(event) =>
                    updateDay(index, { startTime: fromTimeInput(event.target.value) })
                  }
                  InputLabelProps={{ shrink: true }}
                />
              </TableCell>
              <TableCell>
                <TextField
                  type="time"
                  size="small"
                  disabled={disabled || !day.enabled}
                  value={toTimeInput(day.endTime)}
                  onChange={(event) =>
                    updateDay(index, { endTime: fromTimeInput(event.target.value) })
                  }
                  InputLabelProps={{ shrink: true }}
                />
              </TableCell>
              <TableCell>
                <TextField
                  type="time"
                  size="small"
                  disabled={disabled || !day.enabled}
                  value={toTimeInput(day.breakStart)}
                  onChange={(event) =>
                    updateDay(index, { breakStart: fromTimeInput(event.target.value) })
                  }
                  InputLabelProps={{ shrink: true }}
                />
              </TableCell>
              <TableCell>
                <TextField
                  type="time"
                  size="small"
                  disabled={disabled || !day.enabled}
                  value={toTimeInput(day.breakEnd)}
                  onChange={(event) =>
                    updateDay(index, { breakEnd: fromTimeInput(event.target.value) })
                  }
                  InputLabelProps={{ shrink: true }}
                />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Stack>
  )
}
