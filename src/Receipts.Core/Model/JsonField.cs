using System.Globalization;
using System.Text.Json;

namespace Receipts;

/// <summary>
/// A named property of a JSON object, tolerant to the ways models express absence.
/// </summary>
public sealed class JsonField
{
    private readonly JsonElement parent;
    private readonly string name;

    public JsonField(JsonElement parent, string name)
    {
        this.parent = parent;
        this.name = name;
    }

    public bool Present()
    {
        return this.parent.ValueKind == JsonValueKind.Object
            && this.parent.TryGetProperty(this.name, out var value)
            && value.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined)
            && !(value.ValueKind == JsonValueKind.String && value.GetString()!.Trim().Length == 0);
    }

    /// <summary>
    /// The value itself, or an empty object when absent.
    /// </summary>
    public JsonElement Element()
    {
        if (this.Present())
        {
            return this.parent.GetProperty(this.name);
        }
        using var empty = JsonDocument.Parse("{}");
        return empty.RootElement.Clone();
    }

    /// <summary>
    /// The value as text, empty when absent.
    /// </summary>
    public string Text()
    {
        if (!this.Present())
        {
            return string.Empty;
        }
        var value = this.parent.GetProperty(this.name);
        return value.ValueKind == JsonValueKind.String ? value.GetString()!.Trim() : value.ToString();
    }

    /// <summary>
    /// The value as a number, throws <see cref="AbsentFieldException"/> when absent.
    /// Numbers sent as strings are accepted too.
    /// </summary>
    public decimal Number()
    {
        if (!this.Present())
        {
            throw new AbsentFieldException(this.name);
        }
        var value = this.parent.GetProperty(this.name);
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number))
        {
            return number;
        }
        if (value.ValueKind == JsonValueKind.String
            && decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out number))
        {
            return number;
        }
        throw new MalformedAnswerException($"'{this.name}' is not a number: {value}");
    }

    public decimal Number(decimal fallback)
    {
        return this.Present() ? this.Number() : fallback;
    }

    /// <summary>
    /// Items of an array, empty when absent.
    /// </summary>
    public IReadOnlyList<JsonElement> Items()
    {
        if (!this.Present())
        {
            return [];
        }
        var value = this.parent.GetProperty(this.name);
        return value.ValueKind == JsonValueKind.Array ? value.EnumerateArray().ToList() : [];
    }
}
