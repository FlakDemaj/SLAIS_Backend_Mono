using Domain.Simulations.NightShift;

using Infrastructure.Persistence.EntityConfigurations.Base;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys.Simulations.NightShiftTemplateTextEntityConfig;

internal sealed class NightShiftTemplateTextEntityAttributeConfig : BaseUpdatedByEntityConfig<NightShiftTemplateTextEntity>
{
    private string Table { get; }

    public NightShiftTemplateTextEntityAttributeConfig()
    {
        Table = "night_shift_template_texts";
        _schema = "simulation";
        _prefix = "night_shift_template_text_";
    }

    public override void Configure(EntityTypeBuilder<NightShiftTemplateTextEntity> builder)
    {
        builder.ToTable(Table, _schema);

        base.Configure(builder);

        builder
            .Property(text => text.TemplateGuid)
            .HasColumnName("fk_night_shift_template_guid")
            .IsRequired();

        builder
            .Property(text => text.Language)
            .HasColumnName("language")
            .HasColumnType("smallint")
            .IsRequired();

        builder
            .Property(text => text.Name)
            .HasColumnName("name")
            .IsRequired();

        builder
            .Property(text => text.Situation)
            .HasColumnName("situation")
            .IsRequired();

        builder
            .Property(text => text.Emotion)
            .HasColumnName("emotion")
            .IsRequired();

        builder
            .Property(text => text.LearningGoal)
            .HasColumnName("learning_goal")
            .IsRequired();

        builder
            .Property(text => text.Opener)
            .HasColumnName("opener")
            .IsRequired();

        builder
            .Property(text => text.Born)
            .HasColumnName("born")
            .IsRequired();

        builder
            .Property(text => text.Gender)
            .HasColumnName("gender")
            .IsRequired();

        builder
            .Property(text => text.Admission)
            .HasColumnName("admission")
            .IsRequired();

        builder
            .Property(text => text.Diagnoses)
            .HasColumnName("diagnoses")
            .IsRequired();

        builder
            .Property(text => text.Allergies)
            .HasColumnName("allergies")
            .IsRequired();

        builder
            .Property(text => text.Medication)
            .HasColumnName("medication")
            .IsRequired();

        builder
            .Property(text => text.CareLevel)
            .HasColumnName("care_level")
            .IsRequired();

        builder
            .Property(text => text.Risks)
            .HasColumnName("risks")
            .IsRequired();

        builder
            .Property(text => text.Resuscitation)
            .HasColumnName("resuscitation")
            .IsRequired();

        builder
            .Property(text => text.Relatives)
            .HasColumnName("relatives")
            .IsRequired();

        builder.AddIndexes();
    }
}
