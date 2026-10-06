namespace Receipts;

/// <summary>
/// All parts of a prompt printed, one after another, into a media.
/// </summary>
public sealed class PrintedPrompt<T>
{
    private readonly IPrompt prompt;
    private readonly IPartMedia<T> media;

    public PrintedPrompt(IPrompt prompt, IPartMedia<T> media)
    {
        this.prompt = prompt;
        this.media = media;
    }

    public async Task<T> Value(CancellationToken ct = default)
    {
        var current = this.media;
        foreach (var part in this.prompt.Parts())
        {
            current = await part.Print(current, ct);
        }
        return current.Value();
    }
}
