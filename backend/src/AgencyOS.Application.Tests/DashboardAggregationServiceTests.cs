using System.Diagnostics;
using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using Moq;

namespace AgencyOS.Application.Tests;

public class DashboardAggregationServiceTests
{
    private static readonly Guid CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

    private readonly Mock<IRecommendationRepository> _recommendationRepository = new();
    private readonly Mock<IDecisionRepository> _decisionRepository = new();
    private readonly Mock<IPortfolioRepository> _portfolioRepository = new();
    private readonly Mock<ICapacityHistoryRepository> _capacityHistoryRepository = new();
    private readonly Mock<IWorkloadHistoryRepository> _workloadHistoryRepository = new();
    private readonly Mock<IPlanningTemplateRepository> _planningTemplateRepository = new();
    private readonly Mock<IAIRecommendationRepository> _aiRecommendationRepository = new();
    private readonly Mock<IExplainabilityRepository> _explainabilityRepository = new();
    private readonly Mock<IExecutiveRecommendationSummaryRepository> _executiveRecommendationSummaryRepository = new();
    private readonly Mock<IAuditEventRepository> _auditEventRepository = new();

    public DashboardAggregationServiceTests()
    {
        SetupEmptyDefaults();
    }

    private DashboardAggregationService CreateService()
    {
        return new DashboardAggregationService(
            _recommendationRepository.Object,
            _decisionRepository.Object,
            _portfolioRepository.Object,
            _capacityHistoryRepository.Object,
            _workloadHistoryRepository.Object,
            _planningTemplateRepository.Object,
            _aiRecommendationRepository.Object,
            _explainabilityRepository.Object,
            _executiveRecommendationSummaryRepository.Object,
            _auditEventRepository.Object,
            new DashboardHealthCalculationService());
    }

    private void SetupEmptyDefaults()
    {
        _recommendationRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<RecommendationQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Recommendation>());
        _decisionRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<DecisionQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Decision>());
        _portfolioRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<PortfolioQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Portfolio>());
        _capacityHistoryRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<CapacityHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CapacityHistory>());
        _workloadHistoryRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<WorkloadHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<WorkloadHistory>());
        _planningTemplateRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<PlanningTemplateQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PlanningTemplate>());
        _aiRecommendationRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<AIRecommendationQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AIRecommendation>());
        _explainabilityRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<ExplainabilityQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Explainability>());
        _executiveRecommendationSummaryRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<ExecutiveRecommendationSummaryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ExecutiveRecommendationSummary>());
        _auditEventRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<AuditEventQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AuditEvent>());
    }

    [Fact]
    public async Task BuildSummaryAsync_ReturnsCorrectCountsAndHealth()
    {
        var portfolios = new[]
        {
            CreatePortfolio(isActive: true, utilization: 60m, workload: 55m),
            CreatePortfolio(isActive: true, utilization: 90m, workload: 88m),
            CreatePortfolio(isActive: false, utilization: 50m, workload: 50m)
        };
        _portfolioRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<PortfolioQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(portfolios);

        var templates = new[] { CreateTemplate(), CreateTemplate() };
        _planningTemplateRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<PlanningTemplateQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        var recommendation = CreateRecommendation();
        _recommendationRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<RecommendationQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([recommendation]);

        var completedDecision = CreateDecision(recommendation.Id);
        completedDecision.StartImplementation("planner", null, DateTimeOffset.UtcNow);
        completedDecision.Complete("planner", null, DateTimeOffset.UtcNow);

        var pendingDecision = CreateDecision(recommendation.Id);

        _decisionRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<DecisionQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([completedDecision, pendingDecision]);

        var service = CreateService();

        var summary = await service.BuildSummaryAsync(CompanyId, new EnterpriseDashboardQueryParameters());

        Assert.Equal(CompanyId, summary.CompanyId);
        Assert.Equal(3, summary.PortfolioCount);
        Assert.Equal(2, summary.ActivePortfolioCount);
        Assert.Equal(2, summary.PlanningTemplateCount);
        Assert.Equal(1, summary.RecommendationCount);
        Assert.Equal(2, summary.DecisionCount);
        Assert.Equal(1, summary.PendingDecisionCount);
        Assert.Equal(1, summary.CompletedDecisionCount);
        Assert.Equal(PortfolioHealth.AtRisk, summary.OverallHealth.Status);
        Assert.Equal($"/portfolios?companyId={CompanyId}", summary.DrillDownPath);
    }

    [Fact]
    public async Task BuildPlanningAsync_ReturnsCorrectCountsAndStatusBreakdown()
    {
        var activeTemplate = CreateTemplate();
        activeTemplate.Activate(DateTimeOffset.UtcNow);
        var inactiveTemplate = CreateTemplate();

        _planningTemplateRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<PlanningTemplateQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([activeTemplate, inactiveTemplate]);

        var service = CreateService();

        var planning = await service.BuildPlanningAsync(CompanyId, new EnterpriseDashboardQueryParameters());

        Assert.Equal(2, planning.TemplateCount);
        Assert.Equal(1, planning.ActiveTemplateCount);
        Assert.Equal(1, planning.InactiveTemplateCount);
        Assert.Equal(2, planning.StatusBreakdown.Count);
        Assert.Equal($"/planning-templates?companyId={CompanyId}", planning.DrillDownPath);
    }

    [Fact]
    public async Task BuildPortfolioAsync_ReturnsAveragesAndOverallHealth()
    {
        var healthyPortfolio = CreatePortfolio(isActive: true, utilization: 60m, workload: 55m);
        var atRiskPortfolio = CreatePortfolio(isActive: true, utilization: 90m, workload: 88m);

        _portfolioRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<PortfolioQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([healthyPortfolio, atRiskPortfolio]);

        var service = CreateService();

        var portfolio = await service.BuildPortfolioAsync(CompanyId, new EnterpriseDashboardQueryParameters());

        Assert.Equal(2, portfolio.PortfolioCount);
        Assert.Equal(75m, portfolio.AverageUtilizationPercentage);
        Assert.Equal(71.5m, portfolio.AverageWorkloadPercentage);
        Assert.Equal(PortfolioHealth.AtRisk, portfolio.OverallHealth.Status);
        Assert.Equal($"/portfolios?companyId={CompanyId}", portfolio.DrillDownPath);
    }

    [Fact]
    public async Task BuildCapacityAsync_ReturnsTotalsAndAverageUtilization()
    {
        var histories = new[]
        {
            CreateCapacityHistory(configuredHours: 160m, allocatedHours: 100m, utilization: 62.5m),
            CreateCapacityHistory(configuredHours: 160m, allocatedHours: 150m, utilization: 93.75m)
        };

        _capacityHistoryRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<CapacityHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(histories);

        var service = CreateService();

        var capacity = await service.BuildCapacityAsync(CompanyId, new EnterpriseDashboardQueryParameters());

        Assert.Equal(2, capacity.RecordCount);
        Assert.Equal(320m, capacity.TotalCapacityHours);
        Assert.Equal(250m, capacity.TotalAllocatedHours);
        Assert.Equal(78.13m, capacity.AverageUtilizationPercentage);
        Assert.Equal(PortfolioHealth.Healthy, capacity.UtilizationHealth.Status);
        Assert.Equal($"/capacity/history?companyId={CompanyId}", capacity.DrillDownPath);
    }

    [Fact]
    public async Task BuildCapacityAsync_ReturnsZeroedResponse_WhenNoHistory()
    {
        var service = CreateService();

        var capacity = await service.BuildCapacityAsync(CompanyId, new EnterpriseDashboardQueryParameters());

        Assert.Equal(0, capacity.RecordCount);
        Assert.Equal(0m, capacity.AverageUtilizationPercentage);
        Assert.Equal(PortfolioHealth.Underutilized, capacity.UtilizationHealth.Status);
    }

    [Fact]
    public async Task BuildWorkloadAsync_ReturnsTotalsAndAverageWorkload()
    {
        var histories = new[]
        {
            CreateWorkloadHistory(allocatedHours: 90m, capacityHours: 160m, workload: 56.25m),
            CreateWorkloadHistory(allocatedHours: 130m, capacityHours: 160m, workload: 81.25m)
        };

        _workloadHistoryRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<WorkloadHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(histories);

        var service = CreateService();

        var workload = await service.BuildWorkloadAsync(CompanyId, new EnterpriseDashboardQueryParameters());

        Assert.Equal(2, workload.RecordCount);
        Assert.Equal(220m, workload.TotalAllocatedHours);
        Assert.Equal(320m, workload.TotalCapacityHours);
        Assert.Equal(68.75m, workload.AverageWorkloadPercentage);
        Assert.Equal($"/workload/history?companyId={CompanyId}", workload.DrillDownPath);
    }

    [Fact]
    public async Task BuildRecommendationsAsync_ReturnsCountsAndAverageScore()
    {
        var active = CreateRecommendation(score: 80m);
        var archived = CreateRecommendation(score: 60m);
        archived.Archive(DateTimeOffset.UtcNow);

        _recommendationRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<RecommendationQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([active, archived]);

        var service = CreateService();

        var recommendations = await service.BuildRecommendationsAsync(CompanyId, new EnterpriseDashboardQueryParameters());

        Assert.Equal(2, recommendations.TotalCount);
        Assert.Equal(1, recommendations.ActiveCount);
        Assert.Equal(1, recommendations.ArchivedCount);
        Assert.Equal(70m, recommendations.AverageScore);
        Assert.Equal($"/recommendations?companyId={CompanyId}", recommendations.DrillDownPath);
    }

    [Fact]
    public async Task BuildRecommendationsAsync_ReturnsNullAverageScore_WhenNoneScored()
    {
        var recommendation = CreateRecommendation(score: null);

        _recommendationRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<RecommendationQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([recommendation]);

        var service = CreateService();

        var recommendations = await service.BuildRecommendationsAsync(CompanyId, new EnterpriseDashboardQueryParameters());

        Assert.Null(recommendations.AverageScore);
    }

    [Fact]
    public async Task BuildDecisionsAsync_ReturnsStatusBreakdownAndCounts()
    {
        var recommendation = CreateRecommendation();

        var completed = CreateDecision(recommendation.Id);
        completed.StartImplementation("planner", null, DateTimeOffset.UtcNow);
        completed.Complete("planner", null, DateTimeOffset.UtcNow);

        var cancelled = CreateDecision(recommendation.Id);
        cancelled.Cancel("planner", null, DateTimeOffset.UtcNow);

        var pending = CreateDecision(recommendation.Id);

        _decisionRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<DecisionQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([completed, cancelled, pending]);

        var service = CreateService();

        var decisions = await service.BuildDecisionsAsync(CompanyId, new EnterpriseDashboardQueryParameters());

        Assert.Equal(3, decisions.TotalCount);
        Assert.Equal(1, decisions.CompletedCount);
        Assert.Equal(1, decisions.CancelledCount);
        Assert.Equal(3, decisions.DecisionStatusBreakdown.Count);
        Assert.Equal($"/decisions?companyId={CompanyId}", decisions.DrillDownPath);
    }

    [Fact]
    public async Task BuildAiAsync_ReturnsCountsAndAverages()
    {
        var recommendation = CreateRecommendation();
        var aiRecommendations = new[]
        {
            CreateAIRecommendation(recommendation.Id, confidenceScore: 70m),
            CreateAIRecommendation(recommendation.Id, confidenceScore: 90m)
        };

        _aiRecommendationRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<AIRecommendationQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiRecommendations);

        var explainabilities = new[] { CreateExplainability(recommendation.Id) };
        _explainabilityRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<ExplainabilityQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(explainabilities);

        var executiveSummaries = new[]
        {
            CreateExecutiveSummary(recommendation.Id, confidenceLevel: 80m),
            CreateExecutiveSummary(recommendation.Id, confidenceLevel: 60m)
        };
        _executiveRecommendationSummaryRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<ExecutiveRecommendationSummaryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(executiveSummaries);

        var service = CreateService();

        var ai = await service.BuildAiAsync(CompanyId, new EnterpriseDashboardQueryParameters());

        Assert.Equal(2, ai.AIRecommendationCount);
        Assert.Equal(80m, ai.AverageConfidenceScore);
        Assert.Equal(1, ai.ExplainabilityCount);
        Assert.Equal(2, ai.ExecutiveSummaryCount);
        Assert.Equal(70m, ai.AverageExecutiveSummaryConfidenceLevel);
        Assert.Equal($"/ai-recommendations?companyId={CompanyId}", ai.DrillDownPath);
    }

    [Fact]
    public async Task BuildAuditAsync_ReturnsBreakdownsAndLastEventAt()
    {
        var olderEvent = CreateAuditEvent(AuditEntityTypes.Decision, DateTimeOffset.UtcNow.AddDays(-2));
        var newerEvent = CreateAuditEvent(AuditEntityTypes.Recommendation, DateTimeOffset.UtcNow.AddDays(-1));

        _auditEventRepository
            .Setup(repository => repository.QueryAsync(It.IsAny<AuditEventQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([olderEvent, newerEvent]);

        var service = CreateService();

        var audit = await service.BuildAuditAsync(CompanyId, new EnterpriseDashboardQueryParameters());

        Assert.Equal(2, audit.EventCount);
        Assert.Equal(2, audit.EntityTypeBreakdown.Count);
        Assert.Equal(newerEvent.OccurredAt, audit.LastEventAt);
        Assert.Equal($"/audit?companyId={CompanyId}", audit.DrillDownPath);
    }

    [Fact]
    public async Task AllBuildMethods_CompleteQuickly_WithEmptyRepositories()
    {
        var service = CreateService();
        var parameters = new EnterpriseDashboardQueryParameters();

        var stopwatch = Stopwatch.StartNew();

        await Task.WhenAll(
            service.BuildSummaryAsync(CompanyId, parameters),
            service.BuildPlanningAsync(CompanyId, parameters),
            service.BuildPortfolioAsync(CompanyId, parameters),
            service.BuildCapacityAsync(CompanyId, parameters),
            service.BuildWorkloadAsync(CompanyId, parameters),
            service.BuildRecommendationsAsync(CompanyId, parameters),
            service.BuildDecisionsAsync(CompanyId, parameters),
            service.BuildAiAsync(CompanyId, parameters),
            service.BuildAuditAsync(CompanyId, parameters));

        stopwatch.Stop();

        Assert.True(
            stopwatch.ElapsedMilliseconds < 2000,
            $"Expected aggregation smoke test to complete under 2s, took {stopwatch.ElapsedMilliseconds}ms.");
    }

    private static Recommendation CreateRecommendation(decimal? score = 75m) =>
        Recommendation.Create(
            CompanyId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            $"REC-{Guid.NewGuid():N}",
            "Enterprise Dashboard Recommendation",
            "Summary",
            "Reason",
            score,
            1,
            1,
            RecommendationVersions.CurrentDecisionEngineVersion,
            null,
            "{}",
            "{}",
            "{\"ok\":true}",
            "decision-engine",
            DateTimeOffset.UtcNow);

    private static Decision CreateDecision(Guid recommendationId) =>
        Decision.Create(
            recommendationId,
            CompanyId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "planner",
            DateTimeOffset.UtcNow);

    private static Portfolio CreatePortfolio(
        bool isActive,
        decimal utilization = 50m,
        decimal workload = 50m)
    {
        var portfolio = Portfolio.Create(
            CompanyId,
            $"Portfolio {Guid.NewGuid():N}",
            null,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            null,
            [(Guid.NewGuid(), 1)],
            DateTimeOffset.UtcNow);

        portfolio.CalculateCapacity(
            $"{{\"overallUtilizationPercentage\":{utilization.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}",
            DateTimeOffset.UtcNow);
        portfolio.CalculateWorkload(
            $"{{\"overallWorkloadPercentage\":{workload.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}",
            DateTimeOffset.UtcNow);
        portfolio.CalculateHealth(utilization, workload, null, "{}", DateTimeOffset.UtcNow);

        if (!isActive)
        {
            portfolio.Deactivate(DateTimeOffset.UtcNow);
        }

        return portfolio;
    }

    private static PlanningTemplate CreateTemplate() =>
        PlanningTemplate.Create(
            CompanyId,
            $"Template {Guid.NewGuid():N}",
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            ResourceAvailabilityStrategies.RequireActiveConfiguration,
            30,
            0,
            new PlanningTemplateCapacityRules(85m, true),
            null,
            DateTimeOffset.UtcNow);

    private static CapacityHistory CreateCapacityHistory(
        decimal configuredHours,
        decimal allocatedHours,
        decimal utilization) =>
        CapacityHistory.Create(
            Guid.NewGuid(),
            CompanyId,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
            DateOnly.FromDateTime(DateTime.UtcNow),
            20,
            2,
            18,
            configuredHours,
            configuredHours,
            configuredHours,
            allocatedHours,
            utilization,
            CapacityHistoryVersions.Current,
            "{}",
            DateTimeOffset.UtcNow);

    private static WorkloadHistory CreateWorkloadHistory(
        decimal allocatedHours,
        decimal capacityHours,
        decimal workload) =>
        WorkloadHistory.Create(
            Guid.NewGuid(),
            CompanyId,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
            DateOnly.FromDateTime(DateTime.UtcNow),
            allocatedHours,
            capacityHours,
            workload,
            20,
            2,
            18,
            WorkloadHistoryVersions.Current,
            "{}",
            DateTimeOffset.UtcNow);

    private static AIRecommendation CreateAIRecommendation(Guid recommendationId, decimal confidenceScore) =>
        AIRecommendation.Generate(
            recommendationId,
            1,
            1,
            DateTimeOffset.UtcNow,
            "ai-advisor",
            confidenceScore,
            "summary",
            "reasoning",
            "[\"assumption\"]",
            "[\"risk\"]",
            "[\"alternative\"]",
            "Hybrid delivery",
            "{\"delta\":-5}",
            "{\"delta\":-8}",
            AIRecommendationVersions.CurrentModelVersion,
            AIRecommendationVersions.CurrentPromptVersion,
            companyId: CompanyId);

    private static Explainability CreateExplainability(Guid recommendationId) =>
        Explainability.Generate(
            recommendationId,
            null,
            ExplainabilityTypes.Recommendation,
            1,
            DateTimeOffset.UtcNow,
            "explainer",
            "summary",
            "detailed",
            "[\"factor\"]",
            "[\"assumption\"]",
            "[\"risk\"]",
            "confidence explanation",
            "capacity explanation",
            "workload explanation",
            ExplainabilityVersions.CurrentModelVersion,
            ExplainabilityVersions.CurrentPromptVersion,
            companyId: CompanyId);

    private static ExecutiveRecommendationSummary CreateExecutiveSummary(Guid recommendationId, decimal confidenceLevel) =>
        ExecutiveRecommendationSummary.Generate(
            recommendationId,
            null,
            null,
            1,
            DateTimeOffset.UtcNow,
            "exec",
            "Executive briefing",
            "[\"factor\"]",
            "Business impact",
            "Capacity impact",
            "Workload impact",
            "[\"risk\"]",
            "[\"assumption\"]",
            confidenceLevel,
            "[\"action\"]",
            ExecutiveRecommendationSummaryVersions.CurrentModelVersion,
            ExecutiveRecommendationSummaryVersions.CurrentPromptVersion,
            companyId: CompanyId);

    private static AuditEvent CreateAuditEvent(string entityType, DateTimeOffset occurredAt) =>
        AuditEvent.Create(
            entityType,
            Guid.NewGuid(),
            null,
            AuditEventTypes.Created,
            $"{entityType}.Create",
            CompanyId,
            "planner",
            "planner",
            occurredAt,
            AuditSources.Api,
            Guid.NewGuid(),
            null,
            null,
            null,
            null,
            null);
}
