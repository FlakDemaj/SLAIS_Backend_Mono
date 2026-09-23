using Application.Common.Authentication;
using Application.Common.Base;
using Application.Common.DTOs.Simulations.NightShift;
using Application.Common.Interfaces.Repositorys;
using Application.Utils.Logger;
using Application.Utils.Mediator.Interfaces;

using AutoMapper;

using Domain.Common.Enums;
using Domain.Common.Exceptions;

namespace Application.Simulations.NightShift.Querys.GetNightShiftSession;

public class GetNightShiftSessionQueryHandler : BaseHandler<GetNightShiftSessionQuery>, IRequestHandler<GetNightShiftSessionQuery, NightShiftSessionDetailResponseDto>
{
    private readonly INightShiftSessionRepository _nightShiftSessionRepository;

    public GetNightShiftSessionQueryHandler(INightShiftSessionRepository nightShiftSessionRepository, IMapper mapper, ISlaisLogger<GetNightShiftSessionQuery> logger)
        : base(mapper, logger)
    {
        _nightShiftSessionRepository = nightShiftSessionRepository;
    }

    public async Task<NightShiftSessionDetailResponseDto> HandleAsync(GetNightShiftSessionQuery request, IAuthentication? authentication = null, CancellationToken cancellationToken = default)
    {
        if (authentication!.UserRole == Roles.Server)
        {
            throw new SlaisException(NightShiftErrorCodes.Forbidden);
        }

        var session = await _nightShiftSessionRepository.GetByGuidAsync(request.SessionGuid) ?? throw new SlaisException(NightShiftErrorCodes.SessionNotFound);
        if (session.UserGuid != authentication.UserGuid)
        {
            throw new SlaisException(NightShiftErrorCodes.Forbidden);
        }

        var summary = _mapper.Map<NightShiftSessionSummaryResponseDto>(session);
        return new NightShiftSessionDetailResponseDto { SessionId = summary.SessionId, CaseKey = summary.CaseKey, CaseName = summary.CaseName, StartedAt = summary.StartedAt, EndedAt = summary.EndedAt, FinishedAt = summary.FinishedAt, Ended = summary.Ended, HasFeedback = summary.HasFeedback, Turns = summary.Turns, Messages = _mapper.Map<List<NightShiftSessionMessageDto>>(session.Messages.Where(message => message.Role != NightShiftMessageRole.System).ToList()), Feedback = session.Feedback == null ? null : ToFeedback(session.Feedback, session.Language) };
    }

    private NightShiftFeedbackResponseDto ToFeedback(Domain.Simulations.NightShift.NightShiftFeedbackEntity feedback, Language language)
    {
        var mapped = _mapper.Map<NightShiftFeedbackResponseDto>(feedback);
        return new NightShiftFeedbackResponseDto { Feedback = mapped.Summary, Ok = mapped.Ok, Scores = mapped.Scores, Labels = NightShiftLanguage.Labels(language), PerDimension = mapped.PerDimension, Summary = mapped.Summary };
    }
}
