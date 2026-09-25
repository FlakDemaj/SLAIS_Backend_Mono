using Application.Common.DTOs.Base;
using Application.Common.DTOs.Simulations.NightShift;
using Application.Utils.Interfaces.Mediator;

namespace Application.Simulations.NightShift.Templates.Commands.CreateNightShiftTemplate;

public class CreateNightShiftTemplateCommand : IRequest<CreateObjectResponseDto>
{
    public required string Key { get; init; }
    public short TensionStart { get; init; }
    public int SortOrder { get; init; }
    public NightShiftTemplateTextDto? German { get; init; }
    public NightShiftTemplateTextDto? English { get; init; }
}
