namespace Receipts;

public sealed record LineRecord(string Name, decimal Quantity, decimal UnitPrice, decimal Total);
