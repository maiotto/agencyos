namespace AgencyOS.Application.DTOs;

public class AllocationConflictSummaryResponse
{
    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    public int ActiveResourceCount { get; set; }

    public int TotalConflictCount { get; set; }

    public int CriticalConflictCount { get; set; }

    public int HighConflictCount { get; set; }

    public int MediumConflictCount { get; set; }

    public int LowConflictCount { get; set; }

    public int ResourcesWithConflicts { get; set; }
}
