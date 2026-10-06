using System.Collections.Immutable;
using System.Net;
using System.Text;

namespace Receipts.Tests;

/// <summary>
/// A receipt with VAT included in prices, as models answer it.
/// </summary>
internal sealed class CafeJson
{
    public override string ToString()
    {
        return """
            {
              "merchant": { "name": "Corner Cafe", "address": "1 Main St, Berlin", "taxId": "DE123456789" },
              "date": "2026-03-14T12:30",
              "currency": "eur",
              "lines": [
                { "name": "Cappuccino", "quantity": 2, "unitPrice": 2.5, "total": 5.0 },
                { "name": "Bagel", "quantity": 1, "unitPrice": 3.2, "total": 3.2 }
              ],
              "taxes": [ { "name": "VAT", "rate": 19, "amount": 1.31 } ],
              "subtotal": null,
              "total": 8.2,
              "payment": { "method": "card", "cardLast4": "4242" }
            }
            """;
    }
}

/// <summary>
/// The smallest PNG header, enough for format detection and prompts.
/// </summary>
internal sealed class TinyPng
{
    public IImage Image()
    {
        return new PngImage(new ArrayContent([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3]));
    }
}

/// <summary>
/// HTTP handler answering with a prepared response and remembering the request.
/// </summary>
internal sealed class StubHttp : HttpMessageHandler
{
    private readonly HttpStatusCode status;
    private readonly string reply;

    public StubHttp(string reply) : this(HttpStatusCode.OK, reply)
    {
    }

    public StubHttp(HttpStatusCode status, string reply)
    {
        this.status = status;
        this.reply = reply;
    }

    public Uri? Uri { get; private set; }

    public string Body { get; private set; } = string.Empty;

    public ImmutableDictionary<string, string> Headers { get; private set; } = ImmutableDictionary<string, string>.Empty;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        this.Uri = request.RequestUri;
        this.Body = await request.Content!.ReadAsStringAsync(ct);
        this.Headers = request.Headers.ToImmutableDictionary(header => header.Key, header => string.Join(",", header.Value));
        return new HttpResponseMessage(this.status) { Content = new StringContent(this.reply, Encoding.UTF8, "application/json") };
    }
}

/// <summary>
/// Collects media types of printed images.
/// </summary>
internal sealed class MimesMedia : IPartMedia<IReadOnlyList<string>>
{
    private readonly ImmutableList<string> mimes;

    public MimesMedia() : this(ImmutableList<string>.Empty)
    {
    }

    private MimesMedia(ImmutableList<string> mimes)
    {
        this.mimes = mimes;
    }

    public Task<IPartMedia<IReadOnlyList<string>>> Text(string text, CancellationToken ct = default)
    {
        return Task.FromResult<IPartMedia<IReadOnlyList<string>>>(this);
    }

    public async Task<IPartMedia<IReadOnlyList<string>>> Image(IImage image, CancellationToken ct = default)
    {
        await image.Bytes(ct);
        return new MimesMedia(this.mimes.Add((await image.Type(ct)).Mime()));
    }

    public IReadOnlyList<string> Value()
    {
        return this.mimes;
    }
}

/// <summary>
/// Content counting how many times it was read.
/// </summary>
internal sealed class CountingContent : IContent
{
    private readonly IContent origin;
    private int reads;

    public CountingContent(IContent origin)
    {
        this.origin = origin;
    }

    public int Reads => this.reads;

    public Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default)
    {
        Interlocked.Increment(ref this.reads);
        return this.origin.Bytes(ct);
    }
}
