using Domain.Simulations.NightShift;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys.Simulations.NightShiftFeedbackEntityConfig;

internal static class NightShiftFeedbackEntityIndexExtension
{
    internal static void AddIndexes(this EntityTypeBuilder<NightShiftFeedbackEntity> builder)
    {
        builder.HasIndex(feedback => feedback.SessionGuid)
            .IsUnique()
            .HasDatabaseName("idx_night_shift_feedbacks_session");
    }
}
