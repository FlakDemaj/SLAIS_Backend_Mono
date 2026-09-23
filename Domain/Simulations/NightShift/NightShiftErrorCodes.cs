using System.ComponentModel;

namespace Domain.Simulations.NightShift;

public enum NightShiftErrorCodes
{
    [Description("Die Eingabe ist ungültig.")]
    InvalidInput = -530001,

    [Description("Die Sitzung wurde bereits beendet.")]
    SessionAlreadyEnded = -530002,

    [Description("Die Nachricht ist zu lang.")]
    MessageTooLong = -530003,

    [Description("Die Bewertung ist ungültig.")]
    InvalidScore = -530004,

    [Description("Die Sortierreihenfolge ist ungültig.")]
    InvalidSortOrder = -530005,
}
