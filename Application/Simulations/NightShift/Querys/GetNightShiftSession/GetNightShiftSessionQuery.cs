using Application.Common.DTOs.Simulations.NightShift;
using Application.Utils.Interfaces.Mediator;
using Application.Utils.Interfaces.Transaction;

namespace Application.Simulations.NightShift.Querys.GetNightShiftSession;

public class GetNightShiftSessionQuery : IRequest<NightShiftSessionDetailResponseDto>, INoTransaction
{
    public Guid SessionGuid { get; init; }
}
