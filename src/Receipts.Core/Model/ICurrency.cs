namespace Receipts;

public interface ICurrency
{
    /// <summary>
    /// ISO 4217 code, "XXX" when unknown.
    /// </summary>
    string Code();
}
