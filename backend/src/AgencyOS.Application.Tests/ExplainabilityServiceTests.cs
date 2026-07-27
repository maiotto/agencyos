using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AgencyOS.Application.Tests;

public class ExplainabilityServiceTests
{
    private readonly Mock<IExplainabilityRepository> _repository = new();
    private readonly Mock<IRecommendationRepository> _recommendationRepository = new();
    private readonly Mock<IAIRecommendationRepository> _aiRecommendationRepository = new();
    private readonly Mock<IAuditService> _auditService = new();

    private ExplainabilityService CreateService() =>
        new(
            _repository.Object,
            _recommendationRepository.Object,
            _aiRecommendationRepository.Object,
            new ExplainabilityGenerationService(),
            _auditService.Object,
            NullLogger<ExplainabilityService>.Instance);

    public ExplainabilityServiceTests()
    {
        _auditService
            .Setup(service => service.RecordSafeAsync(
                It.IsAny<AuditEventWriteRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task GenerateAsync_ForRecommendation_PersistsAndAudits()
    {
        var recommendation = CreateRecommendation();
        Explainability? persisted = null;
        AuditEventWriteRequest? audit = null;

        _recommendationRepository.Setup(repository => repository.GetByIdAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendation);
        _repository.Setup(repository => repository.GetNextGenerationVersionAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _repository.Setup(repository => repository.AddAsync(
                It.IsAny<Explainability>(),
                It.IsAny<CancellationToken>()))
            .Callback<Explainability, CancellationToken>((item, _) => persisted = item)
            .ReturnsAsync((Explainability item, CancellationToken _) => item);
        _auditService.Setup(service => service.RecordSafeAsync(
                It.IsAny<AuditEventWriteRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<AuditEventWriteRequest, CancellationToken>((request, _) => audit = request)
            .Returns(Task.CompletedTask);

        var result = await CreateService().GenerateAsync(new GenerateExplainabilityRequest
        {
            RecommendationId = recommendation.Id,
            GeneratedBy = "planner"
        });

        Assert.NotNull(persisted);
        Assert.Equal(ExplainabilityTypes.Recommendation, result.ExplanationType);
        Assert.False(string.IsNullOrWhiteSpace(result.ConfidenceExplanation));
        Assert.False(string.IsNullOrWhiteSpace(result.ModelVersion));
        Assert.False(string.IsNullOrWhiteSpace(result.PromptVersion));
        Assert.NotNull(audit);
        Assert.Equal(AuditEntityTypes.Explainability, audit!.EntityType);
        Assert.Equal("Explainability.Generate", audit.Action);
    }

    [Fact]
    public async Task GenerateAsync_ForAIRecommendation_Persists()
    {
        var recommendation = CreateRecommendation();
        var ai = new AIRecommendationGenerationService().Generate(
            recommendation,
            1,
            "planner",
            DateTimeOffset.UtcNow);

        _recommendationRepository.Setup(repository => repository.GetByIdAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendation);
        _aiRecommendationRepository.Setup(repository => repository.GetByIdAsync(
                ai.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ai);
        _repository.Setup(repository => repository.GetNextGenerationVersionAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _repository.Setup(repository => repository.AddAsync(
                It.IsAny<Explainability>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Explainability item, CancellationToken _) => item);

        var result = await CreateService().GenerateAsync(new GenerateExplainabilityRequest
        {
            RecommendationId = recommendation.Id,
            AIRecommendationId = ai.Id,
            GeneratedBy = "planner"
        });

        Assert.Equal(ExplainabilityTypes.AIRecommendation, result.ExplanationType);
        Assert.Equal(ai.Id, result.AIRecommendationId);
    }

    [Fact]
    public async Task GenerateAsync_UnexpectedFailure_DoesNotTouchRecommendation()
    {
        var recommendation = CreateRecommendation();
        _recommendationRepository.Setup(repository => repository.GetByIdAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendation);
        _repository.Setup(repository => repository.GetNextGenerationVersionAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("storage unavailable"));

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService().GenerateAsync(new GenerateExplainabilityRequest
            {
                RecommendationId = recommendation.Id,
                GeneratedBy = "planner"
            }));

        Assert.Contains("was not modified", ex.Message, StringComparison.OrdinalIgnoreCase);
        _repository.Verify(
            repository => repository.AddAsync(It.IsAny<Explainability>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _recommendationRepository.Verify(
            repository => repository.UpdateAsync(It.IsAny<Recommendation>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ArchiveAsync_ArchivesItem()
    {
        var recommendation = CreateRecommendation();
        var item = new ExplainabilityGenerationService().GenerateForRecommendation(
            recommendation,
            1,
            "planner",
            DateTimeOffset.UtcNow);

        _repository.Setup(repository => repository.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        _repository.Setup(repository => repository.UpdateAsync(It.IsAny<Explainability>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Explainability value, CancellationToken _) => value);

        var result = await CreateService().ArchiveAsync(item.Id);

        Assert.True(result.Archived);
        Assert.Equal(ExplainabilityStatus.Archived, result.Status);
    }

    [Fact]
    public void Validators_RejectInvalidRequests()
    {
        var generateValidator = new GenerateExplainabilityRequestValidator();
        Assert.False(generateValidator.Validate(new GenerateExplainabilityRequest()).IsValid);

        var queryValidator = new ExplainabilityQueryParametersValidator();
        Assert.False(queryValidator.Validate(new ExplainabilityQueryParameters
        {
            Status = "Nope"
        }).IsValid);
    }

    [Fact]
    public void GenerationService_ProducesMandatorySections()
    {
        var recommendation = CreateRecommendation();
        var generator = new ExplainabilityGenerationService();
        var explanation = generator.GenerateForRecommendation(
            recommendation,
            1,
            "planner",
            DateTimeOffset.UtcNow);

        Assert.False(string.IsNullOrWhiteSpace(explanation.ExecutiveSummary));
        Assert.False(string.IsNullOrWhiteSpace(explanation.DetailedExplanation));
        Assert.False(string.IsNullOrWhiteSpace(explanation.DecisionFactors));
        Assert.False(string.IsNullOrWhiteSpace(explanation.Assumptions));
        Assert.False(string.IsNullOrWhiteSpace(explanation.Risks));
        Assert.False(string.IsNullOrWhiteSpace(explanation.ConfidenceExplanation));
        Assert.False(string.IsNullOrWhiteSpace(explanation.CapacityExplanation));
        Assert.False(string.IsNullOrWhiteSpace(explanation.WorkloadExplanation));
    }

    private static Recommendation CreateRecommendation() =>
        Recommendation.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "REC-EXP-SVC",
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
