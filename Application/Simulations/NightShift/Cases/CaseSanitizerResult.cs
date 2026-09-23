namespace Application.Simulations.NightShift.Cases;

public class CaseSanitizerResult
{
    public required PatientCase Case { get; init; }

    public bool UsedFallback { get; init; }
}
