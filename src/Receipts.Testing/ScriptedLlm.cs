using System.Collections.Concurrent;

namespace Receipts.Testing;

/// <summary>
/// Model giving prepared answers one by one, failing with exceptions placed among them.
/// </summary>
public sealed class ScriptedLlm : ILlm
{
    private readonly ConcurrentQueue<object> script;

    public ScriptedLlm(params object[] script) : this(new ConcurrentQueue<object>(script))
    {
    }

    public ScriptedLlm(ConcurrentQueue<object> script)
    {
        this.script = script;
    }

    public Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default)
    {
        if (!this.script.TryDequeue(out var next))
        {
            throw new InvalidOperationException("The script has no more answers");
        }
        return next switch
        {
            Exception error => Task.FromException<IJson>(error),
            IJson json => Task.FromResult(json),
            string text => Task.FromResult<IJson>(new JsonOf(text)),
            _ => throw new InvalidOperationException($"Unexpected script entry {next}"),
        };
    }

    public IAcceptedTypes Accepts()
    {
        return new AllTypes();
    }

    /// <summary>
    /// How many answers are left.
    /// </summary>
    public int Left()
    {
        return this.script.Count;
    }
}
