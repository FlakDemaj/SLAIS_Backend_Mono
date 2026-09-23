using Domain.Simulations.NightShift;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys.Simulations.NightShiftTemplateEntityConfig;

internal static class NightShiftTemplateEntityIndexExtension
{
    internal static void AddIndexes(this EntityTypeBuilder<NightShiftTemplateEntity> builder)
    {
        builder
            .HasIndex(template => template.Key)
            .IsUnique()
            .HasFilter("state <> 3")
            .HasDatabaseName("idx_night_shift_templates_key");

        builder
            .HasIndex(template => new { template.State, template.SortOrder })
            .HasDatabaseName("idx_night_shift_templates_state_sort_order");

        builder
            .HasIndex(template => template.CreatedByUserGuid)
            .HasDatabaseName("idx_night_shift_templates_created_by");

        builder
            .HasIndex(template => template.UpdatedByUserGuid)
            .HasDatabaseName("idx_night_shift_templates_updated_by");

        builder
            .HasIndex(template => template.DeletedByUserGuid)
            .HasDatabaseName("idx_night_shift_templates_deleted_by");
    }
}
