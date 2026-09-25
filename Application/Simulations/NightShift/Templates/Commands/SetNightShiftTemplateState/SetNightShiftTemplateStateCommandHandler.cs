using Application.Common.Authentication;
using Application.Common.Base;
using Application.Common.DTOs.Simulations.NightShift;
using Application.Common.Interfaces.Repositorys;
using Application.Utils.Logger;
using Application.Utils.Mediator.Interfaces;

using AutoMapper;

using Domain.Common.Enums;
using Domain.Common.Exceptions;

namespace Application.Simulations.NightShift.Templates.Commands.SetNightShiftTemplateState;

public class SetNightShiftTemplateStateCommandHandler :
    BaseHandler<SetNightShiftTemplateStateCommand>,
    IRequestHandler<SetNightShiftTemplateStateCommand, GetNightShiftTemplateResponseDto>
{
    private readonly INightShiftTemplateRepository _repository;

    public SetNightShiftTemplateStateCommandHandler(
        INightShiftTemplateRepository repository,
        IMapper mapper,
        ISlaisLogger<SetNightShiftTemplateStateCommand> logger)
        : base(mapper, logger)
    {
        _repository = repository;
    }

    public async Task<GetNightShiftTemplateResponseDto> HandleAsync(
        SetNightShiftTemplateStateCommand request,
        IAuthentication? authentication = null,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin(authentication!);
        var template = await _repository.GetByGuidAsync(request.TemplateGuid)
            ?? throw new SlaisException(NightShiftErrorCodes.TemplateNotFound);
        if (template.Version != request.ExpectedVersion)
        {
            throw new SlaisException(NightShiftErrorCodes.TemplateModifiedByOtherUser);
        }

        switch (request.State)
        {
            case "active":
                template.Activate(authentication!.UserGuid);
                break;
            case "archived":
                template.Archive(authentication!.UserGuid);
                break;
            case "pending":
                template.Reopen(authentication!.UserGuid);
                break;
            default:
                throw new SlaisException(NightShiftErrorCodes.TemplateStateTransitionInvalid);
        }

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
