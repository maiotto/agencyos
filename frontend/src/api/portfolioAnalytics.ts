import { apiClient } from './client'
import type {
  PortfolioAnalyticsDetail,
  PortfolioAnalyticsOverview,
  PortfolioAnalyticsQuery,
  PortfolioComparison,
  PortfolioCompareQuery,
  PortfolioHealthAnalytics,
  PortfolioPerformance,
  PortfolioRanking,
  PortfolioTrends,
} from '../types/portfolioAnalytics'

export async function getPortfolioAnalyticsOverview(
  query: PortfolioAnalyticsQuery,
): Promise<PortfolioAnalyticsOverview> {
  const { data } = await apiClient.get<PortfolioAnalyticsOverview>('/portfolio-analytics', { params: query })
  return data
}

export async function getPortfolioAnalyticsDetail(
  portfolioId: string,
  query: PortfolioAnalyticsQuery,
): Promise<PortfolioAnalyticsDetail> {
  const { data } = await apiClient.get<PortfolioAnalyticsDetail>(`/portfolio-analytics/${portfolioId}`, {
    params: query,
  })
  return data
}

export async function getPortfolioAnalyticsTrends(query: PortfolioAnalyticsQuery): Promise<PortfolioTrends> {
  const { data } = await apiClient.get<PortfolioTrends>('/portfolio-analytics/trends', { params: query })
  return data
}

export async function comparePortfolios(query: PortfolioCompareQuery): Promise<PortfolioComparison> {
  const { data } = await apiClient.get<PortfolioComparison>('/portfolio-analytics/compare', { params: query })
  return data
}

export async function getPortfolioRanking(query: PortfolioAnalyticsQuery): Promise<PortfolioRanking> {
  const { data } = await apiClient.get<PortfolioRanking>('/portfolio-analytics/ranking', { params: query })
  return data
}

export async function getPortfolioHealthAnalytics(
  query: PortfolioAnalyticsQuery,
): Promise<PortfolioHealthAnalytics> {
  const { data } = await apiClient.get<PortfolioHealthAnalytics>('/portfolio-analytics/health', { params: query })
  return data
}

export async function getPortfolioPerformance(query: PortfolioAnalyticsQuery): Promise<PortfolioPerformance> {
  const { data } = await apiClient.get<PortfolioPerformance>('/portfolio-analytics/performance', {
    params: query,
  })
  return data
}
