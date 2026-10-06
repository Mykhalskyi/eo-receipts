using System.Collections.Immutable;
using System.Text;

namespace Receipts;

/// <summary>
/// Prints into CSV with one row per item of the chosen list (lines by default);
/// all other values are repeated in every row.
/// </summary>
public sealed class CsvMedia : IMedia<string>
{
    private readonly string rows;
    private readonly IMedia<IReadOnlyList<KeyValuePair<string, string>>> head;
    private readonly ImmutableList<IReadOnlyList<KeyValuePair<string, string>>> items;

    public CsvMedia() : this("lines")
    {
    }

    public CsvMedia(string rows) : this(rows, new FlatMedia(), ImmutableList<IReadOnlyList<KeyValuePair<string, string>>>.Empty)
    {
    }

    private CsvMedia(
        string rows,
        IMedia<IReadOnlyList<KeyValuePair<string, string>>> head,
        ImmutableList<IReadOnlyList<KeyValuePair<string, string>>> items
    )
    {
        this.rows = rows;
        this.head = head;
        this.items = items;
    }

    public IMedia<string> With(string name, string value) => new CsvMedia(this.rows, this.head.With(name, value), this.items);

    public IMedia<string> With(string name, decimal value) => new CsvMedia(this.rows, this.head.With(name, value), this.items);

    public IMedia<string> With(string name, IPrintable section) => new CsvMedia(this.rows, this.head.With(name, section), this.items);

    public IMedia<string> With(string name, IEnumerable<IPrintable> items)
    {
        if (name != this.rows)
        {
            return new CsvMedia(this.rows, this.head.With(name, items), this.items);
        }
        return new CsvMedia(
            this.rows,
            this.head,
            this.items.AddRange(
                items.Select(
                    item => (IReadOnlyList<KeyValuePair<string, string>>)item.Print(new FlatMedia()).Value()
                        .Select(pair => new KeyValuePair<string, string>($"{name}.{pair.Key}", pair.Value))
                        .ToList()
                )
            )
        );
    }

    public string Value()
    {
        var head = this.head.Value();
        var rows = this.items.Count > 0 ? this.items : [[]];
        var columns = head.Select(pair => pair.Key).Concat(rows.SelectMany(row => row.Select(pair => pair.Key))).Distinct().ToList();
        var csv = new StringBuilder();
        csv.AppendLine(string.Join(",", columns.Select(Escaped)));
        foreach (var row in rows)
        {
            var values = head.Concat(row).GroupBy(pair => pair.Key).ToDictionary(group => group.Key, group => group.Last().Value);
            csv.AppendLine(string.Join(",", columns.Select(column => Escaped(values.GetValueOrDefault(column, string.Empty)))));
        }
        return csv.ToString();

        string Escaped(string value)
        {
            return value.IndexOfAny([',', '"', '\n', '\r']) >= 0 ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
        }
    }
}
