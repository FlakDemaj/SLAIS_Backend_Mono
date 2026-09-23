using Domain.Common.Enums;
using Domain.Simulations.NightShift;
using Domain.Tests.Utils.Extensions;

using Tests.Domain.Shared.Builders;

using Xunit;

namespace Domain.Tests.Simulations.NightShift;

public class NightShiftTemplateTextTests
{
    [Fact]
    public void Create_WithEndMarker_ShouldThrowTemplateTextContainsMarker()
    {
        var template = new NightShiftTemplateEntityBuilder().Build();

        var act = () => CreateText(template.Guid, "[[ENDE]]");

        act.ThrowsException(NightShiftErrorCodes.TemplateTextContainsMarker);
    }

    [Fact]
    public void Create_WithNameTooLong_ShouldThrowTemplateTextTooLong()
    {
        var template = new NightShiftTemplateEntityBuilder().Build();

        var act = () => CreateText(template.Guid, new string('a', 61));

        act.ThrowsException(NightShiftErrorCodes.TemplateTextTooLong);
    }

    private static NightShiftTemplateTextEntity CreateText(
        Guid templateGuid,
        string name)
    {
        return NightShiftTemplateTextEntity.Create(
            null,
            templateGuid,
            Language.German,
            name,
            "Situation",
            "Emotion",
            "Lernziel",
            "Opener",
            "Geboren",
            "Geschlecht",
            "Aufnahme",
            "Diagnosen",
            "Allergien",
            "Medikation",
            "Pflegegrad",
            "Risiken",
            "Reanimation",
            "Angehörige");
    }
}
