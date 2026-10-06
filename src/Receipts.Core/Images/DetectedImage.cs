namespace Receipts;

/// <summary>
/// Image whose format is detected from its leading bytes.
/// Throws <see cref="UnsupportedImageException"/> when no signature matches.
/// </summary>
public sealed class DetectedImage : IImage
{
    private readonly IContent content;
    private readonly IEnumerable<ISignature> signatures;

    public DetectedImage(IContent content) : this(content, new KnownSignatures())
    {
    }

    public DetectedImage(IContent content, IEnumerable<ISignature> signatures)
    {
        this.content = new StickyContent(content);
        this.signatures = signatures;
    }

    public Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default)
    {
        return this.content.Bytes(ct);
    }

    public async Task<IMediaType> Type(CancellationToken ct = default)
    {
        var bytes = await this.content.Bytes(ct);
        foreach (var signature in this.signatures)
        {
            if (signature.Matches(bytes.Span))
            {
                return signature.Type();
            }
        }
        throw new UnsupportedImageException(
            $"Unknown image format, leading bytes: {Convert.ToHexString(bytes.Span[..Math.Min(16, bytes.Length)])}"
        );
    }
}
