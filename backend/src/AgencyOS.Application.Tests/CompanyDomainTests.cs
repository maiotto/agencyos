using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class CompanyDomainTests
{
    [Fact]
    public void Create_BuildsActiveCompany()
    {
        var company = CreateValidCompany();

        Assert.NotEqual(Guid.Empty, company.Id);
        Assert.Equal(CompanyStatus.Active, company.Status);
        Assert.True(company.IsActive);
        Assert.False(company.Archived);
        Assert.Equal("{}", company.PlanningConfiguration);
    }

    [Fact]
    public void Create_GeneratesNewIdWhenEmpty()
    {
        var company = Company.Create(
            Guid.Empty,
            "CODE",
            "Name",
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

        Assert.NotEqual(Guid.Empty, company.Id);
    }

    [Theory]
    [InlineData("", "Name")]
    [InlineData("Code", "")]
    public void Create_ThrowsWhenIdentityFieldsMissing(string code, string name)
    {
        Assert.Throws<InvalidOperationException>(() => Company.Create(
            Guid.NewGuid(),
            code,
            name,
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
    }

    [Fact]
    public void Create_ThrowsWhenTimezoneMissing()
    {
        Assert.Throws<InvalidOperationException>(() => Company.Create(
            Guid.NewGuid(),
            "CODE",
            "Name",
            null,
            "  ",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Update_ChangesEditableFields()
    {
        var company = CreateValidCompany();
        var decisionProfileId = Guid.NewGuid();
        var planningTemplateId = Guid.NewGuid();

        company.Update(
            "Updated Name",
            "Updated Legal",
            "America/Sao_Paulo",
            "BR",
            "pt-BR",
            "BRL",
            "{\"a\":1}",
            decisionProfileId,
            null,
            planningTemplateId,
            DateTimeOffset.UtcNow);

        Assert.Equal("Updated Name", company.CompanyName);
        Assert.Equal("Updated Legal", company.LegalName);
        Assert.Equal("America/Sao_Paulo", company.Timezone);
        Assert.Equal(decisionProfileId, company.DecisionProfileId);
        Assert.Equal(planningTemplateId, company.DefaultPlanningTemplateId);
    }

    [Fact]
    public void Update_ThrowsWhenArchived()
    {
        var company = CreateValidCompany();
        company.Archive(DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() => company.Update(
            "Name",
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
    }

    [Fact]
    public void ActivateDeactivate_TogglesStatus()
    {
        var company = CreateValidCompany();

        company.Deactivate(DateTimeOffset.UtcNow);
        Assert.True(company.IsInactive);

        company.Activate(DateTimeOffset.UtcNow);
        Assert.True(company.IsActive);
    }

    [Fact]
    public void Archive_SetsArchivedStatusAndTimestamp()
    {
        var company = CreateValidCompany();
        var archivedAt = DateTimeOffset.UtcNow;

        company.Archive(archivedAt);

        Assert.True(company.Archived);
        Assert.Equal(archivedAt, company.ArchivedAt);
    }

    [Fact]
    public void Archive_ThrowsWhenAlreadyArchived()
    {
        var company = CreateValidCompany();
        company.Archive(DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() => company.Archive(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Select_SucceedsWhenActive_BR2003()
    {
        var company = CreateValidCompany();

        var exception = Record.Exception(company.Select);

        Assert.Null(exception);
    }

    [Fact]
    public void Select_ThrowsWhenNotActive_BR2003()
    {
        var company = CreateValidCompany();
        company.Deactivate(DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(company.Select);
    }

    [Fact]
    public void AssignDecisionProfile_SetsIdentifier_BR2007()
    {
        var company = CreateValidCompany();
        var profileId = Guid.NewGuid();

        company.AssignDecisionProfile(profileId, DateTimeOffset.UtcNow);

        Assert.Equal(profileId, company.DecisionProfileId);
    }

    [Fact]
    public void AssignDecisionProfile_ThrowsWhenEmpty_BR2007()
    {
        var company = CreateValidCompany();

        Assert.Throws<InvalidOperationException>(() =>
            company.AssignDecisionProfile(Guid.Empty, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void AssignDefaultPlanningTemplate_AllowsNull_BR2008()
    {
        var company = CreateValidCompany();
        company.AssignDefaultPlanningTemplate(Guid.NewGuid(), DateTimeOffset.UtcNow);

        company.AssignDefaultPlanningTemplate(null, DateTimeOffset.UtcNow);

        Assert.Null(company.DefaultPlanningTemplateId);
    }

    [Fact]
    public void Validate_ReturnsTrueForWellFormedCompany()
    {
        var company = CreateValidCompany();

        Assert.True(company.Validate());
    }

    private static Company CreateValidCompany() =>
        Company.Create(
            Guid.NewGuid(),
            "CODE" + Guid.NewGuid().ToString("N")[..6],
            "Test Company",
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
