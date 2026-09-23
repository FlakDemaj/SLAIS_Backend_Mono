using Domain.Common.Enums;
using Domain.Simulations.NightShift;

namespace Tests.Domain.Shared.Builders;

public class NightShiftSessionEntityBuilder
{
    private Guid _userGuid = Guid.CreateVersion7();
    private Language _language = Language.German;
    private string _caseKey = "keller";
    private string _caseName = "Keller";
    private string _caseSituation = "Situation";
    private string _caseEmotion = "Emotion";
    private string _caseLearningGoal = "Lernziel";
    private string _caseOpener = "Guten Abend.";
    private short _caseTensionStart = 5;
    private string _recordBorn = "01.01.1940";
    private string _recordGender = "weiblich";
    private string _recordAdmission = "Aufnahme";
    private string _recordDiagnoses = "Diagnosen";
    private string _recordAllergies = "Keine";
    private string _recordMedication = "Medikation";
    private string _recordCareLevel = "Pflegegrad 2";
    private string _recordRisks = "Risiken";
    private string _recordResuscitation = "Ja";
    private string _recordRelatives = "Angehörige";
    private string _promptVersion = "v1";
    private string _model = "model";

    public NightShiftSessionEntityBuilder WithUserGuid(Guid userGuid)
    {
        _userGuid = userGuid;
        return this;
    }

    public NightShiftSessionEntityBuilder WithLanguage(Language language)
    {
        _language = language;
        return this;
    }

    public NightShiftSessionEntityBuilder WithCaseKey(string caseKey)
    {
        _caseKey = caseKey;
        return this;
    }

    public NightShiftSessionEntityBuilder WithCaseName(string caseName)
    {
        _caseName = caseName;
        return this;
    }

    public NightShiftSessionEntityBuilder WithCaseOpener(string caseOpener)
    {
        _caseOpener = caseOpener;
        return this;
    }

    public NightShiftSessionEntityBuilder WithCaseTensionStart(short caseTensionStart)
    {
        _caseTensionStart = caseTensionStart;
        return this;
    }

    public NightShiftSessionEntityBuilder WithPromptVersion(string promptVersion)
    {
        _promptVersion = promptVersion;
        return this;
    }

    public NightShiftSessionEntityBuilder WithModel(string model)
    {
        _model = model;
        return this;
    }

    public NightShiftSessionEntity Build()
    {
        return NightShiftSessionEntity.Create(
            _userGuid,
            _language,
            _caseKey,
            _caseName,
            _caseSituation,
            _caseEmotion,
            _caseLearningGoal,
            _caseOpener,
            _caseTensionStart,
            _recordBorn,
            _recordGender,
            _recordAdmission,
            _recordDiagnoses,
            _recordAllergies,
            _recordMedication,
            _recordCareLevel,
            _recordRisks,
            _recordResuscitation,
            _recordRelatives,
            _promptVersion,
            _model);
    }
}
