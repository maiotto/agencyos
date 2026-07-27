import { useCallback, useEffect, useState } from 'react'
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Divider,
  LinearProgress,
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
import { Link as RouterLink } from 'react-router-dom'
import {
  comparePortfolios,
  getPortfolioAnalyticsOverview,
  getPortfolioAnalyticsTrends,
  getPortfolioHealthAnalytics,
  getPortfolioPerformance,
  getPortfolioRanking,
} from '../api/portfolioAnalytics'
import { getErrorMessage } from '../api/client'
import { useCompany } from '../context/CompanyContext'
import type { HealthIndicator, StatusCountItem } from '../types/enterpriseDashboard'
import type {
  PortfolioAnalyticsOverview,
  PortfolioComparison,
  PortfolioHealthAnalytics,
  PortfolioPerformance,
  PortfolioRanking,
  PortfolioTrends,
} from '../types/portfolioAnalytics'

const HEALTH_COLOR: Record<string, 'default' | 'success' | 'warning' | 'error' | 'info'> = {
  Unknown: 'default',
  Underutilized: 'info',
  Healthy: 'success',
  AtRisk: 'warning',
  Overloaded: 'error',
}

function HealthChip({ health }: { health: HealthIndicator }) {
  return (
    <Chip
      size="small"
      label={health.label}
      color={HEALTH_COLOR[health.status] ?? 'default'}
      variant="outlined"
      title={health.detail ?? undefined}
    />
  )
}

function StatusBreakdownList({ items }: { items: StatusCountItem[] }) {
  if (items.length === 0) {
    return (
      <Typography variant="body2" color="text.secondary">
        No records for the selected period.
      </Typography>
    )
  }

  return (
    <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
      {items.map((item) => (
        <Chip key={item.status} size="small" label={`${item.status}: ${item.count}`} variant="outlined" />
      ))}
    </Stack>
  )
}

function defaultDateRange() {
  const to = new Date()
  const from = new Date()
  from.setDate(to.getDate() - 30)
  return {
    from: from.toISOString().slice(0, 10),
    to: to.toISOString().slice(0, 10),
  }
}

export function PortfolioAnalyticsPage() {
  const { activeCompanyId } = useCompany()
  const initialRange = defaultDateRange()
  const [from, setFrom] = useState(initialRange.from)
  const [to, setTo] = useState(initialRange.to)
  const [periodStart, setPeriodStart] = useState(initialRange.from)
  const [periodEnd, setPeriodEnd] = useState(initialRange.to)

  const [overview, setOverview] = useState<PortfolioAnalyticsOverview | null>(null)
  const [ranking, setRanking] = useState<PortfolioRanking | null>(null)
  const [health, setHealth] = useState<PortfolioHealthAnalytics | null>(null)
  const [performance, setPerformance] = useState<PortfolioPerformance | null>(null)
  const [trends, setTrends] = useState<PortfolioTrends | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const [leftPortfolioId, setLeftPortfolioId] = useState('')
  const [rightPortfolioId, setRightPortfolioId] = useState('')
  const [comparison, setComparison] = useState<PortfolioComparison | null>(null)
  const [compareLoading, setCompareLoading] = useState(false)
  const [compareError, setCompareError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)

    const query = {
      companyId: activeCompanyId || undefined,
      from,
      to,
      periodStart,
      periodEnd,
    }

    try {
      const [overviewData, rankingData, healthData, performanceData, trendsData] = await Promise.all([
        getPortfolioAnalyticsOverview(query),
        getPortfolioRanking(query),
        getPortfolioHealthAnalytics(query),
        getPortfolioPerformance(query),
        getPortfolioAnalyticsTrends(query),
      ])
      setOverview(overviewData)
      setRanking(rankingData)
      setHealth(healthData)
      setPerformance(performanceData)
      setTrends(trendsData)
    } catch (err) {
      setOverview(null)
      setRanking(null)
      setHealth(null)
      setPerformance(null)
      setTrends(null)
      setError(getErrorMessage(err, 'Failed to load Portfolio Analytics.'))
    } finally {
      setLoading(false)
    }
  }, [activeCompanyId, from, to, periodStart, periodEnd])

  useEffect(() => {
    void load()
  }, [load])

  const runCompare = useCallback(async () => {
    if (!leftPortfolioId || !rightPortfolioId) {
      setCompareError('Select both a left and right Portfolio to compare.')
      return
    }

    setCompareLoading(true)
    setCompareError(null)

    try {
      const data = await comparePortfolios({
        companyId: activeCompanyId || undefined,
        leftPortfolioId,
        rightPortfolioId,
        from,
        to,
        periodStart,
        periodEnd,
      })
      setComparison(data)
    } catch (err) {
      setComparison(null)
      setCompareError(getErrorMessage(err, 'Failed to compare Portfolios.'))
    } finally {
      setCompareLoading(false)
    }
  }, [activeCompanyId, leftPortfolioId, rightPortfolioId, from, to, periodStart, periodEnd])

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h4">Portfolio Analytics</Typography>
        <Typography color="text.secondary">
          Read-only Portfolio analytics derived from stored Portfolio health/snapshot fields and
          mission-scoped Recommendation/Decision history (US-404). Nothing here recalculates or persists
          Capacity/Workload — history remains the single source of truth.
        </Typography>
      </Box>

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} alignItems={{ md: 'center' }} flexWrap="wrap">
          <TextField
            label="From"
            type="date"
            value={from}
            onChange={(event) => setFrom(event.target.value)}
            InputLabelProps={{ shrink: true }}
            size="small"
          />
          <TextField
            label="To"
            type="date"
            value={to}
            onChange={(event) => setTo(event.target.value)}
            InputLabelProps={{ shrink: true }}
            size="small"
          />
          <TextField
            label="Period Start"
            type="date"
            value={periodStart}
            onChange={(event) => setPeriodStart(event.target.value)}
            InputLabelProps={{ shrink: true }}
            size="small"
          />
          <TextField
            label="Period End"
            type="date"
            value={periodEnd}
            onChange={(event) => setPeriodEnd(event.target.value)}
            InputLabelProps={{ shrink: true }}
            size="small"
          />
          <Button variant="contained" onClick={() => void load()} disabled={loading}>
            Refresh
          </Button>
        </Stack>
      </Paper>

      {error && <Alert severity="error">{error}</Alert>}

      {loading && !overview ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
          <CircularProgress />
        </Box>
      ) : (
        <>
          {overview && (
            <Paper sx={{ p: 2.5 }}>
              <Stack spacing={1.5}>
                <Stack direction="row" justifyContent="space-between" alignItems="center">
                  <Typography variant="subtitle1">
                    Portfolios ({overview.portfolioCount}) — Overview
                  </Typography>
                  <HealthChip health={overview.overallHealth} />
                </Stack>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Portfolio</TableCell>
                      <TableCell>Status</TableCell>
                      <TableCell>Health</TableCell>
                      <TableCell align="right">Missions</TableCell>
                      <TableCell align="right">Utilization</TableCell>
                      <TableCell align="right">Workload</TableCell>
                      <TableCell align="right">Recommendations</TableCell>
                      <TableCell align="right">Decisions</TableCell>
                      <TableCell />
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {overview.portfolios.map((card) => (
                      <TableRow key={card.portfolioId}>
                        <TableCell>{card.name}</TableCell>
                        <TableCell>{card.status}</TableCell>
                        <TableCell>
                          <HealthChip health={card.health} />
                        </TableCell>
                        <TableCell align="right">{card.missionCount}</TableCell>
                        <TableCell align="right">
                          {card.utilizationPercentage != null ? `${card.utilizationPercentage}%` : '—'}
                        </TableCell>
                        <TableCell align="right">
                          {card.workloadPercentage != null ? `${card.workloadPercentage}%` : '—'}
                        </TableCell>
                        <TableCell align="right">{card.recommendationCount}</TableCell>
                        <TableCell align="right">{card.decisionCount}</TableCell>
                        <TableCell>
                          <Button component={RouterLink} to={card.drillDownPath} size="small">
                            View
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </Stack>
            </Paper>
          )}

          <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
            {health && (
              <Paper sx={{ p: 2.5, flex: '1 1 320px', minWidth: 320 }}>
                <Stack spacing={1.5}>
                  <Stack direction="row" justifyContent="space-between" alignItems="center">
                    <Typography variant="subtitle1">Health Distribution</Typography>
                    <HealthChip health={health.overallHealth} />
                  </Stack>
                  <Stack direction="row" spacing={2} flexWrap="wrap">
                    <Typography variant="body2">Healthy: {health.healthyCount}</Typography>
                    <Typography variant="body2">At Risk: {health.atRiskCount}</Typography>
                    <Typography variant="body2">Overloaded: {health.overloadedCount}</Typography>
                    <Typography variant="body2">Underutilized: {health.underutilizedCount}</Typography>
                    <Typography variant="body2">Unknown: {health.unknownCount}</Typography>
                  </Stack>
                  <StatusBreakdownList items={health.distribution} />
                  {health.riskIndicators.length > 0 && (
                    <>
                      <Divider />
                      <Typography variant="subtitle2">Risk Indicators</Typography>
                      <Stack spacing={1}>
                        {health.riskIndicators.map((indicator) => (
                          <Stack
                            key={`${indicator.portfolioId}-${indicator.reason}`}
                            direction="row"
                            justifyContent="space-between"
                            alignItems="center"
                          >
                            <Box>
                              <Typography variant="body2">{indicator.name}</Typography>
                              <Typography variant="caption" color="text.secondary">
                                {indicator.riskLevel}: {indicator.reason}
                              </Typography>
                            </Box>
                            <Button component={RouterLink} to={indicator.drillDownPath} size="small">
                              View
                            </Button>
                          </Stack>
                        ))}
                      </Stack>
                    </>
                  )}
                </Stack>
              </Paper>
            )}

            {performance && (
              <Paper sx={{ p: 2.5, flex: '1 1 320px', minWidth: 320 }}>
                <Stack spacing={1.5}>
                  <Typography variant="subtitle1">Performance</Typography>
                  <Box>
                    <Stack direction="row" justifyContent="space-between">
                      <Typography variant="body2">Average Utilization</Typography>
                      <Typography variant="body2">{performance.averageUtilizationPercentage}%</Typography>
                    </Stack>
                    <LinearProgress
                      variant="determinate"
                      value={Math.min(performance.averageUtilizationPercentage, 100)}
                      sx={{ height: 8, borderRadius: 4 }}
                    />
                  </Box>
                  <Box>
                    <Stack direction="row" justifyContent="space-between">
                      <Typography variant="body2">Average Workload</Typography>
                      <Typography variant="body2">{performance.averageWorkloadPercentage}%</Typography>
                    </Stack>
                    <LinearProgress
                      variant="determinate"
                      value={Math.min(performance.averageWorkloadPercentage, 100)}
                      sx={{ height: 8, borderRadius: 4 }}
                      color="secondary"
                    />
                  </Box>
                  <Typography variant="body2" color="text.secondary">
                    Avg. Recommendation Score: {performance.averageRecommendationScore ?? '—'}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Decision Completion Rate: {performance.decisionCompletionRatePercentage}% · Cancellation
                    Rate: {performance.decisionCancellationRatePercentage}%
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Recommendation Effectiveness: {performance.recommendationEffectivenessPercentage}%
                  </Typography>
                </Stack>
              </Paper>
            )}
          </Stack>

          {ranking && (
            <Paper sx={{ p: 2.5 }}>
              <Stack spacing={1.5}>
                <Typography variant="subtitle1">Portfolio Ranking</Typography>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Rank</TableCell>
                      <TableCell>Portfolio</TableCell>
                      <TableCell>Health</TableCell>
                      <TableCell align="right">Score</TableCell>
                      <TableCell align="right">Utilization</TableCell>
                      <TableCell align="right">Workload</TableCell>
                      <TableCell align="right">Effectiveness</TableCell>
                      <TableCell />
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {ranking.items.map((item) => (
                      <TableRow key={item.portfolioId}>
                        <TableCell>{item.rank}</TableCell>
                        <TableCell>{item.name}</TableCell>
                        <TableCell>
                          <HealthChip health={item.health} />
                        </TableCell>
                        <TableCell align="right">{item.score}</TableCell>
                        <TableCell align="right">
                          {item.utilizationPercentage != null ? `${item.utilizationPercentage}%` : '—'}
                        </TableCell>
                        <TableCell align="right">
                          {item.workloadPercentage != null ? `${item.workloadPercentage}%` : '—'}
                        </TableCell>
                        <TableCell align="right">
                          {item.recommendationEffectivenessPercentage != null
                            ? `${item.recommendationEffectivenessPercentage}%`
                            : '—'}
                        </TableCell>
                        <TableCell>
                          <Button component={RouterLink} to={item.drillDownPath} size="small">
                            View
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </Stack>
            </Paper>
          )}

          {trends && (
            <Paper sx={{ p: 2.5 }}>
              <Stack spacing={1.5}>
                <Typography variant="subtitle1">Trends (by month)</Typography>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Period</TableCell>
                      <TableCell align="right">Capacity Util.</TableCell>
                      <TableCell align="right">Workload Util.</TableCell>
                      <TableCell>Health</TableCell>
                      <TableCell align="right">Recommendations</TableCell>
                      <TableCell align="right">Decisions</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {trends.points.map((point) => (
                      <TableRow key={point.periodLabel}>
                        <TableCell>{point.periodLabel}</TableCell>
                        <TableCell align="right">
                          {point.capacityUtilization != null ? `${point.capacityUtilization}%` : '—'}
                        </TableCell>
                        <TableCell align="right">
                          {point.workloadUtilization != null ? `${point.workloadUtilization}%` : '—'}
                        </TableCell>
                        <TableCell>{point.healthStatus ?? '—'}</TableCell>
                        <TableCell align="right">{point.recommendationCount}</TableCell>
                        <TableCell align="right">{point.decisionCount}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </Stack>
            </Paper>
          )}

          <Paper sx={{ p: 2.5 }}>
            <Stack spacing={1.5}>
              <Typography variant="subtitle1">Compare Portfolios</Typography>
              <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} alignItems={{ md: 'center' }}>
                <TextField
                  label="Left Portfolio ID"
                  value={leftPortfolioId}
                  onChange={(event) => setLeftPortfolioId(event.target.value)}
                  size="small"
                  sx={{ minWidth: 320 }}
                />
                <TextField
                  label="Right Portfolio ID"
                  value={rightPortfolioId}
                  onChange={(event) => setRightPortfolioId(event.target.value)}
                  size="small"
                  sx={{ minWidth: 320 }}
                />
                <Button variant="contained" onClick={() => void runCompare()} disabled={compareLoading}>
                  Compare
                </Button>
              </Stack>

              {compareError && <Alert severity="error">{compareError}</Alert>}

              {comparison && (
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Field</TableCell>
                      <TableCell>{comparison.left.name}</TableCell>
                      <TableCell>{comparison.right.name}</TableCell>
                      <TableCell align="right">Delta</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {comparison.fieldDiffs.map((diff) => (
                      <TableRow key={diff.field}>
                        <TableCell>{diff.field}</TableCell>
                        <TableCell>{diff.leftValue ?? '—'}</TableCell>
                        <TableCell>{diff.rightValue ?? '—'}</TableCell>
                        <TableCell align="right">{diff.delta ?? '—'}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}

              {comparison && (
                <Stack direction="row" spacing={2}>
                  <Button component={RouterLink} to={comparison.left.drillDownPath} size="small">
                    View {comparison.left.name}
                  </Button>
                  <Button component={RouterLink} to={comparison.right.drillDownPath} size="small">
                    View {comparison.right.name}
                  </Button>
                </Stack>
              )}
            </Stack>
          </Paper>
        </>
      )}
    </Stack>
  )
}
