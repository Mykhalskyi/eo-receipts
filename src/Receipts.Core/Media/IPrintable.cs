namespace Receipts;

/// <summary>
/// An object that prints itself instead of exposing its data through getters.
/// </summary>
public interface IPrintable
{
    IMedia<T> Print<T>(IMedia<T> media);
}
