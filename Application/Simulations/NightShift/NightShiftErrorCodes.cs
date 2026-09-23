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
    InvalidRequest = -520007
}
