using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;

namespace Receipts;

/// <summary>
/// Prompt parts reduced to a hex SHA-256.
/// </summary>
public sealed class HashMedia : IPartMedia<string>
{
    private readonly ImmutableList<byte[]> chunks;

    public HashMedia() : this(ImmutableList<byte[]>.Empty)
    {
    }

    private HashMedia(ImmutableList<byte[]> chunks)
    {
        this.chunks = chunks;
    }

    public Task<IPartMedia<string>> Text(string text, CancellationToken ct = default)
    {
        return Task.FromResult<IPartMedia<string>>(new HashMedia(this.chunks.Add(Encoding.UTF8.GetBytes($"text:{text.Length}:{text}"))));
    }

    public async Task<IPartMedia<string>> Image(IImage image, CancellationToken ct = default)
    {
        var type = await image.Type(ct);
        var bytes = await image.Bytes(ct);
        return new HashMedia(
            this.chunks
                .Add(Encoding.UTF8.GetBytes($"image:{type.Mime()}:{bytes.Length}:"))
                .Add(bytes.ToArray())
        );
    }

    public string Value()
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var chunk in this.chunks)
        {
            hash.AppendData(chunk);
        }
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }
}
