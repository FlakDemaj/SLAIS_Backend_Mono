using System.Reflection;

using Application.Common.Interfaces.Services;
using Application.Simulations.NightShift;
using Application.Simulations.NightShift.Cases;

using Domain.Common.Enums;

namespace Infrastructure.InternalServices.NightShift;

public class NightShiftPromptBuilder : INightShiftPromptBuilder
{
    private readonly string _patient;

    private readonly string _feedback;

    private readonly string _generator;

    private readonly string _translate;

    public string Version { get; }

    public NightShiftPromptBuilder()
    {
        _patient = Load("patient.md");
        _feedback = Load("feedback.md");
        _generator = Load("generator.md");
        _translate = Load("translate.md");
        Version = _patient
            .Split('\n')[0]
            .Replace("version:", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();
    }

    public string BuildPatientPrompt(
        PatientCase patientCase,
        Language language)
    {
        var fields = new Dictionary<string, string>
        {
            ["SPRACHE"] = NightShiftLanguage.PatientSpeechRule(language),
            ["NAME"] = patientCase.Name,
            ["SITUATION"] = patientCase.Situation,
            ["EMOTION"] = patientCase.Emotion,
            ["LERNZIEL"] = patientCase.LearningGoal,
            ["ANSPANNUNG"] = patientCase.TensionStart.ToString(),
            ["STAMMBLATT"] = "- Geboren: " + patientCase.Record.Born + ", Geschlecht: " + patientCase.Record.Gender + "\n"
                + "- Aufnahme: " + patientCase.Record.Admission + "\n"
                + "- Diagnosen: " + patientCase.Record.Diagnoses + "\n"
                + "- Allergien: " + patientCase.Record.Allergies + "\n"
                + "- Medikation: " + patientCase.Record.Medication + "\n"
                + "- Pflegegrad: " + patientCase.Record.CareLevel + "\n"
                + "- Risiken: " + patientCase.Record.Risks + "\n"
                + "- Wiederbelebung/Verfuegung: " + patientCase.Record.Resuscitation + "\n"
                + "- Angehoerige/Betreuung: " + patientCase.Record.Relatives
        };

        return Replace(_patient, fields);
    }

    public string BuildFeedbackPrompt(
        PatientCase patientCase,
        Language language)
    {
        var fields = new Dictionary<string, string>
        {
            ["NAME"] = patientCase.Name,
            ["LERNZIEL"] = patientCase.LearningGoal,
            ["SPRACHE"] = NightShiftLanguage.FeedbackLanguageRule(language)
        };

        return Replace(_feedback, fields);
    }

    public string BuildGeneratorPrompt(Language language)
    {
        var fields = new Dictionary<string, string>
        {
            ["SPRACHE"] = NightShiftLanguage.GeneratorStyleRule(language)
        };

        return Replace(_generator, fields);
    }

    public string BuildTranslationPrompt(
        Language source,
        Language target)
    {
        var fields = new Dictionary<string, string>
        {
            ["QUELLE"] = TranslationLanguage(source),
            ["ZIEL"] = TranslationLanguage(target)
        };

        return Replace(_translate, fields);
    }

    private static string TranslationLanguage(Language language)
    {
        return language == Language.English ? "Englisch" : "Deutsch";
    }

    private static string Replace(
        string source,
        IDictionary<string, string> fields)
    {
        var result = source;

        foreach (var field in fields)
        {
            result = result.Replace("{{" + field.Key + "}}", field.Value, StringComparison.Ordinal);
        }

        return result;
    }

    private static string Load(string file)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly
            .GetManifestResourceNames()
            .Single(name => name.EndsWith(file, StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream!);

        return reader.ReadToEnd();
    }
}
