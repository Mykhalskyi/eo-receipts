namespace Receipts;

public sealed class TextPart : IPart
{
    private readonly string text;

    public TextPart(string text)
    {
        this.text = text;
    }

    public Task<IPartMedia<T>> Print<T>(IPartMedia<T> media, CancellationToken ct = default)
    {
        return media.Text(this.text, ct);
    }
}
