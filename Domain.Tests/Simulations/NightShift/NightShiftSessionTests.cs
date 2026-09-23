using Domain.Simulations.NightShift;
using Domain.Tests.Utils.Extensions;

using FluentAssertions;

using Tests.Domain.Shared.Builders;

using Xunit;

namespace Domain.Tests.Simulations.NightShift;

public class NightShiftSessionTests
{
    [Fact]
    public void Create_WithEmptyCaseKey_ShouldThrowInvalidInput()
    {
        var act = () => new NightShiftSessionEntityBuilder()
            .WithCaseKey(string.Empty)
            .Build();

        act.ThrowsException(NightShiftErrorCodes.InvalidInput);
    }

    [Fact]
    public void Create_WithTensionAbove10_ShouldThrowInvalidInput()
    {
        var act = () => new NightShiftSessionEntityBuilder()
            .WithCaseTensionStart(11)
            .Build();

        act.ThrowsException(NightShiftErrorCodes.InvalidInput);
    }

    [Fact]
    public void MarkEnded_Twice_ShouldKeepFirstEndedAt()
    {
        var session = new NightShiftSessionEntityBuilder().Build();

        session.MarkEnded();
        var endedAt = session.EndedAt;
        session.MarkEnded();

        session.EndedAt.Should().Be(endedAt);
    }

    [Fact]
    public void MarkFinished_ShouldAlsoMarkEnded()
    {
        var session = new NightShiftSessionEntityBuilder().Build();

        session.MarkFinished();

        session.Ended.Should().BeTrue();
        session.EndedAt.Should().NotBeNull();
        session.FinishedAt.Should().NotBeNull();
    }

    [Fact]
    public void EnsureCanChat_WhenEnded_ShouldThrowSessionAlreadyEnded()
    {
        var session = new NightShiftSessionEntityBuilder().Build();
        session.MarkEnded();

        var act = () => session.EnsureCanChat();

        act.ThrowsException(NightShiftErrorCodes.SessionAlreadyEnded);
    }
}
