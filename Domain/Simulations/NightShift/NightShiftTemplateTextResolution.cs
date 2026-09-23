namespace Domain.Simulations.NightShift;

public class NightShiftTemplateTextResolution
{
    public NightShiftTemplateTextEntity Text { get; }

    public bool IsFallback { get; }

    public NightShiftTemplateTextResolution(
        NightShiftTemplateTextEntity text,
        bool isFallback)
    {
        Text = text;
        IsFallback = isFallback;
    }
}
