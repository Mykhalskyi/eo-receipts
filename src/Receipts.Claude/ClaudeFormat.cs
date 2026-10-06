using System.Text.Json.Nodes;

namespace Receipts.Claude;

/// <summary>
/// Content blocks of the Messages API.
/// </summary>
public sealed class ClaudeFormat : IPartFormat
{
    public JsonNode Text(string text)
    {
        return new JsonObject { ["type"] = "text", ["text"] = text };
    }

    public JsonNode Image(string mime, string base64)
    {
        return new JsonObject
        {
            ["type"] = "image",
            ["source"] = new JsonObject { ["type"] = "base64", ["media_type"] = mime, ["data"] = base64 },
        };
    }
}
