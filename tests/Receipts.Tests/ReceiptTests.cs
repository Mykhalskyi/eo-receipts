using Receipts.Testing;

namespace Receipts.Tests;

public sealed class ReceiptTests
{
    [Fact]
    public void ReadsAllFields()
    {
        var receipt = new JsonReceipt(new JsonOf(new CafeJson().ToString()));
        Assert.Equal("Corner Cafe", receipt.Merchant().Name());
        Assert.Equal("DE123456789", receipt.Merchant().TaxId());
        Assert.Equal(new DateTimeOffset(2026, 3, 14, 12, 30, 0, TimeSpan.Zero), receipt.Date());
        Assert.Equal(2, receipt.Lines().Count);
        Assert.Equal("Cappuccino", receipt.Lines()[0].Name());
        Assert.Equal(2m, receipt.Lines()[0].Quantity());
        Assert.Equal(2.5m, receipt.Lines()[0].UnitPrice().Amount());
        Assert.Equal(19m, receipt.Taxes()[0].Rate());
        Assert.Equal(1.31m, receipt.Taxes()[0].Amount().Amount());
        Assert.Equal(8.2m, receipt.Total().Amount());
        Assert.Equal("EUR", receipt.Total().Currency().Code());
        Assert.Equal("4242", receipt.Payment().CardLast4());
    }

    [Fact]
    public void FallsBackToSumOfLinesForSubtotal()
    {
        var receipt = new JsonReceipt(new JsonOf(new CafeJson().ToString()));
        Assert.Equal(8.2m, receipt.Subtotal().Amount());
    }

    [Fact]
    public void FallsBackToTotalWithoutTaxesWithoutLines()
    {
        var receipt = new JsonReceipt(new JsonOf("""{"total": 12, "taxes": [{"name": "VAT", "rate": 20, "amount": 2}]}"""));
        Assert.Equal(10m, receipt.Subtotal().Amount());
    }

    [Fact]
    public void ReportsAbsentDate()
    {
        var receipt = new JsonReceipt(new JsonOf("""{"date": "", "total": 1}"""));
        Assert.Throws<AbsentFieldException>(() => receipt.Date());
    }

    [Fact]
    public void UsesUnknownCurrencyCode()
    {
        var receipt = new JsonReceipt(new JsonOf("""{"currency": "", "total": 1}"""));
        Assert.Equal("XXX", receipt.Total().Currency().Code());
    }

    [Fact]
    public void ToleratesNumbersAsStringsAndMissingParts()
    {
        var receipt = new JsonReceipt(new JsonOf("""{"total": "12.50", "lines": [{"name": "Tea", "unitPrice": "2.5"}]}"""));
        Assert.Equal(12.5m, receipt.Total().Amount());
        Assert.Equal(1m, receipt.Lines()[0].Quantity());
        Assert.Equal(2.5m, receipt.Lines()[0].Total().Amount());
        Assert.Equal(string.Empty, receipt.Merchant().Name());
        Assert.Equal(string.Empty, receipt.Payment().Method());
        Assert.Empty(receipt.Taxes());
    }

    [Fact]
    public void RejectsNonNumericAmounts()
    {
        var receipt = new JsonReceipt(new JsonOf("""{"total": "twelve"}"""));
        Assert.Throws<MalformedAnswerException>(() => receipt.Total());
    }

    [Fact]
    public void RejectsNonJsonAnswer()
    {
        Assert.Throws<MalformedAnswerException>(() => new JsonOf("Sorry, I can't").Root());
    }

    [Fact]
    public void AcceptsConsistentReceipt()
    {
        var receipt = new ValidReceipt(new JsonReceipt(new JsonOf(new CafeJson().ToString())));
        Assert.Equal(8.2m, receipt.Total().Amount());
    }

    [Fact]
    public void ListsEveryContradiction()
    {
        var receipt = new ValidReceipt(
            new JsonReceipt(
                new JsonOf(
                    """
                    {
                      "date": "2999-01-01",
                      "lines": [{ "name": "Tea", "quantity": 2, "unitPrice": 2, "total": 5 }],
                      "taxes": [],
                      "subtotal": 5,
                      "total": 9
                    }
                    """
                )
            )
        );
        var error = Assert.Throws<InvalidReceiptException>(() => receipt.Total());
        Assert.Equal(3, error.Violations.Count);
    }

    [Fact]
    public void AddsMoneyOfSameCurrencyOnly()
    {
        Assert.Equal(3m, new Money(1, "EUR").Plus(new Money(2, "eur")).Amount());
        Assert.Throws<InvalidOperationException>(() => new Money(1, "EUR").Plus(new Money(2, "USD")));
    }

    [Fact]
    public void RemembersValues()
    {
        var receipt = new StickyReceipt(new JsonReceipt(new JsonOf(new CafeJson().ToString())));
        Assert.Same(receipt.Lines(), receipt.Lines());
    }

    [Fact]
    public async Task ReadsReceiptThroughModel()
    {
        var llm = new FakeLlm(new CafeJson().ToString());
        var receipt = await new LlmReceipts(llm).Receipt(new TinyPng().Image());
        Assert.Equal("Corner Cafe", receipt.Merchant().Name());
        var prompt = Assert.Single(llm.Prompts());
        Assert.Equal(["image/png"], await new PrintedPrompt<IReadOnlyList<string>>(prompt, new MimesMedia()).Value());
        Assert.Equal("object", prompt.Schema().Json()["type"]!.GetValue<string>());
    }

    [Fact]
    public async Task FailsFastOnInvalidReceipt()
    {
        var receipts = new ValidReceipts(new LlmReceipts(new FakeLlm("""{"subtotal": 5, "taxes": [], "total": 9}""")));
        await Assert.ThrowsAsync<InvalidReceiptException>(() => receipts.Receipt(new TinyPng().Image()));
    }

    [Fact]
    public async Task ChoosesTotalConfirmedByMajority()
    {
        var receipts = new VotedReceipts(
            new LlmReceipts(new FakeLlm("""{"total": 10, "currency": "EUR", "merchant": {"name": "A"}}""")),
            new LlmReceipts(new FakeLlm("""{"total": 99, "currency": "EUR", "merchant": {"name": "B"}}""")),
            new LlmReceipts(new FakeLlm("""{"total": 10.00, "currency": "EUR", "merchant": {"name": "C"}}"""))
        );
        var receipt = await receipts.Receipt(new TinyPng().Image());
        Assert.Equal(10m, receipt.Total().Amount());
        Assert.NotEqual("B", receipt.Merchant().Name());
    }

    [Fact]
    public async Task RefusesWithoutMajority()
    {
        var receipts = new VotedReceipts(
            new LlmReceipts(new FakeLlm("""{"total": 10}""")),
            new LlmReceipts(new FakeLlm("""{"total": 11}""")),
            new LlmReceipts(new BrokenLlm())
        );
        await Assert.ThrowsAsync<InvalidReceiptException>(() => receipts.Receipt(new TinyPng().Image()));
    }

    [Fact]
    public void AddsCultureHintToPrompt()
    {
        var prompt = new LocalizedPrompt(new ReceiptPrompt(new TinyPng().Image()), new System.Globalization.CultureInfo("de-DE"));
        Assert.Equal(3, prompt.Parts().Count);
    }
}
