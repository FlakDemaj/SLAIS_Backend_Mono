using System.Text.Json;

using Application.Common.Authentication;
using Application.Common.Base;
using Application.Common.DTOs.Simulations.NightShift;
using Application.Common.Interfaces.Services;
using Application.Utils.Logger;
using Application.Utils.Mediator.Interfaces;

using AutoMapper;

using Domain.Common.Enums;
using Domain.Common.Exceptions;

namespace Application.Simulations.NightShift.Templates.Commands.SuggestNightShiftTemplateTranslation;

public class SuggestNightShiftTemplateTranslationCommandHandler :
    BaseHandler<SuggestNightShiftTemplateTranslationCommand>,
    IRequestHandler<SuggestNightShiftTemplateTranslationCommand, NightShiftTemplateTextDto>
{
    private readonly ILlmClient _llmClient;
    private readonly INightShiftPromptBuilder _promptBuilder;

    public SuggestNightShiftTemplateTranslationCommandHandler(
        ILlmClient llmClient,
        INightShiftPromptBuilder promptBuilder,
        IMapper mapper,
        ISlaisLogger<SuggestNightShiftTemplateTranslationCommand> logger)
        : base(mapper, logger)
    {
        _llmClient = llmClient;
        _promptBuilder = promptBuilder;
    }

    public async Task<NightShiftTemplateTextDto> HandleAsync(
        SuggestNightShiftTemplateTranslationCommand request,
        IAuthentication? authentication = null,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin(authentication!);
        var source = NightShiftLanguage.Parse(request.SourceLanguage);
        var target = NightShiftLanguage.Parse(request.TargetLanguage);
        if (source == target)
        {
            throw new SlaisException(NightShiftErrorCodes.InvalidRequest);
        }

        try
        {
            var raw = await _llmClient.ChatAsync(
                [
                    new LlmChatMessage("system", _promptBuilder.BuildTranslationPrompt(source, target)),
                    new LlmChatMessage("user", Serialize(request.Text))
                ],
                0.2,
                900,
                true,
                "translate",
                cancellationToken);
            var text = Deserialize(raw);
            NightShiftTemplateTextFactory.Create(
                null,
                Guid.Empty,
                target,
                text);
            return text;
        }
        catch (JsonException)
        {
            throw new SlaisException(NightShiftErrorCodes.TranslationUnavailable);
        }
        catch (SlaisException exception) when (exception.ErrorCode == (int)NightShiftErrorCodes.LlmUnavailable)
        {
            throw new SlaisException(NightShiftErrorCodes.TranslationUnavailable);
        }
        catch (SlaisException)
        {
            throw new SlaisException(NightShiftErrorCodes.TranslationUnavailable);
        }
    }

    private static string Serialize(NightShiftTemplateTextDto text)
    {
        return JsonSerializer.Serialize(ToDictionary(text));
    }

    private static NightShiftTemplateTextDto Deserialize(string raw)
    {
        using var document = JsonDocument.Parse(raw);
        var root = document.RootElement;
        return new NightShiftTemplateTextDto
        {
            Name = Get(root, "name"),
            Situation = Get(root, "situation"),
            Emotion = Get(root, "emotion"),
            LearningGoal = Get(root, "learning_goal"),
            Opener = Get(root, "opener"),
            Born = Get(root, "born"),
            Gender = Get(root, "gender"),
            Admission = Get(root, "admission"),
            Diagnoses = Get(root, "diagnoses"),
            Allergies = Get(root, "allergies"),
            Medication = Get(root, "medication"),
            CareLevel = Get(root, "care_level"),
            Risks = Get(root, "risks"),
            Resuscitation = Get(root, "resuscitation"),
            Relatives = Get(root, "relatives")
        };
    }

    private static string Get(
        JsonElement root,
        string name)
    {
        if (!root.TryGetProperty(name, out var value)
            || value.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(value.GetString()))
        {
            throw new JsonException();
        }

        return value.GetString()!;
    }

    private static Dictionary<string, string> ToDictionary(NightShiftTemplateTextDto text)
    {
        return new Dictionary<string, string>
        {
            ["name"] = text.Name,
            ["situation"] = text.Situation,
            ["emotion"] = text.Emotion,
            ["learning_goal"] = text.LearningGoal,
            ["opener"] = text.Opener,
            ["born"] = text.Born,
            ["gender"] = text.Gender,
            ["admission"] = text.Admission,
            ["diagnoses"] = text.Diagnoses,
            ["allergies"] = text.Allergies,
            ["medication"] = text.Medication,
            ["care_level"] = text.CareLevel,
            ["risks"] = text.Risks,
            ["resuscitation"] = text.Resuscitation,
            ["relatives"] = text.Relatives
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
