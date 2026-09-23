using Domain.Simulations.NightShift;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys.Simulations.NightShiftMessageEntityConfig;

internal static class NightShiftMessageEntityIndexExtension
{
    internal static void AddIndexes(this EntityTypeBuilder<NightShiftMessageEntity> builder)
    {
        builder.HasIndex(message => new { message.SessionGuid, message.SortOrder })
            .IsUnique()
            .HasDatabaseName("idx_night_shift_messages_session_sort");
    }
}
