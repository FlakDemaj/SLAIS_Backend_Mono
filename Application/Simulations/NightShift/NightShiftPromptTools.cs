using System.Text.Json;

using Domain.Common.Enums;

namespace Application.Simulations.NightShift;

public static class NightShiftPromptTools
{
    public const string EndMarker = "[[ENDE]]";

    public static EndSplitResult SplitEnd(string answer)
    {
        return new EndSplitResult { Clean = answer.Replace(EndMarker, string.Empty, StringComparison.Ordinal).Trim(), Ended = answer.Contains(EndMarker, StringComparison.Ordinal) };
    }

    public static string ActionInstruction(string actionText)
    {
        return "[HANDLUNG DES PFLEGESCHUELERS] " + actionText.Trim() + "\nDas ist KEINE gesprochene Aeusserung, sondern eine pflegerische Handlung/Massnahme, die der Schueler jetzt an dir ausfuehrt. Reagiere als Patient realistisch koerperlich und emotional darauf, passe deine innere Anspannung entsprechend an und lass die Szene weitergehen. Antworte nur mit dem, was du als Patient laut sagst (1-3 kurze Saetze).";
    }

    public static ParsedFeedback ParseFeedback(string raw, Language language)
    {
        try
        {
            using var document = JsonDocument.Parse(raw);
            var scores = Get(document.RootElement, "scores");
            var dimensions = Get(document.RootElement, "per_dimension");
            return new ParsedFeedback
            {
                Ok = true,
                ScoreProfessional = Score(scores, "fachlich"),
                ScoreRapport = Score(scores, "sympathie"),
                ScoreEmpathy = Score(scores, "empathie"),
                ScoreListening = Score(scores, "zuhoeren"),
                ScoreClarity = Score(scores, "klarheit"),
                TextProfessional = Text(dimensions, "fachlich"),
                TextRapport = Text(dimensions, "sympathie"),
                TextEmpathy = Text(dimensions, "empathie"),
                TextListening = Text(dimensions, "zuhoeren"),
                TextClarity = Text(dimensions, "klarheit"),
                Summary = Limit(Get(document.RootElement, "summary").ToString(), 320)
            };
        }
        catch (JsonException)
        {
            return new ParsedFeedback { Ok = false, Summary = NightShiftLanguage.FeedbackUnavailable(language) };
        }
    }

    private static JsonElement Get(JsonElement element, string name)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value))
        {
            return value;
        }

        return default;
    }

    private static short Score(JsonElement scores, string key)
    {
        try
        {
            var value = Get(scores, key);
            return (short)Math.Clamp((int)Math.Round(value.GetDouble()), 0, 10);
        }
        catch
        {
            return 0;
        }
    }

    private static string Text(JsonElement dimensions, string key)
    {
        return Limit(Get(dimensions, key).ToString(), 160);
    }

    private static string Limit(string value, int maximumLength)
    {
        return value[..Math.Min(value.Length, maximumLength)];
    }
}

public class EndSplitResult
{
    public required string Clean { get; init; }

    public bool Ended { get; init; }
}

public class ParsedFeedback
{
    public bool Ok { get; init; }

    public short ScoreProfessional { get; init; }

    public short ScoreRapport { get; init; }

    public short ScoreEmpathy { get; init; }

    public short ScoreListening { get; init; }

    public short ScoreClarity { get; init; }

    public string TextProfessional { get; init; } = string.Empty;

    public string TextRapport { get; init; } = string.Empty;

    public string TextEmpathy { get; init; } = string.Empty;

    public string TextListening { get; init; } = string.Empty;

    public string TextClarity { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;
}
