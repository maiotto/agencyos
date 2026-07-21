namespace AgencyOS.Application.DTOs;

public class ExecutionResourceResponse
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string ResourceType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal CapacityHoursPerWeek { get; set; }

    public decimal? CostRate { get; set; }

    public string? Currency { get; set; }

    public IReadOnlyList<string> Skills { get; set; } = [];

    public string? Availability { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
