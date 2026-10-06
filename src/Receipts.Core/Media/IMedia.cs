namespace Receipts;

/// <summary>
/// Immutable destination of printed data. Every <c>With</c> returns a new media,
/// <see cref="Value"/> renders the result.
/// </summary>
public interface IMedia<out T>
{
    IMedia<T> With(string name, string value);

    IMedia<T> With(string name, decimal value);

    IMedia<T> With(string name, IPrintable section);

    IMedia<T> With(string name, IEnumerable<IPrintable> items);

    T Value();
}
