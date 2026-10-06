namespace Receipts;

/// <summary>
/// Storage of model answers.
/// </summary>
public interface ICache
{
    /// <summary>
    /// The value stored under the key, or the fresh one, which is stored afterwards.
    /// </summary>
    Task<string> Value(string key, Func<CancellationToken, Task<string>> fresh, CancellationToken ct = default);
}
