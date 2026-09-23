using Application.Common.Authentication;
using Application.Common.Base;
using Application.Common.DTOs.Simulations.NightShift;
using Application.Common.Interfaces.Repositorys;
using Application.Common.Interfaces.Services;
using Application.Simulations.NightShift.Cases;
using Application.Utils.Logger;
using Application.Utils.Mediator.Interfaces;

using AutoMapper;

using Domain.Common.Enums;
using Domain.Common.Exceptions;
using Domain.Simulations.NightShift;

namespace Application.Simulations.NightShift.Commands.StartNightShiftSession;

public class StartNightShiftSessionCommandHandler : BaseHandler<StartNightShiftSessionCommand>,
    IRequestHandler<StartNightShiftSessionCommand, StartNightShiftSessionResponseDto>
{
    private readonly INightShiftSessionRepository _nightShiftSessionRepository;
    private readonly INightShiftTemplateRepository _nightShiftTemplateRepository;
    private readonly ILlmClient _llmClient;
    private readonly INightShiftPromptBuilder _promptBuilder;

    public StartNightShiftSessionCommandHandler(
        INightShiftSessionRepository nightShiftSessionRepository,
        INightShiftTemplateRepository nightShiftTemplateRepository,
        ILlmClient llmClient,
        INightShiftPromptBuilder promptBuilder,
        IMapper mapper,
        ISlaisLogger<StartNightShiftSessionCommand> logger)
        : base(mapper, logger)
    {
        _nightShiftSessionRepository = nightShiftSessionRepository;
        _nightShiftTemplateRepository = nightShiftTemplateRepository;
        _llmClient = llmClient;
        _promptBuilder = promptBuilder;
    }

    public async Task<StartNightShiftSessionResponseDto> HandleAsync(
        StartNightShiftSessionCommand request,
        IAuthentication? authentication = null,
        CancellationToken cancellationToken = default)
    {
        EnsureAllowed(authentication!);
        var language = NightShiftLanguage.Parse(request.Language);
        var resolution = await ResolveCaseAsync(request.CaseKey, language, cancellationToken);
        var patientCase = resolution.Case;
        var session = NightShiftSessionEntity.Create(
            authentication!.UserGuid,
            language,
            patientCase.Key,
            patientCase.Name,
            patientCase.Situation,
            patientCase.Emotion,
            patientCase.LearningGoal,
            patientCase.Opener,
            patientCase.TensionStart,
            patientCase.Record.Born,
            patientCase.Record.Gender,
            patientCase.Record.Admission,
            patientCase.Record.Diagnoses,
            patientCase.Record.Allergies,
            patientCase.Record.Medication,
            patientCase.Record.CareLevel,
            patientCase.Record.Risks,
            patientCase.Record.Resuscitation,
            patientCase.Record.Relatives,
            _promptBuilder.Version,
            _llmClient.Model,
            resolution.TemplateGuid);
        await _nightShiftSessionRepository.CreateAsync(session);
        await _nightShiftSessionRepository.AddMessageAsync(
            NightShiftMessageEntity.Create(
                session.Guid,
                NightShiftMessageRole.System,
                _promptBuilder.BuildPatientPrompt(patientCase, language),
                0));
        await _nightShiftSessionRepository.AddMessageAsync(
            NightShiftMessageEntity.Create(
                session.Guid,
                NightShiftMessageRole.Patient,
                patientCase.Opener,
                1));
        return new StartNightShiftSessionResponseDto
        {
            SessionId = session.Guid,
            Temperament = patientCase.Emotion,
            FirstMessage = patientCase.Opener,
            Ended = false,
            Language = NightShiftLanguage.ToCode(language),
            Case = new NightShiftCaseResponseDto
            {
                Key = patientCase.Key,
                Name = patientCase.Name,
                Situation = patientCase.Situation,
                Lernziel = patientCase.LearningGoal,
                LanguageFallback = resolution.IsLanguageFallback
            },
            Stammblatt = new NightShiftStammblattDto
            {
                Geboren = patientCase.Record.Born,
                Geschlecht = patientCase.Record.Gender,
                Aufnahme = patientCase.Record.Admission,
                Diagnosen = patientCase.Record.Diagnoses,
                Allergien = patientCase.Record.Allergies,
                Medikation = patientCase.Record.Medication,
                Pflegegrad = patientCase.Record.CareLevel,
                Risiken = patientCase.Record.Risks,
                Reanimation = patientCase.Record.Resuscitation,
                Angehoerige = patientCase.Record.Relatives
            }
        };
    }

    private async Task<CaseResolution> ResolveCaseAsync(
        string? key,
        Language language,
        CancellationToken cancellationToken)
    {
        var caseKey = key?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(caseKey))
        {
            var templates = await _nightShiftTemplateRepository.GetActiveAsync();
            var template = GetRandomTemplate(templates);
            return FromTemplate(template, language);
        }

        if (caseKey.Equals("random", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var raw = await _llmClient.ChatAsync(
                    [
                        new LlmChatMessage("system", _promptBuilder.BuildGeneratorPrompt(language)),
                        new LlmChatMessage("user", NightShiftLanguage.GeneratorUserMessage(language))
                    ],
                    1,
                    700,
                    true,
                    "generate",
                    cancellationToken);
                var sanitized = CaseSanitizer.FromJson(raw, language, CreateSanitizerFallback(language));
                if (!sanitized.UsedFallback)
                {
                    return new CaseResolution(sanitized.Case, null, false);
                }

                var templates = await _nightShiftTemplateRepository.GetActiveAsync();
                var fallbackTemplate = GetRandomTemplate(templates);
                return FromTemplate(fallbackTemplate, language);
            }
            catch (SlaisException exception) when (exception.ErrorCode == (int)NightShiftErrorCodes.LlmUnavailable)
            {
                var templates = await _nightShiftTemplateRepository.GetActiveAsync();
                var fallbackTemplate = GetRandomTemplate(templates);
                return FromTemplate(fallbackTemplate, language);
            }
        }

        var activeTemplate = await _nightShiftTemplateRepository.GetActiveByKeyAsync(caseKey);
        if (activeTemplate == null)
        {
            throw new SlaisException(NightShiftErrorCodes.InvalidCase);
        }

        return FromTemplate(activeTemplate, language);
    }

    private static NightShiftTemplateEntity GetRandomTemplate(List<NightShiftTemplateEntity> templates)
    {
        if (templates.Count == 0)
        {
            throw new SlaisException(NightShiftErrorCodes.NoActiveTemplates);
        }

        return templates[Random.Shared.Next(templates.Count)];
    }

    private static CaseResolution FromTemplate(
        NightShiftTemplateEntity template,
        Language language)
    {
        var resolution = PatientCaseFactory.FromTemplate(template, language);
        return new CaseResolution(
            resolution.Case,
            template.Guid,
            resolution.IsLanguageFallback);
    }

    private static PatientCase CreateSanitizerFallback(Language language)
    {
        var unavailable = language == Language.English ? "n/a" : "k.A.";
        return new PatientCase
        {
            Key = "random",
            Name = "Patient",
            Situation = string.Empty,
            Emotion = "angespannt",
            LearningGoal = language == Language.English
                ? "Practise de-escalation and empathy."
                : "Deeskalation und Empathie ueben.",
            TensionStart = 6,
            Opener = string.Empty,
            Record = new PatientRecord
            {
                Born = unavailable,
                Gender = unavailable,
                Admission = unavailable,
                Diagnoses = unavailable,
                Allergies = unavailable,
                Medication = unavailable,
                CareLevel = unavailable,
                Risks = unavailable,
                Resuscitation = unavailable,
                Relatives = unavailable
            }
        };
    }

    private static void EnsureAllowed(IAuthentication authentication)
    {
        if (authentication.UserRole == Roles.Server)
        {
            throw new SlaisException(NightShiftErrorCodes.Forbidden);
        }
    }

    private sealed class CaseResolution
    {
        public PatientCase Case { get; }

        public Guid? TemplateGuid { get; }

        public bool IsLanguageFallback { get; }

        public CaseResolution(
            PatientCase patientCase,
            Guid? templateGuid,
            bool isLanguageFallback)
        {
            Case = patientCase;
            TemplateGuid = templateGuid;
            IsLanguageFallback = isLanguageFallback;
        }
    }
}
