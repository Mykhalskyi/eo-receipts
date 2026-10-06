using ImageMagick;
using Receipts.Images;
using Receipts.Testing;

namespace Receipts.Tests;

public sealed class ConversionTests
{
    [Fact]
    public async Task ConvertsToJpeg()
    {
        var jpeg = new JpegOf(this.Picture(MagickFormat.Bmp, "image/bmp"));
        Assert.Equal("image/jpeg", (await jpeg.Type()).Mime());
        Assert.Equal("image/jpeg", (await new DetectedImage(new ArrayContent(await jpeg.Bytes())).Type()).Mime());
    }

    [Fact]
    public async Task ShrinksLargeImagesKeepingFormat()
    {
        var resized = new ResizedImage(this.Picture(MagickFormat.Png, "image/png"), 100);
        Assert.Equal("image/png", (await resized.Type()).Mime());
        using var image = new MagickImage((await resized.Bytes()).ToArray());
        Assert.Equal(100u, image.Width);
        Assert.Equal(50u, image.Height);
    }

    [Fact]
    public async Task WritesUncommonFormatsAsJpeg()
    {
        var gray = new GrayscaleImage(this.Picture(MagickFormat.Tiff, "image/tiff"));
        Assert.Equal("image/jpeg", (await gray.Type()).Mime());
        Assert.Equal(0xFF, (await gray.Bytes()).Span[0]);
    }

    [Fact]
    public async Task RotatesImages()
    {
        using var image = new MagickImage((await new RotatedImage(this.Picture(MagickFormat.Png, "image/png"), 90).Bytes()).ToArray());
        Assert.Equal(200u, image.Width);
        Assert.Equal(400u, image.Height);
    }

    [Fact]
    public async Task PassesAcceptedImagesUnchanged()
    {
        var original = this.Picture(MagickFormat.Png, "image/png");
        var compatible = new CompatibleImage(original, new AcceptedTypes("image/png"));
        Assert.Equal((await original.Bytes()).ToArray(), (await compatible.Bytes()).ToArray());
    }

    [Fact]
    public async Task ConvertsImagesTheModelCantRead()
    {
        var llm = new FakeLlm(new JsonOf("""{"total": 1}"""), new AcceptedTypes("image/jpeg"));
        await new LlmReceipts(new CompatibleLlm(llm)).Receipt(this.Picture(MagickFormat.Bmp, "image/bmp"));
        var prompt = Assert.Single(llm.Prompts());
        Assert.Equal(["image/jpeg"], await new PrintedPrompt<IReadOnlyList<string>>(prompt, new MimesMedia()).Value());
    }

    private IImage Picture(MagickFormat format, string mime)
    {
        using var image = new MagickImage(MagickColors.White, 400, 200);
        return new TypedImage(new ArrayContent(image.ToByteArray(format)), mime);
    }
}
