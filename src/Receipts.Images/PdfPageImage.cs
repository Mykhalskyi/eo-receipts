using ImageMagick;

namespace Receipts.Images;

/// <summary>
/// One page of a PDF document rendered to PNG; pages are numbered from 1.
/// Rendering needs Ghostscript installed on the machine.
/// </summary>
public sealed class PdfPageImage : IImage
{
    private readonly IContent pdf;
    private readonly int page;
    private readonly int density;

    public PdfPageImage(IContent pdf) : this(pdf, 1)
    {
    }

    public PdfPageImage(IContent pdf, int page) : this(pdf, page, 200)
    {
    }

    public PdfPageImage(IContent pdf, int page, int density)
    {
        this.pdf = pdf;
        this.page = page;
        this.density = density;
    }

    public async Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default)
    {
        var bytes = await this.pdf.Bytes(ct);
        var settings = new MagickReadSettings
        {
            Density = new Density(this.density),
            FrameIndex = (uint)(this.page - 1),
            FrameCount = 1,
        };
        try
        {
            using var pages = new MagickImageCollection();
            pages.Read(bytes.ToArray(), settings);
            return pages[0].ToByteArray(MagickFormat.Png);
        }
        catch (MagickException e)
        {
            throw new UnsupportedImageException($"Can't render page {this.page} of the PDF (is Ghostscript installed?): {e.Message}", e);
        }
    }

    public Task<IMediaType> Type(CancellationToken ct = default)
    {
        return Task.FromResult<IMediaType>(new MediaType("image/png"));
    }
}
