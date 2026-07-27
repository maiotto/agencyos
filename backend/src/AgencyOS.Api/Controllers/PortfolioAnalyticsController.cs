using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

/// <summary>
/// Read-only Portfolio Analytics (US-404 / BR-2201..BR-2210). Aggregates existing Portfolio,
/// Recommendation, Recommendation History, Decision, and Capacity/Workload History data. Never
/// modifies a Portfolio and never recalculates Capacity/Workload engines.
/// </summary>
[ApiController]
[Route("portfolio-analytics")]
[Produces("application/json")]
public class PortfolioAnalyticsController : ControllerBase
{
    private readonly IPortfolioAnalyticsService _portfolioAnalyticsService;

    public PortfolioAnalyticsController(IPortfolioAnalyticsService portfolioAnalyticsService)
    {
        _portfolioAnalyticsService = portfolioAnalyticsService;
    }

    /// <summary>
    /// Returns the Portfolio Analytics overview for a Company: one card per Portfolio with
    /// health, mission count, utilization/workload snapshot, and Recommendation/Decision counts.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PortfolioAnalyticsOverviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PortfolioAnalyticsOverviewResponse>> GetOverview(
        [FromQuery] PortfolioAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var overview = await _portfolioAnalyticsService.GetOverviewAsync(parameters, cancellationToken);
        return Ok(overview);
    }

    /// <summary>
    /// Returns Capacity/Workload/Health/Recommendation/Decision trends bucketed by month,
    /// optionally scoped to a single Portfolio's Missions via <c>PortfolioId</c>.
    /// </summary>
    [HttpGet("trends")]
    [ProducesResponseType(typeof(PortfolioTrendsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PortfolioTrendsResponse>> GetTrends(
        [FromQuery] PortfolioAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var trends = await _portfolioAnalyticsService.GetTrendsAsync(parameters, cancellationToken);
        return Ok(trends);
    }

    /// <summary>
    /// Compares two Portfolios side-by-side using stored snapshot fields plus mission-scoped
    /// Recommendation/Decision counts.
    /// </summary>
    [HttpGet("compare")]
    [ProducesResponseType(typeof(PortfolioComparisonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PortfolioComparisonResponse>> GetComparison(
        [FromQuery] PortfolioCompareQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var comparison = await _portfolioAnalyticsService.GetComparisonAsync(parameters, cancellationToken);
        return Ok(comparison);
    }

    /// <summary>
    /// Ranks Portfolios by health severity, snapshot utilization, and Recommendation/Decision
    /// effectiveness.
    /// </summary>
    [HttpGet("ranking")]
    [ProducesResponseType(typeof(PortfolioRankingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PortfolioRankingResponse>> GetRanking(
        [FromQuery] PortfolioAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var ranking = await _portfolioAnalyticsService.GetRankingAsync(parameters, cancellationToken);
        return Ok(ranking);
    }

    /// <summary>
    /// Returns Portfolio health distribution and risk indicators for a Company.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(PortfolioHealthAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PortfolioHealthAnalyticsResponse>> GetHealth(
        [FromQuery] PortfolioAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var health = await _portfolioAnalyticsService.GetHealthAnalyticsAsync(parameters, cancellationToken);
        return Ok(health);
    }

    /// <summary>
    /// Returns Recommendation/Decision effectiveness metrics across a Company's Portfolios.
    /// </summary>
    [HttpGet("performance")]
    [ProducesResponseType(typeof(PortfolioPerformanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PortfolioPerformanceResponse>> GetPerformance(
        [FromQuery] PortfolioAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var performance = await _portfolioAnalyticsService.GetPerformanceAsync(parameters, cancellationToken);
        return Ok(performance);
    }

    /// <summary>
    /// Returns the full read-only analysis for a single Portfolio.
    /// </summary>
    [HttpGet("{portfolioId:guid}")]
    [ProducesResponseType(typeof(PortfolioAnalyticsDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PortfolioAnalyticsDetailResponse>> GetDetail(
        Guid portfolioId,
        [FromQuery] PortfolioAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var detail = await _portfolioAnalyticsService.GetDetailAsync(portfolioId, parameters, cancellationToken);
        return Ok(detail);
    }
}
