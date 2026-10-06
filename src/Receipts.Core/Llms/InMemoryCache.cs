using System.Collections.Concurrent;

namespace Receipts;

/// <summary>
/// Cache in process memory. Concurrent requests for the same key share one call.
/// Failed calls are not remembered.
/// </summary>
public sealed class InMemoryCache : ICache
{
    private readonly ConcurrentDictionary<string, Lazy<Task<string>>> values;

    public InMemoryCache() : this(new ConcurrentDictionary<string, Lazy<Task<string>>>())
    {
    }

    public InMemoryCache(ConcurrentDictionary<string, Lazy<Task<string>>> values)
    {
        this.values = values;
    }

    public async Task<string> Value(string key, Func<CancellationToken, Task<string>> fresh, CancellationToken ct = default)
    {
        var value = this.values.GetOrAdd(key, _ => new Lazy<Task<string>>(() => fresh(CancellationToken.None)));
        try
        {
            return await value.Value.WaitAsync(ct);
        }
        catch when (value.Value.IsFaulted || value.Value.IsCanceled)
        {
            this.values.TryRemove(new KeyValuePair<string, Lazy<Task<string>>>(key, value));
            throw;
        }
    }
}
