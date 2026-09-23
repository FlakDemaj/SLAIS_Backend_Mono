namespace Application.Common.DTOs.Simulations.NightShift;

public class NightShiftMessageResponseDto
{
    public required string Response { get; init; }

    public bool Ended { get; init; }
}
