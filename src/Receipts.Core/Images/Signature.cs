using System.Text;

namespace Receipts;

/// <summary>
/// Signature made of bytes expected at some offset from the file start.
/// </summary>
public sealed class Signature : ISignature
{
    private readonly IMediaType type;
    private readonly int offset;
    private readonly byte[] magic;

    public Signature(string mime, int offset, string ascii) : this(new MediaType(mime), offset, Encoding.ASCII.GetBytes(ascii))
    {
    }

    public Signature(string mime, int offset, params byte[] magic) : this(new MediaType(mime), offset, magic)
    {
    }

    public Signature(IMediaType type, int offset, byte[] magic)
    {
        this.type = type;
        this.offset = offset;
        this.magic = magic;
    }

    public bool Matches(ReadOnlySpan<byte> head)
    {
        return head.Length >= this.offset + this.magic.Length
            && head.Slice(this.offset, this.magic.Length).SequenceEqual(this.magic);
    }

    public IMediaType Type()
    {
        return this.type;
    }
}
