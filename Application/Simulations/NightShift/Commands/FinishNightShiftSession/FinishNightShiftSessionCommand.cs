using Application.Common.DTOs.Simulations.NightShift;
using Application.Utils.Interfaces.Mediator;

namespace Application.Simulations.NightShift.Commands.FinishNightShiftSession;

public class FinishNightShiftSessionCommand : IRequest<NightShiftFeedbackResponseDto>
{
    public Guid SessionGuid { get; init; }
}
