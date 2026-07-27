using AgencyOS.Application.Services;

namespace AgencyOS.Application.Tests;

public class DashboardTrendServiceTests
{
    private readonly DashboardTrendService _service = new();

    [Fact]
    public void Calculate_Int_ReturnsUp_WhenCurrentHigher()
    {
        var trend = _service.Calculate(12, 10);

        Assert.Equal("Up", trend.Direction);
        Assert.Equal(20m, trend.DeltaPercent);
        Assert.Equal(12, trend.CurrentValue);
        Assert.Equal(10, trend.PreviousValue);
    }

    [Fact]
    public void Calculate_Int_ReturnsDown_WhenCurrentLower()
    {
        var trend = _service.Calculate(8, 10);

        Assert.Equal("Down", trend.Direction);
        Assert.Equal(-20m, trend.DeltaPercent);
    }

    [Fact]
    public void Calculate_Int_ReturnsFlat_WhenValuesEqual()
    {
        var trend = _service.Calculate(10, 10);

        Assert.Equal("Flat", trend.Direction);
        Assert.Equal(0m, trend.DeltaPercent);
    }

    [Fact]
    public void Calculate_Int_ReturnsFlat_WhenBothZero()
    {
        var trend = _service.Calculate(0, 0);

        Assert.Equal("Flat", trend.Direction);
        Assert.Equal(0m, trend.DeltaPercent);
    }

    [Fact]
    public void Calculate_Int_ReturnsUp_WhenPreviousZeroAndCurrentPositive()
    {
        var trend = _service.Calculate(5, 0);

        Assert.Equal("Up", trend.Direction);
        Assert.Equal(100m, trend.DeltaPercent);
    }

    [Fact]
    public void Calculate_Decimal_ReturnsUp_WhenCurrentHigher()
    {
        var trend = _service.Calculate(75.5m, 60m);

        Assert.Equal("Up", trend.Direction);
        Assert.True(trend.DeltaPercent > 0);
    }

    [Fact]
    public void Calculate_Decimal_ReturnsFlat_WithinTolerance()
    {
        var trend = _service.Calculate(100.001m, 100m);

        Assert.Equal("Flat", trend.Direction);
    }

    [Fact]
    public void Calculate_Decimal_ReturnsDown_WhenCurrentLower()
    {
        var trend = _service.Calculate(40m, 50m);

        Assert.Equal("Down", trend.Direction);
        Assert.Equal(-20m, trend.DeltaPercent);
    }
}
