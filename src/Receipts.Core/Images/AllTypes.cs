namespace Receipts;

/// <summary>
/// Accepts any image type.
/// </summary>
public sealed class AllTypes : IAcceptedTypes
{
    public bool Contains(IMediaType type)
    {
        return true;
    }
}
