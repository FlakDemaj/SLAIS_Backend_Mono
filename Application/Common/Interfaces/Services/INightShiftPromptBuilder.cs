using Application.Simulations.NightShift.Cases;

using Domain.Common.Enums;

namespace Application.Common.Interfaces.Services;

public interface INightShiftPromptBuilder
{
    string Version { get; }

    string BuildPatientPrompt(PatientCase patientCase, Language language);

    string BuildFeedbackPrompt(PatientCase patientCase, Language language);

    string BuildGeneratorPrompt(Language language);
}
