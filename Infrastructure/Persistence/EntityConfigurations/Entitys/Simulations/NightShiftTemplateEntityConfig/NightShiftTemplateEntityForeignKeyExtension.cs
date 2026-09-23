using Domain.Simulations.NightShift;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys.Simulations.NightShiftTemplateEntityConfig;

internal static class NightShiftTemplateEntityForeignKeyExtension
{
    internal static void AddForeignKeys(this EntityTypeBuilder<NightShiftTemplateEntity> builder)
    {
        builder
            .HasMany(template => template.Texts)
            .WithOne(text => text.Template)
            .HasForeignKey(text => text.TemplateGuid)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
