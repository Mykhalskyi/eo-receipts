using System.Text.Json.Nodes;

namespace Receipts;

/// <summary>
/// JSON Schema of the answer we expect.
/// </summary>
public interface IJsonSchema
{
    /// <summary>
    /// A fresh copy of the schema on every call.
    /// </summary>
    JsonObject Json();
}
