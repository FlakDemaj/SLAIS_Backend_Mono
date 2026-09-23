namespace Application.Common.Interfaces.Services;

public interface ILlmClient
{
    string Model { get; }

    // Implementations throw SlaisException(NightShiftErrorCodes.LlmUnavailable) on transport failure.
    Task<string> ChatAsync(IReadOnlyList<LlmChatMessage> messages, double temperature, int maxTokens, bool jsonMode, string tag, CancellationToken cancellationToken);
}
