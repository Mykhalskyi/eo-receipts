namespace Receipts.Tests;

public sealed class MediaTests
{
    [Fact]
    public void PrintsJson()
    {
        var json = new JsonReceipt(new JsonOf(new CafeJson().ToString())).Print(new JsonMedia()).Value();
        Assert.Equal("Corner Cafe", json["merchant"]!["name"]!.GetValue<string>());
        Assert.Equal("EUR", json["currency"]!.GetValue<string>());
        Assert.Equal(2, json["lines"]!.AsArray().Count);
        Assert.Equal(8.2m, json["subtotal"]!.GetValue<decimal>());
        Assert.Equal("4242", json["payment"]!["cardLast4"]!.GetValue<string>());
    }

    [Fact]
    public void KeepsMediaImmutable()
    {
        var empty = new JsonMedia();
        empty.With("name", "value");
        Assert.Empty(empty.Value());
    }

    [Fact]
    public void PrintsCsvRowPerLine()
    {
        var csv = new JsonReceipt(new JsonOf(new CafeJson().ToString())).Print(new CsvMedia()).Value();
        var rows = csv.TrimEnd().Split(Environment.NewLine);
        Assert.Equal(3, rows.Length);
        Assert.Contains("merchant.name", rows[0]);
        Assert.Contains("lines.total", rows[0]);
        Assert.Contains("taxes.0.amount", rows[0]);
        Assert.Contains("\"1 Main St, Berlin\"", rows[1]);
        Assert.Contains("Bagel", rows[2]);
    }

    [Fact]
    public void PrintsRecord()
    {
        var record = new JsonReceipt(new JsonOf(new CafeJson().ToString())).Print(new RecordMedia()).Value();
        Assert.Equal("Corner Cafe", record.Merchant.Name);
        Assert.Equal(3.2m, record.Lines[1].Total);
        Assert.Equal("card", record.Payment.Method);
        Assert.Equal("2026-03-14T12:30", record.Date);
    }
}
