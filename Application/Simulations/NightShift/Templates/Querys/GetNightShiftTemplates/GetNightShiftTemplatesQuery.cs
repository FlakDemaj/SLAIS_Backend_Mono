using Application.Common.DTOs.Simulations.NightShift;
using Application.Utils.Interfaces.Mediator;
using Application.Utils.Interfaces.Transaction;

namespace Application.Simulations.NightShift.Templates.Querys.GetNightShiftTemplates;

public class GetNightShiftTemplatesQuery : IRequest<List<GetNightShiftTemplateResponseDto>>, INoTransaction
{
}
