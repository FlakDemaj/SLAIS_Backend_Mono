using Application.Common.Authentication;
using Application.Common.Base;
using Application.Common.DTOs.Base;
using Application.Common.DTOs.Simulations.NightShift;
using Application.Common.Interfaces.Repositorys;
using Application.Utils.Logger;
using Application.Utils.Mediator.Interfaces;

using AutoMapper;

using Domain.Common.Enums;
using Domain.Common.Exceptions;
using Domain.Simulations.NightShift;

namespace Application.Simulations.NightShift.Templates.Commands.CreateNightShiftTemplateFromSession;

public class CreateNightShiftTemplateFromSessionCommandHandler :
    BaseHandler<CreateNightShiftTemplateFromSessionCommand>,
    IRequestHandler<CreateNightShiftTemplateFromSessionCommand, CreateObjectResponseDto>
{
    private readonly INightShiftTemplateRepository _templateRepository;
    private readonly INightShiftSessionRepository _sessionRepository;

    public CreateNightShiftTemplateFromSessionCommandHandler(
        INightShiftTemplateRepository templateRepository,
        INightShiftSessionRepository sessionRepository,
        IMapper mapper,
        ISlaisLogger<CreateNightShiftTemplateFromSessionCommand> logger)
        : base(mapper, logger)
    {
        _templateRepository = templateRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task<CreateObjectResponseDto> HandleAsync(
        CreateNightShiftTemplateFromSessionCommand request,
        IAuthentication? authentication = null,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin(authentication!);
        var session = await _sessionRepository.GetByGuidAsync(request.SessionGuid)
            ?? throw new SlaisException(NightShiftErrorCodes.SessionNotFound);
        if (session.TemplateGuid != null || session.CaseKey != "random")
        {
            throw new SlaisException(NightShiftErrorCodes.SessionIsNotRandomCase);
        }

        var key = request.Key.Trim().ToLowerInvariant();
        if (await _templateRepository.GetByKeyAsync(key) != null)
        {
            throw new SlaisException(NightShiftErrorCodes.TemplateKeyAlreadyExists);
        }

        var template = NightShiftTemplateEntity.CreateDraft(
            authentication!.UserGuid,
            key,
            session.CaseTensionStart,
            100);
        var text = new NightShiftTemplateTextDto
        {
            Name = session.CaseName,
            Situation = session.CaseSituation,
            Emotion = session.CaseEmotion,
            LearningGoal = session.CaseLearningGoal,
            Opener = session.CaseOpener,
            Born = session.RecordBorn,
            Gender = session.RecordGender,
            Admission = session.RecordAdmission,
            Diagnoses = session.RecordDiagnoses,
            Allergies = session.RecordAllergies,
            Medication = session.RecordMedication,
            CareLevel = session.RecordCareLevel,
            Risks = session.RecordRisks,
            Resuscitation = session.RecordResuscitation,
            Relatives = session.RecordRelatives
        };
        template.AddOrReplaceText(NightShiftTemplateTextFactory.Create(
            authentication.UserGuid,
            template.Guid,
            session.Language,
            text));
        await _templateRepository.CreateAsync(template);
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
