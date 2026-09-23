using Application.Common.Interfaces.Services;

namespace Integration.Tests.Common;

public class FakeLlmClient : ILlmClient
{
    public string Model { get; } = "fake-model";

    public int CallCount { get; private set; }

    public Task<string> ChatAsync(
        IReadOnlyList<LlmChatMessage> messages,
        double temperature,
        int maxTokens,
        bool jsonMode,
        string tag,
        CancellationToken cancellationToken)
    {
        CallCount++;

        if (messages[^1].Content.Contains("Tschuess", StringComparison.Ordinal))
        {
            return Task.FromResult("Dann gehen Sie. [[ENDE]]");
        }

        if (jsonMode && messages[0].Content.Contains("Bewerte", StringComparison.Ordinal))
        {
            return Task.FromResult("{\"scores\":{\"fachlich\":7,\"sympathie\":6,\"empathie\":8,\"zuhoeren\":5,\"klarheit\":6},\"per_dimension\":{\"fachlich\":\"ok\",\"sympathie\":\"ok\",\"empathie\":\"ok\",\"zuhoeren\":\"ok\",\"klarheit\":\"ok\"},\"summary\":\"Testfeedback\"}");
        }

        return Task.FromResult("Was wollen Sie?");
    }
}
