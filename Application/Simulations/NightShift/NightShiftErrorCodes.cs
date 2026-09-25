using System.ComponentModel;

namespace Application.Simulations.NightShift;

public enum NightShiftErrorCodes
{
    [Description("Session nicht gefunden.")]
    SessionNotFound = -520001,

    [Description("Session bereits beendet.")]
    SessionAlreadyEnded = -520002,

    [Description("Sie haben keinen Zugriff auf diese Funktion.")]
    Forbidden = -520003,

    [Description("Leerer Beitrag.")]
    EmptyMessage = -520004,

    [Description("Ungueltiger Fall.")]
    InvalidCase = -520005,

    [Description("LLM nicht erreichbar.")]
    LlmUnavailable = -520006,

    [Description("Ungueltige Anfrage.")]
    InvalidRequest = -520007,

    [Description("Keine aktiven Fallvorlagen vorhanden.")]
    NoActiveTemplates = -520008,

    [Description("Die Vorlage wurde inzwischen von jemand anderem geaendert.")]
    TemplateModifiedByOtherUser = -520009,

    [Description("Der Uebersetzungsvorschlag konnte nicht erzeugt werden.")]
    TranslationUnavailable = -520010,

    [Description("Der Vorlagenschluessel ist bereits vergeben.")]
    TemplateKeyAlreadyExists = -520011,

    [Description("Die Vorlage wurde nicht gefunden.")]
    TemplateNotFound = -520012,

    [Description("Die Sitzung basiert nicht auf einem Zufallsfall.")]
    SessionIsNotRandomCase = -520013,

    [Description("Die Vorlage ist nicht aktiv.")]
    TemplateNotActive = -520014,

    [Description("Der Zustandswechsel ist nicht erlaubt.")]
    TemplateStateTransitionInvalid = -520015
}
