using Application.Common.DTOs.Simulations.NightShift;
using Application.Utils.Interfaces.Mediator;

namespace Application.Simulations.NightShift.Commands.SendNightShiftMessage;

public class SendNightShiftMessageCommand : IRequest<NightShiftMessageResponseDto>
{
    public Guid SessionGuid { get; init; }

    public required string Text { get; init; }

    public string? Kind { get; init; }
}
