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

    [Description("Der Vorlagenschlüssel ist ungültig.")]
    TemplateKeyInvalid = -530006,

    [Description("Ein Vorlagentext ist zu lang.")]
    TemplateTextTooLong = -530007,

    [Description("Ein Vorlagentext enthält einen unzulässigen Marker.")]
    TemplateTextContainsMarker = -530008,

    [Description("Eine Vorlage benötigt deutsche und englische Texte, um aktiviert zu werden.")]
    TemplateActivationNeedsBothLanguages = -530009,

    [Description("Der Vorlagenstatus ist ungültig.")]
    TemplateStateInvalid = -530010,
}
