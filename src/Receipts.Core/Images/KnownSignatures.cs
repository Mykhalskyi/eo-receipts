using System.Collections;

namespace Receipts;

/// <summary>
/// Signatures of the image formats supported out of the box:
/// JPEG, PNG, GIF, WebP, HEIC/HEIF, TIFF and BMP.
/// </summary>
public sealed class KnownSignatures : IEnumerable<ISignature>
{
    private readonly IReadOnlyList<string> heicBrands;

    public KnownSignatures() : this(["heic", "heix", "hevc", "hevx", "heim", "heis", "mif1", "msf1"])
    {
    }

    public KnownSignatures(IReadOnlyList<string> heicBrands)
    {
        this.heicBrands = heicBrands;
    }

    public IEnumerator<ISignature> GetEnumerator()
    {
        yield return new Signature("image/jpeg", 0, 0xFF, 0xD8, 0xFF);
        yield return new Signature("image/png", 0, 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A);
        yield return new Signature("image/gif", 0, "GIF8");
        yield return new JointSignature(new Signature("image/webp", 0, "RIFF"), new Signature("image/webp", 8, "WEBP"));
        foreach (var brand in this.heicBrands)
        {
            yield return new JointSignature(new Signature("image/heic", 4, "ftyp"), new Signature("image/heic", 8, brand));
        }
        yield return new Signature("image/tiff", 0, 0x49, 0x49, 0x2A, 0x00);
        yield return new Signature("image/tiff", 0, 0x4D, 0x4D, 0x00, 0x2A);
        yield return new Signature("image/bmp", 0, "BM");
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}
