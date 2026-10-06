using System.Text.Json.Nodes;

namespace Receipts;

/// <summary>
/// How a model represents a text and an image inside a JSON content array.
/// </summary>
public interface IPartFormat
{
    JsonNode Text(string text);

    JsonNode Image(string mime, string base64);
}
