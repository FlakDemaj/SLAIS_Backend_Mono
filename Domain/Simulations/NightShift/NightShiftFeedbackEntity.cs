using Domain.Base;
using Domain.Common.Exceptions;

namespace Domain.Simulations.NightShift;

public class NightShiftFeedbackEntity : BaseGuidEntity
{
    public Guid SessionGuid { get; private set; }

    public bool Ok { get; private set; }

    public short ScoreProfessional { get; private set; }

    public short ScoreRapport { get; private set; }

    public short ScoreEmpathy { get; private set; }

    public short ScoreListening { get; private set; }

    public short ScoreClarity { get; private set; }

    public string TextProfessional { get; private set; }

    public string TextRapport { get; private set; }

    public string TextEmpathy { get; private set; }

    public string TextListening { get; private set; }

    public string TextClarity { get; private set; }

    public string Summary { get; private set; }

    public string Model { get; private set; }

    public string PromptVersion { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public NightShiftSessionEntity? Session { get; private set; }

    private NightShiftFeedbackEntity(
        Guid sessionGuid,
        bool ok,
        short scoreProfessional,
        short scoreRapport,
        short scoreEmpathy,
        short scoreListening,
        short scoreClarity,
        string textProfessional,
        string textRapport,
        string textEmpathy,
        string textListening,
        string textClarity,
        string summary,
        string model,
        string promptVersion)
    {
        SessionGuid = sessionGuid;
        Ok = ok;
        ScoreProfessional = scoreProfessional;
        ScoreRapport = scoreRapport;
        ScoreEmpathy = scoreEmpathy;
        ScoreListening = scoreListening;
        ScoreClarity = scoreClarity;
        TextProfessional = textProfessional;
        TextRapport = textRapport;
        TextEmpathy = textEmpathy;
        TextListening = textListening;
        TextClarity = textClarity;
        Summary = summary;
        Model = model;
        PromptVersion = promptVersion;
        CreatedAt = DateTime.UtcNow;
    }

    #region Create

    public static NightShiftFeedbackEntity Create(
        Guid sessionGuid,
        bool ok,
        short scoreProfessional,
        short scoreRapport,
        short scoreEmpathy,
        short scoreListening,
        short scoreClarity,
        string textProfessional,
        string textRapport,
        string textEmpathy,
        string textListening,
        string textClarity,
        string summary,
        string model,
        string promptVersion)
    {
        CheckInputs(
            scoreProfessional,
            scoreRapport,
            scoreEmpathy,
            scoreListening,
            scoreClarity,
            textProfessional,
            textRapport,
            textEmpathy,
            textListening,
            textClarity);

        return new NightShiftFeedbackEntity(
            sessionGuid,
            ok,
            scoreProfessional,
            scoreRapport,
            scoreEmpathy,
            scoreListening,
            scoreClarity,
            textProfessional,
            textRapport,
            textEmpathy,
            textListening,
            textClarity,
            summary,
            model,
            promptVersion);
    }

    public static NightShiftFeedbackEntity NotOk(
        Guid sessionGuid,
        string summary,
        string model,
        string promptVersion)
    {
        return Create(
            sessionGuid,
            false,
            0,
            0,
            0,
            0,
            0,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            summary,
            model,
            promptVersion);
    }

    #endregion

    #region Checks

    private static void CheckInputs(
        short scoreProfessional,
        short scoreRapport,
        short scoreEmpathy,
        short scoreListening,
        short scoreClarity,
        string textProfessional,
        string textRapport,
        string textEmpathy,
        string textListening,
        string textClarity)
    {
        if (!IsScoreValid(scoreProfessional)
            || !IsScoreValid(scoreRapport)
            || !IsScoreValid(scoreEmpathy)
            || !IsScoreValid(scoreListening)
            || !IsScoreValid(scoreClarity))
        {
            throw new SlaisException(NightShiftErrorCodes.InvalidScore);
        }

        if (textProfessional is null
            || textRapport is null
            || textEmpathy is null
            || textListening is null
            || textClarity is null)
        {
            throw new SlaisException(NightShiftErrorCodes.InvalidInput);
        }
    }

    private static bool IsScoreValid(short score)
    {
        return score >= 0 && score <= 10;
    }

    #endregion
}
