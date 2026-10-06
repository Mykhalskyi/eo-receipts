namespace Receipts.OpenAi;

/// <summary>
/// Header carrying the credential; skipped when the credential is empty.
/// </summary>
public sealed class ChatAuthorization
{
    private readonly string header;
    private readonly string prefix;
    private readonly ICredential credential;

    public ChatAuthorization(string header, string prefix, ICredential credential)
    {
        this.header = header;
        this.prefix = prefix;
        this.credential = credential;
    }

    public async Task<IReadOnlyDictionary<string, string>> Headers(CancellationToken ct = default)
    {
        var value = await this.credential.Value(ct);
        return value.Length == 0
            ? new Dictionary<string, string>()
            : new Dictionary<string, string> { [this.header] = $"{this.prefix}{value}" };
    }
}
