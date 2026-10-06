namespace Receipts;

/// <summary>
/// File signature ("magic bytes") of an image format.
/// </summary>
public interface ISignature
{
    bool Matches(ReadOnlySpan<byte> head);

    IMediaType Type();
}
