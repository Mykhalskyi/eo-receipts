namespace Receipts;

/// <summary>
/// Secret used to call a model, e.g. an API key.
/// </summary>
public interface ICredential
{
    Task<string> Value(CancellationToken ct = default);
}
