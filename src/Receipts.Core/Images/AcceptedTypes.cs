namespace Receipts;

public sealed class AcceptedTypes : IAcceptedTypes
{
    private readonly IReadOnlyCollection<string> mimes;

    public AcceptedTypes(params string[] mimes)
    {
        this.mimes = mimes;
    }

    public bool Contains(IMediaType type)
    {
        return this.mimes.Contains(type.Mime(), StringComparer.OrdinalIgnoreCase);
    }
}
