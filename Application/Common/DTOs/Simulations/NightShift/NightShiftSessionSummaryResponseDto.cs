namespace Application.Common.DTOs.Simulations.NightShift;

public class NightShiftSessionSummaryResponseDto
{
    public Guid SessionId { get; init; }

    public Guid? TemplateId { get; init; }

    public required string CaseKey { get; init; }

    public required string CaseName { get; init; }

    public DateTime StartedAt { get; init; }

    public DateTime? EndedAt { get; init; }

    public DateTime? FinishedAt { get; init; }

    public bool Ended { get; init; }

    public bool HasFeedback { get; init; }

    public int Turns { get; init; }
}
