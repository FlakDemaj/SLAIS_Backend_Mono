using Application.Common.DTOs.Simulations.NightShift;
using Application.Utils.Interfaces.Mediator;
using Application.Utils.Interfaces.Transaction;

namespace Application.Simulations.NightShift.Commands.SendNightShiftMessage;

// External LLM calls can take up to 60 seconds; unique indexes guard duplicates.
public class SendNightShiftMessageCommand : IRequest<NightShiftMessageResponseDto>, INoTransaction
{
    public Guid SessionGuid { get; init; }

    public required string Text { get; init; }

    public string? Kind { get; init; }
}
