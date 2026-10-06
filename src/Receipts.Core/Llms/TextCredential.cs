namespace Receipts;

public sealed class TextCredential : ICredential
{
    private readonly string value;

    public TextCredential(string value)
    {
        this.value = value;
    }

    public Task<string> Value(CancellationToken ct = default)
    {
        return Task.FromResult(this.value);
    }
}
