using Application.Common.Authentication;
using Application.Common.Base;
using Application.Common.DTOs.Simulations.NightShift;
using Application.Common.Interfaces.Repositorys;
using Application.Utils.Logger;
using Application.Utils.Mediator.Interfaces;

using AutoMapper;

using Domain.Common.Enums;
using Domain.Common.Exceptions;

namespace Application.Simulations.NightShift.Templates.Querys.GetNightShiftTemplate;

public class GetNightShiftTemplateQueryHandler :
    BaseHandler<GetNightShiftTemplateQuery>,
    IRequestHandler<GetNightShiftTemplateQuery, GetNightShiftTemplateResponseDto>
{
    private readonly INightShiftTemplateRepository _repository;

    public GetNightShiftTemplateQueryHandler(
        INightShiftTemplateRepository repository,
        IMapper mapper,
        ISlaisLogger<GetNightShiftTemplateQuery> logger)
        : base(mapper, logger)
    {
        _repository = repository;
    }

    public async Task<GetNightShiftTemplateResponseDto> HandleAsync(
        GetNightShiftTemplateQuery request,
        IAuthentication? authentication = null,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin(authentication!);
        var template = await _repository.GetByGuidAsync(request.TemplateGuid)
            ?? throw new SlaisException(NightShiftErrorCodes.TemplateNotFound);
        return NightShiftTemplateResponseFactory.ToResponse(template);
    }

    private static void EnsureAdmin(IAuthentication authentication)
    {
        if (authentication.UserRole != Roles.Admin && authentication.UserRole != Roles.SuperAdmin)
        {
            throw new SlaisException(NightShiftErrorCodes.Forbidden);
        }
    }
}
