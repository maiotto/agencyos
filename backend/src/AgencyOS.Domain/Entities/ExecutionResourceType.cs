namespace AgencyOS.Domain.Entities;

public static class ExecutionResourceType
{
    public const string InternalHuman = "Internal Human";
    public const string ExternalHuman = "External Human";
    public const string AiAgent = "AI Agent";
    public const string AiService = "AI Service";
    public const string Automation = "Automation";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            InternalHuman,
            ExternalHuman,
            AiAgent,
            AiService,
            Automation
        };
}
