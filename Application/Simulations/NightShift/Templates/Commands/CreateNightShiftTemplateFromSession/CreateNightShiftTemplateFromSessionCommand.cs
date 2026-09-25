using Application.Common.DTOs.Base;
using Application.Utils.Interfaces.Mediator;

namespace Application.Simulations.NightShift.Templates.Commands.CreateNightShiftTemplateFromSession;

public class CreateNightShiftTemplateFromSessionCommand : IRequest<CreateObjectResponseDto>
{
    public Guid SessionGuid { get; init; }
    public required string Key { get; init; }
}
