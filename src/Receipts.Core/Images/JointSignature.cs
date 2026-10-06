namespace Receipts;

/// <summary>
/// Signature that matches only when both parts match, the type comes from the first one.
/// </summary>
public sealed class JointSignature : ISignature
{
    private readonly ISignature first;
    private readonly ISignature second;

    public JointSignature(ISignature first, ISignature second)
    {
        this.first = first;
        this.second = second;
    }

    public bool Matches(ReadOnlySpan<byte> head)
    {
        return this.first.Matches(head) && this.second.Matches(head);
    }

    public IMediaType Type()
    {
        return this.first.Type();
    }
}
