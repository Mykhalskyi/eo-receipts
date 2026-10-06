namespace Receipts.Images;

/// <summary>
/// Image rotated clockwise by the given degrees.
/// </summary>
public sealed class RotatedImage : IImage
{
    private readonly IImage origin;

    public RotatedImage(IImage origin, double degrees)
    {
        this.origin = new EditedImage(origin, image => image.Rotate(degrees));
    }

    public Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default) => this.origin.Bytes(ct);

    public Task<IMediaType> Type(CancellationToken ct = default) => this.origin.Type(ct);
}
