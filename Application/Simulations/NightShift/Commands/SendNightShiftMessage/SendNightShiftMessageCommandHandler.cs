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

namespace Application.Simulations.NightShift.Commands.SendNightShiftMessage;

public class SendNightShiftMessageCommandHandler : BaseHandler<SendNightShiftMessageCommand>,
    IRequestHandler<SendNightShiftMessageCommand, NightShiftMessageResponseDto>
{
    private readonly INightShiftSessionRepository _nightShiftSessionRepository;
    private readonly ILlmClient _llmClient;
    private readonly INightShiftPromptBuilder _promptBuilder;

    public SendNightShiftMessageCommandHandler(
        INightShiftSessionRepository nightShiftSessionRepository,
        ILlmClient llmClient,
        INightShiftPromptBuilder promptBuilder,
        IMapper mapper,
        ISlaisLogger<SendNightShiftMessageCommand> logger)
        : base(mapper, logger)
    {
        _nightShiftSessionRepository = nightShiftSessionRepository;
        _llmClient = llmClient;
        _promptBuilder = promptBuilder;
    }

    public async Task<NightShiftMessageResponseDto> HandleAsync(
        SendNightShiftMessageCommand request,
        IAuthentication? authentication = null,
        CancellationToken cancellationToken = default)
    {
        EnsureAllowed(authentication!);
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            throw new SlaisException(NightShiftErrorCodes.EmptyMessage);
        }

        var session = await _nightShiftSessionRepository.GetByGuidAsync(request.SessionGuid)
            ?? throw new SlaisException(NightShiftErrorCodes.SessionNotFound);
        if (session.UserGuid != authentication!.UserGuid)
        {
            throw new SlaisException(NightShiftErrorCodes.Forbidden);
        }

        if (session.Ended)
        {
            throw new SlaisException(NightShiftErrorCodes.SessionAlreadyEnded);
        }

        var action = string.Equals(request.Kind, "do", StringComparison.OrdinalIgnoreCase);
        var text = request.Text.Trim();
        var patientCase = ToPatientCase(session);
        var messages = new List<LlmChatMessage>
        {
            new("system", _promptBuilder.BuildPatientPrompt(patientCase, session.Language))
        };
        messages.AddRange(session.Messages
            .Where(message => message.Role != NightShiftMessageRole.System)
            .TakeLast(24)
            .Select(message => new LlmChatMessage(
                message.Role == NightShiftMessageRole.Patient ? "assistant" : "user",
                message.Content)));
        messages.Add(new LlmChatMessage("user", action ? NightShiftPromptTools.ActionInstruction(text) : text));
        var response = await _llmClient.ChatAsync(
            messages,
            0.9,
            160,
            false,
            session.Guid.ToString(),
            cancellationToken);
        var split = NightShiftPromptTools.SplitEnd(response);
        var sortOrder = session.Messages.Count == 0 ? 0 : session.Messages.Max(message => message.SortOrder) + 1;
        await _nightShiftSessionRepository.AddMessageAsync(
            NightShiftMessageEntity.Create(
                session.Guid,
                action ? NightShiftMessageRole.Action : NightShiftMessageRole.Student,
                action ? "[Handlung] " + text : text,
                sortOrder));
        await _nightShiftSessionRepository.AddMessageAsync(
            NightShiftMessageEntity.Create(
                session.Guid,
                NightShiftMessageRole.Patient,
                split.Clean,
                sortOrder + 1));
        if (split.Ended)
        {
            session.MarkEnded();
        }

        return new NightShiftMessageResponseDto
        {
            Response = split.Clean,
            Ended = split.Ended
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
