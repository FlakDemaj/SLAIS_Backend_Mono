using Domain.Simulations.NightShift;

using Infrastructure.Persistence.EntityConfigurations.Base;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations.Entitys.Simulations.NightShiftFeedbackEntityConfig;

internal sealed class NightShiftFeedbackEntityAttributeConfig : BaseGuidEntityConfig<NightShiftFeedbackEntity>
{
    private string Table { get; }

    public NightShiftFeedbackEntityAttributeConfig()
    {
        Table = "night_shift_feedbacks";
        _schema = "simulation";
        _prefix = "night_shift_feedback_";
    }

    public override void Configure(EntityTypeBuilder<NightShiftFeedbackEntity> builder)
    {
        builder.ToTable(Table, _schema);

        base.Configure(builder);

        builder.Property(feedback => feedback.SessionGuid).HasColumnName("fk_night_shift_session_guid").IsRequired();

        builder.Property(feedback => feedback.Ok).HasColumnName("ok").IsRequired();

        builder.Property(feedback => feedback.ScoreProfessional).HasColumnName("score_professional").HasColumnType("smallint").IsRequired();

        builder.Property(feedback => feedback.ScoreRapport).HasColumnName("score_rapport").HasColumnType("smallint").IsRequired();

        builder.Property(feedback => feedback.ScoreEmpathy).HasColumnName("score_empathy").HasColumnType("smallint").IsRequired();

        builder.Property(feedback => feedback.ScoreListening).HasColumnName("score_listening").HasColumnType("smallint").IsRequired();

        builder.Property(feedback => feedback.ScoreClarity).HasColumnName("score_clarity").HasColumnType("smallint").IsRequired();

        builder.Property(feedback => feedback.TextProfessional).HasColumnName("text_professional").IsRequired();

        builder.Property(feedback => feedback.TextRapport).HasColumnName("text_rapport").IsRequired();

        builder.Property(feedback => feedback.TextEmpathy).HasColumnName("text_empathy").IsRequired();

        builder.Property(feedback => feedback.TextListening).HasColumnName("text_listening").IsRequired();

        builder.Property(feedback => feedback.TextClarity).HasColumnName("text_clarity").IsRequired();

        builder.Property(feedback => feedback.Summary).HasColumnName("summary").IsRequired();

        builder.Property(feedback => feedback.Model).HasColumnName("model").IsRequired();

        builder.Property(feedback => feedback.PromptVersion).HasColumnName("prompt_version").IsRequired();

        builder.Property(feedback => feedback.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.AddForeignKeys();
        builder.AddIndexes();
    }

}
