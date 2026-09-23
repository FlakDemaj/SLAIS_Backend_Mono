namespace Application.Simulations.NightShift.Cases;

public class PatientCaseResolution
{
    public required PatientCase Case { get; init; }

    public bool IsLanguageFallback { get; init; }
}
