using Domain.Simulations.NightShift;

using Infrastructure.Persistence.EntityConfigurations.Base;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys.Simulations.NightShiftTemplateEntityConfig;

internal sealed class NightShiftTemplateEntityAttributeConfig : BaseDeletedByEntityConfig<NightShiftTemplateEntity>
{
    private string Table { get; }

    public NightShiftTemplateEntityAttributeConfig()
    {
        Table = "night_shift_templates";
        _schema = "simulation";
        _prefix = "night_shift_template_";
    }

    public override void Configure(EntityTypeBuilder<NightShiftTemplateEntity> builder)
    {
        builder.ToTable(Table, _schema);

        base.Configure(builder);

        builder
            .Property(template => template.Key)
            .HasColumnName("key")
            .IsRequired();

        builder
            .Property(template => template.State)
            .HasColumnName("state")
            .HasColumnType("smallint")
            .IsRequired();

        builder
            .Property(template => template.TensionStart)
            .HasColumnName("tension_start")
            .HasColumnType("smallint")
            .IsRequired();

        builder
            .Property(template => template.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder
            .Property(template => template.Version)
            .HasColumnName("version")
            .IsRequired()
            .HasDefaultValue(1)
            .IsConcurrencyToken();

        builder.AddForeignKeys();
        builder.AddIndexes();
    }
}
