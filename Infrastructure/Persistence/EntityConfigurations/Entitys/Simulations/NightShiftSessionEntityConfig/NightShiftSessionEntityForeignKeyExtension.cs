using Domain.Simulations.NightShift;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys.Simulations.NightShiftSessionEntityConfig;

internal static class NightShiftSessionEntityForeignKeyExtension
{
    internal static void AddForeignKeys(this EntityTypeBuilder<NightShiftSessionEntity> builder)
    {
        builder.HasOne(session => session.User)
            .WithMany()
            .HasForeignKey(session => session.UserGuid)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(session => session.Messages)
            .WithOne(message => message.Session)
            .HasForeignKey(message => message.SessionGuid)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(session => session.Feedback)
            .WithOne(feedback => feedback.Session)
            .HasForeignKey<NightShiftFeedbackEntity>(feedback => feedback.SessionGuid)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
