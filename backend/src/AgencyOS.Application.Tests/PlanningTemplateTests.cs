using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class PlanningTemplateTests
{
    private static readonly Guid CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

    [Fact]
    public void Create_StartsInactive_WithConfigurationReferences()
    {
        var template = CreateValid();

        Assert.Equal(PlanningTemplateStatus.Inactive, template.Status);
        Assert.Equal("Standard Weekly", template.Name);
        Assert.Equal(ResourceAvailabilityStrategies.RequireActiveConfiguration, template.ResourceAvailabilityStrategy);
        Assert.Equal(7, template.DefaultPlanningWindowDays);
    }

    [Fact]
    public void Create_RejectsBlankName()
    {
        Assert.Throws<InvalidOperationException>(() => CreateValid(name: " "));
    }

    [Fact]
    public void Activate_And_Deactivate_AreIdempotent()
    {
        var template = CreateValid();
        template.Activate(DateTimeOffset.UtcNow);
        template.Activate(DateTimeOffset.UtcNow);
        Assert.True(template.IsActive);

        template.Deactivate(DateTimeOffset.UtcNow);
        template.Deactivate(DateTimeOffset.UtcNow);
        Assert.True(template.IsInactive);
    }

    [Fact]
    public void EnsureCanDelete_RejectsActiveTemplate()
    {
        var template = CreateValid();
        template.Activate(DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() => template.EnsureCanDelete());
    }

    [Fact]
    public void EnsureCanApply_RejectsInactiveTemplate()
    {
        var template = CreateValid();
        Assert.Throws<InvalidOperationException>(() => template.EnsureCanApply());
    }

    [Fact]
    public void Clone_CopiesConfigurationOnly_StartsInactive()
    {
        var source = CreateValid();
        source.Activate(DateTimeOffset.UtcNow);

        var clone = source.Clone("Standard Weekly Copy", DateTimeOffset.UtcNow);

        Assert.NotEqual(source.Id, clone.Id);
        Assert.Equal(PlanningTemplateStatus.Inactive, clone.Status);
        Assert.Equal(source.WorkingCalendarId, clone.WorkingCalendarId);
        Assert.Equal(source.WorkingHoursId, clone.WorkingHoursId);
        Assert.Equal(source.DefaultPlanningWindowDays, clone.DefaultPlanningWindowDays);
        Assert.Equal("Standard Weekly Copy", clone.Name);
    }

    [Fact]
    public void ResolveDefaultPlanningWindow_UsesOffsetAndDays()
    {
        var template = CreateValid(windowDays: 5, offsetDays: 2);
        var (start, end) = template.ResolveDefaultPlanningWindow(new DateOnly(2026, 7, 1));

        Assert.Equal(new DateOnly(2026, 7, 3), start);
        Assert.Equal(new DateOnly(2026, 7, 7), end);
    }

    private static PlanningTemplate CreateValid(
        string name = "Standard Weekly",
        int windowDays = 7,
        int offsetDays = 0)
    {
        return PlanningTemplate.Create(
            CompanyId,
            name,
            "Default planning template",
            Guid.NewGuid(),
            Guid.NewGuid(),
            ResourceAvailabilityStrategies.RequireActiveConfiguration,
            windowDays,
            offsetDays,
            new PlanningTemplateCapacityRules(85m, true),
            null,
            DateTimeOffset.UtcNow);
    }
}
