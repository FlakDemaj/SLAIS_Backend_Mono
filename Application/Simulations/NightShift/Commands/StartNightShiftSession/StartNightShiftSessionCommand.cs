using Application.Common.DTOs.Simulations.NightShift;
using Application.Utils.Interfaces.Mediator;
using Application.Utils.Interfaces.Transaction;

namespace Application.Simulations.NightShift.Commands.StartNightShiftSession;

// External LLM calls can take up to 60 seconds; unique indexes guard duplicates.
public class StartNightShiftSessionCommand : IRequest<StartNightShiftSessionResponseDto>, INoTransaction
{
    public Guid? TemplateId { get; init; }

    public string? CaseKey { get; init; }

    public string? Language { get; init; }
}
