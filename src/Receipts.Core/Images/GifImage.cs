namespace Receipts;

/// <summary>
/// GIF image.
/// </summary>
public sealed class GifImage : IImage
{
    private readonly IImage origin;

    public GifImage(IContent content)
    {
        this.origin = new TypedImage(content, "image/gif");
    }

    public Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default) => this.origin.Bytes(ct);

    public Task<IMediaType> Type(CancellationToken ct = default) => this.origin.Type(ct);
}
