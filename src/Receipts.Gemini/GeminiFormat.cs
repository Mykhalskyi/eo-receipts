using System.Text.Json.Nodes;

namespace Receipts.Gemini;

/// <summary>
/// Parts of the Gemini API; images go inline.
/// </summary>
public sealed class GeminiFormat : IPartFormat
{
    public JsonNode Text(string text)
    {
        return new JsonObject { ["text"] = text };
    }

    public JsonNode Image(string mime, string base64)
    {
        return new JsonObject { ["inline_data"] = new JsonObject { ["mime_type"] = mime, ["data"] = base64 } };
    }
}
