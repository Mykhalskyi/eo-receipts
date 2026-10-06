using System.Collections.Concurrent;

namespace Receipts.Testing;

/// <summary>
/// Model that always gives the same answer and remembers the prompts it was asked.
/// </summary>
public sealed class FakeLlm : ILlm
{
    private readonly IJson answer;
    private readonly IAcceptedTypes types;
    private readonly ConcurrentQueue<IPrompt> prompts;

    public FakeLlm(string json) : this(new JsonOf(json))
    {
    }

    public FakeLlm(IJson answer) : this(answer, new AllTypes())
    {
    }

    public FakeLlm(IJson answer, IAcceptedTypes types) : this(answer, types, new ConcurrentQueue<IPrompt>())
    {
    }

    public FakeLlm(IJson answer, IAcceptedTypes types, ConcurrentQueue<IPrompt> prompts)
    {
        this.answer = answer;
        this.types = types;
        this.prompts = prompts;
    }

    public Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default)
    {
        this.prompts.Enqueue(prompt);
        return Task.FromResult(this.answer);
    }

    public IAcceptedTypes Accepts()
    {
        return this.types;
    }

    /// <summary>
    /// Prompts received so far.
    /// </summary>
    public IReadOnlyCollection<IPrompt> Prompts()
    {
        return this.prompts.ToArray();
    }
}
