using Domain.Simulations.NightShift;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys.Simulations.NightShiftTemplateTextEntityConfig;

internal static class NightShiftTemplateTextEntityIndexExtension
{
    internal static void AddIndexes(this EntityTypeBuilder<NightShiftTemplateTextEntity> builder)
    {
        builder
            .HasIndex(text => new { text.TemplateGuid, text.Language })
            .IsUnique()
            .HasDatabaseName("idx_night_shift_template_texts_template_language");
    }
}
