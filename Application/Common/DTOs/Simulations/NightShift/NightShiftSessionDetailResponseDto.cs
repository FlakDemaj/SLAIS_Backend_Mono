namespace Application.Common.DTOs.Simulations.NightShift;

public class NightShiftSessionDetailResponseDto
{
    public required NightShiftSessionSummaryResponseDto Session { get; init; }

    public required NightShiftCaseResponseDto Case { get; init; }

    public required NightShiftStammblattDto Stammblatt { get; init; }

    public required List<NightShiftSessionMessageDto> Messages { get; init; }

    public NightShiftFeedbackResponseDto? Feedback { get; init; }
}
