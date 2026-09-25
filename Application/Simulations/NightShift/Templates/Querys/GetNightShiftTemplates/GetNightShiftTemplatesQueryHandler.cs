using Application.Common.Authentication;
using Application.Common.Base;
using Application.Common.DTOs.Simulations.NightShift;
using Application.Common.Interfaces.Repositorys;
using Application.Utils.Logger;
using Application.Utils.Mediator.Interfaces;

using AutoMapper;

using Domain.Common.Enums;
using Domain.Common.Exceptions;

namespace Application.Simulations.NightShift.Templates.Querys.GetNightShiftTemplates;

public class GetNightShiftTemplatesQueryHandler :
    BaseHandler<GetNightShiftTemplatesQuery>,
    IRequestHandler<GetNightShiftTemplatesQuery, List<GetNightShiftTemplateResponseDto>>
{
    private readonly INightShiftTemplateRepository _repository;

    public GetNightShiftTemplatesQueryHandler(
        INightShiftTemplateRepository repository,
        IMapper mapper,
        ISlaisLogger<GetNightShiftTemplatesQuery> logger)
        : base(mapper, logger)
    {
        _repository = repository;
    }

    public async Task<List<GetNightShiftTemplateResponseDto>> HandleAsync(
        GetNightShiftTemplatesQuery request,
        IAuthentication? authentication = null,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin(authentication!);
        return (await _repository.GetAllAsync())
            .Select(NightShiftTemplateResponseFactory.ToResponse)
            .ToList();
    }

    private static void EnsureAdmin(IAuthentication authentication)
    {
        if (authentication.UserRole != Roles.Admin && authentication.UserRole != Roles.SuperAdmin)
        {
            throw new SlaisException(NightShiftErrorCodes.Forbidden);
        }
    }
}
