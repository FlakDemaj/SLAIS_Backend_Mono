using Application.Common.Authentication;
using Application.Common.Base;
using Application.Common.DTOs.Base;
using Application.Common.Interfaces.Repositorys;
using Application.Utils.Logger;
using Application.Utils.Mediator.Interfaces;

using AutoMapper;

using Domain.Common.Enums;
using Domain.Common.Exceptions;
using Domain.Simulations.NightShift;

namespace Application.Simulations.NightShift.Templates.Commands.CreateNightShiftTemplate;

public class CreateNightShiftTemplateCommandHandler :
    BaseHandler<CreateNightShiftTemplateCommand>,
    IRequestHandler<CreateNightShiftTemplateCommand, CreateObjectResponseDto>
{
    private readonly INightShiftTemplateRepository _repository;

    public CreateNightShiftTemplateCommandHandler(
        INightShiftTemplateRepository repository,
        IMapper mapper,
        ISlaisLogger<CreateNightShiftTemplateCommand> logger)
        : base(mapper, logger)
    {
        _repository = repository;
    }

    public async Task<CreateObjectResponseDto> HandleAsync(
        CreateNightShiftTemplateCommand request,
        IAuthentication? authentication = null,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin(authentication!);
        var key = request.Key.Trim().ToLowerInvariant();
        if (await _repository.GetByKeyAsync(key) != null)
        {
            throw new SlaisException(NightShiftErrorCodes.TemplateKeyAlreadyExists);
        }

        var template = NightShiftTemplateEntity.CreateDraft(
            authentication!.UserGuid,
            key,
            request.TensionStart,
            request.SortOrder);
        AddText(
            template,
            authentication.UserGuid,
            Language.German,
            request.German);
        AddText(
            template,
            authentication.UserGuid,
            Language.English,
            request.English);
        await _repository.CreateAsync(template);
        return new CreateObjectResponseDto
        {
            Success = true,
            ObjectGuid = template.Guid
        };
    }

    private static void AddText(
        NightShiftTemplateEntity template,
        Guid userGuid,
        Language language,
        Application.Common.DTOs.Simulations.NightShift.NightShiftTemplateTextDto? text)
    {
        if (text != null)
        {
            template.AddOrReplaceText(NightShiftTemplateTextFactory.Create(
                userGuid,
                template.Guid,
                language,
                text));
        }
    }

    private static void EnsureAdmin(IAuthentication authentication)
    {
        if (authentication.UserRole != Roles.Admin && authentication.UserRole != Roles.SuperAdmin)
        {
            throw new SlaisException(NightShiftErrorCodes.Forbidden);
        }
    }
}
