using Domain.Simulations.NightShift;

using Infrastructure.Persistence.EntityConfigurations.Base;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys.Simulations.NightShiftSessionEntityConfig;

internal sealed class NightShiftSessionEntityAttributeConfig : BaseCreatedByEntityConfig<NightShiftSessionEntity>
{
    private string Table { get; }

    public NightShiftSessionEntityAttributeConfig()
    {
        Table = "night_shift_sessions";
        _schema = "simulation";
        _prefix = "night_shift_session_";
    }

    public override void Configure(EntityTypeBuilder<NightShiftSessionEntity> builder)
    {
        builder.ToTable(Table, _schema);

        base.Configure(builder);

        builder.Property(session => session.UserGuid).HasColumnName("fk_user_guid").IsRequired();
        builder.Property(session => session.Language).HasColumnName("language").HasColumnType("smallint").IsRequired();
        builder.Property(session => session.CaseKey).HasColumnName("case_key").IsRequired();
        builder.Property(session => session.CaseName).HasColumnName("case_name").IsRequired();
        builder.Property(session => session.CaseSituation).HasColumnName("case_situation").IsRequired();
        builder.Property(session => session.CaseEmotion).HasColumnName("case_emotion").IsRequired();
        builder.Property(session => session.CaseLearningGoal).HasColumnName("case_learning_goal").IsRequired();
        builder.Property(session => session.CaseOpener).HasColumnName("case_opener").IsRequired();
        builder.Property(session => session.CaseTensionStart).HasColumnName("case_tension_start").HasColumnType("smallint").IsRequired();
        builder.Property(session => session.RecordBorn).HasColumnName("record_born").IsRequired();
        builder.Property(session => session.RecordGender).HasColumnName("record_gender").IsRequired();
        builder.Property(session => session.RecordAdmission).HasColumnName("record_admission").IsRequired();
        builder.Property(session => session.RecordDiagnoses).HasColumnName("record_diagnoses").IsRequired();
        builder.Property(session => session.RecordAllergies).HasColumnName("record_allergies").IsRequired();
        builder.Property(session => session.RecordMedication).HasColumnName("record_medication").IsRequired();
        builder.Property(session => session.RecordCareLevel).HasColumnName("record_care_level").IsRequired();
        builder.Property(session => session.RecordRisks).HasColumnName("record_risks").IsRequired();
        builder.Property(session => session.RecordResuscitation).HasColumnName("record_resuscitation").IsRequired();
        builder.Property(session => session.RecordRelatives).HasColumnName("record_relatives").IsRequired();
        builder.Property(session => session.PromptVersion).HasColumnName("prompt_version").IsRequired();
        builder.Property(session => session.Model).HasColumnName("model").IsRequired();
        builder.Property(session => session.StartedAt).HasColumnName("started_at").IsRequired();
        builder.Property(session => session.EndedAt).HasColumnName("ended_at");
        builder.Property(session => session.FinishedAt).HasColumnName("finished_at");
        builder.Property(session => session.Ended).HasColumnName("ended").IsRequired().HasDefaultValue(false);

        builder.AddForeignKeys();
        builder.AddIndexes();
    }
}
