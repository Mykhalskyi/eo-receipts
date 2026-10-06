namespace Receipts;

public interface ITax : IPrintable
{
    string Name();

    /// <summary>
    /// Percent, 20 for 20%.
    /// </summary>
    decimal Rate();

    IMoney Amount();
}
