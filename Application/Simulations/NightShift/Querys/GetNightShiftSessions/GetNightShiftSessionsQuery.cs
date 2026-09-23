using Application.Common.DTOs.Simulations.NightShift;
using Application.Utils.Interfaces.Mediator;
using Application.Utils.Interfaces.Transaction;

namespace Application.Simulations.NightShift.Querys.GetNightShiftSessions;

public class GetNightShiftSessionsQuery : IRequest<List<NightShiftSessionSummaryResponseDto>>, INoTransaction
{
}
