namespace Receipts;

/// <summary>
/// Receipts read by several models at once. The result is a receipt whose total
/// (amount and currency) is confirmed by the majority of all models;
/// without a majority it fails with <see cref="InvalidReceiptException"/>.
/// </summary>
public sealed class VotedReceipts : IReceipts
{
    private readonly IReadOnlyList<IReceipts> voters;

    public VotedReceipts(params IReceipts[] voters) : this((IReadOnlyList<IReceipts>)voters)
    {
    }

    public VotedReceipts(IReadOnlyList<IReceipts> voters)
    {
        this.voters = voters;
    }

    public async Task<IReceipt> Receipt(IImage image, CancellationToken ct = default)
    {
        var shared = new StickyImage(image);
        var votes = await Task.WhenAll(this.voters.Select(voter => this.Vote(voter, shared, ct)));
        var valid = votes.Where(vote => vote.Receipt is not null).ToList();
        if (valid.Count == 0)
        {
            throw new ReceiptException("No model could read the receipt", new AggregateException(votes.Select(vote => vote.Error!)));
        }
        var winner = valid.GroupBy(vote => vote.Key).OrderByDescending(group => group.Count()).First();
        if (winner.Count() * 2 <= this.voters.Count)
        {
            throw new InvalidReceiptException(
                [$"models disagree on the total: {string.Join(", ", votes.Select(vote => vote.Key))}"]
            );
        }
        return winner.First().Receipt!;
    }

    private async Task<(IReceipt? Receipt, string Key, Exception? Error)> Vote(IReceipts voter, IImage image, CancellationToken ct)
    {
        try
        {
            var receipt = new StickyReceipt(await voter.Receipt(image, ct));
            var total = receipt.Total();
            return (receipt, $"{total.Amount():0.00} {total.Currency().Code()}", null);
        }
        catch (ReceiptException e)
        {
            return (null, "failed", e);
        }
    }
}
