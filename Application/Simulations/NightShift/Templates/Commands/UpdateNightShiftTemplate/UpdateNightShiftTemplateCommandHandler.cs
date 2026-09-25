using Application.Common.Authentication;
using Application.Common.Base;
using Application.Common.DTOs.Simulations.NightShift;
using Application.Common.Interfaces.Repositorys;
using Application.Utils.Logger;
using Application.Utils.Mediator.Interfaces;

using AutoMapper;

using Domain.Common.Enums;
using Domain.Common.Exceptions;
using Domain.Simulations.NightShift;

namespace Application.Simulations.NightShift.Templates.Commands.UpdateNightShiftTemplate;

public class UpdateNightShiftTemplateCommandHandler :
    BaseHandler<UpdateNightShiftTemplateCommand>,
    IRequestHandler<UpdateNightShiftTemplateCommand, GetNightShiftTemplateResponseDto>
{
    private readonly INightShiftTemplateRepository _repository;

    public UpdateNightShiftTemplateCommandHandler(
        INightShiftTemplateRepository repository,
        IMapper mapper,
        ISlaisLogger<UpdateNightShiftTemplateCommand> logger)
        : base(mapper, logger)
    {
        _repository = repository;
    }

    public async Task<GetNightShiftTemplateResponseDto> HandleAsync(
        UpdateNightShiftTemplateCommand request,
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

        var key = request.Key.Trim().ToLowerInvariant();
        if (key != template.Key)
        {
            if (template.State != States.Pending)
            {
                throw new SlaisException(NightShiftErrorCodes.TemplateStateTransitionInvalid);
            }

            var existing = await _repository.GetByKeyAsync(key);
            if (existing != null && existing.Guid != template.Guid)
            {
                throw new SlaisException(NightShiftErrorCodes.TemplateKeyAlreadyExists);
            }
        }

        template.UpdateHead(
            authentication!.UserGuid,
            key,
            request.TensionStart,
            request.SortOrder);
        ApplyText(
            template,
            authentication.UserGuid,
            Language.German,
            request.German);
        ApplyText(
            template,
            authentication.UserGuid,
            Language.English,
            request.English);
        return NightShiftTemplateResponseFactory.ToResponse(template);
    }

    private static void ApplyText(
        NightShiftTemplateEntity template,
        Guid userGuid,
        Language language,
        NightShiftTemplateTextDto? text)
    {
        if (text == null)
        {
            return;
        }

        var existing = template.GetText(language);
        if (existing == null)
        {
            template.AddOrReplaceText(NightShiftTemplateTextFactory.Create(
                userGuid,
                template.Guid,
                language,
                text));
            return;
        }

        NightShiftTemplateTextFactory.Update(existing, userGuid, text);
    }

    private static void EnsureAdmin(IAuthentication authentication)
    {
        if (authentication.UserRole != Roles.Admin && authentication.UserRole != Roles.SuperAdmin)
        {
            throw new SlaisException(NightShiftErrorCodes.Forbidden);
        }
    }
}
