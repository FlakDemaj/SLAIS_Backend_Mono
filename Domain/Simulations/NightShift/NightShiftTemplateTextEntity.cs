using Domain.Common.Enums;
using Domain.Common.Exceptions;

using SLAIS.Domain.Commom;

namespace Domain.Simulations.NightShift;

public class NightShiftTemplateTextEntity : BaseUpdatedByEntity
{
    public Guid TemplateGuid { get; private set; }

    public Language Language { get; private set; }

    public string Name { get; private set; }

    public string Situation { get; private set; }

    public string Emotion { get; private set; }

    public string LearningGoal { get; private set; }

    public string Opener { get; private set; }

    public string Born { get; private set; }

    public string Gender { get; private set; }

    public string Admission { get; private set; }

    public string Diagnoses { get; private set; }

    public string Allergies { get; private set; }

    public string Medication { get; private set; }

    public string CareLevel { get; private set; }

    public string Risks { get; private set; }

    public string Resuscitation { get; private set; }

    public string Relatives { get; private set; }

    public NightShiftTemplateEntity? Template { get; private set; }

    private NightShiftTemplateTextEntity(
        Guid? createdByUserGuid,
        Guid templateGuid,
        Language language,
        string name,
        string situation,
        string emotion,
        string learningGoal,
        string opener,
        string born,
        string gender,
        string admission,
        string diagnoses,
        string allergies,
        string medication,
        string careLevel,
        string risks,
        string resuscitation,
        string relatives)
        : base(createdByUserGuid)
    {
        TemplateGuid = templateGuid;
        Language = language;
        Name = name;
        Situation = situation;
        Emotion = emotion;
        LearningGoal = learningGoal;
        Opener = opener;
        Born = born;
        Gender = gender;
        Admission = admission;
        Diagnoses = diagnoses;
        Allergies = allergies;
        Medication = medication;
        CareLevel = careLevel;
        Risks = risks;
        Resuscitation = resuscitation;
        Relatives = relatives;
    }

    #region Create

    public static NightShiftTemplateTextEntity Create(
        Guid? createdByUserGuid,
        Guid templateGuid,
        Language language,
        string name,
        string situation,
        string emotion,
        string learningGoal,
        string opener,
        string born,
        string gender,
        string admission,
        string diagnoses,
        string allergies,
        string medication,
        string careLevel,
        string risks,
        string resuscitation,
        string relatives)
    {
        CheckInputs(
            name,
            situation,
            emotion,
            learningGoal,
            opener,
            born,
            gender,
            admission,
            diagnoses,
            allergies,
            medication,
            careLevel,
            risks,
            resuscitation,
            relatives);

        return new NightShiftTemplateTextEntity(
            createdByUserGuid,
            templateGuid,
            language,
            name,
            situation,
            emotion,
            learningGoal,
            opener,
            born,
            gender,
            admission,
            diagnoses,
            allergies,
            medication,
            careLevel,
            risks,
            resuscitation,
            relatives);
    }

    #endregion

    #region Checks

    private static void CheckInputs(
        string name,
        string situation,
        string emotion,
        string learningGoal,
        string opener,
        string born,
        string gender,
        string admission,
        string diagnoses,
        string allergies,
        string medication,
        string careLevel,
        string risks,
        string resuscitation,
        string relatives)
    {
        var fields = new[]
        {
            name, situation, emotion, learningGoal, opener, born, gender, admission,
            diagnoses, allergies, medication, careLevel, risks, resuscitation, relatives
        };

        if (fields.Any(string.IsNullOrWhiteSpace))
        {
            throw new SlaisException(NightShiftErrorCodes.InvalidInput);
        }

        if (fields.Any(x => x.Contains("{{", StringComparison.Ordinal) || x.Contains("[[ENDE]]", StringComparison.Ordinal)))
        {
            throw new SlaisException(NightShiftErrorCodes.TemplateTextContainsMarker);
        }

        if (name.Length > 60
            || situation.Length > 200
            || learningGoal.Length > 200
            || opener.Length > 200
            || emotion.Length > 120
            || born.Length > 140
            || gender.Length > 140
            || admission.Length > 140
            || diagnoses.Length > 140
            || allergies.Length > 140
            || medication.Length > 140
            || careLevel.Length > 140
            || risks.Length > 140
            || resuscitation.Length > 140
            || relatives.Length > 140)
        {
            throw new SlaisException(NightShiftErrorCodes.TemplateTextTooLong);
        }
    }

    #endregion
}
