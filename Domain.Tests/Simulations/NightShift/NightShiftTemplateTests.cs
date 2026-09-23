using Domain.Common.Enums;
using Domain.Simulations.NightShift;
using Domain.Tests.Utils.Extensions;

using FluentAssertions;

using Tests.Domain.Shared.Builders;

using Xunit;

namespace Domain.Tests.Simulations.NightShift;

public class NightShiftTemplateTests
{
    [Fact]
    public void CreateDraft_WithInvalidKey_ShouldThrowTemplateKeyInvalid()
    {
        var act = () => new NightShiftTemplateEntityBuilder()
            .WithKey("Invalid Key")
            .Build();

        act.ThrowsException(NightShiftErrorCodes.TemplateKeyInvalid);
    }

    [Fact]
    public void Activate_WithoutEnglish_ShouldThrowActivationNeedsBothLanguages()
    {
        var template = new NightShiftTemplateEntityBuilder().Build();

        var act = () => template.Activate(null);

        act.ThrowsException(NightShiftErrorCodes.TemplateActivationNeedsBothLanguages);
    }

    [Fact]
    public void Activate_WithBothLanguages_ShouldSetActive()
    {
        var template = new NightShiftTemplateEntityBuilder()
            .WithEnglishText()
            .Build();

        template.Activate(null);

        template.State.Should().Be(States.Active);
    }

    [Fact]
    public void AddOrReplaceText_SameLanguage_ShouldReplace()
    {
        var template = new NightShiftTemplateEntityBuilder().Build();
        var replacement = CreateText(template.Guid, Language.German, "Ersatz");

        template.AddOrReplaceText(replacement);

        template.Texts.Should().ContainSingle();
        template.GetText(Language.German).Should().BeSameAs(replacement);
    }

    [Fact]
    public void GetTextOrFallback_MissingEnglish_ShouldReturnGermanWithFallbackFlag()
    {
        var template = new NightShiftTemplateEntityBuilder().Build();

        var resolution = template.GetTextOrFallback(Language.English);

        resolution.Text.Language.Should().Be(Language.German);
        resolution.IsFallback.Should().BeTrue();
    }

    private static NightShiftTemplateTextEntity CreateText(
        Guid templateGuid,
        Language language,
        string name)
    {
        return NightShiftTemplateTextEntity.Create(
            null,
            templateGuid,
            language,
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
