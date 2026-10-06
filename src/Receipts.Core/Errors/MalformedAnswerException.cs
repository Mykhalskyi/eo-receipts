namespace Receipts;

/// <summary>
/// The model answered, but the answer is not the JSON we asked for.
/// </summary>
public sealed class MalformedAnswerException : ReceiptException
{
    public MalformedAnswerException(string message) : base(message)
    {
    }

    public MalformedAnswerException(string message, Exception inner) : base(message, inner)
    {
    }
}
