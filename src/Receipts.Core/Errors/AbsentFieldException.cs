namespace Receipts;

/// <summary>
/// The receipt doesn't contain a field that was asked for.
/// </summary>
public sealed class AbsentFieldException : ReceiptException
{
    public AbsentFieldException(string field) : base($"The receipt has no '{field}'")
    {
    }
}
