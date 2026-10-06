namespace Receipts.Images;

/// <summary>
/// The image as it is when the model can read its type, otherwise converted to JPEG.
/// </summary>
public sealed class CompatibleImage : IImage
{
    private readonly IImage origin;
    private readonly IAcceptedTypes types;

    public CompatibleImage(IImage origin, IAcceptedTypes types)
    {
        this.origin = origin;
        this.types = types;
    }

    public async Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default)
    {
        return await (await this.Chosen(ct)).Bytes(ct);
    }

    public async Task<IMediaType> Type(CancellationToken ct = default)
    {
        return await (await this.Chosen(ct)).Type(ct);
    }

    private async Task<IImage> Chosen(CancellationToken ct)
    {
        return this.types.Contains(await this.origin.Type(ct)) ? this.origin : new JpegOf(this.origin);
    }
}
