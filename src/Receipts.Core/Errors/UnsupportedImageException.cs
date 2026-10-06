namespace Receipts;

/// <summary>
/// The image format is unknown or can't be converted.
/// </summary>
public sealed class UnsupportedImageException : ReceiptException
{
    public UnsupportedImageException(string message) : base(message)
    {
    }

    public UnsupportedImageException(string message, Exception inner) : base(message, inner)
    {
    }
}
