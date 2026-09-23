using Domain.Simulations.NightShift;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys.Simulations.NightShiftSessionEntityConfig;

internal static class NightShiftSessionEntityIndexExtension
{
    internal static void AddIndexes(this EntityTypeBuilder<NightShiftSessionEntity> builder)
    {
        builder.HasIndex(session => new { session.UserGuid, session.StartedAt })
            .IsDescending(false, true)
            .HasDatabaseName("idx_night_shift_sessions_fk_user_guid_started_at");
        builder.HasIndex(session => session.CreatedByUserGuid).HasDatabaseName("idx_night_shift_sessions_created_by");

        builder
            .HasIndex(session => session.TemplateGuid)
            .HasDatabaseName("idx_night_shift_sessions_fk_night_shift_template_guid");
    }
}
