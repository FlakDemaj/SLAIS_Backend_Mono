using Domain.Simulations.NightShift;

using Infrastructure.Persistence.EntityConfigurations.Base;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys.Simulations.NightShiftMessageEntityConfig;

internal sealed class NightShiftMessageEntityAttributeConfig : BaseGuidEntityConfig<NightShiftMessageEntity>
{
    private string Table { get; }

    public NightShiftMessageEntityAttributeConfig()
    {
        Table = "night_shift_messages";
        _schema = "simulation";
        _prefix = "night_shift_message_";
    }

    public override void Configure(EntityTypeBuilder<NightShiftMessageEntity> builder)
    {
        builder.ToTable(Table, _schema);

        base.Configure(builder);

        builder
            .Property(message => message.SessionGuid)
            .HasColumnName("fk_night_shift_session_guid")
            .IsRequired();

        builder
            .Property(message => message.Role)
            .HasColumnName("role")
            .HasColumnType("smallint")
            .IsRequired();

        builder
            .Property(message => message.Content)
            .HasColumnName("content")
            .IsRequired();

        builder
            .Property(message => message.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder
            .Property(message => message.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.AddForeignKeys();
        builder.AddIndexes();
    }
}
