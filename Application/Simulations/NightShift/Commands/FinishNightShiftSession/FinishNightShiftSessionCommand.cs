using Application.Common.DTOs.Simulations.NightShift;
using Application.Utils.Interfaces.Mediator;
using Application.Utils.Interfaces.Transaction;

namespace Application.Simulations.NightShift.Commands.FinishNightShiftSession;

// External LLM calls can take up to 60 seconds; unique indexes guard duplicates.
public class FinishNightShiftSessionCommand : IRequest<NightShiftFeedbackResponseDto>, INoTransaction
{
    public Guid SessionGuid { get; init; }
}
