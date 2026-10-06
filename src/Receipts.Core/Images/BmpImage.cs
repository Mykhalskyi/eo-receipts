namespace Receipts;

/// <summary>
/// BMP image.
/// </summary>
public sealed class BmpImage : IImage
{
    private readonly IImage origin;

    public BmpImage(IContent content)
    {
        this.origin = new TypedImage(content, "image/bmp");
    }

    public Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default) => this.origin.Bytes(ct);

    public Task<IMediaType> Type(CancellationToken ct = default) => this.origin.Type(ct);
}
