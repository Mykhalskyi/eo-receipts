namespace Receipts;

/// <summary>
/// The extracted data contradicts itself, e.g. lines don't add up to the total.
/// </summary>
public sealed class InvalidReceiptException : ReceiptException
{
    public InvalidReceiptException(IReadOnlyList<string> violations) : base($"Invalid receipt: {string.Join("; ", violations)}")
    {
        this.Violations = violations;
    }

    public IReadOnlyList<string> Violations { get; }
}
