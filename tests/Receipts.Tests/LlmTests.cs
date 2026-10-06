using Receipts.Testing;

namespace Receipts.Tests;

public sealed class LlmTests
{
    private readonly IPrompt prompt = new ReceiptPrompt(new TinyPng().Image());

    [Fact]
    public async Task RetriesTransientFailuresAndMalformedAnswers()
    {
        var script = new ScriptedLlm(new LlmException("x", 429, "Too many"), "not json", """{"total": 1}""");
        var answer = await new RetryingLlm(script, 3, TimeSpan.Zero).Answer(this.prompt);
        Assert.Equal(1, answer.Root().GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task DoesNotRetryPermanentFailures()
    {
        var script = new ScriptedLlm(new LlmException("x", 400, "Bad request"), """{"total": 1}""");
        await Assert.ThrowsAsync<LlmException>(() => new RetryingLlm(script, 3, TimeSpan.Zero).Answer(this.prompt));
        Assert.Equal(1, script.Left());
    }

    [Fact]
    public async Task GivesUpAfterAllAttempts()
    {
        var script = new ScriptedLlm(new LlmException("x", 503, "a"), new LlmException("x", 503, "b"));
        var error = await Assert.ThrowsAsync<LlmException>(() => new RetryingLlm(script, 2, TimeSpan.Zero).Answer(this.prompt));
        Assert.Contains("b", error.Message);
    }

    [Fact]
    public async Task FallsBackToSecondaryModel()
    {
        var llm = new FallbackLlm(new BrokenLlm(), new FakeLlm("""{"total": 2}"""));
        Assert.Equal(2, (await llm.Answer(this.prompt)).Root().GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task FallsBackOnMalformedAnswer()
    {
        var llm = new FallbackLlm(new FakeLlm("no json here"), new FakeLlm("""{"total": 3}"""));
        Assert.Equal(3, (await llm.Answer(this.prompt)).Root().GetProperty("total").GetInt32());
    }

    [Fact]
    public void AcceptsTypesOfBothModels()
    {
        var llm = new FallbackLlm(
            new FakeLlm(new JsonOf("{}"), new AcceptedTypes("image/png", "image/heic")),
            new FakeLlm(new JsonOf("{}"), new AcceptedTypes("image/png"))
        );
        Assert.False(llm.Accepts().Contains(new MediaType("image/heic")));
    }

    [Fact]
    public async Task ExtractsJsonFromFencedAnswer()
    {
        var llm = new FencedJsonLlm(new FakeLlm(new JsonOf("Here it is:\n```json\n{\"total\": 4}\n```\nAnything else?")));
        var answer = await llm.Answer(this.prompt);
        Assert.Equal("{\"total\": 4}", answer.Text());
        Assert.Equal(4, answer.Root().GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task TimesOutSlowModel()
    {
        var error = await Assert.ThrowsAsync<LlmException>(
            () => new TimedLlm(new SlowLlm(), TimeSpan.FromMilliseconds(50)).Answer(this.prompt)
        );
        Assert.True(error.Transient);
    }

    [Fact]
    public async Task CachesAnswersByPrompt()
    {
        var script = new ScriptedLlm("""{"total": 5}""");
        var llm = new CachedLlm(script, new InMemoryCache());
        await llm.Answer(new ReceiptPrompt(new TinyPng().Image()));
        var again = await llm.Answer(new ReceiptPrompt(new TinyPng().Image()));
        Assert.Equal(5, again.Root().GetProperty("total").GetInt32());
        Assert.Equal(0, script.Left());
        var other = new ReceiptPrompt(new PngImage(new ArrayContent([0x89, 0x50, 0x4E, 0x47, 9])));
        await Assert.ThrowsAsync<InvalidOperationException>(() => llm.Answer(other));
    }

    [Fact]
    public async Task DoesNotCacheFailures()
    {
        var script = new ScriptedLlm(new LlmException("x", 500, "Down"), """{"total": 6}""");
        var llm = new CachedLlm(script, new InMemoryCache());
        await Assert.ThrowsAsync<LlmException>(() => llm.Answer(this.prompt));
        Assert.Equal(6, (await llm.Answer(this.prompt)).Root().GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task KeepsCacheInFiles()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"receipts-{Guid.NewGuid():N}");
        try
        {
            await new CachedLlm(new FakeLlm("""{"total": 7}"""), new FileCache(directory)).Answer(this.prompt);
            var cached = await new CachedLlm(new BrokenLlm(), new FileCache(directory)).Answer(this.prompt);
            Assert.Equal(7, cached.Root().GetProperty("total").GetInt32());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public async Task LimitsConcurrency()
    {
        var counting = new ConcurrencyLlm();
        var llm = new ThrottledLlm(counting, 2);
        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => llm.Answer(this.prompt)));
        Assert.Equal(2, counting.Peak());
    }

    [Fact]
    public async Task HashesDifferentScopesDifferently()
    {
        Assert.NotEqual(
            await new PromptHash(this.prompt, "claude").Value(),
            await new PromptHash(this.prompt, "gemini").Value()
        );
    }

    private sealed class SlowLlm : ILlm
    {
        public async Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default)
        {
            await Task.Delay(Timeout.Infinite, ct);
            return new JsonOf("{}");
        }

        public IAcceptedTypes Accepts() => new AllTypes();
    }

    private sealed class ConcurrencyLlm : ILlm
    {
        private int current;
        private int peak;

        public async Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default)
        {
            var now = Interlocked.Increment(ref this.current);
            InterlockedMax(now);
            await Task.Delay(30, ct);
            Interlocked.Decrement(ref this.current);
            return new JsonOf("{}");
        }

        public IAcceptedTypes Accepts() => new AllTypes();

        public int Peak() => this.peak;

        private void InterlockedMax(int value)
        {
            int seen;
            do
            {
                seen = this.peak;
            }
            while (value > seen && Interlocked.CompareExchange(ref this.peak, value, seen) != seen);
        }
    }
}
