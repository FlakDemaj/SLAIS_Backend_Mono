using Application.Common.DTOs.Simulations.NightShift;

using Domain.Common.Enums;

namespace Application.Simulations.NightShift;

public static class NightShiftLanguage
{
    public static Language Parse(string? code)
    {
        return code is not null && code.Trim().StartsWith("en", StringComparison.OrdinalIgnoreCase)
            ? Language.English
            : Language.German;
    }

    public static string ToCode(Language language)
    {
        return language == Language.English ? "en" : "de";
    }

    public static NightShiftDimensionTextsDto Labels(Language language)
    {
        if (language == Language.English)
        {
            return new NightShiftDimensionTextsDto
            {
                Fachlich = "Professional accuracy",
                Sympathie = "Rapport",
                Empathie = "Empathy",
                Zuhoeren = "Active listening",
                Klarheit = "Clarity"
            };
        }

        return new NightShiftDimensionTextsDto
        {
            Fachlich = "Fachliche Richtigkeit",
            Sympathie = "Sympathie",
            Empathie = "Empathie",
            Zuhoeren = "Aktives Zuhoeren",
            Klarheit = "Klarheit"
        };
    }

    public static string RandomCaseName(Language language)
    {
        return language == Language.English ? "Random patient" : "Zufallspatient";
    }

    public static string RandomCaseSituation(Language language)
    {
        return language == Language.English ? "A new, unknown case" : "Ein neuer, unbekannter Fall";
    }

    public static string NoStudentTurnYet(Language language)
    {
        return language == Language.English
            ? "No student contribution yet - please talk to the patient first."
            : "Noch kein Schuelerbeitrag - bitte zuerst mit dem Patienten sprechen.";
    }

    public static string FeedbackUnavailable(Language language)
    {
        return language == Language.English
            ? "The evaluation could not be generated - please try again."
            : "Auswertung konnte nicht erzeugt werden - bitte erneut versuchen.";
    }

    public static string GeneratorUserMessage(Language language)
    {
        return language == Language.English
            ? "Erzeuge jetzt EINEN neuen, ungewoehnlichen Extremfall als JSON. Alle Textfelder auf Englisch."
            : "Erzeuge jetzt EINEN neuen, ungewoehnlichen Extremfall als JSON.";
    }

    public static string PatientSpeechRule(Language language)
    {
        return language == Language.English ? "Englisch. Du sprichst AUSSCHLIESSLICH Englisch, wie ein Patient in einem englischsprachigen Krankenhaus. Gesprochene Sprache, kurze Saetze. Kein einziges deutsches Wort, auch nicht im Abschiedssatz." : "Deutsch, gesprochene Sprache, kurze Saetze.";
    }

    public static string FeedbackLanguageRule(Language language)
    {
        return language == Language.English ? "Das Gespraech ist auf Englisch gefuehrt worden; schreibe per_dimension und summary auf Englisch." : "Schreibe per_dimension und summary auf Deutsch.";
    }

    public static string GeneratorStyleRule(Language language)
    {
        return language == Language.English ? "Englisch: ALLE Textfelder auf Englisch. name als \"Mrs Miller, 82\" oder \"Mr Demir, 47\". geboren im Format DD/MM/YYYY. pflegegrad: \"none\" oder \"1\" bis \"5\". allergien: \"none known\" oder konkret. reanimation z.B. \"Yes (no DNR); no advance directive\"." : "Deutsch mit normaler Rechtschreibung und Umlauten (ae/oe/ue NICHT verwenden, echte Umlaute schreiben).";
    }
}
