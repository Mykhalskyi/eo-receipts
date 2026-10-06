namespace Receipts;

/// <summary>
/// The default prompt: the receipt image followed by extraction instructions,
/// answered according to <see cref="ReceiptSchema"/>.
/// </summary>
public sealed class ReceiptPrompt : IPrompt
{
    private const string Instructions =
        """
        Extract the data from the receipt in the image and answer with JSON that matches the schema.
        - merchant: name, address and tax id (VAT, TIN, etc.) exactly as printed; empty string when absent.
        - date: purchase date in ISO 8601, "yyyy-MM-dd" or "yyyy-MM-ddTHH:mm" when the time is printed; empty string when absent.
        - currency: ISO 4217 code, infer it from symbols, language and address; "XXX" when it can't be determined.
        - All amounts are plain numbers with a dot as the decimal separator, without currency symbols or thousands separators.
        - lines: one entry per purchased item in printed order; quantity is 1 when not printed;
          discounts and deposits are separate lines with negative totals.
        - taxes: as printed; rate in percent (20 for 20%); amount in the receipt currency.
        - subtotal: the printed amount before taxes, null when not printed.
        - total: the final amount to pay.
        - payment: method such as "cash" or "card" and the last four card digits; empty strings when absent.
        Do not invent data that is not on the receipt.
        """;

    private readonly IImage image;
    private readonly string instructions;

    public ReceiptPrompt(IImage image) : this(image, Instructions)
    {
    }

    public ReceiptPrompt(IImage image, string instructions)
    {
        this.image = image;
        this.instructions = instructions;
    }

    public IReadOnlyList<IPart> Parts()
    {
        return [new ImagePart(this.image), new TextPart(this.instructions)];
    }

    public IJsonSchema Schema()
    {
        return new ReceiptSchema();
    }
}
