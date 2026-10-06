namespace Receipts;

/// <summary>
/// HEIC/HEIF image, the default photo format of iPhones.
/// </summary>
public sealed class HeicImage : IImage
{
    private readonly IImage origin;

    public HeicImage(IContent content)
    {
        this.origin = new TypedImage(content, "image/heic");
    }

    public Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default) => this.origin.Bytes(ct);

    public Task<IMediaType> Type(CancellationToken ct = default) => this.origin.Type(ct);
}
