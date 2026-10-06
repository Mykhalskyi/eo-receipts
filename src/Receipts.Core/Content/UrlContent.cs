namespace Receipts;

/// <summary>
/// Content downloaded from a URL.
/// </summary>
public sealed class UrlContent : IContent
{
    private readonly Uri url;
    private readonly HttpClient http;

    public UrlContent(Uri url, HttpClient http)
    {
        this.url = url;
        this.http = http;
    }

    public async Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default)
    {
        return await this.http.GetByteArrayAsync(this.url, ct);
    }
}
