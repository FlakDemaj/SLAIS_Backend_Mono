namespace Infrastructure.Configurations;

public class NightShiftOptions
{
    public required NightShiftOpenAiOptions OpenAi { get; init; }
}

public class NightShiftOpenAiOptions
{
    public required string ApiKey { get; init; }

    public string Model { get; init; } = "gpt-4o-mini";

    public string BaseUrl { get; init; } = "https://api.openai.com/v1/";
}
