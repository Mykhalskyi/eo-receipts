namespace Receipts;

/// <summary>
/// Types accepted by both sides, e.g. by the primary and the fallback model.
/// </summary>
public sealed class CommonTypes : IAcceptedTypes
{
    private readonly IAcceptedTypes first;
    private readonly IAcceptedTypes second;

    public CommonTypes(IAcceptedTypes first, IAcceptedTypes second)
    {
        this.first = first;
        this.second = second;
    }

    public bool Contains(IMediaType type)
    {
        return this.first.Contains(type) && this.second.Contains(type);
    }
}
