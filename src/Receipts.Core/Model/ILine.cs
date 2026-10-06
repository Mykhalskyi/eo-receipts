namespace Receipts;

/// <summary>
/// One purchased item.
/// </summary>
public interface ILine : IPrintable
{
    string Name();

    decimal Quantity();

    IMoney UnitPrice();

    IMoney Total();
}
