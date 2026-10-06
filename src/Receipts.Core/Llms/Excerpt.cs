namespace Receipts;

/// <summary>
/// The beginning of a long text, for error messages.
/// </summary>
public sealed class Excerpt
{
    private readonly string text;
    private readonly int length;

    public Excerpt(string text) : this(text, 300)
    {
    }

    public Excerpt(string text, int length)
    {
        this.text = text;
        this.length = length;
    }

    public override string ToString()
    {
        return this.text.Length <= this.length ? this.text : $"{this.text[..this.length]}...";
    }
}
