using System.Collections.Immutable;
using System.Globalization;

namespace Receipts;

/// <summary>
/// Prints into flat name/value pairs: sections become "merchant.name",
/// list items become "taxes.0.amount".
/// </summary>
public sealed class FlatMedia : IMedia<IReadOnlyList<KeyValuePair<string, string>>>
{
    private readonly ImmutableList<KeyValuePair<string, string>> pairs;

    public FlatMedia() : this(ImmutableList<KeyValuePair<string, string>>.Empty)
    {
    }

    private FlatMedia(ImmutableList<KeyValuePair<string, string>> pairs)
    {
        this.pairs = pairs;
    }

    public IMedia<IReadOnlyList<KeyValuePair<string, string>>> With(string name, string value)
    {
        return new FlatMedia(this.pairs.Add(new KeyValuePair<string, string>(name, value)));
    }

    public IMedia<IReadOnlyList<KeyValuePair<string, string>>> With(string name, decimal value)
    {
        return this.With(name, value.ToString(CultureInfo.InvariantCulture));
    }

    public IMedia<IReadOnlyList<KeyValuePair<string, string>>> With(string name, IPrintable section)
    {
        return new FlatMedia(this.pairs.AddRange(this.Nested(name, section)));
    }

    public IMedia<IReadOnlyList<KeyValuePair<string, string>>> With(string name, IEnumerable<IPrintable> items)
    {
        return new FlatMedia(this.pairs.AddRange(items.SelectMany((item, index) => this.Nested($"{name}.{index}", item))));
    }

    public IReadOnlyList<KeyValuePair<string, string>> Value()
    {
        return this.pairs;
    }

    private IEnumerable<KeyValuePair<string, string>> Nested(string prefix, IPrintable section)
    {
        return section.Print(new FlatMedia()).Value()
            .Select(pair => new KeyValuePair<string, string>($"{prefix}.{pair.Key}", pair.Value));
    }
}
