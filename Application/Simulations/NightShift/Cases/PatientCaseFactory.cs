using Domain.Common.Enums;
using Domain.Simulations.NightShift;

namespace Application.Simulations.NightShift.Cases;

public static class PatientCaseFactory
{
    public static PatientCaseResolution FromTemplate(
        NightShiftTemplateEntity template,
        Language language)
    {
        var resolution = template.GetTextOrFallback(language);
        var text = resolution.Text;

        return new PatientCaseResolution
        {
            Case = new PatientCase
            {
                Key = template.Key,
                Name = text.Name,
                Situation = text.Situation,
                Emotion = text.Emotion,
                LearningGoal = text.LearningGoal,
                TensionStart = template.TensionStart,
                Opener = text.Opener,
                Record = new PatientRecord
                {
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
                }
            },
            IsLanguageFallback = resolution.IsFallback
        };
    }
}
