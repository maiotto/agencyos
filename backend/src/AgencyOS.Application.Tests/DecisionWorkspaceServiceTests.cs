using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Moq;

namespace AgencyOS.Application.Tests;

public class DecisionWorkspaceServiceTests
{
    private readonly Mock<IDecisionOverviewService> _overviewService = new();
    private readonly Mock<IDecisionNavigationService> _navigationService = new();
    private readonly Mock<IDecisionSummaryService> _summaryService = new();
    private readonly Mock<ICompanyRepository> _companyRepository = new();
    private readonly Mock<ICompanyContext> _companyContext = new();

    public DecisionWorkspaceServiceTests()
    {
        _companyContext.SetupProperty(context => context.CompanyId);
        _companyRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => CreateCompany(id));

        _overviewService
            .Setup(service => service.GetOverviewAsync(
                It.IsAny<Guid>(),
                It.IsAny<DecisionWorkspaceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, DecisionWorkspaceQueryParameters _, CancellationToken __) =>
                new DecisionOverviewResponse { CompanyId = companyId, Kpis = new DecisionKpiSummaryResponse() });

        _navigationService
            .Setup(service => service.GetNavigation(It.IsAny<Guid>()))
            .Returns((Guid companyId) => new DecisionNavigationResponse { CompanyId = companyId, Actions = [] });

        _summaryService
            .Setup(service => service.GetDecisionsSectionAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, DateTimeOffset? _, DateTimeOffset? __, CancellationToken ___) =>
                new DecisionsSectionResponse { CompanyId = companyId });

        _summaryService
            .Setup(service => service.GetTimelineSectionAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, DateTimeOffset? _, DateTimeOffset? __, Guid? decisionId, CancellationToken ___) =>
                new DecisionTimelineSectionResponse { CompanyId = companyId, DecisionId = decisionId });

        _summaryService
            .Setup(service => service.GetOutcomesSectionAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, DateTimeOffset? _, DateTimeOffset? __, CancellationToken ___) =>
                new DecisionOutcomesSectionResponse { CompanyId = companyId });

        _summaryService
            .Setup(service => service.GetAuditSectionAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken _) =>
                new DecisionAuditSectionResponse { CompanyId = companyId, From = from, To = to });
    }

    private DecisionWorkspaceService CreateService() =>
        new(
            _overviewService.Object,
            _navigationService.Object,
            _summaryService.Object,
            _companyRepository.Object,
            _companyContext.Object);

    [Fact]
    public async Task GetWorkspaceAsync_ResolvesCompanyId_FromParametersFirst()
    {
        var explicitCompanyId = Guid.NewGuid();
        _companyContext.Object.CompanyId = Guid.NewGuid();

        var workspace = await CreateService().GetWorkspaceAsync(
            new DecisionWorkspaceQueryParameters { CompanyId = explicitCompanyId });

        Assert.Equal(explicitCompanyId, workspace.CompanyId);
        Assert.True(workspace.Overview.RequiresHumanApproval);
    }

    [Fact]
    public async Task GetWorkspaceAsync_ResolvesCompanyId_FromCompanyContext_WhenParametersUnset()
    {
        var contextCompanyId = Guid.NewGuid();
        _companyContext.Object.CompanyId = contextCompanyId;

        var workspace = await CreateService().GetWorkspaceAsync(new DecisionWorkspaceQueryParameters());

        Assert.Equal(contextCompanyId, workspace.CompanyId);
    }

    [Fact]
    public async Task GetWorkspaceAsync_FallsBackToDefaultCompany_WhenUnset()
    {
        _companyContext.Object.CompanyId = null;

        var workspace = await CreateService().GetWorkspaceAsync(new DecisionWorkspaceQueryParameters());

        Assert.Equal(AgencyOSCompanies.DefaultCompanyId, workspace.CompanyId);
    }

    [Fact]
    public async Task GetWorkspaceAsync_ThrowsNotFound_WhenCompanyMissing()
    {
        _companyRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().GetWorkspaceAsync(new DecisionWorkspaceQueryParameters { CompanyId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task GetWorkspaceAsync_DefaultsToTrailingThirtyDayWindow()
    {
        var workspace = await CreateService().GetWorkspaceAsync(new DecisionWorkspaceQueryParameters());

        Assert.NotNull(workspace.From);
        Assert.NotNull(workspace.To);
        Assert.True((workspace.To!.Value - workspace.From!.Value).TotalDays is > 29 and < 31);
    }

    [Fact]
    public async Task GetWorkspaceAsync_PassesThroughDecisionId_Unmodified()
    {
        var decisionId = Guid.NewGuid();

        var workspace = await CreateService().GetWorkspaceAsync(
            new DecisionWorkspaceQueryParameters { DecisionId = decisionId });

        Assert.Equal(decisionId, workspace.DecisionId);
        Assert.Equal(decisionId, workspace.Timeline.DecisionId);
    }

    [Fact]
    public async Task GetDecisionsAsync_NeverCallsSummaryWriteMethods()
    {
        await CreateService().GetDecisionsAsync(new DecisionWorkspaceQueryParameters());

        _summaryService.Verify(
            service => service.GetDecisionsSectionAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _summaryService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetKpisAsync_DelegatesToOverviewService_AndReturnsKpisOnly()
    {
        var companyId = Guid.NewGuid();
        _overviewService
            .Setup(service => service.GetOverviewAsync(
                companyId,
                It.IsAny<DecisionWorkspaceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DecisionOverviewResponse
            {
                CompanyId = companyId,
                Kpis = new DecisionKpiSummaryResponse { TotalCount = 7 }
            });

        var kpis = await CreateService().GetKpisAsync(
            new DecisionWorkspaceQueryParameters { CompanyId = companyId });

        Assert.Equal(7, kpis.TotalCount);
    }

    private static Company CreateCompany(Guid? id = null) =>
        Company.Create(
            id ?? AgencyOSCompanies.DefaultCompanyId,
            "AOS",
            "AgencyOS Default",
            null,
            "UTC",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            DateTimeOffset.UtcNow);
}

/// <summary>
/// End-to-end verification that the Decision Workspace never calls a write method on
/// <see cref="IDecisionService"/> — wires the real <see cref="DecisionOverviewService"/> and
/// <see cref="DecisionSummaryService"/> against a mocked <see cref="IDecisionService"/>
/// (DEC-504-001).
/// </summary>
public class DecisionWorkspaceServiceReadOnlyTests
{
    [Fact]
    public async Task GetWorkspaceAsync_NeverCallsDecisionWriteMethods_EndToEnd()
    {
        var companyId = Guid.NewGuid();
        var decisionService = new Mock<IDecisionService>();
        decisionService
            .Setup(service => service.GetAllAsync(It.IsAny<DecisionQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DecisionResponse>());

        var auditQueryService = new Mock<IAuditQueryService>();
        auditQueryService
            .Setup(service => service.GetAllAsync(It.IsAny<AuditEventQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuditEventResponse>());

        var overviewService = new DecisionOverviewService(decisionService.Object);
        var summaryService = new DecisionSummaryService(decisionService.Object, auditQueryService.Object);
        var navigationService = new DecisionNavigationService();

        var companyRepository = new Mock<ICompanyRepository>();
        companyRepository
            .Setup(repository => repository.GetByIdAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Company.Create(
                companyId,
                "AOS",
                "AgencyOS Default",
                null,
                "UTC",
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                DateTimeOffset.UtcNow));

        var companyContext = new Mock<ICompanyContext>();

        var workspaceService = new DecisionWorkspaceService(
            overviewService,
            navigationService,
            summaryService,
            companyRepository.Object,
            companyContext.Object);

        await workspaceService.GetWorkspaceAsync(new DecisionWorkspaceQueryParameters { CompanyId = companyId });

        decisionService.Verify(
            service => service.GetAllAsync(It.IsAny<DecisionQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
        decisionService.VerifyNoOtherCalls();
    }
}
