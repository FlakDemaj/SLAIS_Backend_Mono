using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Application.Common.Interfaces.Services;
using Application.Simulations.NightShift;
using Application.Utils.Logger;

using Domain.Common.Exceptions;

using Infrastructure.Configurations;

using Microsoft.Extensions.Options;

namespace Infrastructure.InternalServices.NightShift;

public class OpenAiLlmClient : ILlmClient
{
    private readonly HttpClient _httpClient;

    private readonly NightShiftOpenAiOptions _options;

    private readonly ISlaisLogger<OpenAiLlmClient> _logger;

    public string Model
    {
        get
        {
            return _options.Model;
        }
    }

    public OpenAiLlmClient(
        HttpClient httpClient,
        IOptions<NightShiftOptions> options,
        ISlaisLogger<OpenAiLlmClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value.OpenAi;
        _logger = logger;
    }

    public async Task<string> ChatAsync(
        IReadOnlyList<LlmChatMessage> messages,
        double temperature,
        int maxTokens,
        bool jsonMode,
        string tag,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");

                if (jsonMode)
                {
                    request.Content = JsonContent.Create(new
                    {
                        model = Model,
                        messages = messages.Select(message => new { role = message.Role, content = message.Content }),
                        temperature,
                        max_tokens = maxTokens,
                        response_format = new { type = "json_object" }
                    });
                }
                else
                {
                    request.Content = JsonContent.Create(new
                    {
                        model = Model,
                        messages = messages.Select(message => new { role = message.Role, content = message.Content }),
                        temperature,
                        max_tokens = maxTokens
                    });
                }

                using var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var reason = "OpenAI returned status " + response.StatusCode + ".";
                    if (response.StatusCode != HttpStatusCode.TooManyRequests && (int)response.StatusCode < 500)
                    {
                        LogUnavailable(tag, reason);
                        throw new SlaisException(NightShiftErrorCodes.LlmUnavailable);
                    }

                    throw new HttpRequestException(reason);
                }

                using var document = JsonDocument.Parse(
                    await response.Content.ReadAsStringAsync(cancellationToken));
                var content = document.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(content))
                {
                    LogUnavailable(tag, "OpenAI returned no message content.");
                    throw new SlaisException(NightShiftErrorCodes.LlmUnavailable);
                }

                return content.Trim();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (SlaisException)
            {
                throw;
            }
            catch (HttpRequestException exception)
            {
                if (attempt == 1)
                {
                    LogUnavailable(tag, exception.Message);
                    throw new SlaisException(NightShiftErrorCodes.LlmUnavailable);
                }
            }
            catch (TaskCanceledException exception)
            {
                if (attempt == 1)
                {
                    LogUnavailable(tag, exception.Message);
                    throw new SlaisException(NightShiftErrorCodes.LlmUnavailable);
                }
            }
            catch (JsonException exception)
            {
                LogUnavailable(tag, exception.Message);
                throw new SlaisException(NightShiftErrorCodes.LlmUnavailable);
            }
            catch (KeyNotFoundException exception)
            {
                LogUnavailable(tag, exception.Message);
                throw new SlaisException(NightShiftErrorCodes.LlmUnavailable);
            }
            catch (IndexOutOfRangeException exception)
            {
                LogUnavailable(tag, exception.Message);
                throw new SlaisException(NightShiftErrorCodes.LlmUnavailable);
            }
            catch (InvalidOperationException exception)
            {
                LogUnavailable(tag, exception.Message);
                throw new SlaisException(NightShiftErrorCodes.LlmUnavailable);
            }

            if (attempt == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }

        throw new SlaisException(NightShiftErrorCodes.LlmUnavailable);
    }

    private void LogUnavailable(string tag, string reason)
    {
        _logger.LogError("NightShift LLM unavailable for " + tag + ": " + reason, null);
    }
}
