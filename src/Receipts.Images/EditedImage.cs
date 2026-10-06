using ImageMagick;

namespace Receipts.Images;

/// <summary>
/// Image changed by Magick.NET. The result keeps the original format when it is
/// widely readable (JPEG, PNG, WebP, GIF) and becomes JPEG otherwise, unless a target type is given.
/// </summary>
public sealed class EditedImage : IImage
{
    private readonly IImage origin;
    private readonly Action<MagickImage> edit;
    private readonly IAcceptedTypes kept;
    private readonly IMediaType fallback;

    public EditedImage(IImage origin, Action<MagickImage> edit)
        : this(origin, edit, new AcceptedTypes("image/jpeg", "image/png", "image/webp", "image/gif"), new MediaType("image/jpeg"))
    {
    }

    public EditedImage(IImage origin, Action<MagickImage> edit, IMediaType target)
        : this(origin, edit, new AcceptedTypes(), target)
    {
    }

    public EditedImage(IImage origin, Action<MagickImage> edit, IAcceptedTypes kept, IMediaType fallback)
    {
        this.origin = origin;
        this.edit = edit;
        this.kept = kept;
        this.fallback = fallback;
    }

    public async Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default)
    {
        var bytes = await this.origin.Bytes(ct);
        var type = await this.Type(ct);
        try
        {
            using var image = new MagickImage(bytes.ToArray());
            this.edit(image);
            return image.ToByteArray(this.Format(type));
        }
        catch (MagickException e)
        {
            throw new UnsupportedImageException($"Can't convert the image: {e.Message}", e);
        }
    }

    public async Task<IMediaType> Type(CancellationToken ct = default)
    {
        var type = await this.origin.Type(ct);
        return this.kept.Contains(type) ? type : this.fallback;
    }

    private MagickFormat Format(IMediaType type)
    {
        return type.Mime().ToLowerInvariant() switch
        {
            "image/jpeg" => MagickFormat.Jpeg,
            "image/png" => MagickFormat.Png,
            "image/webp" => MagickFormat.WebP,
            "image/gif" => MagickFormat.Gif,
            "image/tiff" => MagickFormat.Tiff,
            "image/bmp" => MagickFormat.Bmp,
            _ => throw new UnsupportedImageException($"Can't write images of type {type.Mime()}"),
        };
    }
}
