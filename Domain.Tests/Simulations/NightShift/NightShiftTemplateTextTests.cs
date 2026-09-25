using Domain.Common.Enums;
using Domain.Simulations.NightShift;
using Domain.Tests.Utils.Extensions;

using FluentAssertions;

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

    [Fact]
    public void Text_Update_WithMarker_ShouldThrow()
    {
        var template = new NightShiftTemplateEntityBuilder().Build();
        var text = CreateText(template.Guid, "Name");

        var act = () => text.Update(
            null,
            "{{marker}}",
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

        act.ThrowsException(NightShiftErrorCodes.TemplateTextContainsMarker);
    }

    [Fact]
    public void Text_Update_ShouldAssignAllFields()
    {
        var template = new NightShiftTemplateEntityBuilder().Build();
        var text = CreateText(template.Guid, "Name");

        text.Update(
            null,
            "New name",
            "New situation",
            "New emotion",
            "New learning goal",
            "New opener",
            "New born",
            "New gender",
            "New admission",
            "New diagnoses",
            "New allergies",
            "New medication",
            "New care level",
            "New risks",
            "New resuscitation",
            "New relatives");

        text.Name.Should().Be("New name");
        text.Situation.Should().Be("New situation");
        text.Emotion.Should().Be("New emotion");
        text.LearningGoal.Should().Be("New learning goal");
        text.Opener.Should().Be("New opener");
        text.Born.Should().Be("New born");
        text.Gender.Should().Be("New gender");
        text.Admission.Should().Be("New admission");
        text.Diagnoses.Should().Be("New diagnoses");
        text.Allergies.Should().Be("New allergies");
        text.Medication.Should().Be("New medication");
        text.CareLevel.Should().Be("New care level");
        text.Risks.Should().Be("New risks");
        text.Resuscitation.Should().Be("New resuscitation");
        text.Relatives.Should().Be("New relatives");
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
