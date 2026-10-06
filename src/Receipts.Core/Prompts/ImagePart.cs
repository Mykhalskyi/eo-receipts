namespace Receipts;

public sealed class ImagePart : IPart
{
    private readonly IImage image;

    public ImagePart(IImage image)
    {
        this.image = image;
    }

    public Task<IPartMedia<T>> Print<T>(IPartMedia<T> media, CancellationToken ct = default)
    {
        return media.Image(this.image, ct);
    }
}
