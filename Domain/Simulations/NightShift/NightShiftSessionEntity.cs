using Domain.Common.Enums;
using Domain.Common.Exceptions;

namespace Domain.Simulations.NightShift;

public class NightShiftSessionEntity : NightShiftSessionNavigationPropertyEntity
{
    public Guid? TemplateGuid { get; private set; }

    public Guid UserGuid { get; private set; }

    public Language Language { get; private set; }

    public string CaseKey { get; private set; }

    public string CaseName { get; private set; }

    public string CaseSituation { get; private set; }

    public string CaseEmotion { get; private set; }

    public string CaseLearningGoal { get; private set; }

    public string CaseOpener { get; private set; }

    public short CaseTensionStart { get; private set; }

    public string RecordBorn { get; private set; }

    public string RecordGender { get; private set; }

    public string RecordAdmission { get; private set; }

    public string RecordDiagnoses { get; private set; }

    public string RecordAllergies { get; private set; }

    public string RecordMedication { get; private set; }

    public string RecordCareLevel { get; private set; }

    public string RecordRisks { get; private set; }

    public string RecordResuscitation { get; private set; }

    public string RecordRelatives { get; private set; }

    public string PromptVersion { get; private set; }

    public string Model { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime? EndedAt { get; private set; }

    public DateTime? FinishedAt { get; private set; }

    public bool Ended { get; private set; }

    private NightShiftSessionEntity(
        Guid userGuid,
        Language language,
        string caseKey,
        string caseName,
        string caseSituation,
        string caseEmotion,
        string caseLearningGoal,
        string caseOpener,
        short caseTensionStart,
        string recordBorn,
        string recordGender,
        string recordAdmission,
        string recordDiagnoses,
        string recordAllergies,
        string recordMedication,
        string recordCareLevel,
        string recordRisks,
        string recordResuscitation,
        string recordRelatives,
        string promptVersion,
        string model,
        Guid? templateGuid)
        : base(userGuid)
    {
        UserGuid = userGuid;
        Language = language;
        CaseKey = caseKey;
        CaseName = caseName;
        CaseSituation = caseSituation;
        CaseEmotion = caseEmotion;
        CaseLearningGoal = caseLearningGoal;
        CaseOpener = caseOpener;
        CaseTensionStart = caseTensionStart;
        RecordBorn = recordBorn;
        RecordGender = recordGender;
        RecordAdmission = recordAdmission;
        RecordDiagnoses = recordDiagnoses;
        RecordAllergies = recordAllergies;
        RecordMedication = recordMedication;
        RecordCareLevel = recordCareLevel;
        RecordRisks = recordRisks;
        RecordResuscitation = recordResuscitation;
        RecordRelatives = recordRelatives;
        PromptVersion = promptVersion;
        Model = model;
        TemplateGuid = templateGuid;
        StartedAt = DateTime.UtcNow;
        EndedAt = null;
        FinishedAt = null;
        Ended = false;
    }

    #region Create

    public static NightShiftSessionEntity Create(
        Guid userGuid,
        Language language,
        string caseKey,
        string caseName,
        string caseSituation,
        string caseEmotion,
        string caseLearningGoal,
        string caseOpener,
        short caseTensionStart,
        string recordBorn,
        string recordGender,
        string recordAdmission,
        string recordDiagnoses,
        string recordAllergies,
        string recordMedication,
        string recordCareLevel,
        string recordRisks,
        string recordResuscitation,
        string recordRelatives,
        string promptVersion,
        string model,
        Guid? templateGuid = null)
    {
        CheckInputs(
            caseKey,
            caseName,
            caseOpener,
            caseTensionStart,
            promptVersion,
            model);

        return new NightShiftSessionEntity(
            userGuid,
            language,
            caseKey,
            caseName,
            caseSituation,
            caseEmotion,
            caseLearningGoal,
            caseOpener,
            caseTensionStart,
            recordBorn,
            recordGender,
            recordAdmission,
            recordDiagnoses,
            recordAllergies,
            recordMedication,
            recordCareLevel,
            recordRisks,
            recordResuscitation,
            recordRelatives,
            promptVersion,
            model,
            templateGuid);
    }

    #endregion

    #region Behaviour

    public void MarkEnded()
    {
        if (Ended)
        {
            return;
        }

        Ended = true;
        EndedAt = DateTime.UtcNow;
    }

    public void MarkFinished()
    {
        FinishedAt = DateTime.UtcNow;
        MarkEnded();
    }

    public void EnsureCanChat()
    {
        if (Ended)
        {
            throw new SlaisException(NightShiftErrorCodes.SessionAlreadyEnded);
        }
    }

    #endregion

    #region Checks

    private static void CheckInputs(
        string caseKey,
        string caseName,
        string caseOpener,
        short caseTensionStart,
        string promptVersion,
        string model)
    {
        if (string.IsNullOrWhiteSpace(caseKey)
            || string.IsNullOrWhiteSpace(caseName)
            || string.IsNullOrWhiteSpace(caseOpener)
            || string.IsNullOrWhiteSpace(promptVersion)
            || string.IsNullOrWhiteSpace(model)
            || caseTensionStart < 0
            || caseTensionStart > 10)
        {
            throw new SlaisException(NightShiftErrorCodes.InvalidInput);
        }
    }

    #endregion
}
