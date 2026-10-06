namespace Receipts;

/// <summary>
/// Immutable destination of prompt parts. Each call returns a new media.
/// </summary>
public interface IPartMedia<T>
{
    Task<IPartMedia<T>> Text(string text, CancellationToken ct = default);

    Task<IPartMedia<T>> Image(IImage image, CancellationToken ct = default);

    T Value();
}
