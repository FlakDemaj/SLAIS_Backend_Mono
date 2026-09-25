namespace Application.Common.DTOs.Simulations.NightShift;

public class GetNightShiftTemplateResponseDto
{
    public Guid TemplateId { get; init; }
    public required string Key { get; init; }
    public required string State { get; init; }
    public short TensionStart { get; init; }
    public int SortOrder { get; init; }
    public int Version { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public required Dictionary<string, NightShiftTemplateTextDto> Texts { get; init; }
}
