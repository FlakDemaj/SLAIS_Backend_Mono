using Application.Common.Interfaces.Services;
using Application.Simulations.NightShift;

using Domain.Common.Exceptions;

namespace Integration.Tests.Common;

public class FakeLlmClient : ILlmClient
{
    public string Model { get; } = "fake-model";

    public int CallCount { get; private set; }

    public string FeedbackMode { get; set; } = "valid";

    public bool ThrowUnavailable { get; set; }

    public Task<string> ChatAsync(
        IReadOnlyList<LlmChatMessage> messages,
        double temperature,
        int maxTokens,
        bool jsonMode,
        string tag,
        CancellationToken cancellationToken)
    {
        CallCount++;

        if (ThrowUnavailable)
        {
            throw new SlaisException(NightShiftErrorCodes.LlmUnavailable);
        }

        if (messages[^1].Content.Contains("Tschuess", StringComparison.Ordinal))
        {
            return Task.FromResult("Dann gehen Sie. [[ENDE]]");
        }

        if (jsonMode && messages[0].Content.Contains("Bewerte", StringComparison.Ordinal))
        {
            if (FeedbackMode == "malformed")
            {
                return Task.FromResult("not json");
            }

            return Task.FromResult("{\"scores\":{\"fachlich\":7,\"sympathie\":6,\"empathie\":8,\"zuhoeren\":5,\"klarheit\":6},\"per_dimension\":{\"fachlich\":\"ok\",\"sympathie\":\"ok\",\"empathie\":\"ok\",\"zuhoeren\":\"ok\",\"klarheit\":\"ok\"},\"summary\":\"Testfeedback\"}");
        }

        if (jsonMode && messages[0].Content.Contains("Fachuebersetzer", StringComparison.Ordinal))
        {
            return Task.FromResult("{\"name\":\"Mr Keller, 58\",\"situation\":\"English situation\",\"emotion\":\"worried\",\"learning_goal\":\"English learning goal\",\"opener\":\"Please help me.\",\"born\":\"14/03/1968\",\"gender\":\"male\",\"admission\":\"English admission\",\"diagnoses\":\"English diagnoses\",\"allergies\":\"none\",\"medication\":\"English medication\",\"care_level\":\"2\",\"risks\":\"English risks\",\"resuscitation\":\"yes\",\"relatives\":\"English relatives\"}");
        }

        if (jsonMode && messages[0].Content.Contains("Generator", StringComparison.Ordinal))
        {
            return Task.FromResult("{\"name\":\"Frau Test, 70\",\"situation\":\"Testsituation\",\"emotion\":\"angespannt\",\"lernziel\":\"Deeskalation ueben.\",\"anspannung_start\":5,\"opener\":\"Bitte helfen Sie mir.\",\"stammblatt\":{\"geboren\":\"01.01.1956\",\"geschlecht\":\"weiblich\",\"aufnahme\":\"Station Test\",\"diagnosen\":\"Testdiagnose\",\"allergien\":\"keine\",\"medikation\":\"keine\",\"pflegegrad\":\"0\",\"risiken\":\"keine\",\"reanimation\":\"Ja\",\"angehoerige\":\"keine\"}}");
        }

        return Task.FromResult("Was wollen Sie?");
    }
}
