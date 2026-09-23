using Domain.Simulations.NightShift;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys.Simulations.NightShiftMessageEntityConfig;

internal static class NightShiftMessageEntityForeignKeyExtension
{
    internal static void AddForeignKeys(this EntityTypeBuilder<NightShiftMessageEntity> builder)
    {
        builder.HasOne(message => message.Session)
            .WithMany(session => session.Messages)
            .HasForeignKey(message => message.SessionGuid)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
