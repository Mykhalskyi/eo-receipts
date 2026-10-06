namespace Receipts;

/// <summary>
/// Data-only snapshot of a receipt, produced by <see cref="RecordMedia"/>.
/// </summary>
public sealed record ReceiptRecord(
    MerchantRecord Merchant,
    string Date,
    string Currency,
    IReadOnlyList<LineRecord> Lines,
    IReadOnlyList<TaxRecord> Taxes,
    decimal Subtotal,
    decimal Total,
    PaymentRecord Payment
);
