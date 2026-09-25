using Application.Common.DTOs.Simulations.NightShift;
using Application.Utils.Interfaces.Mediator;

namespace Application.Simulations.NightShift.Templates.Commands.SetNightShiftTemplateState;

public class SetNightShiftTemplateStateCommand : IRequest<GetNightShiftTemplateResponseDto>
{
    public Guid TemplateGuid { get; init; }
    public int ExpectedVersion { get; init; }
    public required string State { get; init; }
}
