using Application.Common.DTOs.Base;
using Application.Utils.Interfaces.Mediator;

namespace Application.Simulations.NightShift.Templates.Commands.DeleteNightShiftTemplate;

public class DeleteNightShiftTemplateCommand : IRequest<CreateObjectResponseDto>
{
    public Guid TemplateGuid { get; init; }
}
