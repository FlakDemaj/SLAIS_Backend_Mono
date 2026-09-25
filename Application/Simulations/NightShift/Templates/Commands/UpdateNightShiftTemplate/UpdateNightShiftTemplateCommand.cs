using Application.Common.DTOs.Simulations.NightShift;
using Application.Utils.Interfaces.Mediator;

namespace Application.Simulations.NightShift.Templates.Commands.UpdateNightShiftTemplate;

public class UpdateNightShiftTemplateCommand : IRequest<GetNightShiftTemplateResponseDto>
{
    public Guid TemplateGuid { get; init; }
    public int ExpectedVersion { get; init; }
    public required string Key { get; init; }
    public short TensionStart { get; init; }
    public int SortOrder { get; init; }
    public NightShiftTemplateTextDto? German { get; init; }
    public NightShiftTemplateTextDto? English { get; init; }
}
