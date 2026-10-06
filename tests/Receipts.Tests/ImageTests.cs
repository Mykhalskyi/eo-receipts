namespace Receipts.Tests;

public sealed class ImageTests
{
    [Theory]
    [InlineData("FFD8FFE000104A464946", "image/jpeg")]
    [InlineData("89504E470D0A1A0A0000", "image/png")]
    [InlineData("474946383961010001", "image/gif")]
    [InlineData("524946460000000057454250", "image/webp")]
    [InlineData("000000186674797068656963", "image/heic")]
    [InlineData("0000001C667479706D696631", "image/heic")]
    [InlineData("49492A0008000000", "image/tiff")]
    [InlineData("4D4D002A00000008", "image/tiff")]
    [InlineData("424D3600000000000000", "image/bmp")]
    public async Task DetectsFormatFromLeadingBytes(string head, string mime)
    {
        var image = new DetectedImage(new ArrayContent(Convert.FromHexString(head)));
        Assert.Equal(mime, (await image.Type()).Mime());
    }

    [Fact]
    public async Task RefusesUnknownFormat()
    {
        var image = new DetectedImage(new ArrayContent("%PDF-1.7"u8.ToArray()));
        await Assert.ThrowsAsync<UnsupportedImageException>(() => image.Type());
    }

    [Fact]
    public async Task DetectsCustomSignature()
    {
        var image = new DetectedImage(
            new ArrayContent(Convert.FromHexString("0000000C6A5020200D0A870A")),
            [new Signature("image/jp2", 4, "jP  ")]
        );
        Assert.Equal("image/jp2", (await image.Type()).Mime());
    }

    [Fact]
    public async Task DetectedImageReadsContentOnce()
    {
        var content = new CountingContent(new ArrayContent(Convert.FromHexString("FFD8FF00")));
        var image = new DetectedImage(content);
        await image.Type();
        await image.Bytes();
        await image.Type();
        Assert.Equal(1, content.Reads);
    }

    [Fact]
    public async Task KeepsTypeOfTypedImages()
    {
        var content = new ArrayContent([1, 2, 3]);
        Assert.Equal("image/heic", (await new HeicImage(content).Type()).Mime());
        Assert.Equal("image/webp", (await new WebpImage(content).Type()).Mime());
        Assert.Equal(new byte[] { 1, 2, 3 }, (await new TiffImage(content).Bytes()).ToArray());
    }

    [Fact]
    public async Task ReadsBase64AndStreams()
    {
        Assert.Equal(new byte[] { 1, 2, 3 }, (await new Base64Content("AQID").Bytes()).ToArray());
        using var stream = new MemoryStream([4, 5]);
        Assert.Equal(new byte[] { 4, 5 }, (await new StreamedContent(stream).Bytes()).ToArray());
    }

    [Fact]
    public void ComparesAcceptedTypesIgnoringCase()
    {
        var types = new CommonTypes(new AcceptedTypes("image/png", "image/jpeg"), new AcceptedTypes("IMAGE/PNG"));
        Assert.True(types.Contains(new MediaType("image/png")));
        Assert.False(types.Contains(new MediaType("image/jpeg")));
    }
}
