using Application.Common.DTOs.Simulations.NightShift;
using Application.Utils.Interfaces.Mediator;
using Application.Utils.Interfaces.Transaction;

namespace Application.Simulations.NightShift.Querys.GetNightShiftCases;

public class GetNightShiftCasesQuery : IRequest<List<NightShiftCaseResponseDto>>, INoTransaction
{
    public string? Language { get; init; }
}
