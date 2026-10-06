using System.Text.Json.Nodes;

namespace Receipts.OpenAi;

/// <summary>
/// Content parts of the Chat Completions API; images go as data URLs.
/// </summary>
public sealed class OpenAiFormat : IPartFormat
{
    public JsonNode Text(string text)
    {
        return new JsonObject { ["type"] = "text", ["text"] = text };
    }

    public JsonNode Image(string mime, string base64)
    {
        return new JsonObject
        {
            ["type"] = "image_url",
            ["image_url"] = new JsonObject { ["url"] = $"data:{mime};base64,{base64}" },
        };
    }
}
