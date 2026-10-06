namespace Receipts;

/// <summary>
/// Image with a media type known in advance.
/// </summary>
public sealed class TypedImage : IImage
{
    private readonly IContent content;
    private readonly IMediaType type;

    public TypedImage(IContent content, string mime) : this(content, new MediaType(mime))
    {
    }

    public TypedImage(IContent content, IMediaType type)
    {
        this.content = content;
        this.type = type;
    }

    public Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default)
    {
        return this.content.Bytes(ct);
    }

    public Task<IMediaType> Type(CancellationToken ct = default)
    {
        return Task.FromResult(this.type);
    }
}
