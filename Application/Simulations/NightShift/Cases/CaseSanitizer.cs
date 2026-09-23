using System.Text.Json;

using Domain.Common.Enums;

namespace Application.Simulations.NightShift.Cases;

public static class CaseSanitizer
{
    public static CaseSanitizerResult FromJson(
        string json,
        Language language,
        PatientCase fallback)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return FromElement(document.RootElement, language, fallback);
        }
        catch (JsonException)
        {
            return new CaseSanitizerResult
            {
                Case = fallback,
                UsedFallback = true
            };
        }
    }

    private static CaseSanitizerResult FromElement(
        JsonElement element,
        Language language,
        PatientCase fallback)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return new CaseSanitizerResult
            {
                Case = fallback,
                UsedFallback = true
            };
        }

        var record = Get(element, "stammblatt");
        var unavailable = language == Language.English ? "n/a" : "k.A.";
        return new CaseSanitizerResult
        {
            Case = new PatientCase
            {
                Key = "random",
                Name = StringValue(Get(element, "name"), "Patient", 60),
                Situation = StringValue(Get(element, "situation"), string.Empty, 200),
                Emotion = StringValue(Get(element, "emotion"), "angespannt", 120),
                LearningGoal = StringValue(Get(element, "lernziel"), language == Language.English ? "Practise de-escalation and empathy." : "Deeskalation und Empathie ueben.", 200),
                TensionStart = NumberValue(Get(element, "anspannung_start"), 0, 10, 6),
                Opener = StringValue(Get(element, "opener"), fallback.Opener, 200),
                Record = new PatientRecord
                {
                    Born = StringValue(Get(record, "geboren"), unavailable, 140),
                    Gender = StringValue(Get(record, "geschlecht"), unavailable, 140),
                    Admission = StringValue(Get(record, "aufnahme"), unavailable, 140),
                    Diagnoses = StringValue(Get(record, "diagnosen"), unavailable, 140),
                    Allergies = StringValue(Get(record, "allergien"), unavailable, 140),
                    Medication = StringValue(Get(record, "medikation"), unavailable, 140),
                    CareLevel = StringValue(Get(record, "pflegegrad"), unavailable, 140),
                    Risks = StringValue(Get(record, "risiken"), unavailable, 140),
                    Resuscitation = StringValue(Get(record, "reanimation"), unavailable, 140),
                    Relatives = StringValue(Get(record, "angehoerige"), unavailable, 140)
                }
            },
            UsedFallback = false
        };
    }

    private static JsonElement Get(JsonElement element, string name)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value))
        {
            return value;
        }

        return default;
    }

    private static string StringValue(JsonElement element, string fallback, int maximumLength)
    {
        var value = element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined ? string.Empty : element.ToString().Trim();
        if (string.IsNullOrEmpty(value))
        {
            value = fallback;
        }

        return value[..Math.Min(maximumLength, value.Length)];
    }

    private static short NumberValue(JsonElement element, int minimum, int maximum, int fallback)
    {
        var value = double.TryParse(element.ToString(), out var number) ? (int)Math.Round(number) : fallback;
        return (short)Math.Clamp(value, minimum, maximum);
    }
}
