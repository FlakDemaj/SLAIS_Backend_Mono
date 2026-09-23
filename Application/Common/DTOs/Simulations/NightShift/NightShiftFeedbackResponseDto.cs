namespace Application.Common.DTOs.Simulations.NightShift;

public class NightShiftFeedbackResponseDto
{
    public required string Feedback { get; init; }

    public bool Ok { get; init; }

    public required NightShiftDimensionScoresDto Scores { get; init; }

    public required NightShiftDimensionTextsDto Labels { get; init; }

    public required NightShiftDimensionTextsDto PerDimension { get; init; }

    public required string Summary { get; init; }
}
