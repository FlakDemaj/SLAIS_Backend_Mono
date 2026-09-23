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

namespace Application.Simulations.NightShift.Commands.FinishNightShiftSession;

public class FinishNightShiftSessionCommandHandler : BaseHandler<FinishNightShiftSessionCommand>,
    IRequestHandler<FinishNightShiftSessionCommand, NightShiftFeedbackResponseDto>
{
    private readonly INightShiftSessionRepository _nightShiftSessionRepository;
    private readonly ILlmClient _llmClient;
    private readonly INightShiftPromptBuilder _promptBuilder;

    public FinishNightShiftSessionCommandHandler(
        INightShiftSessionRepository nightShiftSessionRepository,
        ILlmClient llmClient,
        INightShiftPromptBuilder promptBuilder,
        IMapper mapper,
        ISlaisLogger<FinishNightShiftSessionCommand> logger)
        : base(mapper, logger)
    {
        _nightShiftSessionRepository = nightShiftSessionRepository;
        _llmClient = llmClient;
        _promptBuilder = promptBuilder;
    }

    public async Task<NightShiftFeedbackResponseDto> HandleAsync(
        FinishNightShiftSessionCommand request,
        IAuthentication? authentication = null,
        CancellationToken cancellationToken = default)
    {
        EnsureAllowed(authentication!);
        var session = await _nightShiftSessionRepository.GetByGuidAsync(request.SessionGuid)
            ?? throw new SlaisException(NightShiftErrorCodes.SessionNotFound);
        if (session.UserGuid != authentication!.UserGuid)
        {
            throw new SlaisException(NightShiftErrorCodes.Forbidden);
        }

        if (session.Feedback != null)
        {
            return ToResponse(session.Feedback, session.Language);
        }

        if (!session.Messages.Any(message => message.Role == NightShiftMessageRole.Student || message.Role == NightShiftMessageRole.Action))
        {
            return NotOkResponse(NightShiftLanguage.NoStudentTurnYet(session.Language), session.Language);
        }

        var patientCase = ToPatientCase(session);
        var messages = new List<LlmChatMessage>
        {
            new("system", _promptBuilder.BuildFeedbackPrompt(patientCase, session.Language))
        };
        messages.AddRange(session.Messages
            .Where(message => message.Role != NightShiftMessageRole.System)
            .Select(message => new LlmChatMessage(
                message.Role == NightShiftMessageRole.Patient ? "assistant" : "user",
                message.Content)));
        var raw = await _llmClient.ChatAsync(
            messages,
            0.3,
            900,
            true,
            session.Guid.ToString(),
            cancellationToken);
        var parsed = NightShiftPromptTools.ParseFeedback(raw, session.Language);
        if (!parsed.Ok)
        {
            return NotOkResponse(parsed.Summary, session.Language);
        }

        var feedback = NightShiftFeedbackEntity.Create(
            session.Guid,
            true,
            parsed.ScoreProfessional,
            parsed.ScoreRapport,
            parsed.ScoreEmpathy,
            parsed.ScoreListening,
            parsed.ScoreClarity,
            parsed.TextProfessional,
            parsed.TextRapport,
            parsed.TextEmpathy,
            parsed.TextListening,
            parsed.TextClarity,
            parsed.Summary,
            session.Model,
            _promptBuilder.Version);
        await _nightShiftSessionRepository.AddFeedbackAsync(feedback);
        session.MarkFinished();
        return ToResponse(feedback, session.Language);
    }

    private NightShiftFeedbackResponseDto NotOkResponse(string summary, Language language)
    {
        return new NightShiftFeedbackResponseDto
        {
            Feedback = summary,
            Ok = false,
            Scores = new NightShiftDimensionScoresDto(),
            Labels = NightShiftLanguage.Labels(language),
            PerDimension = new NightShiftDimensionTextsDto
            {
                Fachlich = string.Empty,
                Sympathie = string.Empty,
                Empathie = string.Empty,
                Zuhoeren = string.Empty,
                Klarheit = string.Empty
            },
            Summary = summary
        };
    }

    private NightShiftFeedbackResponseDto ToResponse(NightShiftFeedbackEntity feedback, Language language)
    {
        var mapped = _mapper.Map<NightShiftFeedbackResponseDto>(feedback);
        return new NightShiftFeedbackResponseDto
        {
            Feedback = mapped.Summary,
            Ok = mapped.Ok,
            Scores = mapped.Scores,
            Labels = NightShiftLanguage.Labels(language),
            PerDimension = mapped.PerDimension,
            Summary = mapped.Summary
        };
    }

    private static PatientCase ToPatientCase(NightShiftSessionEntity session)
    {
        return new PatientCase
        {
            Key = session.CaseKey,
            Name = session.CaseName,
            Situation = session.CaseSituation,
            Emotion = session.CaseEmotion,
            LearningGoal = session.CaseLearningGoal,
            TensionStart = session.CaseTensionStart,
            Opener = session.CaseOpener,
            Record = new PatientRecord
            {
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
}
