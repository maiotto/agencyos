import { useCallback, useEffect, useState } from 'react'
import {
  Alert,
  Box,
  Button,
  Checkbox,
  Chip,
  CircularProgress,
  Divider,
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
  compareCrossPortfolioScenarios,
  getCrossPortfolioBalance,
  getCrossPortfolioConflicts,
  getCrossPortfolioOverview,
  getCrossPortfolioScenarios,
  simulateCrossPortfolioPlan,
} from '../api/crossPortfolioPlanning'
import { getErrorMessage } from '../api/client'
import { useCompany } from '../context/CompanyContext'
import type { HealthIndicator } from '../types/enterpriseDashboard'
import type {
  ConflictSummary,
  CrossPortfolioBalance,
  CrossPortfolioOverview,
  CrossPortfolioScenarioList,
  ScenarioComparison,
  SimulationSummary,
} from '../types/crossPortfolioPlanning'

const HEALTH_COLOR: Record<string, 'default' | 'success' | 'warning' | 'error' | 'info'> = {
  Unknown: 'default',
  Underutilized: 'info',
  Healthy: 'success',
  AtRisk: 'warning',
  Overloaded: 'error',
}

const SEVERITY_COLOR: Record<string, 'default' | 'success' | 'warning' | 'error' | 'info'> = {
  None: 'success',
  Low: 'info',
  Medium: 'warning',
  High: 'error',
  Critical: 'error',
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

function defaultDateRange() {
  const to = new Date()
  const from = new Date()
  from.setDate(to.getDate() + 30)
  return {
    periodStart: to.toISOString().slice(0, 10),
    periodEnd: from.toISOString().slice(0, 10),
  }
}

export function CrossPortfolioPlanningPage() {
  const { activeCompanyId } = useCompany()
  const initialRange = defaultDateRange()
  const [periodStart, setPeriodStart] = useState(initialRange.periodStart)
  const [periodEnd, setPeriodEnd] = useState(initialRange.periodEnd)

  const [overview, setOverview] = useState<CrossPortfolioOverview | null>(null)
  const [scenarios, setScenarios] = useState<CrossPortfolioScenarioList | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const [selectedPortfolioIds, setSelectedPortfolioIds] = useState<string[]>([])

  const [conflicts, setConflicts] = useState<ConflictSummary | null>(null)
  const [conflictsLoading, setConflictsLoading] = useState(false)
  const [conflictsError, setConflictsError] = useState<string | null>(null)

  const [balance, setBalance] = useState<CrossPortfolioBalance | null>(null)
  const [balanceLoading, setBalanceLoading] = useState(false)
  const [balanceError, setBalanceError] = useState<string | null>(null)

  const [scenarioName, setScenarioName] = useState('')
  const [simulation, setSimulation] = useState<SimulationSummary | null>(null)
  const [simulateLoading, setSimulateLoading] = useState(false)
  const [simulateError, setSimulateError] = useState<string | null>(null)

  const [leftScenarioId, setLeftScenarioId] = useState('')
  const [rightScenarioId, setRightScenarioId] = useState('')
  const [comparison, setComparison] = useState<ScenarioComparison | null>(null)
  const [compareLoading, setCompareLoading] = useState(false)
  const [compareError, setCompareError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)

    const query = {
      companyId: activeCompanyId || undefined,
      periodStart,
      periodEnd,
    }

    try {
      const [overviewData, scenariosData] = await Promise.all([
        getCrossPortfolioOverview(query),
        getCrossPortfolioScenarios(query),
      ])
      setOverview(overviewData)
      setScenarios(scenariosData)
    } catch (err) {
      setOverview(null)
      setScenarios(null)
      setError(getErrorMessage(err, 'Failed to load Cross-Portfolio Planning overview.'))
    } finally {
      setLoading(false)
    }
  }, [activeCompanyId, periodStart, periodEnd])

  useEffect(() => {
    void load()
  }, [load])

  const togglePortfolio = useCallback((portfolioId: string) => {
    setSelectedPortfolioIds((current) =>
      current.includes(portfolioId) ? current.filter((id) => id !== portfolioId) : [...current, portfolioId],
    )
  }, [])

  const runConflicts = useCallback(async () => {
    if (selectedPortfolioIds.length < 1) {
      setConflictsError('Select at least one Portfolio to analyze conflicts.')
      return
    }

    setConflictsLoading(true)
    setConflictsError(null)

    try {
      const data = await getCrossPortfolioConflicts({
        companyId: activeCompanyId || undefined,
        periodStart,
        periodEnd,
        portfolioIds: selectedPortfolioIds,
      })
      setConflicts(data)
    } catch (err) {
      setConflicts(null)
      setConflictsError(getErrorMessage(err, 'Failed to analyze conflicts.'))
    } finally {
      setConflictsLoading(false)
    }
  }, [activeCompanyId, periodStart, periodEnd, selectedPortfolioIds])

  const runBalance = useCallback(async () => {
    if (selectedPortfolioIds.length < 1) {
      setBalanceError('Select at least one Portfolio to view balance.')
      return
    }

    setBalanceLoading(true)
    setBalanceError(null)

    try {
      const data = await getCrossPortfolioBalance({
        companyId: activeCompanyId || undefined,
        periodStart,
        periodEnd,
        portfolioIds: selectedPortfolioIds,
      })
      setBalance(data)
    } catch (err) {
      setBalance(null)
      setBalanceError(getErrorMessage(err, 'Failed to load enterprise balance.'))
    } finally {
      setBalanceLoading(false)
    }
  }, [activeCompanyId, periodStart, periodEnd, selectedPortfolioIds])

  const runSimulate = useCallback(async () => {
    if (selectedPortfolioIds.length < 2) {
      setSimulateError('Select at least two Portfolios to simulate a Cross-Portfolio Plan.')
      return
    }

    setSimulateLoading(true)
    setSimulateError(null)

    try {
      const data = await simulateCrossPortfolioPlan({
        companyId: activeCompanyId || undefined,
        portfolioIds: selectedPortfolioIds,
        periodStart,
        periodEnd,
        scenarioName: scenarioName || undefined,
      })
      setSimulation(data)
      await load()
    } catch (err) {
      setSimulation(null)
      setSimulateError(getErrorMessage(err, 'Failed to simulate Cross-Portfolio Plan.'))
    } finally {
      setSimulateLoading(false)
    }
  }, [activeCompanyId, selectedPortfolioIds, periodStart, periodEnd, scenarioName, load])

  const runCompare = useCallback(async () => {
    if (!leftScenarioId || !rightScenarioId) {
      setCompareError('Select both a left and right scenario to compare.')
      return
    }

    setCompareLoading(true)
    setCompareError(null)

    try {
      const data = await compareCrossPortfolioScenarios({
        companyId: activeCompanyId || undefined,
        leftScenarioId,
        rightScenarioId,
      })
      setComparison(data)
    } catch (err) {
      setComparison(null)
      setCompareError(getErrorMessage(err, 'Failed to compare scenarios.'))
    } finally {
      setCompareLoading(false)
    }
  }, [activeCompanyId, leftScenarioId, rightScenarioId])

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h4">Cross-Portfolio Planning</Typography>
        <Typography color="text.secondary">
          Read-only advisory simulation across multiple Portfolios (US-405): enterprise Capacity/Workload
          balance, Resource/Mission conflict detection, and advisory rebalancing recommendations reusing the
          existing Capacity/Workload/Allocation-Conflict engines.
        </Typography>
      </Box>

      <Alert severity="info">
        Advisory only — human approval required. Simulations never modify Portfolios. Scenarios are temporary,
        in-memory records for this process only; the Audit trail is the durable record of every simulation.
      </Alert>

      <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
        <Button component={RouterLink} to="/portfolios" size="small" variant="outlined">
          Portfolios
        </Button>
        <Button component={RouterLink} to="/portfolio-analytics" size="small" variant="outlined">
          Portfolio Analytics
        </Button>
        <Button component={RouterLink} to="/enterprise-dashboard" size="small" variant="outlined">
          Enterprise Dashboard
        </Button>
        <Button component={RouterLink} to="/audit" size="small" variant="outlined">
          Audit
        </Button>
      </Stack>

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} alignItems={{ md: 'center' }} flexWrap="wrap">
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
                <Typography variant="subtitle1">
                  Portfolios ({overview.portfolioCount}) — Select for Analysis
                </Typography>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell padding="checkbox" />
                      <TableCell>Portfolio</TableCell>
                      <TableCell>Status</TableCell>
                      <TableCell>Health</TableCell>
                      <TableCell align="right">Missions</TableCell>
                      <TableCell align="right">Utilization</TableCell>
                      <TableCell align="right">Workload</TableCell>
                      <TableCell />
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {overview.portfolios.map((card) => (
                      <TableRow key={card.portfolioId} hover selected={selectedPortfolioIds.includes(card.portfolioId)}>
                        <TableCell padding="checkbox">
                          <Checkbox
                            checked={selectedPortfolioIds.includes(card.portfolioId)}
                            onChange={() => togglePortfolio(card.portfolioId)}
                          />
                        </TableCell>
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
                        <TableCell>
                          <Button component={RouterLink} to={card.drillDownPath} size="small">
                            View
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
                <Typography variant="caption" color="text.secondary">
                  {selectedPortfolioIds.length} Portfolio(s) selected. Simulate requires at least two.
                </Typography>
              </Stack>
            </Paper>
          )}

          <Paper sx={{ p: 2.5 }}>
            <Stack spacing={1.5}>
              <Typography variant="subtitle1">Analyze Selection</Typography>
              <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} alignItems={{ md: 'center' }} flexWrap="wrap">
                <TextField
                  label="Scenario Name (optional)"
                  value={scenarioName}
                  onChange={(event) => setScenarioName(event.target.value)}
                  size="small"
                  sx={{ minWidth: 260 }}
                />
                <Button variant="outlined" onClick={() => void runConflicts()} disabled={conflictsLoading}>
                  Analyze Conflicts
                </Button>
                <Button variant="outlined" onClick={() => void runBalance()} disabled={balanceLoading}>
                  View Balance
                </Button>
                <Button variant="contained" onClick={() => void runSimulate()} disabled={simulateLoading}>
                  Simulate
                </Button>
              </Stack>

              {conflictsError && <Alert severity="error">{conflictsError}</Alert>}
              {balanceError && <Alert severity="error">{balanceError}</Alert>}
              {simulateError && <Alert severity="error">{simulateError}</Alert>}
            </Stack>
          </Paper>

          {conflicts && (
            <Paper sx={{ p: 2.5 }}>
              <Stack spacing={1.5}>
                <Stack direction="row" justifyContent="space-between" alignItems="center">
                  <Typography variant="subtitle1">Conflict Analysis</Typography>
                  <Chip
                    size="small"
                    label={conflicts.severity}
                    color={SEVERITY_COLOR[conflicts.severity] ?? 'default'}
                    variant="outlined"
                  />
                </Stack>
                <Typography variant="body2" color="text.secondary">
                  Portfolio Mission overlaps: {conflicts.portfolioConflictCount} · Resource conflicts:{' '}
                  {conflicts.resourceConflictCount}
                </Typography>

                {conflicts.portfolioConflicts.length > 0 && (
                  <>
                    <Typography variant="subtitle2">Mission Overlaps</Typography>
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>Mission ID</TableCell>
                          <TableCell>Portfolios</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {conflicts.portfolioConflicts.map((item) => (
                          <TableRow key={item.missionId}>
                            <TableCell>{item.missionId}</TableCell>
                            <TableCell>{item.portfolioNames.join(', ')}</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </>
                )}

                {conflicts.resourceConflicts.length > 0 && (
                  <>
                    <Typography variant="subtitle2">Resource Conflicts</Typography>
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>Resource</TableCell>
                          <TableCell>Type</TableCell>
                          <TableCell>Severity</TableCell>
                          <TableCell>Description</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {conflicts.resourceConflicts.map((item) => (
                          <TableRow key={item.conflictId}>
                            <TableCell>{item.executionResourceName}</TableCell>
                            <TableCell>{item.conflictType}</TableCell>
                            <TableCell>{item.severity}</TableCell>
                            <TableCell>{item.description}</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </>
                )}

                {conflicts.portfolioConflictCount === 0 && conflicts.resourceConflictCount === 0 && (
                  <Typography variant="body2" color="text.secondary">
                    No conflicts detected for the selected Portfolios and period.
                  </Typography>
                )}
              </Stack>
            </Paper>
          )}

          {balance && (
            <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
              <Paper sx={{ p: 2.5, flex: '1 1 320px', minWidth: 320 }}>
                <Stack spacing={1.5}>
                  <Typography variant="subtitle1">Enterprise Capacity</Typography>
                  <Typography variant="body2" color="text.secondary">
                    Avg: {balance.capacity.averageUtilizationPercentage ?? '—'}% · Min:{' '}
                    {balance.capacity.minUtilizationPercentage ?? '—'}% · Max:{' '}
                    {balance.capacity.maxUtilizationPercentage ?? '—'}%
                    {balance.capacity.capacityEngineUsed ? ' · Live Capacity Engine data included' : ''}
                  </Typography>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Portfolio</TableCell>
                        <TableCell align="right">Utilization</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {balance.capacity.portfolios.map((item) => (
                        <TableRow key={item.portfolioId}>
                          <TableCell>{item.name}</TableCell>
                          <TableCell align="right">
                            {item.utilizationPercentage != null ? `${item.utilizationPercentage}%` : '—'}
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </Stack>
              </Paper>

              <Paper sx={{ p: 2.5, flex: '1 1 320px', minWidth: 320 }}>
                <Stack spacing={1.5}>
                  <Typography variant="subtitle1">Enterprise Workload</Typography>
                  <Typography variant="body2" color="text.secondary">
                    Avg: {balance.workload.averageWorkloadPercentage ?? '—'}% · Min:{' '}
                    {balance.workload.minWorkloadPercentage ?? '—'}% · Max:{' '}
                    {balance.workload.maxWorkloadPercentage ?? '—'}%
                    {balance.workload.workloadEngineUsed ? ' · Live Workload Engine data included' : ''}
                  </Typography>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Portfolio</TableCell>
                        <TableCell align="right">Workload</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {balance.workload.portfolios.map((item) => (
                        <TableRow key={item.portfolioId}>
                          <TableCell>{item.name}</TableCell>
                          <TableCell align="right">
                            {item.workloadPercentage != null ? `${item.workloadPercentage}%` : '—'}
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </Stack>
              </Paper>

              <Paper sx={{ p: 2.5, flex: '1 1 100%' }}>
                <Stack spacing={1.5}>
                  <Typography variant="subtitle1">Advisory Balancing Recommendations</Typography>
                  {balance.recommendations.length === 0 ? (
                    <Typography variant="body2" color="text.secondary">
                      No balancing recommendations for the selected Portfolios.
                    </Typography>
                  ) : (
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>Category</TableCell>
                          <TableCell>From</TableCell>
                          <TableCell>To</TableCell>
                          <TableCell>Recommendation</TableCell>
                          <TableCell>Rationale</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {balance.recommendations.map((rec, index) => (
                          <TableRow key={`${rec.category}-${index}`}>
                            <TableCell>{rec.category}</TableCell>
                            <TableCell>{rec.sourcePortfolioName ?? '—'}</TableCell>
                            <TableCell>{rec.targetPortfolioName ?? '—'}</TableCell>
                            <TableCell>{rec.recommendation}</TableCell>
                            <TableCell>{rec.rationale}</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  )}
                  <Divider />
                  <Typography variant="subtitle2">Missions (priority preserved)</Typography>
                  <Stack spacing={1}>
                    {balance.portfolios.map((portfolio) => (
                      <Box key={portfolio.portfolioId}>
                        <Typography variant="body2">
                          {portfolio.name} ({portfolio.missionCount} missions)
                        </Typography>
                        <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
                          {portfolio.missions
                            .slice()
                            .sort((a, b) => a.priority - b.priority)
                            .map((mission) => (
                              <Chip
                                key={mission.missionId}
                                size="small"
                                variant="outlined"
                                label={`P${mission.priority}: ${mission.missionId.slice(0, 8)}`}
                              />
                            ))}
                        </Stack>
                      </Box>
                    ))}
                  </Stack>
                </Stack>
              </Paper>
            </Stack>
          )}

          {simulation && (
            <Paper sx={{ p: 2.5 }}>
              <Stack spacing={1}>
                <Typography variant="subtitle1">Simulation Result</Typography>
                <Typography variant="body2" color="text.secondary">
                  Scenario ID: {simulation.scenarioId} · Simulated at{' '}
                  {new Date(simulation.simulatedAt).toLocaleString()}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {simulation.scenario.portfolios.length} Portfolios · Conflicts:{' '}
                  {simulation.scenario.conflicts.portfolioConflictCount +
                    simulation.scenario.conflicts.resourceConflictCount}{' '}
                  · Recommendations: {simulation.scenario.recommendations.length}
                </Typography>
                <Alert severity="warning">{simulation.advisoryDisclaimer}</Alert>
              </Stack>
            </Paper>
          )}

          {scenarios && (
            <Paper sx={{ p: 2.5 }}>
              <Stack spacing={1.5}>
                <Typography variant="subtitle1">Temporary Scenarios ({scenarios.scenarios.length})</Typography>
                {scenarios.scenarios.length === 0 ? (
                  <Typography variant="body2" color="text.secondary">
                    No scenarios simulated yet for this Company in this process lifetime.
                  </Typography>
                ) : (
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Scenario</TableCell>
                        <TableCell>Created</TableCell>
                        <TableCell align="right">Portfolios</TableCell>
                        <TableCell align="right">Avg Utilization</TableCell>
                        <TableCell align="right">Avg Workload</TableCell>
                        <TableCell align="right">Conflicts</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {scenarios.scenarios.map((item) => (
                        <TableRow key={item.scenarioId}>
                          <TableCell>{item.scenarioName ?? item.scenarioId}</TableCell>
                          <TableCell>{new Date(item.createdAt).toLocaleString()}</TableCell>
                          <TableCell align="right">{item.portfolioCount}</TableCell>
                          <TableCell align="right">
                            {item.averageUtilizationPercentage != null
                              ? `${item.averageUtilizationPercentage}%`
                              : '—'}
                          </TableCell>
                          <TableCell align="right">
                            {item.averageWorkloadPercentage != null ? `${item.averageWorkloadPercentage}%` : '—'}
                          </TableCell>
                          <TableCell align="right">{item.conflictCount}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                )}
              </Stack>
            </Paper>
          )}

          <Paper sx={{ p: 2.5 }}>
            <Stack spacing={1.5}>
              <Typography variant="subtitle1">Compare Scenarios</Typography>
              <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} alignItems={{ md: 'center' }}>
                <TextField
                  label="Left Scenario ID"
                  value={leftScenarioId}
                  onChange={(event) => setLeftScenarioId(event.target.value)}
                  size="small"
                  sx={{ minWidth: 320 }}
                />
                <TextField
                  label="Right Scenario ID"
                  value={rightScenarioId}
                  onChange={(event) => setRightScenarioId(event.target.value)}
                  size="small"
                  sx={{ minWidth: 320 }}
                />
                <Button variant="contained" onClick={() => void runCompare()} disabled={compareLoading}>
                  Compare Scenarios
                </Button>
              </Stack>

              {compareError && <Alert severity="error">{compareError}</Alert>}

              {comparison && (
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Field</TableCell>
                      <TableCell>Left</TableCell>
                      <TableCell>Right</TableCell>
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
            </Stack>
          </Paper>
        </>
      )}
    </Stack>
  )
}

