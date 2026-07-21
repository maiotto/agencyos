namespace AgencyOS.Application.DTOs;

public class UpdateExecutionResourceRequest
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string ResourceType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal CapacityHoursPerWeek { get; set; }

    public decimal? CostRate { get; set; }

    public string? Currency { get; set; }

    public IReadOnlyList<string>? Skills { get; set; }

    public string? Availability { get; set; }

    public string? Notes { get; set; }
}
