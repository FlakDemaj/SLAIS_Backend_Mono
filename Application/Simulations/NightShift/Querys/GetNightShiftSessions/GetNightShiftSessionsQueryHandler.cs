using Application.Common.Authentication;
using Application.Common.Base;
using Application.Common.DTOs.Simulations.NightShift;
using Application.Common.Interfaces.Repositorys;
using Application.Utils.Logger;
using Application.Utils.Mediator.Interfaces;

using AutoMapper;

using Domain.Common.Enums;
using Domain.Common.Exceptions;

namespace Application.Simulations.NightShift.Querys.GetNightShiftSessions;

public class GetNightShiftSessionsQueryHandler : BaseHandler<GetNightShiftSessionsQuery>,
    IRequestHandler<GetNightShiftSessionsQuery, List<NightShiftSessionSummaryResponseDto>>
{
    private readonly INightShiftSessionRepository _nightShiftSessionRepository;

    public GetNightShiftSessionsQueryHandler(
        INightShiftSessionRepository nightShiftSessionRepository,
        IMapper mapper,
        ISlaisLogger<GetNightShiftSessionsQuery> logger)
        : base(mapper, logger)
    {
        _nightShiftSessionRepository = nightShiftSessionRepository;
    }

    public async Task<List<NightShiftSessionSummaryResponseDto>> HandleAsync(
        GetNightShiftSessionsQuery request,
        IAuthentication? authentication = null,
        CancellationToken cancellationToken = default)
    {
        if (authentication!.UserRole == Roles.Server)
        {
            throw new SlaisException(NightShiftErrorCodes.Forbidden);
        }

        var sessions = await _nightShiftSessionRepository.GetByUserGuidAsync(authentication.UserGuid);
        return _mapper.Map<List<NightShiftSessionSummaryResponseDto>>(sessions);
    }
}
