namespace Application.Common.DTOs.Simulations.NightShift;

public class NightShiftCaseResponseDto
{
    public required string Key { get; init; }

    public required string Name { get; init; }

    public required string Situation { get; init; }

    public required string Lernziel { get; init; }
}
