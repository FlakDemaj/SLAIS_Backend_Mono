using Application.Common.DTOs.Simulations.NightShift;

using Domain.Common.Enums;
using Domain.Simulations.NightShift;

namespace Application.Simulations.NightShift.Templates;

public static class NightShiftTemplateResponseFactory
{
    public static GetNightShiftTemplateResponseDto ToResponse(NightShiftTemplateEntity template)
    {
        return new GetNightShiftTemplateResponseDto
        {
            TemplateId = template.Guid,
            Key = template.Key,
            State = ToState(template.State),
            TensionStart = template.TensionStart,
            SortOrder = template.SortOrder,
            Version = template.Version,
            CreatedAt = template.CreatedDate,
            UpdatedAt = template.UpdateDate,
            Texts = template.Texts.ToDictionary(
                text => NightShiftLanguage.ToCode(text.Language),
                ToTextDto)
        };
    }

    public static NightShiftTemplateTextDto ToTextDto(NightShiftTemplateTextEntity text)
    {
        return new NightShiftTemplateTextDto
        {
            Name = text.Name,
            Situation = text.Situation,
            Emotion = text.Emotion,
            LearningGoal = text.LearningGoal,
            Opener = text.Opener,
            Born = text.Born,
            Gender = text.Gender,
            Admission = text.Admission,
            Diagnoses = text.Diagnoses,
            Allergies = text.Allergies,
            Medication = text.Medication,
            CareLevel = text.CareLevel,
            Risks = text.Risks,
            Resuscitation = text.Resuscitation,
            Relatives = text.Relatives
        };
    }

    private static string ToState(States state)
    {
        return state switch
        {
            States.Pending => "pending",
            States.Active => "active",
            States.Deactived => "archived",
            _ => throw new InvalidOperationException("Unsupported template state.")
        };
    }
}
