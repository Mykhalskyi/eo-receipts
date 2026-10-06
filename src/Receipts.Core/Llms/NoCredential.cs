namespace Receipts;

/// <summary>
/// No credential, for local servers without authentication.
/// </summary>
public sealed class NoCredential : ICredential
{
    public Task<string> Value(CancellationToken ct = default)
    {
        return Task.FromResult(string.Empty);
    }
}
