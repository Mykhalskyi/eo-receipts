using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Receipts;

/// <summary>
/// Logs every call with its duration and outcome.
/// </summary>
public sealed class LoggedLlm : ILlm
{
    private readonly ILlm origin;
    private readonly ILogger logger;
    private readonly string name;

    public LoggedLlm(ILlm origin, ILogger logger) : this(origin, logger, "llm")
    {
    }

    public LoggedLlm(ILlm origin, ILogger logger, string name)
    {
        this.origin = origin;
        this.logger = logger;
        this.name = name;
    }

    public async Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            var answer = await this.origin.Answer(prompt, ct);
            this.logger.LogInformation("{Llm} answered in {Elapsed} ms with {Length} chars", this.name, watch.ElapsedMilliseconds, answer.Text().Length);
            return answer;
        }
        catch (Exception e)
        {
            this.logger.LogWarning(e, "{Llm} failed in {Elapsed} ms", this.name, watch.ElapsedMilliseconds);
            throw;
        }
    }

    public IAcceptedTypes Accepts()
    {
        return this.origin.Accepts();
    }
}
