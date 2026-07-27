using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Moq;

namespace AgencyOS.Application.Tests;

public class DecisionSummaryServiceTests
{
    private readonly Mock<IDecisionService> _decisionService = new();
    private readonly Mock<IAuditQueryService> _auditQueryService = new();

    private DecisionSummaryService CreateService() => new(_decisionService.Object, _auditQueryService.Object);

    private static DecisionResponse CreateDecision(
        Guid companyId,
        string decisionStatus,
        string implementationStatus = DecisionImplementationStatus.NotStarted,
        string? outcome = null,
        DateTimeOffset? decisionDate = null,
        DateTimeOffset? completedDate = null,
        Guid? id = null,
        IReadOnlyList<DecisionTimelineEntryResponse>? timeline = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            RecommendationId = Guid.NewGuid(),
            CompanyId = companyId,
            MissionId = Guid.NewGuid(),
            ContractId = Guid.NewGuid(),
            DecisionStatus = decisionStatus,
            ImplementationStatus = implementationStatus,
            DecisionDate = decisionDate ?? DateTimeOffset.UtcNow,
            CompletedDate = completedDate,
            Outcome = outcome,
            CreatedBy = "tester",
            Timeline = timeline ?? []
        };

    private static DecisionTimelineEntryResponse CreateTimelineEntry(DateTimeOffset occurredAt, string eventType = "Created") =>
        new()
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            FromDecisionStatus = DecisionStatus.Created,
            ToDecisionStatus = DecisionStatus.Created,
            FromImplementationStatus = DecisionImplementationStatus.NotStarted,
            ToImplementationStatus = DecisionImplementationStatus.NotStarted,
            Actor = "tester",
            OccurredAt = occurredAt
        };

    private void SetupDecisions(IReadOnlyList<DecisionResponse> decisions)
    {
        _decisionService
            .Setup(service => service.GetAllAsync(It.IsAny<DecisionQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(decisions);
    }

    [Fact]
    public async Task GetDecisionsSectionAsync_GroupsByDecisionStatus()
    {
        var companyId = Guid.NewGuid();
        SetupDecisions(
        [
            CreateDecision(companyId, DecisionStatus.Created),
            CreateDecision(companyId, DecisionStatus.InProgress),
            CreateDecision(companyId, DecisionStatus.Completed),
            CreateDecision(companyId, DecisionStatus.Cancelled)
        ]);

        var section = await CreateService().GetDecisionsSectionAsync(companyId, null, null, CancellationToken.None);

        Assert.Equal(4, section.TotalCount);
        Assert.Single(section.Pending);
        Assert.Single(section.InProgress);
        Assert.Single(section.Completed);
        Assert.Single(section.Cancelled);
        Assert.Equal("/decisions/new", section.CreateAction.DrillDownPath);
        Assert.True(section.CreateAction.RequiresHumanApproval);
    }

    [Fact]
    public async Task GetDecisionsSectionAsync_SetsDrillDownPaths()
    {
        var companyId = Guid.NewGuid();
        var decision = CreateDecision(companyId, DecisionStatus.Created);
        SetupDecisions([decision]);

        var section = await CreateService().GetDecisionsSectionAsync(companyId, null, null, CancellationToken.None);

        var card = section.Pending.Single();
        Assert.Equal($"/decisions/{decision.Id}", card.DrillDownPath);
        Assert.Equal($"/recommendations/{decision.RecommendationId}", card.RecommendationDrillDownPath);
    }

    [Fact]
    public async Task GetTimelineSectionAsync_ReturnsFocusedDecisionTimeline_WhenDecisionIdProvided()
    {
        var companyId = Guid.NewGuid();
        var decisionId = Guid.NewGuid();
        var timeline = new List<DecisionTimelineEntryResponse>
        {
            CreateTimelineEntry(DateTimeOffset.UtcNow.AddHours(-2), "Created"),
            CreateTimelineEntry(DateTimeOffset.UtcNow.AddHours(-1), "StartedImplementation")
        };
        var decision = CreateDecision(companyId, DecisionStatus.InProgress, id: decisionId, timeline: timeline);

        _decisionService
            .Setup(service => service.GetByIdAsync(decisionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(decision);

        var section = await CreateService().GetTimelineSectionAsync(companyId, null, null, decisionId, CancellationToken.None);

        Assert.Equal(decisionId, section.DecisionId);
        Assert.Equal(2, section.Items.Count);
        Assert.All(section.Items, item => Assert.Equal(decisionId, item.DecisionId));
        _decisionService.Verify(
            service => service.GetAllAsync(It.IsAny<DecisionQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetTimelineSectionAsync_ThrowsNotFound_WhenDecisionBelongsToDifferentCompany()
    {
        var companyId = Guid.NewGuid();
        var otherCompanyId = Guid.NewGuid();
        var decisionId = Guid.NewGuid();
        var decision = CreateDecision(otherCompanyId, DecisionStatus.InProgress, id: decisionId);

        _decisionService
            .Setup(service => service.GetByIdAsync(decisionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(decision);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().GetTimelineSectionAsync(companyId, null, null, decisionId, CancellationToken.None));
    }

    [Fact]
    public async Task GetTimelineSectionAsync_AggregatesRecentDecisionTimelines_WhenDecisionIdMissing()
    {
        var companyId = Guid.NewGuid();
        var decisionA = CreateDecision(
            companyId,
            DecisionStatus.Completed,
            timeline: [CreateTimelineEntry(DateTimeOffset.UtcNow.AddHours(-3))]);
        var decisionB = CreateDecision(
            companyId,
            DecisionStatus.InProgress,
            timeline: [CreateTimelineEntry(DateTimeOffset.UtcNow.AddHours(-1))]);
        SetupDecisions([decisionA, decisionB]);

        var section = await CreateService().GetTimelineSectionAsync(companyId, null, null, null, CancellationToken.None);

        Assert.Null(section.DecisionId);
        Assert.Equal(2, section.Items.Count);
        Assert.Equal(decisionB.Id, section.Items.First().DecisionId);
        _decisionService.Verify(
            service => service.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetOutcomesSectionAsync_ReturnsOnlyDecisionsWithNonEmptyOutcome()
    {
        var companyId = Guid.NewGuid();
        SetupDecisions(
        [
            CreateDecision(companyId, DecisionStatus.Completed, DecisionImplementationStatus.Completed, "Positive"),
            CreateDecision(companyId, DecisionStatus.Completed, DecisionImplementationStatus.Completed, null),
            CreateDecision(companyId, DecisionStatus.Completed, DecisionImplementationStatus.Completed, "   ")
        ]);

        var section = await CreateService().GetOutcomesSectionAsync(companyId, null, null, CancellationToken.None);

        Assert.Single(section.Outcomes);
        Assert.Equal("Positive", section.Outcomes[0].Outcome);
        Assert.True(section.RecordOutcomeAction.RequiresHumanApproval);
    }

    [Fact]
    public async Task GetAuditSectionAsync_FiltersByDecisionEntityType()
    {
        var companyId = Guid.NewGuid();
        AuditEventQueryParameters? captured = null;
        _auditQueryService
            .Setup(service => service.GetAllAsync(It.IsAny<AuditEventQueryParameters>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEventQueryParameters, CancellationToken>((parameters, _) => captured = parameters)
            .ReturnsAsync(new List<AuditEventResponse>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    EntityType = AuditEntityTypes.Decision,
                    EntityId = Guid.NewGuid(),
                    EventType = "Created",
                    Action = "Create",
                    OccurredAt = DateTimeOffset.UtcNow,
                    UserId = "tester"
                }
            });

        var section = await CreateService().GetAuditSectionAsync(companyId, null, null, CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(AuditEntityTypes.Decision, captured!.EntityType);
        Assert.Equal(companyId, captured.CompanyId);
        Assert.Single(section.Items);
    }
}
