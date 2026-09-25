using Application.Common.DTOs.Simulations.NightShift;
using Application.Utils.Interfaces.Mediator;
using Application.Utils.Interfaces.Transaction;

namespace Application.Simulations.NightShift.Templates.Querys.GetNightShiftTemplate;

public class GetNightShiftTemplateQuery : IRequest<GetNightShiftTemplateResponseDto>, INoTransaction
{
    public Guid TemplateGuid { get; init; }
}
