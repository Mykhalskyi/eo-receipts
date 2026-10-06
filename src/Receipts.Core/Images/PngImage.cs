namespace Receipts;

/// <summary>
/// PNG image.
/// </summary>
public sealed class PngImage : IImage
{
    private readonly IImage origin;

    public PngImage(IContent content)
    {
        this.origin = new TypedImage(content, "image/png");
    }

    public Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default) => this.origin.Bytes(ct);

    public Task<IMediaType> Type(CancellationToken ct = default) => this.origin.Type(ct);
}
