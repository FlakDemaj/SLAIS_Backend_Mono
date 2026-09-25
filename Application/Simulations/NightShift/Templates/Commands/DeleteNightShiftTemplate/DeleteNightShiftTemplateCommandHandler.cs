using Application.Common.Authentication;
using Application.Common.Base;
using Application.Common.DTOs.Base;
using Application.Common.Interfaces.Repositorys;
using Application.Utils.Logger;
using Application.Utils.Mediator.Interfaces;

using AutoMapper;

using Domain.Common.Enums;
using Domain.Common.Exceptions;

namespace Application.Simulations.NightShift.Templates.Commands.DeleteNightShiftTemplate;

public class DeleteNightShiftTemplateCommandHandler :
    BaseHandler<DeleteNightShiftTemplateCommand>,
    IRequestHandler<DeleteNightShiftTemplateCommand, CreateObjectResponseDto>
{
    private readonly INightShiftTemplateRepository _repository;

    public DeleteNightShiftTemplateCommandHandler(
        INightShiftTemplateRepository repository,
        IMapper mapper,
        ISlaisLogger<DeleteNightShiftTemplateCommand> logger)
        : base(mapper, logger)
    {
        _repository = repository;
    }

    public async Task<CreateObjectResponseDto> HandleAsync(
        DeleteNightShiftTemplateCommand request,
        IAuthentication? authentication = null,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin(authentication!);
        var template = await _repository.GetByGuidAsync(request.TemplateGuid)
            ?? throw new SlaisException(NightShiftErrorCodes.TemplateNotFound);
        template.MarkDeleted(authentication!.UserGuid);
        return new CreateObjectResponseDto
        {
            Success = true,
            ObjectGuid = template.Guid
        };
    }

    private static void EnsureAdmin(IAuthentication authentication)
    {
        if (authentication.UserRole != Roles.Admin && authentication.UserRole != Roles.SuperAdmin)
        {
            throw new SlaisException(NightShiftErrorCodes.Forbidden);
        }
    }
}
