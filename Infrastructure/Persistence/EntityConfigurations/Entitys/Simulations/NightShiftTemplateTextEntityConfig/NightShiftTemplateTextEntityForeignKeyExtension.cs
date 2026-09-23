using Domain.Simulations.NightShift;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys.Simulations.NightShiftTemplateTextEntityConfig;

internal static class NightShiftTemplateTextEntityForeignKeyExtension
{
    internal static void AddForeignKeys(this EntityTypeBuilder<NightShiftTemplateTextEntity> builder)
    {
    }
}
