using ImageMagick;

namespace Receipts.Images;

/// <summary>
/// Image scaled down so its longest side is not longer than the limit; smaller images stay as they are.
/// Fewer pixels mean fewer tokens and lower cost.
/// </summary>
public sealed class ResizedImage : IImage
{
    private readonly IImage origin;

    public ResizedImage(IImage origin, int side)
    {
        this.origin = new EditedImage(origin, image => image.Resize(new MagickGeometry((uint)side, (uint)side) { Greater = true }));
    }

    public Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default) => this.origin.Bytes(ct);

    public Task<IMediaType> Type(CancellationToken ct = default) => this.origin.Type(ct);
}
