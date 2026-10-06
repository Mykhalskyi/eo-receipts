namespace Receipts;

/// <summary>
/// TIFF image, common for scanners.
/// </summary>
public sealed class TiffImage : IImage
{
    private readonly IImage origin;

    public TiffImage(IContent content)
    {
        this.origin = new TypedImage(content, "image/tiff");
    }

    public Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default) => this.origin.Bytes(ct);

    public Task<IMediaType> Type(CancellationToken ct = default) => this.origin.Type(ct);
}
