using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using Moq;

namespace AgencyOS.Application.Tests;

public class CrossPortfolioConflictDetectionServiceTests
{
    private readonly Mock<IAllocationConflictDetectionService> _allocationConflictDetectionService = new();

    public CrossPortfolioConflictDetectionServiceTests()
    {
        _allocationConflictDetectionService
            .Setup(service => service.GetSummaryAsync(It.IsAny<AllocationConflictQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AllocationConflictSummaryResponse());
        _allocationConflictDetectionService
            .Setup(service => service.GetAllAsync(It.IsAny<AllocationConflictQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AllocationConflictResponse>());
    }

    private CrossPortfolioConflictDetectionService CreateService() =>
        new(_allocationConflictDetectionService.Object);

    [Fact]
    public async Task DetectAsync_DetectsMissionOverlap_AcrossPortfolios()
    {
        var sharedMissionId = Guid.NewGuid();
        var portfolioOne = CreatePortfolio([(sharedMissionId, 1)]);
        var portfolioTwo = CreatePortfolio([(sharedMissionId, 1)]);

        var service = CreateService();

        var result = await service.DetectAsync(
            [portfolioOne, portfolioTwo],
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime),
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddDays(30).UtcDateTime));

        Assert.Equal(1, result.PortfolioConflictCount);
        var conflict = Assert.Single(result.PortfolioConflicts);
        Assert.Equal(sharedMissionId, conflict.MissionId);
        Assert.Contains(portfolioOne.Id, conflict.PortfolioIds);
        Assert.Contains(portfolioTwo.Id, conflict.PortfolioIds);
    }

    [Fact]
    public async Task DetectAsync_ReturnsNoPortfolioConflicts_WhenMissionsDoNotOverlap()
    {
        var portfolioOne = CreatePortfolio([(Guid.NewGuid(), 1)]);
        var portfolioTwo = CreatePortfolio([(Guid.NewGuid(), 1)]);

        var service = CreateService();

        var result = await service.DetectAsync(
            [portfolioOne, portfolioTwo],
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime),
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddDays(30).UtcDateTime));

        Assert.Equal(0, result.PortfolioConflictCount);
        Assert.Empty(result.PortfolioConflicts);
    }

    [Fact]
    public async Task DetectAsync_DelegatesResourceConflicts_ToAllocationConflictDetectionService()
    {
        var resourceConflict = new AllocationConflictResponse
        {
            ConflictId = Guid.NewGuid(),
            ConflictType = "Capacity Exceeded",
            ExecutionResourceId = Guid.NewGuid(),
            ExecutionResourceCode = "ER-001",
            ExecutionResourceName = "Alice",
            Severity = AllocationConflictCalculation.Severity.Critical,
            Description = "Overallocated",
            SuggestedResolution = "Reduce hours"
        };

        _allocationConflictDetectionService
            .Setup(service => service.GetSummaryAsync(It.IsAny<AllocationConflictQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AllocationConflictSummaryResponse { TotalConflictCount = 1, CriticalConflictCount = 1 });
        _allocationConflictDetectionService
            .Setup(service => service.GetAllAsync(It.IsAny<AllocationConflictQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([resourceConflict]);

        var portfolio = CreatePortfolio([(Guid.NewGuid(), 1)]);
        var service = CreateService();

        var result = await service.DetectAsync(
            [portfolio],
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime),
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddDays(30).UtcDateTime));

        Assert.Equal(1, result.ResourceConflictCount);
        var item = Assert.Single(result.ResourceConflicts);
        Assert.Equal(resourceConflict.ConflictId, item.ConflictId);
        Assert.Equal("Critical", result.Severity);
    }

    [Fact]
    public async Task DetectAsync_ReturnsNoneSeverity_WhenNoConflictsFound()
    {
        var portfolio = CreatePortfolio([(Guid.NewGuid(), 1)]);
        var service = CreateService();

        var result = await service.DetectAsync(
            [portfolio],
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime),
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddDays(30).UtcDateTime));

        Assert.Equal("None", result.Severity);
        Assert.Equal(0, result.PortfolioConflictCount);
        Assert.Equal(0, result.ResourceConflictCount);
    }

    [Fact]
    public async Task DetectAsync_ElevatesSeverity_WhenPortfolioConflictExistsWithoutResourceConflicts()
    {
        var sharedMissionId = Guid.NewGuid();
        var portfolioOne = CreatePortfolio([(sharedMissionId, 1)]);
        var portfolioTwo = CreatePortfolio([(sharedMissionId, 1)]);
        var service = CreateService();

        var result = await service.DetectAsync(
            [portfolioOne, portfolioTwo],
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime),
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddDays(30).UtcDateTime));

        Assert.Equal("Medium", result.Severity);
    }

    private static Portfolio CreatePortfolio(IReadOnlyList<(Guid MissionId, int Priority)> missions) =>
        Portfolio.Create(
            AgencyOSCompanies.DefaultCompanyId,
            $"Portfolio {Guid.NewGuid():N}",
            null,
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime),
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddDays(30).UtcDateTime),
            null,
            missions,
            DateTimeOffset.UtcNow);
}
