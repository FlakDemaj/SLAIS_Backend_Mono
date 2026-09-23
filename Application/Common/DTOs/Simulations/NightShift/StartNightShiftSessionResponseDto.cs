namespace Application.Common.DTOs.Simulations.NightShift;

public class StartNightShiftSessionResponseDto
{
    public Guid SessionId { get; init; }

    public required string Temperament { get; init; }

    public required string FirstMessage { get; init; }

    public bool Ended { get; init; }

    public required string Language { get; init; }

    public required NightShiftCaseResponseDto Case { get; init; }

    public required NightShiftStammblattDto Stammblatt { get; init; }
}
