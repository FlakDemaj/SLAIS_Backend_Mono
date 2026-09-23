using Application.Common.DTOs.Simulations.NightShift;
using Application.Utils.Interfaces.Mediator;

namespace Application.Simulations.NightShift.Commands.StartNightShiftSession;

public class StartNightShiftSessionCommand : IRequest<StartNightShiftSessionResponseDto>
{
    public string? CaseKey { get; init; }

    public string? Language { get; init; }
}
