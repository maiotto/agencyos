import { apiClient } from './client'
import type {
  CompareCrossPortfolioScenariosRequest,
  ConflictSummary,
  CrossPortfolioBalance,
  CrossPortfolioOverview,
  CrossPortfolioPlanningQuery,
  CrossPortfolioScenarioList,
  CrossPortfolioSelectionQuery,
  ScenarioComparison,
  SimulateCrossPortfolioPlanRequest,
  SimulationSummary,
} from '../types/crossPortfolioPlanning'

export async function getCrossPortfolioOverview(query: CrossPortfolioPlanningQuery): Promise<CrossPortfolioOverview> {
  const { data } = await apiClient.get<CrossPortfolioOverview>('/cross-portfolio-planning', { params: query })
  return data
}

export async function getCrossPortfolioScenarios(
  query: CrossPortfolioPlanningQuery,
): Promise<CrossPortfolioScenarioList> {
  const { data } = await apiClient.get<CrossPortfolioScenarioList>('/cross-portfolio-planning/scenarios', {
    params: query,
  })
  return data
}

function toSelectionParams(query: CrossPortfolioSelectionQuery): Record<string, string | undefined> {
  const { portfolioIds, ...rest } = query
  return { ...rest, portfolioIds: portfolioIds.join(',') }
}

export async function getCrossPortfolioConflicts(query: CrossPortfolioSelectionQuery): Promise<ConflictSummary> {
  const { data } = await apiClient.get<ConflictSummary>('/cross-portfolio-planning/conflicts', {
    params: toSelectionParams(query),
  })
  return data
}

export async function getCrossPortfolioBalance(query: CrossPortfolioSelectionQuery): Promise<CrossPortfolioBalance> {
  const { data } = await apiClient.get<CrossPortfolioBalance>('/cross-portfolio-planning/balance', {
    params: toSelectionParams(query),
  })
  return data
}

export async function simulateCrossPortfolioPlan(
  request: SimulateCrossPortfolioPlanRequest,
): Promise<SimulationSummary> {
  const { data } = await apiClient.post<SimulationSummary>('/cross-portfolio-planning/simulate', request)
  return data
}

export async function compareCrossPortfolioScenarios(
  request: CompareCrossPortfolioScenariosRequest,
): Promise<ScenarioComparison> {
  const { data } = await apiClient.post<ScenarioComparison>('/cross-portfolio-planning/compare', request)
  return data
}
