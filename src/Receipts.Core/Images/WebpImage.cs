namespace Receipts;

/// <summary>
/// WebP image.
/// </summary>
public sealed class WebpImage : IImage
{
    private readonly IImage origin;

    public WebpImage(IContent content)
    {
        this.origin = new TypedImage(content, "image/webp");
    }

    public Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default) => this.origin.Bytes(ct);

    public Task<IMediaType> Type(CancellationToken ct = default) => this.origin.Type(ct);
}
