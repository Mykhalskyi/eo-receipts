namespace Receipts;

/// <summary>
/// JPEG image.
/// </summary>
public sealed class JpegImage : IImage
{
    private readonly IImage origin;

    public JpegImage(IContent content)
    {
        this.origin = new TypedImage(content, "image/jpeg");
    }

    public Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default) => this.origin.Bytes(ct);

    public Task<IMediaType> Type(CancellationToken ct = default) => this.origin.Type(ct);
}
