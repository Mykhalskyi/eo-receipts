namespace Receipts.Testing;

/// <summary>
/// Model that always fails with the given exception.
/// </summary>
public sealed class BrokenLlm : ILlm
{
    private readonly Exception error;

    public BrokenLlm() : this(new LlmException("broken", 503, "Unavailable"))
    {
    }

    public BrokenLlm(Exception error)
    {
        this.error = error;
    }

    public Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default)
    {
        return Task.FromException<IJson>(this.error);
    }

    public IAcceptedTypes Accepts()
    {
        return new AllTypes();
    }
}
