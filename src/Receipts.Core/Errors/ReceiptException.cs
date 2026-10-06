namespace Receipts;

/// <summary>
/// Base of all failures raised by the library.
/// </summary>
public class ReceiptException : Exception
{
    public ReceiptException(string message) : base(message)
    {
    }

    public ReceiptException(string message, Exception inner) : base(message, inner)
    {
    }
}
