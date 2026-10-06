namespace Receipts;

/// <summary>
/// Content encoded as a Base64 string.
/// </summary>
public sealed class Base64Content : IContent
{
    private readonly string base64;

    public Base64Content(string base64)
    {
        this.base64 = base64;
    }

    public Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default)
    {
        return Task.FromResult<ReadOnlyMemory<byte>>(Convert.FromBase64String(this.base64));
    }
}
