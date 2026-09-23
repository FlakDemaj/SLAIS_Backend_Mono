namespace Application.Common.DTOs.Simulations.NightShift;

public class NightShiftSessionMessageDto
{
    public required string Role { get; init; }

    public required string Text { get; init; }

    public DateTime At { get; init; }
}
