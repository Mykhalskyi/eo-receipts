namespace Receipts.Images;

/// <summary>
/// The same image re-encoded as JPEG. Use it for formats a model can't read (HEIC, TIFF, BMP).
/// </summary>
public sealed class JpegOf : IImage
{
    private readonly IImage origin;

    public JpegOf(IImage origin) : this(origin, 90)
    {
    }

    public JpegOf(IImage origin, int quality)
    {
        this.origin = new EditedImage(origin, image => image.Quality = (uint)quality, new MediaType("image/jpeg"));
    }

    public Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default) => this.origin.Bytes(ct);

    public Task<IMediaType> Type(CancellationToken ct = default) => this.origin.Type(ct);
}
