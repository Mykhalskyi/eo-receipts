namespace Receipts;

public interface IMoney
{
    decimal Amount();

    ICurrency Currency();

    IMoney Plus(IMoney other);
}
