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
    private readonly ILlmClient _llmClient;
    private readonly INightShiftPromptBuilder _promptBuilder;

    public StartNightShiftSessionCommandHandler(
        INightShiftSessionRepository nightShiftSessionRepository,
        ILlmClient llmClient,
        INightShiftPromptBuilder promptBuilder,
        IMapper mapper,
        ISlaisLogger<StartNightShiftSessionCommand> logger)
        : base(mapper, logger)
    {
        _nightShiftSessionRepository = nightShiftSessionRepository;
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
        var patientCase = await ResolveCaseAsync(request.CaseKey, language, cancellationToken);
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
            _llmClient.Model);
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
                Lernziel = patientCase.LearningGoal
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

    private async Task<PatientCase> ResolveCaseAsync(
        string? key,
        Language language,
        CancellationToken cancellationToken)
    {
        var caseKey = key?.Trim();
        if (string.IsNullOrEmpty(caseKey))
        {
            var cases = CuratedCases.For(language);
            return cases[Random.Shared.Next(cases.Count)];
        }

        if (caseKey.Equals(CuratedCases.RandomKey, StringComparison.OrdinalIgnoreCase))
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
                return CaseSanitizer.FromJson(raw, language);
            }
            catch (SlaisException exception) when (exception.ErrorCode == (int)NightShiftErrorCodes.LlmUnavailable)
            {
                return CuratedCases.Fallback(language);
            }
        }

        return CuratedCases.Find(caseKey, language) ?? throw new SlaisException(NightShiftErrorCodes.InvalidCase);
    }

    private static void EnsureAllowed(IAuthentication authentication)
    {
        if (authentication.UserRole == Roles.Server)
        {
            throw new SlaisException(NightShiftErrorCodes.Forbidden);
        }
    }
}
