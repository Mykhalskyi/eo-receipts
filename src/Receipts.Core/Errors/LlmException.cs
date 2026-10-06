namespace Receipts;

/// <summary>
/// The model failed to answer: HTTP error, network failure, timeout or refusal.
/// </summary>
public sealed class LlmException : ReceiptException
{
    public LlmException(string provider, int status, string message) : base($"{provider}: {message} (status {status})")
    {
        this.Provider = provider;
        this.Status = status;
    }

    public LlmException(string provider, int status, string message, Exception inner) : base($"{provider}: {message} (status {status})", inner)
    {
        this.Provider = provider;
        this.Status = status;
    }

    public string Provider { get; }

    /// <summary>
    /// HTTP status of the failed call, 0 when the request never reached the server.
    /// </summary>
    public int Status { get; }

    /// <summary>
    /// Whether repeating the same call may succeed.
    /// </summary>
    public bool Transient => this.Status is 0 or 408 or 409 or 429 or >= 500;
}
