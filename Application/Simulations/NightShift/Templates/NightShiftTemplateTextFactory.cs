using Application.Common.DTOs.Simulations.NightShift;

using Domain.Common.Enums;
using Domain.Simulations.NightShift;

namespace Application.Simulations.NightShift.Templates;

internal static class NightShiftTemplateTextFactory
{
    public static NightShiftTemplateTextEntity Create(
        Guid? userGuid,
        Guid templateGuid,
        Language language,
        NightShiftTemplateTextDto text)
    {
        return NightShiftTemplateTextEntity.Create(
            userGuid,
            templateGuid,
            language,
            text.Name,
            text.Situation,
            text.Emotion,
            text.LearningGoal,
            text.Opener,
            text.Born,
            text.Gender,
            text.Admission,
            text.Diagnoses,
            text.Allergies,
            text.Medication,
            text.CareLevel,
            text.Risks,
            text.Resuscitation,
            text.Relatives);
    }

    public static void Update(
        NightShiftTemplateTextEntity entity,
        Guid? userGuid,
        NightShiftTemplateTextDto text)
    {
        entity.Update(
            userGuid,
            text.Name,
            text.Situation,
            text.Emotion,
            text.LearningGoal,
            text.Opener,
            text.Born,
            text.Gender,
            text.Admission,
            text.Diagnoses,
            text.Allergies,
            text.Medication,
            text.CareLevel,
            text.Risks,
            text.Resuscitation,
            text.Relatives);
    }
}
