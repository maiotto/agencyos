using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AgencyOS.Application.Tests;

public class ExecutiveRecommendationSummaryServiceTests
{
    private readonly Mock<IExecutiveRecommendationSummaryRepository> _repository = new();
    private readonly Mock<IRecommendationRepository> _recommendationRepository = new();
    private readonly Mock<IAIRecommendationRepository> _aiRecommendationRepository = new();
    private readonly Mock<IExplainabilityRepository> _explainabilityRepository = new();
    private readonly Mock<IDecisionRepository> _decisionRepository = new();
    private readonly Mock<IAuditService> _auditService = new();

    private ExecutiveRecommendationSummaryService CreateService() =>
        new(
            _repository.Object,
            _recommendationRepository.Object,
            _aiRecommendationRepository.Object,
            _explainabilityRepository.Object,
            _decisionRepository.Object,
            new ExecutiveRecommendationSummaryGenerationService(),
            new ExecutiveRecommendationSummaryComparisonService(),
            _auditService.Object,
            NullLogger<ExecutiveRecommendationSummaryService>.Instance);

    public ExecutiveRecommendationSummaryServiceTests()
    {
        _auditService
            .Setup(service => service.RecordSafeAsync(
                It.IsAny<AuditEventWriteRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _aiRecommendationRepository
            .Setup(repository => repository.GetByRecommendationIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AIRecommendation>());
        _explainabilityRepository
            .Setup(repository => repository.GetByRecommendationIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Explainability>());
        _decisionRepository
            .Setup(repository => repository.GetByRecommendationIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Decision?)null);
    }

    [Fact]
    public async Task GenerateAsync_PersistsAndAudits()
    {
        var recommendation = CreateRecommendation();
        ExecutiveRecommendationSummary? persisted = null;
        AuditEventWriteRequest? audit = null;

        _recommendationRepository.Setup(repository => repository.GetByIdAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendation);
        _repository.Setup(repository => repository.GetNextSummaryVersionAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _repository.Setup(repository => repository.AddAsync(
                It.IsAny<ExecutiveRecommendationSummary>(),
                It.IsAny<CancellationToken>()))
            .Callback<ExecutiveRecommendationSummary, CancellationToken>((item, _) => persisted = item)
            .ReturnsAsync((ExecutiveRecommendationSummary item, CancellationToken _) => item);
        _auditService.Setup(service => service.RecordSafeAsync(
                It.IsAny<AuditEventWriteRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<AuditEventWriteRequest, CancellationToken>((request, _) => audit = request)
            .Returns(Task.CompletedTask);

        var result = await CreateService().GenerateAsync(new GenerateExecutiveRecommendationSummaryRequest
        {
            RecommendationId = recommendation.Id,
            GeneratedBy = "exec"
        });

        Assert.NotNull(persisted);
        Assert.Equal(1, result.SummaryVersion);
        Assert.False(string.IsNullOrWhiteSpace(result.RecommendedActions));
        Assert.NotNull(audit);
        Assert.Equal(AuditEntityTypes.ExecutiveRecommendationSummary, audit!.EntityType);
        Assert.Equal("ExecutiveRecommendationSummary.Generate", audit.Action);
    }

    [Fact]
    public async Task CreateNewVersionAsync_CreatesNextVersion()
    {
        var recommendation = CreateRecommendation();
        var existing = new ExecutiveRecommendationSummaryGenerationService().Generate(
            recommendation,
            null,
            null,
            null,
            1,
            "exec",
            DateTimeOffset.UtcNow);

        _repository.Setup(repository => repository.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _recommendationRepository.Setup(repository => repository.GetByIdAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendation);
        _repository.Setup(repository => repository.GetNextSummaryVersionAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);
        _repository.Setup(repository => repository.AddAsync(
                It.IsAny<ExecutiveRecommendationSummary>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExecutiveRecommendationSummary item, CancellationToken _) => item);

        var result = await CreateService().CreateNewVersionAsync(
            existing.Id,
            new CreateExecutiveRecommendationSummaryVersionRequest { GeneratedBy = "exec" });

        Assert.Equal(2, result.SummaryVersion);
        Assert.Equal(recommendation.Id, result.RecommendationId);
    }

    [Fact]
    public async Task GenerateAsync_UnexpectedFailure_DoesNotTouchRecommendation()
    {
        var recommendation = CreateRecommendation();
        _recommendationRepository.Setup(repository => repository.GetByIdAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendation);
        _repository.Setup(repository => repository.GetNextSummaryVersionAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("storage unavailable"));

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService().GenerateAsync(new GenerateExecutiveRecommendationSummaryRequest
            {
                RecommendationId = recommendation.Id,
                GeneratedBy = "exec"
            }));

        Assert.Contains("was not modified", ex.Message, StringComparison.OrdinalIgnoreCase);
        _repository.Verify(
            repository => repository.AddAsync(
                It.IsAny<ExecutiveRecommendationSummary>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _recommendationRepository.Verify(
            repository => repository.UpdateAsync(It.IsAny<Recommendation>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CompareAsync_ReturnsDifferences()
    {
        var recommendation = CreateRecommendation();
        var summary = new ExecutiveRecommendationSummaryGenerationService().Generate(
            recommendation,
            null,
            null,
            null,
            1,
            "exec",
            DateTimeOffset.UtcNow);

        _repository.Setup(repository => repository.GetByIdAsync(summary.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);
        _recommendationRepository.Setup(repository => repository.GetByIdAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendation);

        var comparison = await CreateService().CompareAsync(summary.Id);

        Assert.Equal(recommendation.Id, comparison.RecommendationId);
        Assert.NotEmpty(comparison.Differences);
    }

    [Fact]
    public void Validators_RejectInvalidRequests()
    {
        var generateValidator = new GenerateExecutiveRecommendationSummaryRequestValidator();
        Assert.False(generateValidator.Validate(new GenerateExecutiveRecommendationSummaryRequest()).IsValid);

        var versionValidator = new CreateExecutiveRecommendationSummaryVersionRequestValidator();
        Assert.False(versionValidator.Validate(new CreateExecutiveRecommendationSummaryVersionRequest()).IsValid);

        var queryValidator = new ExecutiveRecommendationSummaryQueryParametersValidator();
        Assert.False(queryValidator.Validate(new ExecutiveRecommendationSummaryQueryParameters
        {
            Status = "Nope"
        }).IsValid);
    }

    [Fact]
    public void GenerationService_ProducesMandatorySections()
    {
        var recommendation = CreateRecommendation();
        var summary = new ExecutiveRecommendationSummaryGenerationService().Generate(
            recommendation,
            null,
            null,
            null,
            1,
            "exec",
            DateTimeOffset.UtcNow);

        Assert.False(string.IsNullOrWhiteSpace(summary.ExecutiveSummary));
        Assert.False(string.IsNullOrWhiteSpace(summary.KeyDecisionFactors));
        Assert.False(string.IsNullOrWhiteSpace(summary.RecommendedActions));
        Assert.InRange(summary.ConfidenceLevel, 0, 100);
    }

    private static Recommendation CreateRecommendation() =>
        Recommendation.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "REC-EXEC-SVC",
            "Human + AI",
            "Summary",
            "Reason",
            72m,
            2,
            1,
            RecommendationVersions.CurrentDecisionEngineVersion,
            null,
            "{\"hours\":40}",
            "{\"allocated\":20}",
            "{\"ok\":true}",
            "decision-engine",
            DateTimeOffset.UtcNow);
}
