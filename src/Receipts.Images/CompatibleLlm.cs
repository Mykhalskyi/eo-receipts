namespace Receipts.Images;

/// <summary>
/// Model that never receives an image type it can't read:
/// such images are converted to JPEG before the call.
/// </summary>
public sealed class CompatibleLlm : ILlm
{
    private readonly ILlm origin;

    public CompatibleLlm(ILlm origin)
    {
        this.origin = origin;
    }

    public Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default)
    {
        return this.origin.Answer(new CompatiblePrompt(prompt, this.origin.Accepts()), ct);
    }

    public IAcceptedTypes Accepts()
    {
        return new AllTypes();
    }
}
