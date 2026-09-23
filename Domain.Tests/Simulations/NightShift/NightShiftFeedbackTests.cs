using Domain.Common.Enums;
using Domain.Simulations.NightShift;
using Domain.Tests.Utils.Extensions;

using FluentAssertions;

using Xunit;

namespace Domain.Tests.Simulations.NightShift;

public class NightShiftFeedbackTests
{
    [Fact]
    public void Feedback_Create_WithScore11_ShouldThrowInvalidScore()
    {
        var act = () => NightShiftFeedbackEntity.Create(
            Guid.CreateVersion7(),
            true,
            11,
            0,
            0,
            0,
            0,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            "summary",
            "model",
            "v1");

        act.ThrowsException(NightShiftErrorCodes.InvalidScore);
    }

    [Fact]
    public void Feedback_NotOk_ShouldHaveZeroScores()
    {
        var feedback = NightShiftFeedbackEntity.NotOk(
            Guid.CreateVersion7(),
            "summary",
            "model",
            "v1");

        feedback.Ok.Should().BeFalse();
        feedback.ScoreProfessional.Should().Be(0);
        feedback.ScoreRapport.Should().Be(0);
        feedback.ScoreEmpathy.Should().Be(0);
        feedback.ScoreListening.Should().Be(0);
        feedback.ScoreClarity.Should().Be(0);
    }

    [Fact]
    public void Message_Create_WithNegativeSortOrder_ShouldThrowInvalidSortOrder()
    {
        var act = () => NightShiftMessageEntity.Create(
            Guid.CreateVersion7(),
            NightShiftMessageRole.Student,
            "Nachricht",
            -1);

        act.ThrowsException(NightShiftErrorCodes.InvalidSortOrder);
    }
}
