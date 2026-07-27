import { Box, Checkbox, FormControlLabel, FormGroup, FormHelperText, FormLabel } from '@mui/material'
import { WORKING_DAYS, type WorkingDay } from '../types/workingCalendar'

interface WorkingDaySelectorProps {
  value: string[]
  onChange: (days: string[]) => void
  error?: string
  disabled?: boolean
}

export function WorkingDaySelector({
  value,
  onChange,
  error,
  disabled = false,
}: WorkingDaySelectorProps) {
  const toggleDay = (day: WorkingDay) => {
    if (value.includes(day)) {
      onChange(value.filter((item) => item !== day))
      return
    }

    onChange([...value, day])
  }

  return (
    <Box>
      <FormLabel component="legend" sx={{ mb: 1, fontWeight: 600 }}>
        Working days
      </FormLabel>
      <FormGroup row sx={{ gap: 0.5 }}>
        {WORKING_DAYS.map((day) => (
          <FormControlLabel
            key={day}
            control={
              <Checkbox
                checked={value.includes(day)}
                onChange={() => toggleDay(day)}
                disabled={disabled}
              />
            }
            label={day.slice(0, 3)}
          />
        ))}
      </FormGroup>
      {error ? <FormHelperText error>{error}</FormHelperText> : null}
    </Box>
  )
}
