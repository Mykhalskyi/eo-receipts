using System.Collections.Immutable;
using System.Globalization;

namespace Receipts;

/// <summary>
/// Prompt with a hint about the language and number/date conventions of the receipt.
/// </summary>
public sealed class LocalizedPrompt : IPrompt
{
    private readonly IPrompt origin;
    private readonly CultureInfo culture;

    public LocalizedPrompt(IPrompt origin, CultureInfo culture)
    {
        this.origin = origin;
        this.culture = culture;
    }

    public IReadOnlyList<IPart> Parts()
    {
        return this.origin.Parts()
            .Append(
                new TextPart(
                    $"The receipt is most likely from {this.culture.EnglishName}: numbers may use "
                    + $"'{this.culture.NumberFormat.NumberDecimalSeparator}' as the decimal separator "
                    + $"and dates may look like '{this.culture.DateTimeFormat.ShortDatePattern}'."
                )
            )
            .ToImmutableList();
    }

    public IJsonSchema Schema()
    {
        return this.origin.Schema();
    }
}
