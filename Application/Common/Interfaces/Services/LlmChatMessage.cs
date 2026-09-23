namespace Application.Common.Interfaces.Services;

public class LlmChatMessage
{
    public string Role { get; }

    public string Content { get; }

    public LlmChatMessage(string role, string content)
    {
        Role = role;
        Content = content;
    }
}
