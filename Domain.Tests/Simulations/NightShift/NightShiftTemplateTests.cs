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

    [Fact]
    public void Version_ShouldIncrementOnEveryChange()
    {
        var template = NightShiftTemplateEntity.CreateDraft(null, "keller", 8, 10);

        template.Version.Should().Be(1);

        template.Texts.Add(CreateText(template.Guid, Language.English, "Mr Keller"));
        template.AddOrReplaceText(CreateText(template.Guid, Language.German, "Herr Keller"));

        template.Version.Should().Be(2);

        template.Activate(null);

        template.Version.Should().Be(3);

        template.Archive(null);

        template.Version.Should().Be(4);

        template.Reopen(null);

        template.Version.Should().Be(5);

        template.MarkDeleted(Guid.Empty);

        template.Version.Should().Be(6);
    }

    [Fact]
    public void Activate_FromDeleted_ShouldThrowTemplateStateInvalid()
    {
        var template = new NightShiftTemplateEntityBuilder()
            .WithEnglishText()
            .WithState(States.Deleted)
            .Build();

        var act = () => template.Activate(null);

        act.ThrowsException(NightShiftErrorCodes.TemplateStateInvalid);
    }

    [Fact]
    public void Reopen_FromArchived_ShouldSetPending()
    {
        var template = new NightShiftTemplateEntityBuilder()
            .WithState(States.Deactived)
            .Build();

        template.Reopen(null);

        template.State.Should().Be(States.Pending);
    }

    [Fact]
    public void Archive_FromDeleted_ShouldThrowTemplateStateInvalid()
    {
        var template = new NightShiftTemplateEntityBuilder()
            .WithState(States.Deleted)
            .Build();

        var act = () => template.Archive(null);

        act.ThrowsException(NightShiftErrorCodes.TemplateStateInvalid);
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
