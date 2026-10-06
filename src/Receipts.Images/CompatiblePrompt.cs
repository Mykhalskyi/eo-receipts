namespace Receipts.Images;

/// <summary>
/// Prompt whose images are made readable for the given types.
/// </summary>
public sealed class CompatiblePrompt : IPrompt
{
    private readonly IPrompt origin;
    private readonly IAcceptedTypes types;

    public CompatiblePrompt(IPrompt origin, IAcceptedTypes types)
    {
        this.origin = origin;
        this.types = types;
    }

    public IReadOnlyList<IPart> Parts()
    {
        return this.origin.Parts().Select(part => (IPart)new CompatiblePart(part, this.types)).ToList();
    }

    public IJsonSchema Schema()
    {
        return this.origin.Schema();
    }
}
