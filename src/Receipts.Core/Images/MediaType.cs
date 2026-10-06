namespace Receipts;

public sealed class MediaType : IMediaType
{
    private readonly string mime;

    public MediaType(string mime)
    {
        this.mime = mime;
    }

    public string Mime()
    {
        return this.mime;
    }

    public override string ToString()
    {
        return this.mime;
    }
}
