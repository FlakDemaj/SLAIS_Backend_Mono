using Domain.Simulations.NightShift;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys.Simulations.NightShiftFeedbackEntityConfig;

internal static class NightShiftFeedbackEntityForeignKeyExtension
{
    internal static void AddForeignKeys(this EntityTypeBuilder<NightShiftFeedbackEntity> builder)
    {
        builder.HasOne(feedback => feedback.Session)
            .WithOne(session => session.Feedback)
            .HasForeignKey<NightShiftFeedbackEntity>(feedback => feedback.SessionGuid)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
