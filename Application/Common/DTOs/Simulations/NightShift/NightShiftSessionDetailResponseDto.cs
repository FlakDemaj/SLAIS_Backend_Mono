namespace Application.Common.DTOs.Simulations.NightShift;

public class NightShiftSessionDetailResponseDto
{
    public Guid SessionId { get; init; }

    public required string CaseKey { get; init; }

    public required string CaseName { get; init; }

    public DateTime StartedAt { get; init; }

    public DateTime? EndedAt { get; init; }

    public DateTime? FinishedAt { get; init; }

    public bool Ended { get; init; }

    public bool HasFeedback { get; init; }

    public int Turns { get; init; }

    public required List<NightShiftSessionMessageDto> Messages { get; init; }

    public NightShiftFeedbackResponseDto? Feedback { get; init; }
}
