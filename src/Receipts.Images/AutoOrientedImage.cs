namespace Receipts.Images;

/// <summary>
/// Image turned according to its EXIF orientation, as phone cameras expect it to be shown.
/// </summary>
public sealed class AutoOrientedImage : IImage
{
    private readonly IImage origin;

    public AutoOrientedImage(IImage origin)
    {
        this.origin = new EditedImage(origin, image => image.AutoOrient());
    }

    public Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default) => this.origin.Bytes(ct);

    public Task<IMediaType> Type(CancellationToken ct = default) => this.origin.Type(ct);
}
