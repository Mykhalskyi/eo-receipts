namespace Receipts.Images;

public sealed class GrayscaleImage : IImage
{
    private readonly IImage origin;

    public GrayscaleImage(IImage origin)
    {
        this.origin = new EditedImage(origin, image => image.Grayscale());
    }

    public Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default) => this.origin.Bytes(ct);

    public Task<IMediaType> Type(CancellationToken ct = default) => this.origin.Type(ct);
}
