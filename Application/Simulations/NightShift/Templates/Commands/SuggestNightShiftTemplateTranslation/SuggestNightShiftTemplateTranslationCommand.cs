using Application.Common.DTOs.Simulations.NightShift;
using Application.Utils.Interfaces.Mediator;
using Application.Utils.Interfaces.Transaction;

namespace Application.Simulations.NightShift.Templates.Commands.SuggestNightShiftTemplateTranslation;

public class SuggestNightShiftTemplateTranslationCommand : IRequest<NightShiftTemplateTextDto>, INoTransaction
{
    public required string SourceLanguage { get; init; }
    public required string TargetLanguage { get; init; }
    public required NightShiftTemplateTextDto Text { get; init; }
}
