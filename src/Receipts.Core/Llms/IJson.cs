using System.Text.Json;

namespace Receipts;

/// <summary>
/// JSON answer of a model.
/// </summary>
public interface IJson
{
    string Text();

    /// <summary>
    /// Parsed answer, throws <see cref="MalformedAnswerException"/> when it is not JSON.
    /// </summary>
    JsonElement Root();
}
