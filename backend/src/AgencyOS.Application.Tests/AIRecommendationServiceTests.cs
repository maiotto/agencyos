using AgencyOS.Application.Audit;
using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AgencyOS.Application.Tests;

public class AIRecommendationServiceTests
{
    private readonly Mock<IAIRecommendationRepository> _repository = new();
    private readonly Mock<IRecommendationRepository> _recommendationRepository = new();
    private readonly Mock<IAuditService> _auditService = new();

    private AIRecommendationService CreateService() =>
        new(
            _repository.Object,
            _recommendationRepository.Object,
            new AIRecommendationGenerationService(),
            new AIRecommendationComparisonService(),
            _auditService.Object,
            NullLogger<AIRecommendationService>.Instance);

    public AIRecommendationServiceTests()
    {
        _auditService
            .Setup(service => service.RecordSafeAsync(
                It.IsAny<AuditEventWriteRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task GenerateAsync_PersistsAdvisoryRecommendation_AndAudits()
    {
        var recommendation = CreateRecommendation();
        AIRecommendation? persisted = null;
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
                It.IsAny<AIRecommendation>(),
                It.IsAny<CancellationToken>()))
            .Callback<AIRecommendation, CancellationToken>((item, _) => persisted = item)
            .ReturnsAsync((AIRecommendation item, CancellationToken _) => item);
        _auditService.Setup(service => service.RecordSafeAsync(
                It.IsAny<AuditEventWriteRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<AuditEventWriteRequest, CancellationToken>((request, _) => audit = request)
            .Returns(Task.CompletedTask);

        var result = await CreateService().GenerateAsync(new GenerateAIRecommendationRequest
        {
            RecommendationId = recommendation.Id,
            GeneratedBy = "planner"
        });

        Assert.NotNull(persisted);
        Assert.Equal(recommendation.Id, result.RecommendationId);
        Assert.Equal(1, result.GenerationVersion);
        Assert.False(string.IsNullOrWhiteSpace(result.ModelVersion));
        Assert.False(string.IsNullOrWhiteSpace(result.PromptVersion));
        Assert.NotNull(audit);
        Assert.Equal(AuditEntityTypes.AIRecommendation, audit!.EntityType);
        Assert.Equal("AIRecommendation.Generate", audit.Action);
    }

    [Fact]
    public async Task GenerateAsync_RejectsArchivedRecommendation()
    {
        var recommendation = CreateRecommendation();
        recommendation.Archive(DateTimeOffset.UtcNow);
        _recommendationRepository.Setup(repository => repository.GetByIdAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendation);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService().GenerateAsync(new GenerateAIRecommendationRequest
            {
                RecommendationId = recommendation.Id,
                GeneratedBy = "planner"
            }));
    }

    [Fact]
    public async Task CompareAsync_ReturnsDifferences()
    {
        var recommendation = CreateRecommendation();
        var ai = new AIRecommendationGenerationService().Generate(
            recommendation,
            1,
            "planner",
            DateTimeOffset.UtcNow);

        _repository.Setup(repository => repository.GetByIdAsync(ai.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ai);
        _recommendationRepository.Setup(repository => repository.GetByIdAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendation);

        var comparison = await CreateService().CompareAsync(ai.Id);

        Assert.Equal(recommendation.Id, comparison.RecommendationId);
        Assert.NotEmpty(comparison.Differences);
    }

    [Fact]
    public async Task ArchiveAsync_ArchivesItem()
    {
        var recommendation = CreateRecommendation();
        var ai = new AIRecommendationGenerationService().Generate(
            recommendation,
            1,
            "planner",
            DateTimeOffset.UtcNow);

        _repository.Setup(repository => repository.GetByIdAsync(ai.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ai);
        _repository.Setup(repository => repository.UpdateAsync(It.IsAny<AIRecommendation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AIRecommendation item, CancellationToken _) => item);

        var result = await CreateService().ArchiveAsync(ai.Id);

        Assert.True(result.Archived);
        Assert.Equal(AIRecommendationStatus.Archived, result.Status);
    }

    [Fact]
    public async Task GenerateAsync_UnexpectedFailure_DoesNotTouchRecommendation_AndWrapsAsBusinessRule()
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
            CreateService().GenerateAsync(new GenerateAIRecommendationRequest
            {
                RecommendationId = recommendation.Id,
                GeneratedBy = "planner"
            }));

        Assert.Contains("was not modified", ex.Message, StringComparison.OrdinalIgnoreCase);
        _recommendationRepository.Verify(
            repository => repository.UpdateAsync(It.IsAny<Recommendation>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _repository.Verify(
            repository => repository.AddAsync(It.IsAny<AIRecommendation>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public void Validators_RejectInvalidRequests()
    {
        var generateValidator = new GenerateAIRecommendationRequestValidator();
        Assert.False(generateValidator.Validate(new GenerateAIRecommendationRequest()).IsValid);

        var queryValidator = new AIRecommendationQueryParametersValidator();
        Assert.False(queryValidator.Validate(new AIRecommendationQueryParameters
        {
            Status = "Nope"
        }).IsValid);
    }

    private static Recommendation CreateRecommendation() =>
        Recommendation.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "REC-AI-SVC",
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
